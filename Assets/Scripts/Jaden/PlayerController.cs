using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    //CapsuleCollider capCol;
    Rigidbody rb;
    Animator animr;
    bool animBuffer = false;
    MeshRenderer headMesh;
    GameObject headProj;
    Transform projSpawn;
    List<GameObject> enemiesHit;

    Vector2 mInput;
    [SerializeField] float speed = 2f;
    [SerializeField] int damage = 5;
    enum States { Idle, Walking, Attacking }; // not implemented yet
    int currentState = 0; 

    private void Awake() {
        //capCol = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        animr = GetComponent<Animator>();
        headMesh = transform.Find("Axe_Controller/AxeHitbox/StoredHead").GetComponent<MeshRenderer>();
        projSpawn = transform.Find("ProjSpawn");
        headProj = Resources.Load("Prefabs/HeadProjectile", typeof(GameObject)) as GameObject;
        
        #region debug
        if (headMesh != null) { Debug.Log("Axe headmesh found on player."); }
        else { Debug.LogWarning("Axe headmesh not found on player."); }
        if (headProj != null) { Debug.Log("Head projectile found in Resources."); }
        else { Debug.LogWarning("Head projectile not found in Resources."); }
        #endregion
    }

    private void FixedUpdate() {
        Vector3 movement = new Vector3(mInput.x, 0, mInput.y).normalized;
        if (currentState != (int)States.Attacking) transform.position += (movement * Time.deltaTime * speed);
        //@TODO(Jaden): add lerp when you stop moving, also maaybe snapto?
    }

    #region Player inputs
    public void Move(InputAction.CallbackContext context) {
        mInput = context.ReadValue<Vector2>();
        if (context.performed == true) { 
            transform.rotation = Quaternion.LookRotation(new Vector3(mInput.x, 0, mInput.y));
            animr.SetBool("walking", true);
            currentState = (int)States.Walking;
        }
        if (context.canceled == true) {
            //currentState = (int)States.Idle;
            animr.SetBool("walking", false);
            //currentState = (int)States.Idle;
        }
        //Debug.Log("Move activated, current value: " + mInput);
    }

    public void Attack(InputAction.CallbackContext context) {
        if (animBuffer == false) StartCoroutine(AnimBuffer("attack", .73f, true));
        currentState = (int)States.Attacking;
        //transform.rotation = transform.LookAt( ) Event.current.mousePosition
        //@TODO(Jaden): have player aim at mouse when attacking
    }

    public void Lob(InputAction.CallbackContext context) {
        if (animBuffer == false) {
            if (headMesh.enabled == true) {
                currentState = (int)States.Attacking;
                StartCoroutine(AnimBuffer("lobThrow", .7f, true));
                // all functionality following is in LobThrow which'll be triggered in the animator
            } else { currentState = (int)States.Attacking; StartCoroutine(AnimBuffer("lob", .73f, true)); }
            }
    }

    public void Restart(InputAction.CallbackContext context) {
        Debug.Log("Restart called");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion

    public void LobThrow() { // triggered in animator
        headMesh.enabled = false;
        GameObject iHeadProj = Instantiate(headProj, projSpawn.position, transform.rotation);
        //iHeadProj.transform.Translate(new Vector3(mInput.x, 0, mInput.y) * projSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == (int)Layers.EnemyHurtbox) {
            Debug.Log("The enemy is hitting me");
        }
        else if (other.gameObject.layer == (int)Layers.EnemyHitbox) { 
            // NOTE(Roskuski): I hit the enemy!
            if (animr.GetBool("lob") == true) {
                //todo: enemy instantly dies
                Debug.Log("Lob landed");
                headMesh.enabled = true;
            }
        }
    }

    //@TODO(Jaden): Add OnTriggerEnter to make axe hitbox work, remember to do hitstun on enemy
    // so it doesn't melt their health

    #region Minor utility functions
    IEnumerator AnimBuffer(string animName, float duration, bool offWhenDone) {
        animr.SetBool(animName, true);
        animBuffer = true;
        yield return new WaitForSeconds(duration);
        animBuffer = false;
        if (offWhenDone) { animr.SetBool(animName, false); currentState = (int)States.Idle; }
    }
    #endregion
}
