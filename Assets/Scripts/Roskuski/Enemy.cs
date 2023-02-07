using System.Collections;
using System.Collections.Generic;
using static System.Math;
using UnityEngine;
using UnityEngine.AI;


/* NOTE(Roskuski):
 * Enemy AI Directive: What this enemy wants to do.
 * Enemy AI Personality: Determines how this enemy chooses what to do.
 * Enemy AI Wants: actions this AI wants to do when given the oppertunity
 */


// @TODO(Roskuski): At a high level: enemies which are flanking should break out of flank if they get too close to the player.

public class Enemy : MonoBehaviour
{
    // NOTE(Roskuski): Enemy ai state

    const int TraitMax = 1000;
    // NOTE(Roskuski): How aggsive this enemy will behave. Values below TraitMax will act Defensively!
    [SerializeField, Range(0, 2*TraitMax)] int traitAggressive = 1000;
    // NOTE(Roskuski): Prefence for attacking from the player's behind and flanks. Values below TraitMax will prefer attacking from the front!
    [SerializeField, Range(0, 2*TraitMax)] int traitSneaky = 1000;

    enum Directive {
        // Do nothing, intentionally
        Inactive,
        
        // Maintain a certain distance from the player, perhaps with a certain offset
        MaintainDistancePlayer,

        // @TODO Stunned state

        // Attack the player!
        PerformAttack, 
    }
    [SerializeField] Directive directive;

    enum Attack {
        None,
        Sweep,
        Lunge,
    }
    Attack currentAttack = Attack.None;
    
    float inactiveWait = 2;
    float targetDistance;
    Vector3 targetOffset; 

    int failedAttackRolls = 0;
    float attackTimer = 1.0f; // @TODO(Roskuski) Fine tune this parameter
    

    // NOTE(Roskuski): End of ai state

    public const float LungeSpeed = 15;

    public const int MaxHealth = 10;
    int health = MaxHealth;

    bool didHealthChange = false;

    // NOTE(Roskuski): Internal references
    NavMeshAgent navAgent;
    MeshRenderer meshWithMat;
    GameObject hitboxObj;
    

    // NOTE(Roskuski): External references
    GameManager gameMan;

    // NOTE(Roskuski): To be called from sources of damage
    public void ReceiveDamage(int damage) {
        health -= damage;
        didHealthChange = true;
        Debug.Log("Player hit! Damage dealt: " + damage + " Remaining health: " + health);
    }

    /* NOTE(Roskuski): 
     * When rolling to select a chance the personality value is used as a sliding window into the total choice table.
     * trait: one of the AI traits
     * rollRange: controls the random roll from -rollRange to rollRange.
     * bias: value to add to the trait roll
     * choiceChances: array of lengths each choice takes up, see notes below
     * return: Index of the choice that we rolled
     *
     * choiceChances fill chances starting from the left most side of the choice "tape"
     * it is invalid to call this function without having a choice for every possible roll
     *
     *                                                           Enemy Trait Value
     *                                             roll range min|   roll range max
     *                    trait of zero\                   |     |     |                   /trait of max
     * Enemy Trait Space                |------------------[-----x--*--]------------------|
     *                                                              |
     * Choice Chances                   |------|----------|---------*------|--------|-----|
     *                                  choice1           choice3   |               choice5
     *                                         choice2              |      choice4         
     *                                                              roll + bias 
     */
    int RollTraitChoice(int trait, int[] choiceChances, int rollRange, int bias = 0) {
        int choiceTotal = 0;
        foreach (int value in choiceChances) {
            choiceTotal += value;
        }
        Debug.Assert(2*TraitMax == choiceTotal, choiceTotal + " Is not " + 2*TraitMax);

        int result = -1;
        int roll = Random.Range(-rollRange, rollRange + 1); // Max is exclusive in Range.Range(int,int)
        roll += bias + trait;
        // NOTE(Roskuski): Right now when doing a trait roll, the roll is capped to the max and min trait values.
        // There might be a reason to allow the enemies to roll beyond TraitMax and min, like when a bias would make it so, or when their trait is at an extreme and they also roll the same extreme.
        // implmenting this will complicate the math, and slightly complicate the process of making choiceChances.
        if (roll < 0)          { roll = 0; }
        if (roll > 2*TraitMax) { roll = 2*TraitMax; }

        int rollingTotal = 0;
        for (int index = 0; index < choiceChances.Length; index += 1) {
            rollingTotal += choiceChances[index];
            if (rollingTotal >= roll) {
                result = index;
                break;
            }
        }
        
        Debug.Assert(result != -1);
        //Debug.Log(this.name + " chose " + result + " with a roll of " + roll);
        return result;
    }

