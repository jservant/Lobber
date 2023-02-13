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

    // NOTE(Roskuski): This should stay in sync with the animation controller. DO NOT ADD ELEMENTS IN THE MIDDLE OF THE ENUM
    enum Directive {
        // Do nothing, intentionally
        Inactive = 0,
        
        // Maintain a certain distance from the player, perhaps with a certain offset
        MaintainDistancePlayer,

        // Attack the player!
        PerformAttack, 

        // @TODO Stunned state
    }
    [SerializeField] Directive directive;

    // NOTE(Roskuski): This should stay in sync with the animation controller. DO NOT ADD ELEMENTS IN THE MIDDLE OF THE ENUM
    enum Attack : int {
        None = 0,
        Slash,
        Lunge,
    }
    Attack currentAttack = Attack.None; // NOTE(Roskuski): Do not set this manually, use the setting function, as that keeps animation state insync
    
    float inactiveWait = 2;
    float approchDistance;
    Vector3 targetOffset; 
    bool preferRightStrafe;

    int failedAttackRolls = 0;
    float attackTimer = 1.0f; // @TODO(Roskuski) Fine tune this parameter

    Quaternion moveDirection = Quaternion.identity;

    // NOTE(Roskuski): End of ai state

    [SerializeField] public int MaxHealth = 10;
    int health = 0;

    public const float LungeSpeed = 15;
    // NOTE(Roskuski): copied from the default settings of navMeshAgent
    public const float MoveSpeed = 7f;
    public const float TurnSpeed = 360.0f; // NOTE(Roskuski): in Degrees per second

    public const float TightApprochDistance = 4;
    public const float CloseApprochDistance = 6;
    public const float LooseApprochDistance = 10; 
    public const float ApprochDeviance = 2;

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
        int roll = RollTrait(trait, rollRange, bias);

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

    // Yields the raw trait roll, see documentation above
    int RollTrait(int trait, int rollRange, int bias = 0) {
        int roll = Random.Range(-rollRange, rollRange + 1); // Max is exclusive in Range.Range(int,int)
        roll += bias + trait;
        // NOTE(Roskuski): Right now when doing a trait roll, the roll is capped to the max and min trait values.
        // There might be a reason to allow the enemies to roll beyond TraitMax and min, like when a bias would make it so, or when their trait is at an extreme and they also roll the same extreme.
        // implmenting this will complicate the math, and slightly complicate the process of making choiceChances.
        if (roll < 0)          { roll = 0; }
        if (roll > 2*TraitMax) { roll = 2*TraitMax; }
        return roll;
    }

    bool CanAttemptNavigation() {
        return navAgent.pathStatus == NavMeshPathStatus.PathComplete ||
            navAgent.pathStatus == NavMeshPathStatus.PathPartial;
    }

    float DistanceToTravel() {
        return navAgent.remainingDistance - approchDistance;
    }

    void ChangeDirective_Inactive(float inactiveWait) {
        this.directive = Directive.Inactive;
        this.inactiveWait = inactiveWait;
    }

    void ChangeDirective_MaintainDistancePlayer(float stoppingDistance, Vector3 targetOffset = default(Vector3)) {
        this.directive = Directive.MaintainDistancePlayer;
        this.approchDistance = stoppingDistance + Random.Range(0, ApprochDeviance);
        this.targetOffset = targetOffset;
    }

    // helper: logic for deteriming whigh following range is being used.
    bool UsingApprochRange(float distance) {
        return (approchDistance >= distance - ApprochDeviance) && (approchDistance <= distance + ApprochDeviance);
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == (int)Layers.PlayerHurtbox) {
            // NOTE(Roskuski): Debug.Log("The Player is hitting me!");
            ReceiveDamage(5);
            // @TODO(Roskuski): How do we want to pass damage here?
            // we could _Name_ the other object with the amount of damage we're dealing.
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

        traitAggressive = Random.Range(0, TraitMax*2);
        traitSneaky = Random.Range(0, TraitMax*2);
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
                if (inactiveWait < 0) {
                    preferRightStrafe = Random.Range(0, 2) == 1 ? true : false;
                    int choiceAggressive = RollTraitChoice(traitAggressive, new int[]{250, 250, 1000, 500}, 500);
                    attackTimer = new float[]{3.0f, 2.0f, 1.0f, 0.5f}[choiceAggressive];

                    choiceAggressive = RollTraitChoice(traitAggressive, new int[]{500, 1000, 500}, 500);
                    switch (choiceAggressive) {
                        default: Debug.Assert(false, choiceAggressive); break;
                        case 0:
                            ChangeDirective_MaintainDistancePlayer(TightApprochDistance);
                        break;
                        case 1:
                            ChangeDirective_MaintainDistancePlayer(CloseApprochDistance);
                        break;
                        case 2:
                            ChangeDirective_MaintainDistancePlayer(LooseApprochDistance);
                        break;
                    }
                }
            break;
            case Directive.MaintainDistancePlayer: 
                navAgent.nextPosition = this.transform.position;
                
                navAgent.SetDestination(playerPosition + targetOffset);

                bool isBackpedaling = false;
                bool isStrafing = false; 

                if (CanAttemptNavigation() && navAgent.path.corners.Length >= 2) {
                    const float NearRadius = 2;

                    Vector3 nextNodeDelta = navAgent.path.corners[1] - this.transform.position;

                    Collider[] nearEnemies = Physics.OverlapSphere(this.transform.position, NearRadius, Mask.Get(Layers.EnemyHitbox));
                    Vector3[] nearEnemyDeltas;
                    if (nearEnemies.Length != 0) {
                        nearEnemyDeltas = new Vector3[nearEnemies.Length - 1];
                    }
                    else {
                        nearEnemyDeltas = new Vector3[0];
                    }

                    for (int index = 0; index < nearEnemyDeltas.Length; index += 1) {
                        if (nearEnemies[index].gameObject != this.gameObject) {
                            nearEnemyDeltas[index] = nearEnemies[index].transform.position - this.transform.position;
                        }
                    }
                    
                    float[] directionWeights = new float[16]; 
                    Quaternion angleStep = Quaternion.AngleAxis(360.0f / directionWeights.Length, Vector3.up);

                    // @TODO(Roskuski) @BeforePlaytest1 make pathfinding account for ledges enemies shouldn't fall off the arena

                    // Pathfinding phase 
                    {
                        Vector3 consideredDelta = Vector3.forward; 
                        for (int index = 0; index < directionWeights.Length; index += 1) {
                            directionWeights[index] = Vector3.Dot(nextNodeDelta.normalized, consideredDelta) + 1.0f;
                            // NOTE(Roskuski): Advance the angle to the next index.
                            consideredDelta = angleStep * consideredDelta;
                        }
                    }

                    // Consider if the player is moving straight for me!
                    // @TODO(Roskuski): is this a good way of determining if mInput is not inputting stuff?
                    if (DistanceToTravel() < 1.5f) {
                        Vector3 mInput3d = new Vector3(gameMan.playerController.mInput.x, 0, gameMan.playerController.mInput.y);
                        if (gameMan.playerController.mInput != Vector2.zero &&
                                (Vector3.Dot(mInput3d, this.transform.rotation * Vector3.forward) < -0.7)) {
                            isBackpedaling = true; 

                            Vector3 consideredDelta = Vector3.forward; 
                            for (int index = 0; index < directionWeights.Length; index += 1) {
                                float maxDot = Vector3.Dot(Quaternion.AngleAxis(180, Vector3.up) * nextNodeDelta.normalized, consideredDelta) + 1.0f;
                                directionWeights[index] = maxDot;

                                // NOTE(Roskuski): Advance the angle to the next index.
                                consideredDelta = angleStep * consideredDelta;
                            }
                        }
                        else { // Lets strafe around the player
                            isStrafing = true;
                            Vector3 consideredDelta = Vector3.forward; 
                            for (int index = 0; index < directionWeights.Length; index += 1) {
                                float rightScore = Vector3.Dot(Quaternion.AngleAxis(90, Vector3.up) * nextNodeDelta.normalized, consideredDelta) + 1.0f;
                                float leftScore = Vector3.Dot(Quaternion.AngleAxis(-90, Vector3.up) * nextNodeDelta.normalized, consideredDelta) + 1.0f;
                                if (preferRightStrafe) {
                                    leftScore *= 0.75f;
                                }
                                else if (!preferRightStrafe) {
                                    rightScore *= 0.75f;
                                }

                                directionWeights[index] = Mathf.Max(leftScore, rightScore);

                                // NOTE(Roskuski): Advance the angle to the next index.
                                consideredDelta = angleStep * consideredDelta;
                            }

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

                    // Consider Ledges
                    {
                        // @TODO(Roskuski): Currently it's very easy to convince enemies to walk off ledges by making them backpedal into them
                        // we should prevent this from happening in 90% of the time
                        Vector3 consideredDelta = Vector3.forward; 
                        for (int index = 0; index < directionWeights.Length; index += 1) {
                            float ledgeMod = 1;
                            NavMeshHit hit;
                            bool success = NavMesh.SamplePosition(this.transform.position + consideredDelta * MoveSpeed * Time.deltaTime, out hit, 0.5f, NavMesh.AllAreas);
                            if (success) { 
                                float hitDistance = Vector3.Distance(hit.position, this.transform.position);
                                ledgeMod = Mathf.Clamp01(hitDistance / (MoveSpeed * Time.deltaTime));
                            }
                            else {
                                ledgeMod = 0;
                            }

                            directionWeights[index] *= ledgeMod;

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
                }


                animator.SetBool("isBackpedaling", isBackpedaling);

                float speedModifier = 1.0f;
                if (isBackpedaling) {
                    speedModifier = 0.8f;
                }
                else if (isStrafing) {
                    speedModifier = 0.6f;
                }
                else {
                    speedModifier = Mathf.Lerp(0.5f, 1, (Vector3.Distance(this.transform.position, playerPosition + targetOffset) - approchDistance) + 1);
                }

                this.transform.position += moveDirection * Vector3.forward * MoveSpeed * speedModifier * Time.deltaTime;

                // Rotate the visual seperately
                if (distanceToPlayer < 6.25 || speedModifier < 0.75f) {
                    Vector3 deltaToPlayerNoY = deltaToPlayer;
                    deltaToPlayerNoY.y = 0;
                    Quaternion rotationDelta = Quaternion.LookRotation(deltaToPlayerNoY, Vector3.up);
                    this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, rotationDelta, TurnSpeed * Time.deltaTime);
                }
                else if (Quaternion.Angle(this.transform.rotation, this.moveDirection) > 2) {
                    this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, this.moveDirection, TurnSpeed * Time.deltaTime);
                }

                if (DistanceToTravel() < 1) {

                    // @TODO(Roskuski): This might look weird if the feet don't line up when we attempt to make an attack.
                    // might want to only choose an attack when it would blend sensiably in the animation.
                    // adding a data keyframe will make this a snap to impl.
                    //

                    // @TODO(Roskuski): determine formation changes here before attacking
                    // We probably want to bracket different choices based off of following distance.
                    // should we also make following distances a random range?

                    attackTimer -= Time.deltaTime;
                    if (attackTimer <= 0) {
                        if (UsingApprochRange(LooseApprochDistance)) {
                        }
                        else if (UsingApprochRange(TightApprochDistance) && false) {
                            int aggressiveChoice = RollTraitChoice(traitAggressive, new int[]{350, 350, 350, 950}, 500, failedAttackRolls * 200);
                            switch (aggressiveChoice) {
                                default: Debug.Assert(false); break;
                                case 0: // 
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
            break;
            case Directive.PerformAttack:
                // @BeforePlaytest1 @TODO(Roskuski): The current method for determining attacks sucks.

                // @TODO(Roskuski): These time values are based off of how long the animation is.
                // It might be best to get some varibled populated at the beginning of the game that contain
                // the length in seconds of each of these animations.
                // Regardless, I do not want to have these magic values in the code.
                // I can probably get the animation times from com while in their respective switch statments. Hopefully
                // enough time will have pass such that the animation is now active

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
                        // @TODO(Roskuski): we can encode timing logic into the keyframes of the animation. This can allow us to simplify the logic here.
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

        animator.SetInteger("Ai Directive", (int)directive);

        if (health < 0) {
            Destroy(this.gameObject);
        }
    }
}
