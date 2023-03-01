using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour {


	readonly Attacks[][] AttackCancel = {
		// AttackButton
		//              None,              LightAttack,       HeavyAttack,       Throw                || Current Attack
		new Attacks[]{  Attacks.None     , Attacks.LAttack  , Attacks.Chop     , Attacks.HeadThrow,}, // None
		new Attacks[]{  Attacks.None     , Attacks.LAttack2 , Attacks.Chop     , Attacks.HeadThrow,}, // LAttack
		new Attacks[]{  Attacks.None     , Attacks.LAttack  , Attacks.Chop     , Attacks.HeadThrow,}, // LAttack2
		new Attacks[]{  Attacks.None     , Attacks.None     , Attacks.None     , Attacks.None     ,}, // LAttack3
		new Attacks[]{  Attacks.None     , Attacks.None     , Attacks.None     , Attacks.HeadThrow,}, // Chop
		new Attacks[]{  Attacks.None     , Attacks.None     , Attacks.None     , Attacks.None     ,}, // Sweep
		new Attacks[]{  Attacks.None     , Attacks.None     , Attacks.None     , Attacks.None     ,}, // Spin
		new Attacks[]{  Attacks.None     , Attacks.None     , Attacks.None     , Attacks.HeadThrow,}, // HeadThrow
	};

	// CancelWindows[X][0] is startPercent. CancelWindows[X][1] is endPercent
	struct CancelWindow {
		public float startPercent;
		public float endPercent;
		public CancelWindow(float a, float b) {
			startPercent = a;
			endPercent = b;
		}
	}

	readonly CancelWindow[] CancelWindows = {
		new CancelWindow(1.0f, 0.0f), // None
		new CancelWindow(0.5f, 0.0f), // LAttack
		new CancelWindow(0.5f, 0.0f), // LAttack2
		new CancelWindow(0.0f, 0.0f), // LAttack3
		new CancelWindow(0.5f, 0.0f), // Chop
		new CancelWindow(0.0f, 0.0f), // Sweep
		new CancelWindow(0.0f, 0.0f), // Spin
		new CancelWindow(0.3f, 0.0f), // HeadThrow
	};


	static bool animationTimesPopulated = false;
	static Dictionary<string, float> animationTimes;

	//CapsuleCollider capCol;
	Rigidbody rb;
	Animator animr;
	MeshRenderer headMesh;
	TrailRenderer headMeshTrail;
	GameObject headProj;
	Transform projSpawn;
	BoxCollider axeHitbox;
	GameManager gameManager;

	Vector3 enemyTarget;
	List<GameObject> enemiesHit;

	public DefaultPlayerActions pActions;

	public Vector2 trueInput;                     // movement vector read from left stick
	public Vector2 rAimInput;                     // aiming vector read from right stick
	float trueAngle = 0f;                         // movement angle float generated from trueInput
	public Vector2 mInput;                        // processed movement vector read from input
	[SerializeField] Vector3 movement;            // actual movement vector used. mInput(x, y) = movement(x, z)
	[SerializeField] float speed = 10f;           // top player speed
	float speedTime = 0f;                         // how long has player been moving for?
	[SerializeField] float maxSpeedTime = 0.4f;   // how long does it take for player to reach max speed?
	[SerializeField] float dashForce = 10f;		  // dash strength (how far do you go)
	[SerializeField] float dashTime = 0f;         // how long has player been moving for?
	[SerializeField] float maxDashTime = 1f;      // how long does it take for player to reach max speed?
	[SerializeField] float maxDashCooldown = 1f;      // how long does it take for player to reach max speed?
	[SerializeField] float dashCooldown = 1f;
	int ammo = 0;
	float turnSpeed = 0.1f;
	[SerializeField] AnimationCurve movementCurve;
	[SerializeField] AnimationCurve dashCurve;
	[SerializeField] float animTimer = 0f;
	[SerializeField] float animDuration = 0f;
	[SerializeField] float targetSphereRadius = 2f;
	bool freeAim = false;

	public enum States { Idle = 0, Walking, Attacking, Hitstunned, Dashing };
	public States currentState = 0;
	public enum Attacks { None = 0, LAttack, LAttack2, LAttack3, Chop, Sweep, Spin, HeadThrow };
	// some of these names are temp names that won't be used
	public Attacks currentAttack = 0;
	public enum AttackButton { None = 0, LightAttack, HeavyAttack, Throw };

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
		gameManager = transform.Find("/GameManager").GetComponent<GameManager>();
		headProj = gameManager.SkullPrefab;

		#region debug
		if (headMesh != null) { Debug.Log("Axe headmesh found on player."); } else { Debug.LogWarning("Axe headmesh not found on player."); }
		if (headProj != null) { Debug.Log("Head projectile found in Resources."); } else { Debug.LogWarning("Head projectile not found in Resources."); }
		#endregion

		if (!animationTimesPopulated) {
			animationTimesPopulated = true;
			animationTimes = new Dictionary<string, float>(animr.runtimeAnimatorController.animationClips.Length);
			foreach (AnimationClip clip in animr.runtimeAnimatorController.animationClips) {
				animationTimes[clip.name] = clip.length;
			}
		}

		dashCooldown = maxDashCooldown;
	}

	private void FixedUpdate() { // calculate movement here
								 // accel/decel for movement
		if (mInput != Vector2.zero && currentState == States.Attacking) { speedTime -= (Time.fixedDeltaTime / 2); }
		// if attacking, reduce movement at half speed to produce sliding effect
		else if (mInput != Vector2.zero) { speedTime += Time.fixedDeltaTime; } // else build up speed while moving
		else { speedTime -= Time.fixedDeltaTime; }
		// if no movement input and not attacking, decelerate
		speedTime = Mathf.Clamp(speedTime, 0, maxSpeedTime);
		// clamp accel value between 0 and a static maximum

		if (currentState == States.Dashing) {
			// probably add an animation here at some point
			//rb.AddForce(transform.forward * dashForce, ForceMode.Force); //trueInput.x, transform.position.y, trueInput.y
			dashTime += Time.fixedDeltaTime;
			Vector3 moveDirection = Quaternion.Euler(0f, trueAngle, 0f) * Vector3.forward;
			transform.position += moveDirection.normalized * (dashForce * Mathf.Lerp(0, 1, dashCurve.Evaluate(dashTime / maxDashTime))) * Time.fixedDeltaTime;
			if (dashTime >= maxDashTime) {
				dashTime = 0;
				currentState = States.Idle;
				//rb.velocity = Vector3.zero;
				dashCooldown = maxDashCooldown;
				trueAngle = 0;
			}
		}
		else if (movement.magnitude >= 0.1f && currentState != States.Hitstunned) {
			// ryan's adapted movement code, meant to lerp player movement/rotation
			float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
			float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnSpeed);
			transform.rotation = Quaternion.Euler(0f, angle, 0f);

			Vector3 moveDirection = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

			transform.position += moveDirection.normalized * (speed * Mathf.Lerp(0, 1, movementCurve.Evaluate(speedTime / maxSpeedTime))) * Time.fixedDeltaTime;
		}
		else {
			transform.rotation = Quaternion.Euler(0f, transform.rotation.y, 0f);
		}
	}

	private void Update() { // calculate time and input here
		trueInput = pActions.Player.Move.ReadValue<Vector2>();
		rAimInput = pActions.Player.Aim.ReadValue<Vector2>();
		if (dashCooldown > 0) { dashCooldown -= Time.deltaTime; }

		AttackButton preppingAttack = AttackButton.None;
		if (currentState != States.Hitstunned) {
			if (currentState != States.Attacking && currentState != States.Dashing) {
				mInput = pActions.Player.Move.ReadValue<Vector2>();
				if (pActions.Player.Move.WasReleasedThisFrame()) {
					animr.Play("Base Layer.Character_Idle");
					currentState = States.Idle;
				}
				else if (pActions.Player.Move.phase == InputActionPhase.Started) {
					currentState = States.Walking;
					animr.Play("Base Layer.Character_Run");
					movement = movement = new Vector3(mInput.x, 0, mInput.y);
				}
				else if (pActions.Player.Move.phase == InputActionPhase.Waiting) { animr.Play("Base Layer.Character_Idle"); }
			}

			if (pActions.Player.LightAttack.WasPerformedThisFrame()) {
				preppingAttack = AttackButton.LightAttack;
			}

			if (pActions.Player.HeavyAttack.WasPerformedThisFrame()) {
				preppingAttack = AttackButton.HeavyAttack;
			}

			if (ammo > 0 && pActions.Player.Throw.WasPerformedThisFrame()) {
				preppingAttack = AttackButton.Throw;
			}


			if (pActions.Player.Dash.WasPerformedThisFrame() && trueInput.sqrMagnitude >= 0.1f) {
				Debug.Log("Dash activated, trueInput value: " + trueInput);
				currentState = States.Dashing;
				trueAngle = Mathf.Atan2(trueInput.x, trueInput.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
				transform.rotation = Quaternion.Euler(0f, trueAngle, 0f);
			}

			if (pActions.Player.Restart.WasPerformedThisFrame()) {
				Debug.Log("Restart called");
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			}
		}

		if (preppingAttack != AttackButton.None) {
			Attacks nextAttack = AttackCancel[(int)currentAttack][(int)preppingAttack];
			CancelWindow cancelWindow = CancelWindows[(int)currentAttack];
			if (nextAttack != Attacks.None) {
				if (animTimer <= animDuration * cancelWindow.startPercent && animTimer >= animDuration * cancelWindow.endPercent) {
					setCurrentAttack(nextAttack);
				}
			}
		}

		if (currentAttack != Attacks.None || currentState == States.Hitstunned) {
			// animator controller
			animTimer -= Time.deltaTime * animr.GetCurrentAnimatorStateInfo(0).speed;
			if (animTimer <= 0 && preppingAttack == AttackButton.None) { // reset everything after animation is done
				currentAttack = Attacks.None;
				//animr.SetInteger("CurrentAttack", currentAttack);
				animr.Play("Base Layer.Character_Idle");
				currentState = States.Idle;
				animTimer = 0; animDuration = 0f;
			}
		}
	}

	public void LobThrow() { // triggered in animator
		ChangeAmmo(-1);
		freeAim = false;
		GameObject iHeadProj = Instantiate(headProj, projSpawn.position, transform.rotation);
	}

	Vector3 GetTargetSphereLocation() {
		if (trueInput == Vector2.zero && rAimInput == Vector2.zero) {
			return transform.position + transform.rotation * new Vector3(0, 1.2f, 2.5f);
		}
		else if (rAimInput.sqrMagnitude >= 0.1f) {
			return transform.position + Quaternion.LookRotation(new Vector3(rAimInput.x, 0, rAimInput.y), Vector3.up) * new Vector3(0, 1.2f, 2.5f);
		}
		else {
			return transform.position + Quaternion.LookRotation(new Vector3(trueInput.x, 0, trueInput.y), Vector3.up) * new Vector3(0, 1.2f, 2.5f);
		}
	}

	void SnapToTarget() { // attack homing function
		enemyTarget = Vector3.zero; // free the target vector
		Collider[] eColliders = Physics.OverlapSphere(GetTargetSphereLocation(), targetSphereRadius, Mask.Get(Layers.EnemyHurtbox));
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
		speedTime = maxSpeedTime; // makes player move forward after attacking in tandem with ryan's code
	}

	//@TODO(Jaden): Add i-frames and trigger hitstun state when hit
	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.EnemyHitbox && currentState != States.Dashing) { // player is getting hit
			Debug.Log(other.name + " just hit me, the player!");
			animr.Play("Character_GetHit");
			animTimer = animr.GetCurrentAnimatorStateInfo(0).length; animDuration = animTimer;
			currentState = States.Hitstunned;
		}
		else if (other.gameObject.layer == (int)Layers.EnemyHurtbox) { // player is hitting enemy
			// NOTE(Roskuski): I hit the enemy!
		}
		else if (other.gameObject.layer == (int)Layers.Pickup) {
			ChangeAmmo(1);
			GameObject.Destroy(other.gameObject);
		}
	}

	void ChangeAmmo(int Amount) {
		ammo += Amount;
		gameManager.ammoUI.text = "AMMO: " + ammo;
		if (Amount >= 1) {
			headMesh.enabled = true;
			headMeshTrail.enabled = true;
		}
		else {
			headMesh.enabled = false;
			headMeshTrail.enabled = false;
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
	static readonly string[] AttackToClipName = {
		"None",
		"Character_Attack1",
		"Character_Attack2",
		"LAttack3 Not Implmented",
		"Character_Chop",
		"Sweep Not Implmented",
		"Spin Not Implmented",
		"Character_Chop_Throw"
	};
	static readonly string[] AttackToStateName = {
		"None",
		"Base Layer.Character_Attack1",
		"Base Layer.Character_Attack2",
		"LAttack3 Not Implmented",
		"Base Layer.Character_Chop",
		"Sweep Not Implmented",
		"Spin Not Implmented",
		"Base Layer.Character_Chop_Throw",
	};

	void setCurrentAttack(Attacks attack, bool doSnap = true) {
		currentState = States.Attacking;
		if (attack == Attacks.HeadThrow) {
			freeAim = true;
		}

		animr.Play(AttackToStateName[(int)attack], -1, 0);
		currentAttack = attack;
		animTimer = animationTimes[AttackToClipName[(int)attack]]; animDuration = animTimer;

		if (pActions.Player.Aim.ReadValue<Vector2>().sqrMagnitude >= 0.02) {
			movement = new Vector3(rAimInput.x, 0, rAimInput.y); // this and last line allow for movement between hits
		} else if (pActions.Player.Move.ReadValue<Vector2>().sqrMagnitude >= 0.02) {
			movement = new Vector3(trueInput.x, 0, trueInput.y);
		}

		if (doSnap) {
			SnapToTarget();
		} else { speedTime = 0; } // stops player movement when throwing. change later if other attacks don't snap
	}

	void OnEnable() { pActions.Enable(); }
	void OnDisable() { pActions.Disable(); }

	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(GetTargetSphereLocation(), targetSphereRadius);
	}
	#endregion
}
