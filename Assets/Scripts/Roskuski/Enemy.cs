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
    // @TODO(Rosksuki): Pacience trait, dynamic stat: repersents how badly this enemy wants to play the game

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

    // NOTE(Roskuski): This should stay in sync with the animation controller
    enum Attack : int {
        None = 0,
        Slash,
        Lunge,
    }
    Attack currentAttack = Attack.None; // NOTE(Roskuski): Do not set this manually, use the setting function, as that keeps animation state insync
    
    float inactiveWait = 2;
    float stoppingDistance;
    Vector3 targetOffset; 

    int failedAttackRolls = 0;
    float attackTimer = 1.0f; // @TODO(Roskuski) Fine tune this parameter

    [SerializeField] Quaternion moveDirection = Quaternion.identity;

    // NOTE(Roskuski): End of ai state

    [SerializeField] public float LungeSpeed = 15;

    [SerializeField] public int MaxHealth = 10;
    int health = 0;

    // NOTE(Roskuski): copied from the default settings of navMeshAgent
    [SerializeField] public float MoveSpeed = 3.5f;
    [SerializeField] public float TurnSpeed = 180; // NOTE(Roskuski): in Degrees per second

    // NOTE(Roskuski): Internal references
    NavMeshAgent navAgent;
    BoxCollider swordHitbox;
    Animator animator;
    

    // NOTE(Roskuski): External references
    GameManager gameMan;

    // NOTE(Roskuski): To be called from sources of damage
    public void ReceiveDamage(int damage) {
        health -= damage;
        Debug.Log("Player hit! Damage dealt: " + damage + " Remaining health: " + health);
    }

    void setCurrentAttack(Attack attack) {
        currentAttack = attack;
        animator.SetInteger("CurrentAttack", (int)currentAttack);
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
            result -= stoppingDistance;
        }
        return result;
    }

    void ChangeDirective_Inactive(float inactiveWait) {
        this.directive = Directive.Inactive;
        this.inactiveWait = inactiveWait;
    }

    void ChangeDirective_MaintainDistancePlayer(float stoppingDistance, Vector3 targetOffset = default(Vector3)) {
        this.directive = Directive.MaintainDistancePlayer;
        this.stoppingDistance = stoppingDistance;
        this.targetOffset = targetOffset;
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == (int)Layers.PlayerHurtbox) {
            // NOTE(Roskuski): Debug.Log("The Player is hitting me!");
            ReceiveDamage(5);
            // @TODO(Roskuski): How do we want to pass damage here?
            // we could *Name* the other object with the amount of damage we're dealing.
            // passing information via object names is kinda hacky but I don't think there's a better way to pass information into here wihtout using a get component
        }
        if (other.gameObject.layer == (int)Layers.PlayerHitbox) {
            // NOTE(Roskuski): Debug.Log("I hit the player!"); 
        }
    }

    void Start() {
        navAgent = this.GetComponent<NavMeshAgent>();
        animator = transform.Find("Visual").GetComponent<Animator>();
        swordHitbox = transform.Find("Visual/Sword_Base_Model").GetComponent<BoxCollider>();

        gameMan = transform.Find("/GameManager").GetComponent<GameManager>();

        swordHitbox.enabled = false;
        navAgent.updatePosition = false; 
        navAgent.updateRotation = false;

        gameMan.enemyList.Add(this);

        health = MaxHealth;
    }

    void OnDestroy() {
        gameMan.enemyList.Remove(this);
    }

    void Update() {
        Vector3 playerPosition = gameMan.player.position;
        Quaternion playerRotation = gameMan.player.rotation; 
        Vector3 deltaToPlayer = gameMan.player.position - this.transform.position;
        float distanceToPlayer = Vector3.Distance(this.transform.position, gameMan.player.position);


        // Directive Changing
        switch (directive) {
            case Directive.Inactive:
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
            break;
            case Directive.MaintainDistancePlayer: 
                navAgent.nextPosition = this.transform.position;
                navAgent.SetDestination(playerPosition + targetOffset);
                navAgent.stoppingDistance = stoppingDistance;

                if (CanAttemptNavigation() && navAgent.path.corners.Length >= 2) {
                    // @TODO(Roskuski) this logic might breakdown when accounting for height. Test Test Test!

                    const float NearRadius = 2;

                    Vector3 nextNodeDelta = navAgent.path.corners[1] - this.transform.position;

                    Collider[] nearEnemies = Physics.OverlapSphere(this.transform.position, NearRadius, Mask.Get(Layers.EnemyHitbox));
                    Vector3[] nearEnemyDeltas = new Vector3[nearEnemies.Length - 1];
                    for (int index = 0; index < nearEnemyDeltas.Length; index += 1) {
                        if (nearEnemies[index].gameObject != this.gameObject) {
                            nearEnemyDeltas[index] = nearEnemies[index].transform.position - this.transform.position;
                        }
                    }
                    
                    float[] directionWeights = new float[16]; 
                    Quaternion angleStep = Quaternion.AngleAxis(360.0f / directionWeights.Length, Vector3.up);

                    // Pathfinding phase 
                    {
                        Vector3 consideredDelta = Vector3.forward; 
                        for (int index = 0; index < directionWeights.Length; index += 1) {
                            directionWeights[index] = Vector3.Dot(nextNodeDelta.normalized, consideredDelta) + 1.0f;
                            // NOTE(Roskuski): Advance the angle to the next index.
                            consideredDelta = angleStep * consideredDelta;
                        }
                    }

                    // Consider enemies
                    {
                        Vector3 consideredDelta = Vector3.forward; 
                        for (int index = 0; index < directionWeights.Length; index += 1) {
                            // NOTE(Roskuski): [0, 1] 1 is no enemy contention, 0 is maximum enemy contention
                            float enemyMod = 1;
                            if (nearEnemyDeltas.Length > 0) {
                                float totalEnemyWeight = 0;
                                for (int enemyIndex = 0; enemyIndex < nearEnemyDeltas.Length; enemyIndex += 1) {
                                    // @TODO(Roskuski): the effect does not seem great. make the effect one one enemy extreme as POC?
                                    // NOTE(Roskuski): According to the docs, Mathf.Lerp clamps it's thrid param
                                    float enemyWeight = Mathf.Lerp(0, 1, nearEnemyDeltas[enemyIndex].magnitude / (NearRadius * 0.5f));
                                    totalEnemyWeight += (Vector3.Dot(consideredDelta.normalized, nearEnemyDeltas[enemyIndex].normalized) + 1) * enemyWeight; 
                                }
                                totalEnemyWeight /= nearEnemyDeltas.Length;
                                enemyMod = 1 - (totalEnemyWeight / 2);
                            }
                            directionWeights[index] *= enemyMod;

                            // NOTE(Roskuski): Advance the angle to the next index.
                            consideredDelta = angleStep * consideredDelta;
                        }
                    }

                    // Bias towards current direction
                    {
                        Vector3 consideredDelta = Vector3.forward; 
                        int currentDirectionIndex = -1;
                        float bestFit = -1;
                        for (int index = 0; index < directionWeights.Length; index += 1) {
                            float score = Vector3.Dot(consideredDelta.normalized, this.moveDirection.normalized * Vector3.forward);
                            if (score > bestFit) {
                                currentDirectionIndex = index;
                                bestFit = score;
                            }

                            // NOTE(Roskuski): Advance the angle to the next index.
                            consideredDelta = angleStep * consideredDelta;
                        }
                        directionWeights[currentDirectionIndex] *= 1.1f;
                    }

                    Quaternion chosenAngle = Quaternion.identity;
                    Quaternion consideredAngle = Quaternion.identity;
                    float bestWeight = -2;
                    for (int index = 0; index < directionWeights.Length; index += 1) {
                        if (directionWeights[index] > bestWeight) {
                            bestWeight = directionWeights[index];
                            chosenAngle = consideredAngle;
                        }
                        // NOTE(Roskuski): Advance the angle to the next index.
                        consideredAngle *= angleStep;
                    }


                    moveDirection = Quaternion.RotateTowards(moveDirection, chosenAngle, TurnSpeed * Time.deltaTime);

                    // Rotate the visual seperately
                    if (distanceToPlayer < 6.25) {
                        Vector3 deltaToPlayerNoY = deltaToPlayer;
                        deltaToPlayerNoY.y = 0;
                        Quaternion rotationDelta = Quaternion.LookRotation(deltaToPlayerNoY, Vector3.up);
                        this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, rotationDelta, TurnSpeed * Time.deltaTime);
                    }
                    else if (Quaternion.Angle(this.transform.rotation, this.moveDirection) > 2) {
                        this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, this.moveDirection, TurnSpeed * Time.deltaTime);
                    }
                }

                this.transform.position += moveDirection * Vector3.forward * MoveSpeed * Time.deltaTime;

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
            break;
            case Directive.PerformAttack:
                // @BeforePlaytest1 @TODO(Roskuski): The current method for determining attacks sucks.

                // @TODO(Roskuski): These time values are based off of how long the animation is.
                // It might be best to get some varibled populated at the beginning of the game that contain
                // the length in seconds of each of these animations.
                // Regardless, I do not want to have these magic values in the code.
                switch (currentAttack) {
                    case Attack.None:
                        navAgent.enabled = false;
                        if (distanceToPlayer <= 4) {
                            setCurrentAttack(Attack.Slash);
                            swordHitbox.enabled = true;
                            attackTimer = 0.733f;
                        }
                        else {
                            setCurrentAttack(Attack.Lunge);
                            swordHitbox.enabled = true;
                            attackTimer = 0.867f; 
                        }
                        break;
                    case Attack.Slash:
                        attackTimer -= Time.deltaTime;
                        if (attackTimer < 0) {
                            swordHitbox.enabled = false;
                            setCurrentAttack(Attack.None);
                            directive = Directive.Inactive;
                            navAgent.enabled = true;
                            inactiveWait = 1.0f;
                        }
                        break;
                    case Attack.Lunge:
                        attackTimer -= Time.deltaTime;
                        if (attackTimer > (0.867f * 0.05f) && attackTimer < (0.867f * 0.5f)) {
                            this.transform.position += (this.transform.rotation * Vector3.forward) * LungeSpeed * Time.deltaTime;
                        }
                        if (attackTimer < 0) {
                            // If we found ourselves off geometry, wait util we finish falling.
                            swordHitbox.enabled = false;
                            setCurrentAttack(Attack.None);
                            directive = Directive.Inactive;
                            inactiveWait = 1.0f;
                            navAgent.enabled = true;
                        }
                        break;
                }
            break;
            default: Debug.Assert(false); break;
        }

        if (health < 0) {
            Destroy(this.gameObject);
        }
    }
}
