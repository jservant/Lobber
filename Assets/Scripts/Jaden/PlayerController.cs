using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

    //CapsuleCollider capCol;
    Rigidbody rb;
    Animator animr;
    //[SerializeField] bool animBuffer = false;
    MeshRenderer headMesh;
    GameObject headProj;
    Transform projSpawn;
    BoxCollider axeHitbox;
    List<GameObject> enemiesHit;

    public DefaultPlayerActions pActions;

    public Vector2 mInput;                        // movement vector read from input
    Vector3 movement;                             // actual movement vector used. mInput(x, y) = movement(x, z)
    [SerializeField] float speed = 10f;           // top player speed
    float timeMoved = 0f;                         // how long has player been moving for?
    [SerializeField] float maxSpeedTime = 2f;     // how long does it take for player to reach max speed?
    [SerializeField] int damage = 5;
    [SerializeField] float turnSpeed = 0.1f;
    [SerializeField] AnimationCurve curve;
    [SerializeField] float animTimer = 0f;
    public enum States { Idle, Walking, Attacking, Hitstunned };
    public States currentState = 0;

    public enum Attacks { None, Attack, Chop, ChopThrow };
    public Attacks currentAttack = 0;

    float turnVelocity;

    private void Awake()
    {
        //capCol = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        animr = GetComponent<Animator>();
        pActions = new DefaultPlayerActions();

        headMesh = transform.Find("Weapon_Controller/Hitbox/StoredHead").GetComponent<MeshRenderer>();
        axeHitbox = transform.Find("Weapon_Controller/Hitbox").GetComponent<BoxCollider>();
        projSpawn = transform.Find("ProjSpawn");
        headProj = Resources.Load("ActivePrefabs/HeadProjectile", typeof(GameObject)) as GameObject;

        #region debug
        if (headMesh != null) { Debug.Log("Axe headmesh found on player."); } else { Debug.LogWarning("Axe headmesh not found on player."); }
        if (headProj != null) { Debug.Log("Head projectile found in Resources."); } else { Debug.LogWarning("Head projectile not found in Resources."); }
        #endregion
    }

    private void FixedUpdate() // calculate movement here
    {
        // accel/decel for movement
        if (mInput != Vector2.zero && currentState == States.Attacking) { timeMoved -= (Time.fixedDeltaTime / 2); }
        // if attacking, reduce movement at half speed to produce sliding effect
        else if (mInput != Vector2.zero) { timeMoved += Time.fixedDeltaTime; } // && currentState != (int)States.Attacking
                                                                               // else build up speed while moving
        else { timeMoved -= Time.fixedDeltaTime; }
        // if no movement input and not attacking, decelerate
        timeMoved = Mathf.Clamp(timeMoved, 0, maxSpeedTime);
        // clamp accel value between 0 and a static maximum

        // ryan's adapted movement code, meant to lerp player movement/rotation
        if (movement.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnSpeed);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            transform.position += moveDirection.normalized * (speed * Mathf.Lerp(0, 1, curve.Evaluate(timeMoved / maxSpeedTime))) * Time.fixedDeltaTime;
            // this moves the player and includes the anim curve accel/decel
        } else {
            transform.rotation = Quaternion.Euler(0f, transform.rotation.y, 0f);
        }

        //@TODO(Jaden): maaybe snapto enemy? 
    }

    private void Update() // calculate time and input here
    {
        if (currentState != States.Hitstunned) { Input(); }
        if (currentAttack != Attacks.None || currentState == States.Hitstunned)
        {
            // animator controller
            animTimer -= Time.deltaTime;
            if (animTimer <= 0) // reset everything after animation is done
            {
                if (currentAttack == Attacks.Chop) { axeHitbox.size = new Vector3(axeHitbox.size.x, axeHitbox.size.y, 50f); }
                currentAttack = Attacks.None;
                //animr.SetInteger("CurrentAttack", currentAttack);
                animr.Play("Base Layer.Character_Idle");
                currentState = States.Idle;
                animTimer = 0;
            }
        }

        //Camera.main.transform.LookAt(transform);
        //Camera.main.transform.Translate(new Vector3(camControl.x, camControl.y, 0) * Time.deltaTime);
    }

    void Input() // processes input, runs in update
    {
        if (currentState != States.Attacking)
        {
            mInput = pActions.Player.Move.ReadValue<Vector2>();
            if (pActions.Player.Move.WasReleasedThisFrame())
            {
                //animr.SetBool("walking", false);
                animr.Play("Base Layer.Character_Idle");
                currentState = States.Idle;
            } else if (pActions.Player.Move.phase == InputActionPhase.Started)
            {
                //animr.SetBool("walking", true);
                currentState = States.Walking;
                animr.Play("Base Layer.Character_Run");
                movement = movement = new Vector3(mInput.x, 0, mInput.y);
            } else if (pActions.Player.Move.phase == InputActionPhase.Waiting) { animr.Play("Base Layer.Character_Idle"); }
        }

        if (pActions.Player.Attack.WasPerformedThisFrame())
        {
            if (currentAttack == Attacks.None)
            {
                setCurrentAttack(Attacks.Attack, "Base Layer.Character_Attack1", 0.533f);
            }
            //StartCoroutine(AnimBuffer("attack", .38f, true)); } //.73f
            currentState = States.Attacking;
            //@TODO(Jaden): Move forward slightly when attacking
        }

        if (pActions.Player.Lob.WasPerformedThisFrame())
        {
            if (currentAttack == Attacks.None)
            {
                if (headMesh.enabled == true)
                {
                    currentState = States.Attacking;
                    setCurrentAttack(Attacks.ChopThrow, "Base Layer.Character_Chop_Throw", 1.067f);
                    //StartCoroutine(AnimBuffer("lobThrow", .65f, true));
                    // all functionality following is in LobThrow which'll be triggered in the animator
                } else
                {
                    currentState = States.Attacking;
                    axeHitbox.size = new Vector3(axeHitbox.size.x, axeHitbox.size.y, 120f);
                    setCurrentAttack(Attacks.Chop, "Base Layer.Character_Chop", 1.333f);
                }
            }
        }

        if (pActions.Player.Restart.WasPerformedThisFrame())
        {
            Debug.Log("Restart called");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void LobThrow()
    { // triggered in animator
        headMesh.enabled = false;
        GameObject iHeadProj = Instantiate(headProj, projSpawn.position, transform.rotation);
        //iHeadProj.transform.Translate(new Vector3(mInput.x, 0, mInput.y) * projSpeed * Time.deltaTime);
    }

    //@TODO(Jaden): Add i-frames and trigger hitstun state when hit
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == (int)Layers.EnemyHitbox)
        { // player is getting hit
            Debug.Log(other.name + " just hit me, the player!");
            animTimer = .55f;
            currentState = States.Hitstunned;
            animr.Play("Character_GetHit");
        } else if (other.gameObject.layer == (int)Layers.EnemyHurtbox)
        { // player is hitting enemy
            // NOTE(Roskuski): I hit the enemy!
            if (currentAttack == Attacks.Chop)
            {
                //todo: enemy instantly dies
                headMesh.enabled = true;
            }
        }
    }

    //@TODO(Jaden): Add OnTriggerEnter to make axe hitbox work, remember to do hitstun on enemy
    // so it doesn't melt their health

    #region Minor utility functions
    /*IEnumerator AnimBuffer(string animName, float duration, bool offWhenDone)
    {
        if (animr.GetBool(animName) == true) yield break;
        animr.SetBool(animName, true);
        animBuffer = true;
        yield return new WaitForSeconds(duration);
        animBuffer = false;
        if (offWhenDone) {
            animr.SetBool(animName, false);
            if (animr.GetBool("walking") == true) { currentState = (int)States.Walking; }
            else currentState = (int)States.Idle;
        }
    }*/

    /*void LookAtMouse()
    {
        Vector3 mPos = Vector3.zero;
        Plane plane = new Plane(Vector3.up, 0);
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (plane.Raycast(ray, out distance))
        {
            mPos = ray.GetPoint(distance);
        }
        Vector3 heightCorrectedPoint = new Vector3(mPos.x, transform.position.y, mPos.z);
        //movement = heightCorrectedPoint
        Debug.Log("Mouse Look At point: " + heightCorrectedPoint);
        transform.LookAt(heightCorrectedPoint);
        //movement = heightCorrectedPoint; mayb for mouse attack dashing?
        //Debug.Log("heightCorrectedPoint: " + heightCorrectedPoint);
    }*/

    void setCurrentAttack(Attacks attack, string animName, float duration)
    {
        currentAttack = attack;
        animr.Play(animName);
        //animr.SetInteger("CurrentAttack", currentAttack);
        animTimer = duration;
    }

    void OnEnable() { pActions.Enable(); }
    void OnDisable() { pActions.Disable(); }
    #endregion
}
