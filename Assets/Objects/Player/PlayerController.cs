using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour {

	#region Combo tree
	readonly QueueInfo[][] QueueInfoTable = {
		// When in None
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(1.0f, 0.0f, Attacks.LAttack), // Light Attack
			               new QueueInfo(1.0f, 0.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(1.0f, 0.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(1.0f, 0.0f, Attacks.Dashing), // Dash
						   new QueueInfo(1.0f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in LAttack
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.5f, 0.0f, Attacks.LAttack2), // Light Attack
			               new QueueInfo(0.5f, 0.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.5f, 0.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Dash
						   new QueueInfo(0.5f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in LAttack2
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.0f, 0.0f, Attacks.LAttack3), // Light Attack
			               new QueueInfo(0.5f, 0.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.5f, 0.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Dash
						   new QueueInfo(0.5f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in LAttack3
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Dash
						   new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in Chop
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // Nonel
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Light Attack
			               new QueueInfo(0.5f, 0.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.5f, 0.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Dash
						   new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in Sweep
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Dash
						   new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in Spin
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Dash
						   new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in HeadThrow
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Heavy Attack
			               new QueueInfo(0.30f, 0.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Dash
						   new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in Dashing
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.5f, 0.0f, Attacks.LAttack), // Light Attack
			               new QueueInfo(0.5f, 0.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Dash
						   new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
	};

	struct QueueInfo {
		public float startPercent;
		public float endPercent;
		public Attacks attack;
		public QueueInfo(float startPercent, float endPercent, Attacks attack) {
			this.startPercent = startPercent;
			this.endPercent = endPercent;
			this.attack = attack;
		}
	}

	static bool animationTimesPopulated = false;
	static Dictionary<string, float> animationTimes;
	#endregion

	#region State machines
	//[Header("States:")]
	public enum States { Idle = 0, Walking, Attacking, Death };
	public States currentState = 0;
	public enum Attacks { None = 0, LAttack, LAttack2, LAttack3, Chop, Slam, Spin, HeadThrow, Dashing, LethalDash, ShotgunThrow };
	public Attacks currentAttack = 0;
	public Attacks queuedAttack = 0;
	public enum AttackButton { None = 0, LightAttack, HeavyAttack, Throw, Dash, ModLight, ModHeavy, ModThrow, ModDash };
	[Space]
	#endregion

	[Header("Object assignments:")]
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

	[Header("Movement:")]
	public Vector2 trueInput;							// movement vector read from left stick
	float trueAngle = 0f;								// movement angle float generated from trueInput
	public Vector2 mInput;								// processed movement vector read from input
	[SerializeField] Vector3 movement;					// actual movement vector used. mInput(x, y) = movement(x, z)
	bool freeAim = false;
	public Vector2 rAimInput;							// aiming vector read from right stick
	[Header("Speed:")]
	[SerializeField] AnimationCurve movementCurve;
	[SerializeField] float topSpeed = 10f;				// top player speed
	float speedTime = 0f;								// how long has player been moving for?
	[SerializeField] float maxSpeedTime = 0.4f;			// how long does it take for player to reach max speed?
	[SerializeField] float attackDecelModifier = 5f;	// modifier that makes player decelerate slower when attacking (moves them out further)
	[SerializeField] float turnSpeed = 0.05f;
	[Header("Dashing:")]
	[SerializeField] AnimationCurve dashCurve;
	[SerializeField] float dashForce = 10f;				// dash strength (how far do you go)
	[SerializeField] float dashTime = 0f;				// how long has player been dashing for?
	[SerializeField] float maxDashCooldown = 1.5f;		// how long does it take for player to dash again after dashing?
	[SerializeField] float dashCooldown = 1f;
	[Header("Health/Damage:")]
	public int healthMax = 20;
	public int health = 0;
	public float meter = 0;
	public float meterMax = 5;
	Quaternion kbAngle;
	float kbForce = 15f;                          // knockback speed
	float maxKbTime = 1f;                         // knockback time
	float kbTime = 0f;                            // knockback time
	[SerializeField] float targetSphereRadius = 2f;
	[Header("Animation timers:")]
	[SerializeField] float animTimer = 0f;
	[SerializeField] float animDuration = 0f;
	//public bool prepAttack = false;
	float turnVelocity = 0f;  // annoying float that is only referenced and has to exist for movement math to work

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
		health = healthMax;
	}

	private void FixedUpdate() { // calculate movement here
		// accel/decel for movement
		if (mInput != Vector2.zero && currentState == States.Attacking) { speedTime -= (Time.fixedDeltaTime / attackDecelModifier); }
		// if attacking, reduce movement at half speed to produce sliding effect
		else if (mInput != Vector2.zero) { speedTime += Time.fixedDeltaTime; } // else build up speed while moving
		else { speedTime -= Time.fixedDeltaTime; }
		// if no movement input and not attacking, decelerate
		speedTime = Mathf.Clamp(speedTime, 0, maxSpeedTime);
		// clamp accel value between 0 and a static maximum
		kbTime -= Time.fixedDeltaTime;
		if (kbTime <= 0) {
			kbAngle = Quaternion.identity;
			kbTime = 0f;
		}

		Vector3 moveDelta;
		if (currentAttack == Attacks.Dashing) {
			dashTime += Time.fixedDeltaTime;
			animr.SetBool("isDashing", true);
			Vector3 dashDirection = Quaternion.Euler(0f, trueAngle, 0f) * Vector3.forward;
			moveDelta = dashDirection.normalized * (dashForce * Mathf.Lerp(0, 1, dashCurve.Evaluate(dashTime / animationTimes[AttackToClipName[(int)currentAttack]])));
			if (dashTime >= animationTimes[AttackToClipName[(int)currentAttack]]) {
				currentState = States.Idle;
				trueAngle = 0;
				currentAttack = 0;
				animr.SetInteger("currentAttack", 0);
				dashTime = 0;
			}
		}
		else {
			float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
			float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnSpeed);
			transform.rotation = Quaternion.Euler(0f, angle, 0f);
			Vector3 moveDirection = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
			moveDelta = moveDirection.normalized * (topSpeed * Mathf.Lerp(0, 1, movementCurve.Evaluate(speedTime / maxSpeedTime)));
		} 
		float moveWeight = Mathf.Lerp(1, 0, Mathf.Clamp01(kbTime / maxKbTime));
		float kbWeight = moveWeight - 1f;
		Vector3 kbDelta = (kbAngle * Vector3.forward) * kbForce;
		transform.position += (moveDelta * moveWeight + kbDelta * kbWeight) * Time.fixedDeltaTime;

		if (freeAim) {
			if (rAimInput != Vector2.zero) {
				transform.rotation = Quaternion.LookRotation(new Vector3(rAimInput.x, 0, rAimInput.y));
			}
			else if (trueInput != Vector2.zero) {
				transform.rotation = Quaternion.LookRotation(new Vector3(trueInput.x, 0, trueInput.y));
			}
		}
	}

	private void Update() { // calculate time and input here
		trueInput = pActions.Player.Move.ReadValue<Vector2>();
		rAimInput = pActions.Player.Aim.ReadValue<Vector2>();
		if (dashCooldown > 0) { dashCooldown -= Time.deltaTime; }
		AttackButton preppingAttack = AttackButton.None;

		if (transform.position.y <= -50f) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		if (health > healthMax) { health = healthMax; }
		if (meter > meterMax) { meter = meterMax; }

		if (currentState != States.Attacking) {
			mInput = pActions.Player.Move.ReadValue<Vector2>();
			if (pActions.Player.Move.WasReleasedThisFrame()) {
				animr.SetBool("isWalking", false);
				currentState = States.Idle;
			}
			else if (pActions.Player.Move.phase == InputActionPhase.Started) {
				currentState = States.Walking;
				animr.SetBool("isWalking", true);
				movement = movement = new Vector3(mInput.x, 0, mInput.y);
			}
			else if (pActions.Player.Move.phase == InputActionPhase.Waiting) { animr.SetBool("isWalking", false); }
		}

		if (pActions.Player.LightAttack.WasPerformedThisFrame()) {
			preppingAttack = AttackButton.LightAttack;
		}

		if (pActions.Player.HeavyAttack.WasPerformedThisFrame()) {
			preppingAttack = AttackButton.HeavyAttack;
		}

		if (meter >= 1f && pActions.Player.Throw.WasPerformedThisFrame()) {
			preppingAttack = AttackButton.Throw;
		}

		if (pActions.Player.Dash.WasPerformedThisFrame() && trueInput.sqrMagnitude >= 0.1f && dashCooldown <= 0f) {
			preppingAttack = AttackButton.Dash;
			dashTime = 0;
			dashCooldown = maxDashCooldown;
			trueAngle = Mathf.Atan2(trueInput.x, trueInput.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
			transform.rotation = Quaternion.Euler(0f, trueAngle, 0f);
		}

		if (pActions.Player.Restart.WasPerformedThisFrame()) {
			Debug.Log("Restart called");
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		if (preppingAttack != AttackButton.None) {
			QueueInfo queueInfo = QueueInfoTable[(int)currentAttack][(int)preppingAttack];
			if (queueInfo.attack != Attacks.None) {
				if (animTimer <= animDuration * queueInfo.startPercent && animTimer >= animDuration * queueInfo.endPercent) {
					queuedAttack = queueInfo.attack;
				}
			}
		}

		// animator controller
		animTimer -= Time.deltaTime * animr.GetCurrentAnimatorStateInfo(0).speed;
		if (animTimer <= 0) {
			if (queuedAttack!= Attacks.None) {
				setCurrentAttack(queuedAttack);
				queuedAttack = Attacks.None;
			}
			else {
				currentAttack = Attacks.None;
				currentState = States.Idle;
				animTimer = 0;
				animDuration = 0f;
			}
		}

		animr.SetInteger("prepAttack", (int)queuedAttack);
		animr.SetInteger("currentAttack", (int)currentAttack);
	}

	//@TODO(Jaden): Add i-frames and trigger hitstun state when hit
	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.EnemyHitbox && currentAttack != Attacks.Dashing && kbTime <= 0) { // player is getting hit
			health--;
			if (health < 0) {
				health = 0;
				Death();
			}
			kbAngle = Quaternion.LookRotation(other.transform.position - this.transform.position);
			kbTime = maxKbTime;
			currentAttack = Attacks.None;
			currentState = States.Idle;
			animr.SetBool("isWalking", false);
			animr.SetInteger("currentAttack", (int)Attacks.None);
			animr.SetInteger("prepAttack", (int)AttackButton.None);
			animr.SetTrigger("wasHurt");
			Debug.Log("OWIE " + other.name + " JUST HIT ME! I have " + health + " health");
		}
		else if (other.gameObject.layer == (int)Layers.EnemyHurtbox) { // player is hitting enemy
			// NOTE(Roskuski): I hit the enemy!
		}
		else if (other.gameObject.layer == (int)Layers.Pickup) {
			HeadPopped headPopped = other.gameObject.GetComponent<HeadPopped>();
			HeadPickup headPickup = other.gameObject.GetComponent<HeadPickup>();
			if (headPopped) headPopped.collected = true;
			if (headPickup) headPickup.collected = true;

			GameObject.Destroy(other.transform.gameObject);
		}
		else if (other.gameObject.layer == (int)Layers.TrapHitbox && currentAttack != Attacks.Dashing && kbTime <= 0) {
			kbAngle = Quaternion.LookRotation(other.transform.position - this.transform.position);
			kbTime = maxKbTime;
		}
	}

	//@TODO(Jaden): Add OnTriggerEnter to make axe hitbox work, remember to do hitstun on enemy
	// so it doesn't melt their health

	//TODO(@Jaden): Make the player flash when getting hit.
	// Rough code sketch from Christian:

	/*public IEnumerator FlashColor(Color color, float flashTime, float flashes)
	{
		foreach (Renderer r in GetComponentsInChildren<Renderer>())
		{
			foreach (Material mat in r.materials)
			{
				mat.shader. Shader.Find("Universal Render Pipeline/Lit")
			}
		}
	}*/

	#region Combat functions
	public void LobThrow() { // triggered in animator
		ChangeMeter(-1);
		freeAim = false;
		GameObject iHeadProj = Instantiate(headProj, projSpawn.position, transform.rotation);
	}

	public void AttackBurst() { // called in animator to give burst of speed as attacks become active
		speedTime = maxSpeedTime * 1.5f; // makes player move forward after attacking in tandem with ryan's code
	}

	void ChangeMeter(float Amount) {
		meter += Amount;
		if (Amount >= 1) {
			headMesh.enabled = true;
			headMeshTrail.enabled = true;
		}
		else {
			headMesh.enabled = false;
			headMeshTrail.enabled = false;
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

	void setCurrentAttack(Attacks attack) {
		bool doSnap = true;
		currentState = States.Attacking;

		if (attack == Attacks.HeadThrow) {
			freeAim = true;
			doSnap = false;
		}

		animr.SetInteger("currentAttack", (int)attack);
		currentAttack = attack;
		animr.SetInteger("prepAttack", (int)AttackButton.None);
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

	void Death() {
		currentState = States.Death;
		currentAttack = Attacks.None;
		//animr.Play("Character_Death_Test");
		animr.SetBool("isWalking", false);
		animr.SetInteger("currentAttack", (int)Attacks.None);
		animr.SetInteger("prepAttack", (int)AttackButton.None);
		animr.SetBool("isDead", true);
		float deathTimer = animationTimes["Character_Death_Test"];
		deathTimer -= Time.deltaTime;
		Debug.Log("Player died, restarting scene shortly");
		if (deathTimer <= -1f) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
		// implement more proper death state eventually
	}
	#endregion

	#region Animation arrays
	static readonly string[] AttackToClipName = {
		"None",
		"Character_Attack1",
		"Character_Attack2",
		"LAttack3 Not Implmented",
		"Character_Chop",
		"Character_Sweep",
		"Spin Not Implmented",
		"Character_Chop_Throw",
		"Character_Roll"
	};
	#endregion

	#region Minor utility functions
	void OnEnable() { pActions.Enable(); }
	void OnDisable() { pActions.Disable(); }

	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(GetTargetSphereLocation(), targetSphereRadius);

		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, 17);
	}
	#endregion
}
