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


public class Enemy : MonoBehaviour
{
    // NOTE(Roskuski): Enemy ai state

    const int TraitMax = 1000;
    // NOTE(Roskuski): Range[0, 2*TraitMax] How aggsive this enemy will behave. Values below TraitMax will act Defensively!
    [SerializeField] int traitAggressive = 1000;
    // NOTE(Roskuski): Range[0, 2*TraitMax] Prefence for attacking from the player's behind and flanks. Values below TraitMax will prefer attacking from the front!
    [SerializeField] int traitSneaky = 1000;

    enum AiDirective {
        // Do nothing, intentionally
        Inactive,
        
        // Maintain a certain distance from the player
        MaintainDistance,
        // try to Flank to the player's left
        MaintainLeftFlank,
        // try to Flank to the player's right
        MaintainRightFlank,
        // try to Flank to the player's behind
        MaintainBackFlank,

        // @TODO Stunned state

        // Attack the player!
        PerformAttack, 
        // @TODO more attack states?
        // Or attack states are secondary?
    }
    AiDirective directive;
    
    float spawnWait = 2;
    float targetDistance;
    

    // NOTE(Roskuski): End of ai state


    public const int MaxHealth = 10;
    int health = MaxHealth;

    bool didHealthChange = false;

    // NOTE(Roskuski): Internal references
    NavMeshAgent navAgent;
    MeshRenderer meshWithMat;

    // NOTE(Roskuski): External references
    GameManager gameMan;

    // NOTE(Roskuski): To be called from sources of damage
    public void ReceiveDamage(int damage) {
        health -= damage;
        didHealthChange = true;
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
        Debug.Assert(2*TraitMax + Abs(bias) == choiceTotal);

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
            if (rollingTotal > roll) {
                result = index;
                break;
            }
        }
        
        Debug.Assert(result != -1);
        Debug.Log(this.name + " chose " + result + " with a roll of " + roll);
        return result;
    }

    bool CanAttemptNavigation() {
        return navAgent.pathStatus == NavMeshPathStatus.PathComplete ||
            navAgent.pathStatus == NavMeshPathStatus.PathPartial;
    }

    float DistanceToTravel() {
        float result = 0;
        if (navAgent.path.corners.Length > 2) {
            result += Vector3.Distance(this.transform.position, navAgent.path.corners[1]);
            for (int index = 1; index < navAgent.path.corners.Length; index += 1) {
                result += Vector3.Distance(navAgent.path.corners[index - 1], navAgent.path.corners[index]);
            }
        }
        return result;
    }

    // Start is called before the first frame update
    void Start()
    {
        navAgent = this.GetComponent<NavMeshAgent>();
        meshWithMat = transform.Find("Visual/The One with the material").GetComponent<MeshRenderer>();

        gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update() {

        switch (directive) {
            case AiDirective.Inactive:           meshWithMat.material.color = Color.cyan; break;
            case AiDirective.MaintainDistance:   meshWithMat.material.color = Color.green; break;
            case AiDirective.MaintainLeftFlank:  meshWithMat.material.color = Color.blue; break;
            case AiDirective.MaintainRightFlank: meshWithMat.material.color = Color.yellow; break;
            case AiDirective.MaintainBackFlank:  meshWithMat.material.color = Color.black; break;
            case AiDirective.PerformAttack:      meshWithMat.material.color = Color.red; break;
        }
        Vector3 playerPosition = gameMan.player.position;
        Vector3 playerPosition = gameMan.player.position;

        // Directive Changing
        if (directive == AiDirective.Inactive) {
            spawnWait -= Time.deltaTime;
            if (spawnWait < 0) {
                int aggressiveChoice = RollTraitChoice(traitAggressive, new int[]{1000, 1000}, 500);
                if (aggressiveChoice == 0) { // Defensive, make wide gap
                    
                    targetDistance = 6;
                    int sneakyChoice = RollTraitChoice(traitSneaky, new int[]{500, 600, 900}, 500);
                    if (sneakyChoice == 0) { // I don't care!
                        directive = AiDirective.MaintainDistance;
                    }
                    else if (sneakyChoice == 1) { // Left/Right flank
                        // @TODO(Roskuski): Determine based off of distance to the player.
                        int coinFlip = Random.Range(0, 2);
                        if (coinFlip == 0) {
                            directive = AiDirective.MaintainLeftFlank;
                        }
                        else {
                            directive = AiDirective.MaintainRightFlank;
                        }
                    }
                    else if (sneakyChoice == 2) { // Back flank
                        directive = AiDirective.MaintainBackFlank;
                    }
                }
                else if (aggressiveChoice == 1) { // Agressive, make narrow gap
                    directive = AiDirective.MaintainDistance;
                    targetDistance = 3;
                }
            }
        }
        else if (directive == AiDirective.MaintainDistance) {
            navAgent.SetDestination(playerPosition); 
            navAgent.stoppingDistance = targetDistance;
            if (DistanceToTravel() < 0.05) {
                int aggressiveChoice = RollTraitChoice(traitAggressive, new int[]{1250, 500}, 500);
                int sneakyChoice = RollTraitChoice(traitSneaky, new int[]{500, 600, 900}, 500);
                if (aggressiveChoice == 0) { // consider a flank
                }
                else if (aggressiveChoice == 1) { // Lets attack now!
                }
            }
        }
        else if (directive == AiDirective.MaintainLeftFlank) {
            // @Nexttime(Roskuski) Implment flanking!
            //playerPosition 
            //navAgent.SetDestination();
        }
        else if (directive == AiDirective.MaintainRightFlank) {
        }
        else if (directive == AiDirective.MaintainBackFlank) {
        }
        else if (directive == AiDirective.PerformAttack) {
        }
        else { Debug.Assert(false); }
    }
}
