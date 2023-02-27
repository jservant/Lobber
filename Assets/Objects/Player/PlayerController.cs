using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour {
	//CapsuleCollider capCol;
	Rigidbody rb;
	Animator animr;
	MeshRenderer headMesh;
	TrailRenderer headMeshTrail;
	GameObject headProj;
	Transform playerPointer;
	Transform projSpawn;
	[SerializeField] Transform spherePoint;
	BoxCollider axeHitbox;
	GameManager gameManager;

	Vector3 enemyTarget;
	List<GameObject> enemiesHit;

	public DefaultPlayerActions pActions;

	public Vector2 trueInput;                     // movement vector read from input
	public Vector2 mInput;                        // processed movement vector read from input
	[SerializeField] Vector3 movement;            // actual movement vector used. mInput(x, y) = movement(x, z)
	[SerializeField] float speed = 10f;           // top player speed
	float timeMoved = 0f;                         // how long has player been moving for?
	[SerializeField] float maxSpeedTime = 2f;     // how long does it take for player to reach max speed?
	[SerializeField] int ammo = 0;
	[SerializeField] float turnSpeed = 0.1f;
	[SerializeField] AnimationCurve curve;
	[SerializeField] float animTimer = 0f;
	[SerializeField] float animDuration = 0f;
	[SerializeField] float targetSphereRadius = 2f;
	bool freeAim = false;

	public enum States { Idle, Walking, Attacking, Hitstunned };
	public States currentState = 0;
	public enum Attacks { None, LAttack, LAttack2, LAttack3, Chop, Sweep, Spin, HeadThrow };
	// some of these names are temp names that won't be used
	public Attacks currentAttack = 0;
	public enum AttackButton { None, LightAttack, HeavyAttack };
	public AttackButton preppingAttack = 0;

	//public bool prepAttack = false;
	float turnVelocity;

	private void Awake() {
		//capCol = GetComponent<CapsuleCollider>();
		rb = GetComponent<Rigidbody>();
		animr = GetComponent<Animator>();
		pActions = new DefaultPlayerActions();

		headMesh = transform.Find("Weapon_Controller/Hitbox/StoredHead").GetComponent<MeshRenderer>();
		headMeshTrail = transform.Find("Weapon_Controller/Hitbox/StoredHead").GetComponent<TrailRenderer>();
		axeHitbox = transform.Find("Weapon_Controller/Hitbox").GetComponent<BoxCollider>();
		projSpawn = transform.Find("ProjSpawn");
		playerPointer = transform.Find("PlayerPointer");
		spherePoint = transform.Find("PlayerPointer/SpherePoint");
		gameManager = transform.Find("/GameManager").GetComponent<GameManager>();
		headProj = gameManager.SkullPrefab;


		#region debug
		if (headMesh != null) { Debug.Log("Axe headmesh found on player."); } else { Debug.LogWarning("Axe headmesh not found on player."); }
		if (headProj != null) { Debug.Log("Head projectile found in Resources."); } else { Debug.LogWarning("Head projectile not found in Resources."); }
		#endregion
	}

	private void FixedUpdate() { // calculate movement here
		// accel/decel for movement
		if (mInput != Vector2.zero && currentState == States.Attacking) { timeMoved -= (Time.fixedDeltaTime / 2); }
		// if attacking, reduce movement at half speed to produce sliding effect
		else if (mInput != Vector2.zero) { timeMoved += Time.fixedDeltaTime; } // else build up speed while moving
		else { timeMoved -= Time.fixedDeltaTime; }
		// if no movement input and not attacking, decelerate
		timeMoved = Mathf.Clamp(timeMoved, 0, maxSpeedTime);
		// clamp accel value between 0 and a static maximum

		// ryan's adapted movement code, meant to lerp player movement/rotation
		if (movement.magnitude >= 0.1f && currentState != States.Hitstunned) {
			float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
			float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnSpeed);
			if (freeAim) { transform.LookAt(new Vector3(spherePoint.position.x, transform.position.y, spherePoint.position.z)); }
			else { transform.rotation = Quaternion.Euler(0f, angle, 0f); }

			Vector3 moveDirection = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

			transform.position += moveDirection.normalized * (speed * Mathf.Lerp(0, 1, curve.Evaluate(timeMoved / maxSpeedTime))) * Time.fixedDeltaTime;
		}
		else {
			transform.rotation = Quaternion.Euler(0f, transform.rotation.y, 0f);
		}

		//@TODO(Jaden): maaybe snapto enemy? 
	}

	private void Update() { // calculate time and input here
		trueInput = pActions.Player.Move.ReadValue<Vector2>();

		if (currentState != States.Hitstunned) { Input(); }
		if (currentAttack != Attacks.None || currentState == States.Hitstunned) {
			// animator controller
			animTimer -= Time.deltaTime;
			if (animTimer <= 0 && preppingAttack == AttackButton.None) // reset everything after animation is done
			{
				currentAttack = Attacks.None;
				//animr.SetInteger("CurrentAttack", currentAttack);
				animr.Play("Base Layer.Character_Idle");
				currentState = States.Idle;
				animTimer = 0; animDuration = 0f;
			}
			else if (animTimer <= 0 && preppingAttack != AttackButton.None) {
				// NOTE(@Jaden): Combo Tree
				switch(currentAttack) { // Check what attack is happening
					case Attacks.LAttack: // Attack 1 -> Attack 2
						if (preppingAttack == AttackButton.LightAttack) { // LAttack1 -> LAttack2
							followupAttack(Attacks.LAttack2, "Base Layer.Character_Attack2", 0.567f/2);
						} else if (preppingAttack == AttackButton.HeavyAttack) { // LAttack -> Chop (end)
							followupAttack(Attacks.Chop, "Base Layer.Character_Chop", 1.333f/2);
						} break;
					/*case Attacks.LAttack2: // Attack 2 -> Attack 3
						if (preppingAttack == AttackButton.LightAttack) { // LAttack2 -> LAttack3
							// followupAttack for attack 3, if that ends up happening
							break;
						} else if (preppingAttack == AttackButton.HeavyAttack) { // LAttack2 -> Sweep
							// followupAttack for sweep, when that's implemented
							break;
						}
						break;
					case Attacks.LAttack3: // Attack 3 -> Finishers (not possible yet)
						if (preppingAttack == AttackButton.LightAttack) { // LAttack3 -> ?
							// mayyyyybe loop back to attack1? probably not a good idea tho, might just cut this
							break;
						} else if (preppingAttack == AttackButton.HeavyAttack) { // LAttack3 -> Big finisher
							// followupAttack for spin or some other big finisher, if that's implemented
							break;
						}
						break;*/
					default:
						preppingAttack = AttackButton.None;
						break;
				}
			}
		}

		//reads input even when player is attacking or hitstunned
		float pointerAngle = Mathf.Atan2(trueInput.x, trueInput.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
		playerPointer.rotation = Quaternion.Euler(0f, pointerAngle, 0f);

		//Camera.main.transform.LookAt(transform);
		//Camera.main.transform.Translate(new Vector3(camControl.x, camControl.y, 0) * Time.deltaTime);
	}

	void followupAttack(Attacks attack, string animName, float duration) {
		animr.Play(animName);
		currentAttack = attack;
		if (pActions.Player.Move.ReadValue<Vector2>().sqrMagnitude >= 0.02) {
			movement = movement = new Vector3(trueInput.x, 0, trueInput.y); // this and last line allow for movement between hits
		}
		SnapToTarget();
		animTimer = duration; animDuration = duration;
		// TODO(@Jaden): Change duration to read the animation clip's length when it's finalized
		preppingAttack = AttackButton.None;
	}


	void Input() { // processes input, runs in update
		if (currentState != States.Attacking) {
			mInput = pActions.Player.Move.ReadValue<Vector2>();
			if (pActions.Player.Move.WasReleasedThisFrame()) {
				//animr.SetBool("walking", false);
				animr.Play("Base Layer.Character_Idle");
				currentState = States.Idle;
			}
			else if (pActions.Player.Move.phase == InputActionPhase.Started) {
				//animr.SetBool("walking", true);
				currentState = States.Walking;
				animr.Play("Base Layer.Character_Run");
				movement = movement = new Vector3(mInput.x, 0, mInput.y);
			}
			else if (pActions.Player.Move.phase == InputActionPhase.Waiting) { animr.Play("Base Layer.Character_Idle"); }
		}

		if (pActions.Player.LightAttack.WasPerformedThisFrame()) {
			if (currentAttack != Attacks.None && animTimer <= animDuration / 2) {
				preppingAttack = AttackButton.LightAttack;
			}
			else if (currentAttack == Attacks.None) {
				setCurrentAttack(Attacks.LAttack, "Base Layer.Character_Attack1", 0.633f/2); // added +0.1 in leeway
				SnapToTarget();
			}
			currentState = States.Attacking;
			//@TODO(Jaden): Move forward slightly when attacking
		}

		if (pActions.Player.HeavyAttack.WasPerformedThisFrame()) {
			if (currentAttack != Attacks.None && animTimer <= animDuration / 2) {
				preppingAttack = AttackButton.HeavyAttack;
			} else if (currentAttack == Attacks.None) {
				if (headMesh.enabled == true) {
					currentState = States.Attacking;
					freeAim = true;
					setCurrentAttack(Attacks.HeadThrow, "Base Layer.Character_Chop_Throw", 1.067f);
					// all functionality following is in LobThrow which'll be triggered in the animator
				}
				else {
					currentState = States.Attacking;
					setCurrentAttack(Attacks.Chop, "Base Layer.Character_Chop", 1.333f/2);
					SnapToTarget();
				}
			}
		}

		if (pActions.Player.Restart.WasPerformedThisFrame()) {
			Debug.Log("Restart called");
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
	}

	public void LobThrow() { // triggered in animator
		ammo--;
		gameManager.ammoUI.text = "AMMO: " + ammo;
		if (ammo <= 0) { headMesh.enabled = false; }
		headMeshTrail.enabled = false;
		freeAim = false;
		GameObject iHeadProj = Instantiate(headProj, projSpawn.position, transform.rotation);
		//iHeadProj.transform.Translate(new Vector3(mInput.x, 0, mInput.y) * projSpeed * Time.deltaTime);
	}

	void SnapToTarget() {
		enemyTarget = Vector3.zero; // free the target vector
		Collider[] eColliders = Physics.OverlapSphere(spherePoint.position, targetSphereRadius, Mask.Get(Layers.EnemyHurtbox));
		// find all colliders in the sphere
		//Debug.Log("eColliders length: " + eColliders.Length);
		float difference = 10f;
		for (int index = 0; index < eColliders.Length; index += 1) { // for each v3
			//print("Collider #" + index + " name: " + eColliders[index].gameObject.name);

			float newDifference = Vector3.Distance(transform.position, eColliders[index].transform.position);
			if (newDifference < difference) {
				difference = newDifference;
				enemyTarget = eColliders[index].transform.position;
			}
		}
		if (enemyTarget != Vector3.zero) {
			print("Player's position: " + transform.position + " Target enemy's position: " + enemyTarget);
										   //TODO(@Jaden): doesn't work right when camera is rotated?
			this.movement = (enemyTarget - transform.position).normalized;
			transform.LookAt(enemyTarget); // point player at closest enemy
		}
		/*else {
			print("No enemies found. Player movement vector: " + movement);
			enemyTarget = movement;
		}*/
		timeMoved = maxSpeedTime; // makes player move forward after attacking in tandem with ryan's code
	}

	//@TODO(Jaden): Add i-frames and trigger hitstun state when hit
	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.EnemyHitbox) { // player is getting hit
			Debug.Log(other.name + " just hit me, the player!");
			animTimer = .55f; animDuration = .55f;
			currentState = States.Hitstunned;
			animr.Play("Character_GetHit");
		}
		else if (other.gameObject.layer == (int)Layers.EnemyHurtbox) { // player is hitting enemy
																	   // NOTE(Roskuski): I hit the enemy!
			if (currentAttack == Attacks.Chop) {
				//todo: enemy instantly dies
				ammo++;
				gameManager.ammoUI.text = "AMMO: " + ammo;
				headMesh.enabled = true;
				headMeshTrail.enabled = true;
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

	void setCurrentAttack(Attacks attack, string animName, float duration) {
		currentAttack = attack;
		animr.Play(animName);
		//animr.SetInteger("CurrentAttack", currentAttack);
		animTimer = duration; animDuration = duration; // TODO(@Jaden): Change duration to read the animation clip's length when it's finalized
	}


	void OnEnable() { pActions.Enable(); }
	void OnDisable() { pActions.Disable(); }

	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(spherePoint.position, targetSphereRadius);
	}
	#endregion
}
