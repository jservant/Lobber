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
    int traitAggressive = 1000;
    // NOTE(Roskuski): Range[0, 2*TraitMax] Prefence for attacking from the player's behind and flanks. Values below TraitMax will prefer attacking from the front!
    int traitSneaky = 1000;

    enum AiDirective {
        // Do nothing, intentionally
        Inactive,
        // Get to, and Maintain a distance from the player.
        MaintainDistance,
    }
    AiDirective directive;
    
    float spawnWait = 2;
    float targetDistance;
    bool wantsAttack;

    // NOTE(Roskuski): End of ai state
    public const int MaxHealth = 10;
    int health = MaxHealth;

    bool didHealthChange = false;

    // NOTE(Roskuski): Internal references
    NavMeshAgent navAgent;

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
            }
        }
        
        Debug.Assert(result != -1);
        return result;
    }

    void DirectiveChange() {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        navAgent = this.GetComponent<NavMeshAgent>();

        gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update() {
        Vector3 playerPosition = gameMan.player.position;
        // NOTE(Roskuski) if we're hurt, instill the fear of god into this enemy
        if (didHealthChange) {
        }
        bool wantDirectiveChange = false;

        // Directive Changing
        if (directive == AiDirective.Inactive) {
            spawnWait -= Time.deltaTime;
            if (spawnWait < 0) {
                int choice = RollTraitChoice(traitAggressive, new int[]{1000, 1000}, 500);
                if (choice == 0) { // Defensive, make wide gap
                    //AiDirective
                }
                if (choice == 1) { // Agressive, make narrow gap
                }
            }
            targetDistance = 2;
            navAgent.stoppingDistance = targetDistance; // @TODO(Roskuski): aggressivly keep this in sync with targetDistance?
            navAgent.SetDestination(playerPosition);
        }
    }
}
