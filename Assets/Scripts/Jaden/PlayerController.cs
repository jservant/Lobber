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
    List<GameObject> enemiesHit;

    public DefaultPlayerActions pActions;

    public Vector2 mInput;
    Vector3 movement;
    [SerializeField] float speed = 10f;           // top player speed
    float timeMoved = 0f;                         // how long has player been moving for?
    [SerializeField] float maxSpeedTime = 2f;     // how long does it take for player to reach max speed?
    [SerializeField] int damage = 5;
    [SerializeField] float turnSpeed = 0.1f;
    [SerializeField] AnimationCurve curve;
    [SerializeField] float animDuration = 0f;
    [SerializeField] float animTimer = 0f;
    [SerializeField] enum States { Idle, Walking, Attacking, Hitstunned };
    [SerializeField] int currentState = 0;

    [SerializeField] enum Attacks { None, Chop, Lob, LobThrow };
    [SerializeField] int currentAttack = 0;

    float turnVelocity;

    private void Awake()
    {
        //capCol = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        animr = GetComponent<Animator>();
        pActions = new DefaultPlayerActions();

        headMesh = transform.Find("Weapon_Controller/Hitbox/StoredHead").GetComponent<MeshRenderer>();
        projSpawn = transform.Find("ProjSpawn");
        headProj = Resources.Load("ActivePrefabs/HeadProjectile", typeof(GameObject)) as GameObject;

        #region debug
        if (headMesh != null) { Debug.Log("Axe headmesh found on player."); } else { Debug.LogWarning("Axe headmesh not found on player."); }
        if (headProj != null) { Debug.Log("Head projectile found in Resources."); } else { Debug.LogWarning("Head projectile not found in Resources."); }
        #endregion
    }

    private void FixedUpdate()
    {
        if (mInput != Vector2.zero && currentState == (int)States.Attacking) { timeMoved -= (Time.fixedDeltaTime / 2); }
        else if (mInput != Vector2.zero ) { timeMoved += Time.fixedDeltaTime; } // && currentState != (int)States.Attacking
        else { timeMoved -= Time.fixedDeltaTime; }
        timeMoved = Mathf.Clamp(timeMoved, 0, maxSpeedTime);

        if (movement.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnSpeed);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            transform.position += moveDirection.normalized * (speed * Mathf.Lerp(0, 1, curve.Evaluate(timeMoved / maxSpeedTime))) * Time.fixedDeltaTime;
        }

        //@TODO(Jaden): maaybe snapto? 
    }

    private void Update()
    {
        if (currentState != (int)States.Hitstunned) { Input(); }
        if (currentAttack != (int)Attacks.None)
        {
            animTimer -= Time.deltaTime;
            if (animTimer <= 0)
            {
                currentAttack = (int)Attacks.None;
                //animr.SetInteger("CurrentAttack", currentAttack);
                animr.Play("Base Layer.Character_Idle");
                currentState = (int)States.Idle;
                animTimer = 0;
            }
        }

        Vector3 camControl = pActions.Player.CameraControl.ReadValue<Vector2>();

        //Camera.main.transform.LookAt(transform);
        //Camera.main.transform.Translate(new Vector3(camControl.x, camControl.y, 0) * Time.deltaTime);
    }
        #region Player inputs
    void Input() // runs in update
    {
        #region Movement
        if (currentState != (int)States.Attacking) { 
            mInput = pActions.Player.Move.ReadValue<Vector2>();
            if (pActions.Player.Move.WasReleasedThisFrame())
            {
                //animr.SetBool("walking", false);
                animr.Play("Base Layer.Character_Idle");
                currentState = (int)States.Idle;
            } else if (pActions.Player.Move.phase == InputActionPhase.Started) {
                //animr.SetBool("walking", true);
                currentState = (int)States.Walking;
                animr.Play("Base Layer.Character_Run");
                movement = movement = new Vector3(mInput.x, 0, mInput.y);
            } 
            else if (pActions.Player.Move.phase == InputActionPhase.Waiting) { animr.Play("Base Layer.Character_Idle"); }
        }
        
        #endregion

        #region Attacking
        if (pActions.Player.Attack.WasPerformedThisFrame()) {
            if (currentAttack == (int)Attacks.None)
            {
                setCurrentAttack(Attacks.Chop, "Base Layer.Character_Attack1", 0.533f); 
            }
                //StartCoroutine(AnimBuffer("attack", .38f, true)); } //.73f
            currentState = (int)States.Attacking;
            //@TODO(Jaden): Move forward slightly when attacking
        }
        #endregion

        #region Lobbing

        if (pActions.Player.Lob.WasPerformedThisFrame()) {
            if (currentAttack == (int)Attacks.None) {
                if (headMesh.enabled == true) {
                    currentState = (int)States.Attacking;
                    setCurrentAttack(Attacks.LobThrow, "Base Layer.Character_LobThrow", 1.067f);
                    //StartCoroutine(AnimBuffer("lobThrow", .65f, true));
                    // all functionality following is in LobThrow which'll be triggered in the animator
                } else { 
                    currentState = (int)States.Attacking;
                    setCurrentAttack(Attacks.Lob, "Base Layer.Character_Attack2", 0.533f);
                }
            }
        }
        #endregion
    }

    public void Restart(InputAction.CallbackContext context)
    {
        Debug.Log("Restart called");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion

    public void LobThrow() { // triggered in animator
        headMesh.enabled = false;
        GameObject iHeadProj = Instantiate(headProj, projSpawn.position, transform.rotation);
        //iHeadProj.transform.Translate(new Vector3(mInput.x, 0, mInput.y) * projSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) //@TODO(Jaden): Add i-frames and trigger hitstun state when hit
    {
        if (other.gameObject.layer == (int)Layers.EnemyHurtbox)
        {
            Debug.Log(other.gameObject.name + "is hurting the player");
        } else if (other.gameObject.layer == (int)Layers.EnemyHitbox)
        {
            // NOTE(Roskuski): I hit the enemy!
            if (currentAttack == (int)Attacks.Lob)
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
        currentAttack = (int)attack;
        animr.Play(animName);
        //animr.SetInteger("CurrentAttack", currentAttack);
        animTimer = duration;
    }

    void OnEnable() { pActions.Enable(); }
    void OnDisable() { pActions.Disable(); }
    #endregion
}
