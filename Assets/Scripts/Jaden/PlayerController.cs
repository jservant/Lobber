using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {
    //CapsuleCollider capCol;
    Rigidbody rb;
    Animator animr;
    bool animBuffer = false;
    MeshRenderer headMesh;
    GameObject headProj;
    Transform projSpawn;
    List<GameObject> enemiesHit;

    PlayerInput pInput;
    InputActionAsset inputActions;
    InputAction moveAction;
    InputAction attackAction;
    InputAction lobAction;

    Vector2 mInput;
    Vector3 movement;
    [SerializeField] float speed = 10f;           // top player speed
    float timeMoved = 0f;                         // how long has player been moving for?
    [SerializeField] float maxSpeedTime = 2f;     // how long does it take for player to reach max speed?
    [SerializeField] int damage = 5;
    [SerializeField] AnimationCurve curve;
    enum States { Idle, Walking, Attacking };
    int currentState = 0;
    bool isGamepad;

    private void Awake()
    {
        //capCol = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        animr = GetComponent<Animator>();

        pInput = GetComponent<PlayerInput>();
        inputActions = pInput.actions;
        moveAction = inputActions.FindActionMap("Player").FindAction("Move");
        attackAction = inputActions.FindActionMap("Player").FindAction("Attack");
        lobAction = inputActions.FindActionMap("Player").FindAction("Lob");

        headMesh = transform.Find("Axe_Controller/AxeHitbox/StoredHead").GetComponent<MeshRenderer>();
        projSpawn = transform.Find("ProjSpawn");
        headProj = Resources.Load("Prefabs/HeadProjectile", typeof(GameObject)) as GameObject;

        #region debug
        if (headMesh != null) { Debug.Log("Axe headmesh found on player."); } else { Debug.LogWarning("Axe headmesh not found on player."); }
        if (headProj != null) { Debug.Log("Head projectile found in Resources."); } else { Debug.LogWarning("Head projectile not found in Resources."); }
        #endregion
    }

    private void FixedUpdate()
    {
        Input();
        if (mInput != Vector2.zero) { timeMoved += Time.fixedDeltaTime; } else { timeMoved -= Time.fixedDeltaTime; }
        timeMoved = Mathf.Clamp(timeMoved, 0, maxSpeedTime);
        if (currentState != (int)States.Attacking) transform.position += (movement * Time.fixedDeltaTime * (speed * Mathf.Lerp(0, 1, curve.Evaluate(timeMoved / maxSpeedTime)))); //.normalized;
        //@TODO(Jaden): maaybe snapto? add normalize
    }

    #region Player inputs
    void Input()
    {
        if (currentState != (int)States.Attacking)
        {
            mInput = moveAction.ReadValue<Vector2>();
            //if (mInput == Vector2.zero) { animr.SetBool("walking", false); }
        }
        if (moveAction.phase == InputActionPhase.Started) { 
            movement = movement = new Vector3(mInput.x, 0, mInput.y);
            transform.rotation = Quaternion.LookRotation(movement);
            animr.SetBool("walking", true);
            currentState = (int)States.Walking;
        } if (moveAction.WasReleasedThisFrame()) { animr.SetBool("walking", false); }

    }

    /*    public void Move(InputAction.CallbackContext context) {
            mInput = context.ReadValue<Vector2>();
            if (context.performed == true && currentState != (int)States.Attacking) {
                movement = new Vector3(mInput.x, 0, mInput.y);
                transform.rotation = Quaternion.LookRotation(movement);
                animr.SetBool("walking", true);
                currentState = (int)States.Walking;
            }
            if (context.canceled == true) {
                animr.SetBool("walking", false);
                //TODO(@Jaden): lerp movement
            }
            //Debug.Log("Move activated, current value: " + mInput);
        }*/

    public void Attack(InputAction.CallbackContext context)
    {
        if (animBuffer == false) StartCoroutine(AnimBuffer("attack", .73f, true));
        currentState = (int)States.Attacking;
        //@TODO(Jaden): Move forward slightly when attacking
        if (context.performed && isGamepad == false) { LookAtMouse(); }
    }

    public void Lob(InputAction.CallbackContext context)
    {
        if (animBuffer == false)
        {
            if (headMesh.enabled == true)
            {
                currentState = (int)States.Attacking;
                StartCoroutine(AnimBuffer("lobThrow", .7f, true));
                // all functionality following is in LobThrow which'll be triggered in the animator
            } else { currentState = (int)States.Attacking; StartCoroutine(AnimBuffer("lob", .73f, true)); }
        }
        if (context.performed && isGamepad == false) { LookAtMouse(); }

    }

    public void Restart(InputAction.CallbackContext context)
    {
        Debug.Log("Restart called");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion

    public void LobThrow()
    { // triggered in animator
        headMesh.enabled = false;
        GameObject iHeadProj = Instantiate(headProj, projSpawn.position, transform.rotation);
        //iHeadProj.transform.Translate(new Vector3(mInput.x, 0, mInput.y) * projSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == (int)Layers.EnemyHurtbox)
        {
            Debug.Log("The enemy is hitting me");
        } else if (other.gameObject.layer == (int)Layers.EnemyHitbox)
        {
            // NOTE(Roskuski): I hit the enemy!
            if (animr.GetBool("lob") == true)
            {
                //todo: enemy instantly dies
                Debug.Log("Lob landed");
                headMesh.enabled = true;
            }
        }
    }

    //@TODO(Jaden): Add OnTriggerEnter to make axe hitbox work, remember to do hitstun on enemy
    // so it doesn't melt their health

    #region Minor utility functions
    IEnumerator AnimBuffer(string animName, float duration, bool offWhenDone)
    {
        animr.SetBool(animName, true);
        animBuffer = true;
        yield return new WaitForSeconds(duration);
        animBuffer = false;
        if (offWhenDone)
        {
            animr.SetBool(animName, false);
            currentState = (int)States.Idle;
            if (animr.GetBool("walking") == true)
            {

            }
        }
    }

    void LookAtMouse()
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
        //movement = heightCorrectedPoint;
        transform.LookAt(heightCorrectedPoint);
        //Debug.Log("heightCorrectedPoint: " + heightCorrectedPoint);
    }

    public void OnDeviceChange(PlayerInput pInput)
    {
        isGamepad = pInput.currentControlScheme.Equals("Gamepad") ? true : false;
    }
    void OnEnable() { inputActions.FindActionMap("Player").Enable(); }
    void OnDisable() { inputActions.FindActionMap("Player").Disable(); }
    #endregion
}
