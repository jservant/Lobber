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
			               new QueueInfo(0.0f, 1.0f, Attacks.LAttack), // Light Attack
			               new QueueInfo(0.0f, 1.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.0f, 1.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 1.0f, Attacks.Dashing), // Dash
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in LAttack
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.3f, 1.0f, Attacks.LAttack2), // Light Attack
			               new QueueInfo(0.3f, 1.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.3f, 1.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 1.0f, Attacks.Dashing), // Dash
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in LAttack2
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.3f, 1.0f, Attacks.LAttack3), // Light Attack
			               new QueueInfo(0.3f, 1.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.3f, 1.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 1.0f, Attacks.Dashing), // Dash
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in LAttack3
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Throw
			               new QueueInfo(0.0f, 1.0f, Attacks.Dashing), // Dash
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in Chop
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // Nonel
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Light Attack
			               new QueueInfo(0.7f, 1.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.0f, 1.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 1.0f, Attacks.Dashing), // Dash
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in Slam
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
			               new QueueInfo(0.0f, 1.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.7f, 1.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 1.0f, Attacks.Dashing), // Dash
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in Dashing
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.0f, 1.0f, Attacks.LAttack), // Light Attack
			               new QueueInfo(0.0f, 1.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.0f, 1.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Dash
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in LethalDash
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Dash
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in ShotgunThrow
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Heavy Attack
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
	CapsuleCollider capCol;
	Rigidbody rb;
	Animator animr;
	MeshRenderer headMesh;
	TrailRenderer headMeshTrail;
	GameObject headProj;
	Transform projSpawn;
	Light spotLight;
	GameManager gameMan;
	//List<GameObject> enemiesHit;
	public DefaultPlayerActions pActions;

	[Header("Movement:")]
	public Vector2 trueInput;							// movement vector read from left stick
	float trueAngle = 0f;								// movement angle float generated from trueInput
	public Vector2 mInput;								// processed movement vector read from input
	[SerializeField] Vector3 movement;					// actual movement vector used. mInput(x, y) = movement(x, z)
	public bool freeAim = false;
	public Vector2 rAimInput;							// aiming vector read from right stick
	[Header("Speed:")]
	[SerializeField] AnimationCurve movementCurve;
	[SerializeField] float topSpeed = 10f;				// top player speed
	[SerializeField] float speedTime = 0f;				// how long has player been moving for?
	[SerializeField] float maxSpeedTime = 0.2f;			// how long does it take for player to reach max speed?
	[SerializeField] float attackDecelModifier = 5f;	// modifier that makes player decelerate slower when attacking (moves them out further)
	[SerializeField] float turnSpeed = 0.05f;
	[Header("Dashing:")]
	[SerializeField] AnimationCurve dashCurve;
	[SerializeField] float dashForce = 10f;				// dash strength (how far do you go)
	[SerializeField] float dashTime = 0f;				// how long has player been dashing for?
	[SerializeField] float maxDashCooldown = 1.5f;		// how long does it take for player to dash again after dashing?
	[SerializeField] float dashCooldown = 1f;
	[Header("Health/Damage:")]
	public bool vulnerable = true;
	public int healthMax = 20;
	public int health = 0;
	public float meter = 0;
	public float meterMax = 5;
	Quaternion kbAngle;
	float kbForce = 15f;                            // knockback speed
	float maxKbTime = 1f;                           // knockback time
	float kbTime = 0f;                              // knockback time
	[SerializeField] float targetSphereRadius = 2f; // publically editable
	float tsr = 0f;									// used internally

	Vector3 homingInitalPosition;
	Vector3 homingTargetDelta;
	Vector3 homingPrevValue;
	float homingTimer;
	float homingTimerMax;
	bool doHoming = false;

	// NOTE(Roskuski): C# doesn't support globals that are scoped to functions
	float AnimatorNormalizedTimeOfNextOrCurrentAttackState_LastValue;
	int AnimatorNormalizedTimeOfNextOrCurrentAttackState_LastSource;
	bool wasNextValid = false; 
	float turnVelocity = 0f;  // annoying float that is only referenced and has to exist for movement math to work

	private void Awake() {
		capCol = GetComponent<CapsuleCollider>();
		rb = GetComponent<Rigidbody>();
		animr = GetComponent<Animator>();
		pActions = new DefaultPlayerActions();

		headMesh = transform.Find("Weapon_Controller/Hitbox/StoredHead").GetComponent<MeshRenderer>();
		headMeshTrail = transform.Find("Weapon_Controller/Hitbox/StoredHead").GetComponent<TrailRenderer>();
		projSpawn = transform.Find("ProjSpawn");
		spotLight = transform.Find("Spot Light").GetComponent<Light>();
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		headProj = gameMan.SkullPrefab;

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
		tsr = targetSphereRadius;
	}

	void PreformedCheckedMovement(Vector3 translationDelta) {
		RaycastHit hitInfo;
		float checkDistance = translationDelta.magnitude > 0.1f ? translationDelta.magnitude : 0.1f;
		if (Physics.SphereCast(this.transform.position + Vector3.up * 2.6f/2.0f, 0.5f, translationDelta, out hitInfo, checkDistance)) {

			// Move up to the wall, with a safe distance
			Vector3 hitDelta = hitInfo.point - this.transform.position;
			Vector3 hitMove = (new Vector3 (hitDelta.x, 0, hitDelta.y) * translationDelta.magnitude) - translationDelta.normalized * 0.5f;
			bool tookParticalMove = false;
			if (Vector3.Dot(hitDelta, translationDelta) >= 0.9f) {
				tookParticalMove = true;
				this.transform.position += hitMove;
			}

			// Account for the distance we have already moved
			Vector3 remainingMove = translationDelta;
			if (tookParticalMove) {
				remainingMove = remainingMove - hitMove;
			}

			// figure out if we want to slide left or right
			float leftScore = Vector3.Dot(Quaternion.AngleAxis(-90, Vector3.up) * hitInfo.normal, remainingMove);
			float rightScore = Vector3.Dot(Quaternion.AngleAxis(90, Vector3.up) * hitInfo.normal, remainingMove);

			float angleToSlide = 90;
			if (leftScore > rightScore) {
				angleToSlide = -90;
			}

			// clip our movement in the direction of the opposite normal of the wall
			float angleToRight = Vector3.SignedAngle(hitInfo.normal, Vector3.right, Vector3.up);
			remainingMove = Quaternion.AngleAxis(angleToRight, Vector3.up) * remainingMove;
			remainingMove.x = 0;
			remainingMove = Quaternion.AngleAxis(-angleToRight, Vector3.up) * remainingMove;

			// calculate the new movement after clipping
			Vector3 remainingDelta = Quaternion.AngleAxis(angleToSlide, Vector3.up) * hitInfo.normal * remainingMove.magnitude;
			
			// attempt to do that move successfully
			PreformedCheckedMovement(remainingDelta); // @TODO(Roskuski): There is a realistic chance that this enters into a infinite recursion. thankfully unity should continue to chug along even in the case of this.
		}
		else {
			this.transform.position += translationDelta;
		}
	}

	private void FixedUpdate() { // calculate movement here
		// accel/decel for movement
		if (mInput != Vector2.zero && currentState == States.Attacking) { speedTime -= (Time.fixedDeltaTime / attackDecelModifier); }
		// if attacking, reduce movement at half speed to produce sliding effect
		else if (mInput != Vector2.zero) { speedTime += Time.fixedDeltaTime; } // else build up speed while moving
		else { speedTime -= Time.fixedDeltaTime; }
		// if no movement input and not attacking, decelerate
		if (currentState != States.Attacking) { speedTime = Mathf.Clamp(speedTime, 0, maxSpeedTime); }
		// clamp accel value between 0 and a static maximum
		kbTime -= Time.fixedDeltaTime;
		if (kbTime <= 0) {
			kbAngle = Quaternion.identity;
			kbTime = 0f;
		}

		Vector3 translationDelta = Vector3.zero;
		if (doHoming) {
			Vector3 nextHomingPos = Vector3.Lerp(homingInitalPosition + homingTargetDelta, homingInitalPosition, Mathf.Clamp01(Mathf.Pow((homingTimer/homingTimerMax), 2)));

			if (homingPrevValue == Vector3.zero) {
				translationDelta = nextHomingPos - transform.position;
			}
			else {
				translationDelta = nextHomingPos - homingPrevValue;
			}
			homingPrevValue = nextHomingPos;

			homingTimer -= Time.fixedDeltaTime;
			if (homingTimer < 0) {
				doHoming = false;
			}
		}
		else {
			Vector3 moveDelta;
			if (currentAttack == Attacks.Dashing) {
				dashTime += Time.fixedDeltaTime;
				animr.SetBool("isDashing", true);
				this.transform.rotation = Quaternion.Euler(0f, trueAngle, 0f);
				Vector3 dashDirection = Quaternion.Euler(0f, trueAngle, 0f) * Vector3.forward;
				moveDelta = dashDirection.normalized * (dashForce * Mathf.Lerp(0, 1, dashCurve.Evaluate(dashTime / animationTimes["Character_Roll"])));
				if (dashTime >= animationTimes["Character_Roll"]) {
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
				Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
				moveDelta = moveDirection.normalized * (topSpeed * Mathf.Lerp(0, 1, movementCurve.Evaluate(speedTime / maxSpeedTime)));
			} 
			float moveWeight = Mathf.Lerp(1, 0, Mathf.Clamp01(kbTime / maxKbTime));
			float kbWeight = moveWeight - 1f;
			Vector3 kbDelta = (kbAngle * Vector3.forward) * kbForce;

			translationDelta = (moveDelta * moveWeight + kbDelta * kbWeight) * Time.fixedDeltaTime;
		}

		PreformedCheckedMovement(translationDelta);
		
		if (currentAttack != Attacks.Dashing) {
			RaycastHit hitInfo;
			if (Physics.SphereCast(transform.position + Vector3.up * 2.6f/2.0f, 0.5f, Vector3.down, out hitInfo, 2.6f, Mask.Get(Layers.Ground))) {
				float distanceToGround = hitInfo.distance - 2.6f/2.0f;
				transform.position -= new Vector3(0, distanceToGround, 0);
			}
			else {
				transform.position -= new Vector3(0, 30f, 0) * Time.fixedDeltaTime;
			}
		}


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

		AnimatorStateInfo Next = animr.GetNextAnimatorStateInfo(0);
		AnimatorStateInfo Current = animr.GetCurrentAnimatorStateInfo(0);

		bool isNextValid = Next.fullPathHash != 0;

		if (transform.position.y <= -50f) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		if (health > healthMax) { health = healthMax; }
		if (meter > meterMax) { meter = meterMax; }

		if (currentState != States.Death) {
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
			}

			if (queuedAttack == Attacks.Dashing) {
				trueAngle = Mathf.Atan2(trueInput.x, trueInput.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
			}

			if (preppingAttack != AttackButton.None) {
				QueueInfo queueInfo = QueueInfoTable[(int)currentAttack][(int)preppingAttack];
				float animationPercent = AnimatorNormalizedTimeOfNextOrCurrentAttackState() % 1.0f;
				if (queueInfo.attack != Attacks.None) {
					if (animationPercent >= queueInfo.startPercent && animationPercent <= queueInfo.endPercent) {
						queuedAttack = queueInfo.attack;
					}
				}
			}

			animr.SetInteger("prepAttack", (int)queuedAttack);

			// animator controller
			if (currentAttack == Attacks.None ||
					(!wasNextValid && isNextValid && queuedAttack != Attacks.None && IsAttackState(Next)) || // NOTE(Roskuski): This is a hack to make sure that this script stays in sync with the animator when it takes a transition early.
					(AnimatorNormalizedTimeOfNextOrCurrentAttackState() >= 1.0f)) {
				if (queuedAttack != Attacks.None) {
					setCurrentAttack(queuedAttack);
					queuedAttack = Attacks.None;
				}
				else {
					currentAttack = Attacks.None;
					currentState = States.Idle;
				}
			}

			animr.SetInteger("currentAttack", (int)currentAttack);

			wasNextValid = isNextValid;
		}

		if (pActions.Player.Restart.WasPerformedThisFrame()) {
			Debug.Log("Restart called");
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		
	}

	//@TODO(Jaden): Add i-frames and trigger hitstun state when hit
	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.EnemyHitbox && vulnerable == true && kbTime <= 0) { // player is getting hit
			currentAttack = Attacks.None;
			animr.SetBool("isWalking", false);
			animr.SetInteger("currentAttack", (int)Attacks.None);
			animr.SetInteger("prepAttack", (int)AttackButton.None);
			health--;
			if (health <= 0) {
				health = 0;
				StartCoroutine(Death());
			} else {
				animr.SetTrigger("wasHurt");
				currentState = States.Idle;
				kbAngle = Quaternion.LookRotation(other.transform.position - this.transform.position);
				kbTime = maxKbTime;
				float healthPercentage = (float)health / (float)healthMax;
				spotLight.intensity = 50f * (healthPercentage);
				//Debug.Log("Spotlight intensity should be ", 50f * healthPercentage);
				Debug.Log("OWIE " + other.name + " JUST HIT ME! I have " + health + " health");
			}
		}
		else if (other.gameObject.layer == (int)Layers.EnemyHurtbox) { // player is hitting enemy
			// NOTE(Roskuski): I hit the enemy!
		}
		else if (other.gameObject.layer == (int)Layers.Pickup) {
			HeadPickup headPickup = other.gameObject.GetComponent<HeadPickup>();
			if (headPickup.canCollect) {
				headPickup.collected = true;
				GameObject.Destroy(other.transform.gameObject);
			}
		}
		else if (other.gameObject.layer == (int)Layers.TrapHitbox && vulnerable == true && kbTime <= 0) {
			kbAngle = Quaternion.LookRotation(other.transform.position - this.transform.position);
			kbTime = maxKbTime;
		}
	}

	#region Combat functions
	IEnumerator Death() {
		animr.SetBool("isDead", true);
		currentState = States.Death;
		gameMan.mainUI.enabled = false;
		capCol.enabled = false;
		rb.useGravity = false;
		spotLight.intensity = 0;
		float deathTimer = animationTimes["Character_Death_Test"];
		deathTimer -= Time.deltaTime;
		Debug.Log("Player died, restarting scene shortly");
		yield return new WaitForSeconds(deathTimer + 1);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void ChangeMeter(float Amount) {
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
	void setCurrentAttack(Attacks attack) {
		bool setupHoming = true;
		currentState = States.Attacking;

		if (attack == Attacks.HeadThrow) {
			tsr = targetSphereRadius * 2.5f;
			speedTime = 0;
			//freeAim = true;
			setupHoming = false;
		}
		else if (attack == Attacks.Chop) {
			freeAim = true;
		}
		else { tsr = targetSphereRadius; }

		animr.SetInteger("currentAttack", (int)attack);
		currentAttack = attack;

		if (pActions.Player.Aim.ReadValue<Vector2>().sqrMagnitude >= 0.02) {
			movement = new Vector3(rAimInput.x, 0, rAimInput.y); // this and next line allow for movement between hits
		}
		else if (pActions.Player.Move.ReadValue<Vector2>().sqrMagnitude >= 0.02) {
			movement = new Vector3(trueInput.x, 0, trueInput.y);
		}

		if (setupHoming) {
			SetupHoming();
		}
		else { speedTime = 0; } // stops player movement when throwing. change later if other attacks don't snap
	}

	void SetupHoming() { // attack homing function
		if (doHoming) {
			doHoming = false;
		}

		Collider[] eColliders = Physics.OverlapSphere(GetTargetSphereLocation(), tsr, Mask.Get(Layers.EnemyHurtbox));

		homingTargetDelta = Vector3.forward * 10;
		for (int index = 0; index < eColliders.Length; index += 1) {
			Vector3 newDelta = eColliders[index].transform.position - transform.position;
			if (newDelta.magnitude < homingTargetDelta.magnitude) {
				homingTargetDelta = newDelta;
				//if (currentAttack == Attacks.HeadThrow) { homingTargetDelta = eColliders[index].transform.position; }
				// temporarily using the delta v3 as just a standard v3 for this attack
			}
		}

		if (homingTargetDelta != Vector3.forward * 10) {
			switch (currentAttack) {
				case Attacks.None:
				default:
					//Debug.Assert(false);
					break;
				case Attacks.LAttack:
					homingTargetDelta *= 0.80f;
					break;
				case Attacks.LAttack2:
					homingTargetDelta *= 1;
					break;
				case Attacks.LAttack3:
					homingTargetDelta *= 1;
					break;
				case Attacks.Chop:
					homingTargetDelta *= 0.50f;
					break;
			}
			//if (currentAttack == Attacks.HeadThrow) { transform.LookAt(homingTargetDelta); }
			//else
			transform.LookAt(homingTargetDelta + transform.position);
		}
		else {
			Vector3 Location = GetTargetSphereLocation();
			Location = new Vector3(Location.x, transform.position.y, Location.z);
			homingTargetDelta = Quaternion.LookRotation(Location - transform.position, Vector3.up) * Vector3.forward * 2;
		}
	}

	Vector3 GetTargetSphereLocation() {
		if (trueInput == Vector2.zero && rAimInput == Vector2.zero) {
			if (currentAttack == Attacks.HeadThrow) { return transform.position + transform.rotation * new Vector3(0, 1.2f, 7f); }
			else return transform.position + transform.rotation * new Vector3(0, 1.2f, 3f);
		}
		else if (rAimInput.sqrMagnitude >= 0.1f) {
			if (currentAttack == Attacks.HeadThrow) { return transform.position + Quaternion.LookRotation(new Vector3(rAimInput.x, 0, rAimInput.y), Vector3.up) * new Vector3(0, 1.2f, 7f); }
			else return transform.position + Quaternion.LookRotation(new Vector3(rAimInput.x, 0, rAimInput.y), Vector3.up) * new Vector3(0, 1.2f, 2.5f);
		}
		else {
			if (currentAttack == Attacks.HeadThrow) { return transform.position + Quaternion.LookRotation(new Vector3(trueInput.x, 0, trueInput.y), Vector3.up) * new Vector3(0, 1.2f, 7f); }
			else return transform.position + Quaternion.LookRotation(new Vector3(trueInput.x, 0, trueInput.y), Vector3.up) * new Vector3(0, 1.2f, 2.5f);
		}
	}

	bool IsAttackState(AnimatorStateInfo stateInfo) {
		// NOTE(Roskuski): If we're NOT any of these states
		return !(
				stateInfo.IsName("Base.Run") ||
				stateInfo.IsName("Base.Idle") ||
				stateInfo.IsName("Base.Hit") ||
				stateInfo.IsName("Base.Death")
			);
	}

	float AnimatorNormalizedTimeOfNextOrCurrentAttackState() {
		// @TODO(Roskuski): I don't think it's a great choice, but for the time being I'm verifing that the AnimatorStateInfo is valid by checking if it's fullPathHash is 0. I imagine that the chances of actually having a zero hash is pretty low.
		float Result = 100.0f;
		int ResultSource = -1;

		AnimatorStateInfo Next = animr.GetNextAnimatorStateInfo(0);
		AnimatorStateInfo Current = animr.GetCurrentAnimatorStateInfo(0);

		if (Next.fullPathHash != 0 && IsAttackState(Next)) {
			Result = Next.normalizedTime;
			ResultSource = 1;
		}
		else if (Current.fullPathHash != 0 && IsAttackState(Current)) {
			Result = Current.normalizedTime;
			ResultSource = 0;
		}

		if ((ResultSource == AnimatorNormalizedTimeOfNextOrCurrentAttackState_LastSource) &&
		    (Result < AnimatorNormalizedTimeOfNextOrCurrentAttackState_LastValue)) {
			AnimatorNormalizedTimeOfNextOrCurrentAttackState_LastValue = Result;
			AnimatorNormalizedTimeOfNextOrCurrentAttackState_LastSource = ResultSource;
			Result += 1;
		}
		else {
			AnimatorNormalizedTimeOfNextOrCurrentAttackState_LastValue = Result;
			AnimatorNormalizedTimeOfNextOrCurrentAttackState_LastSource = ResultSource;
		}

		return Result;
	}

	public void LobThrow() { // triggered in animator
		ChangeMeter(-1);
		SetupHoming();
		//freeAim = false;
		GameObject iHeadProj = Instantiate(headProj, projSpawn.position, transform.rotation);
	}

	public void StartHoming(float time) { // called in animator; starts the homing lerp in FixedUpdate()
		homingInitalPosition = this.transform.position;
		homingTimer = time;
		homingTimerMax = time;
		doHoming = true;
		homingPrevValue = Vector3.zero;
	}
	#endregion

	#region Animation arrays
	static readonly string[] AttackToStateName = {
		"None",
		"Base.Attacks.LAttack",
		"Base.Attacks.LAttack2",
		"Base.Attacks.LAttack3",
		"Base.Attacks.Chop",
		"Base.Attacks.Slam",
		"Base.Attacks.Spin",
		"Base.Attacks.Throw",
		"Base.Attacks.Dash",
		"Base.Attacks.LethalDash",
		"Base.Attacks.ShotgunThrow"
	};
	#endregion

	#region Minor utility functions
	void OnEnable() { pActions.Enable(); }
	void OnDisable() { pActions.Disable(); }

	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(GetTargetSphereLocation(), tsr);

		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, 17);
	}
	#endregion

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
}