    bool CanAttemptNavigation() {
        return navAgent.pathStatus == NavMeshPathStatus.PathComplete ||
            navAgent.pathStatus == NavMeshPathStatus.PathPartial;
    }

    float DistanceToTravel() {
        float result = -1;
        if (navAgent.path.corners.Length >= 2) {
            result += Vector3.Distance(this.transform.position, navAgent.path.corners[1]);
            for (int index = 1; index < navAgent.path.corners.Length; index += 1) {
                result += Vector3.Distance(navAgent.path.corners[index - 1], navAgent.path.corners[index]);
            }
            result -= targetDistance;
        }
        return result;
    }

    void ChangeDirective_MaintainDistancePlayer(float targetDistance, Vector3 targetOffset = default(Vector3)) {
        this.directive = Directive.MaintainDistancePlayer;
        this.targetDistance = targetDistance;
        this.targetOffset = targetOffset;
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == (int)Layers.PlayerHurtbox) {
            // @TODO(Roskuski): Debug.Log("The Player is hitting me!");
        }
        if (other.gameObject.layer == (int)Layers.PlayerHitbox) {
            // @TODO(Roskuski): Debug.Log("I hit the player!"); 
        }
    }

    void Start() {
        navAgent = this.GetComponent<NavMeshAgent>();
        meshWithMat = transform.Find("Visual/The One with the material").GetComponent<MeshRenderer>();
        hitboxObj = transform.Find("AttackBox").gameObject;

        gameMan = transform.Find("/GameManager").GetComponent<GameManager>();

        hitboxObj.SetActive(false);
        gameMan.enemyList.Add(this);
    }

    void OnDestroy() {
        gameMan.enemyList.Remove(this);
    }

    void Update() {

        switch (directive) {
            case Directive.Inactive: meshWithMat.material.color = Color.cyan; break;
            case Directive.MaintainDistancePlayer: meshWithMat.material.color = Color.white; break;
            case Directive.PerformAttack: meshWithMat.material.color = Color.red; break;
        }
        Vector3 playerPosition = gameMan.player.position;
        Quaternion playerRotation = gameMan.player.rotation; 
        Vector3 deltaToPlayer = gameMan.player.position - this.transform.position;
        float distanceToPlayer = Vector3.Distance(this.transform.position, gameMan.player.position);

        // Directive Changing
        if (directive == Directive.Inactive) {
            inactiveWait -= Time.deltaTime; 
            // @TODO(Roskuski) roll for attackTimer?
            attackTimer = 1.0f;
            if (inactiveWait < 0) {
                int aggressiveChoice = RollTraitChoice(traitAggressive, new int[]{1000, 1000}, 500);
                if (aggressiveChoice == 0) { // Defensive, make wide gap

                    int sneakyChoice = RollTraitChoice(traitSneaky, new int[]{500, 600, 900}, 500);
                    if (sneakyChoice == 0) { // I don't care, but I'm scared!
                        ChangeDirective_MaintainDistancePlayer(6);
                    }
                    else if (sneakyChoice == 1) { // Left/Right flank
                        // @TODO(Roskuski): Determine based off of distance to the player.
                        int coinFlip = Random.Range(0, 2);
                        if (coinFlip == 0) {
                            ChangeDirective_MaintainDistancePlayer(2, Vector3.left * 6);
                        }
                        else {
                            ChangeDirective_MaintainDistancePlayer(2, Vector3.right * 6);
                        }
                    }
                    else if (sneakyChoice == 2) { // Back flank
                        ChangeDirective_MaintainDistancePlayer(2, Vector3.back * 6);
                    }
                }
                else if (aggressiveChoice == 1) { // Agressive, make narrow gap
                    ChangeDirective_MaintainDistancePlayer(3);
                }
            }
        }
        else if (directive == Directive.MaintainDistancePlayer) {
            navAgent.SetDestination(playerPosition + targetOffset);
            navAgent.stoppingDistance = targetDistance;

            // @TODO(Roskuski) this is an okay first pass at making enemys avoid eachother.
            // I think they should make an attempt to spread out as well, perhaps start orbiting the player in a sense?
            // Or that the ones the flank to a particular direction should target a wider area around the player.
            // This can be done cheeply by introducing randomness to the target location offset chosen.
            //
            // @TODO(Roskuski): Make enemies try and spread out from eachother when their density is too high.
            // we can probably just add project their next pathnode in a perpendicular direction from the player.
            // Maybe this flows into flanking states
            foreach (Enemy obj in gameMan.enemyList) {
                if (obj != this) { // ignore self
                    float delta = Vector3.Distance(obj.transform.position, this.transform.position);
                    if (delta <= 2) { 
                        this.transform.position += Vector3.Normalize(this.transform.position - obj.transform.position) * Mathf.Lerp(0, 2.0f/3.0f, 2 - delta);
                    }
                }
            }

            if (distanceToPlayer < 3) {
                ChangeDirective_MaintainDistancePlayer(3);
            }

            if (DistanceToTravel() < 1) {
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0) {
                    int aggressiveChoice = RollTraitChoice(traitAggressive, new int[]{350, 350, 350, 950}, 500, failedAttackRolls * 200);
                    switch (aggressiveChoice) {
                        default: Debug.Assert(false); break;
                        case 0: // fail with long wait
                            failedAttackRolls += 1;
                            attackTimer = 3;
                            break;
                        case 1: // fail with normal wait
                            failedAttackRolls += 1;
                            attackTimer = 1.5f;
                            break;
                        case 2: // fail with short wait
                            failedAttackRolls += 1;
                            attackTimer = 0.5f;
                            break;
                        case 3: // attack!
                            failedAttackRolls = 0;
                            directive = Directive.PerformAttack;
                            break;
                    }
                }
            }
        }
        else if (directive == Directive.PerformAttack) {
            switch (currentAttack) {
                case Attack.None:
                    navAgent.enabled = false;
                    if (distanceToPlayer <= 4) {
                        currentAttack = Attack.Sweep;
                        hitboxObj.SetActive(true);
                        attackTimer = 1.5f;
                    }
                    else {
                        currentAttack = Attack.Lunge;
                        hitboxObj.SetActive(true);
                        attackTimer = 0.5f; 
                    }
                    break;
                case Attack.Sweep:
                    attackTimer -= Time.deltaTime;
                    if (attackTimer < 0) {
                        hitboxObj.SetActive(false);
                        currentAttack = Attack.None;
                        directive = Directive.Inactive;
                        navAgent.enabled = true;
                        inactiveWait = 1.0f;
                    }
                    break;
                case Attack.Lunge:
                    attackTimer -= Time.deltaTime;
                    this.transform.position += (this.transform.rotation * Vector3.forward) * LungeSpeed * Time.deltaTime;
                    if (attackTimer < 0) {
                        // If we found ourselves off geometry, wait util we finish falling.
                        if (Physics.Raycast(this.transform.position, Vector3.down, 2)) {
                            hitboxObj.SetActive(false);
                            currentAttack = Attack.None;
                            directive = Directive.Inactive;
                            inactiveWait = 1.0f;
                            navAgent.enabled = true;
                        }
                    }
                    break;
            }
        }
        else { Debug.Assert(false); }

        if (health < 0) {
            Destroy(this.gameObject);
        }
    }

}
