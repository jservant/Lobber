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
			               new QueueInfo(0.0f, 1.0f, Attacks.Spin), // Mod Light Attack
			               new QueueInfo(0.0f, 1.0f, Attacks.Slam), // Mod Heavy Attack
			               new QueueInfo(0.0f, 1.0f, Attacks.ShotgunThrow), // Mod Throw
			               new QueueInfo(0.0f, 1.0f, Attacks.LethalDash)}, // Mod Dash
		// When in LAttack
		new QueueInfo[]{ new QueueInfo(0.0f, 0f, Attacks.None), // None
			               new QueueInfo(0.0f, 1.0f, Attacks.LAttack2), // Light Attack
			               new QueueInfo(0.3f, 1.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.3f, 1.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 1.0f, Attacks.Dashing), // Dash
			               new QueueInfo(0.0f, 1.0f, Attacks.Spin), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in LAttack2
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.1f, 1.0f, Attacks.LAttack3), // Light Attack
			               new QueueInfo(0.1f, 1.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.1f, 1.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 1.0f, Attacks.Dashing), // Dash
			               new QueueInfo(0.0f, 1.0f, Attacks.Spin), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in LAttack3
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.3f, 1.0f, Attacks.LAttack), // Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Throw
			               new QueueInfo(0.0f, 1.0f, Attacks.Dashing), // Dash
			               new QueueInfo(0.0f, 1.0f, Attacks.Spin), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in Chop
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // Nonel
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Light Attack
			               new QueueInfo(0.7f, 1.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.0f, 1.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 1.0f, Attacks.Dashing), // Dash
			               new QueueInfo(0.0f, 1.0f, Attacks.Spin), // Mod Light Attack
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
			               new QueueInfo(0.0f, 1.0f, Attacks.LAttack), // Light Attack
			               new QueueInfo(0.0f, 1.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.0f, 1.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 1.0f, Attacks.Dashing), // Dash
			               new QueueInfo(0.0f, 1.0f, Attacks.Spin), // Mod Light Attack
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
			               new QueueInfo(0.0f, 1.0f, Attacks.Spin), // Mod Light Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Mod Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None)}, // Mod Dash
		// When in LethalDash
		new QueueInfo[]{ new QueueInfo(0.0f, 0.0f, Attacks.None), // None
			               new QueueInfo(0.0f, 1.0f, Attacks.LAttack), // Light Attack
			               new QueueInfo(0.0f, 1.0f, Attacks.None), // Heavy Attack
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Throw
			               new QueueInfo(0.0f, 0.0f, Attacks.None), // Dash
			               new QueueInfo(0.0f, 1.0f, Attacks.Spin), // Mod Light Attack
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

	readonly KnockbackInfo[] EmptyAxeAttackKnockbackTable = new KnockbackInfo[] {
		//                Set Direction,      force , time 
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.0f), // None
		new KnockbackInfo(Quaternion.identity, 10.0f, 0.25f), // LAttack
		new KnockbackInfo(Quaternion.identity, 10.0f, 0.25f), // LAttack2
		new KnockbackInfo(Quaternion.identity, 30.0f, 0.25f), // LAttack3
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.25f), // Chop
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.25f), // Slam
		new KnockbackInfo(Quaternion.identity, 10.0f, 0.25f), // Spin
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.25f), // HeadThrow
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.25f), // Dashing
		new KnockbackInfo(Quaternion.identity, 30.0f, 0.25f), // LethalDashing
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.25f), // ShotgunThrow
	};

	readonly KnockbackInfo[] FullAxeAttackKnockbackTable = new KnockbackInfo[] {
		//                Set Direction,      force , time 
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.0f), // None
		new KnockbackInfo(Quaternion.identity, 20.0f, 0.25f), // LAttack
		new KnockbackInfo(Quaternion.identity, 20.0f, 0.25f), // LAttack2
		new KnockbackInfo(Quaternion.identity, 40.0f, 0.25f), // LAttack3
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.25f), // Chop
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.25f), // Slam
		new KnockbackInfo(Quaternion.identity, 20.0f, 0.25f), // Spin
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.25f), // HeadThrow
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.25f), // Dashing
		new KnockbackInfo(Quaternion.identity, 40.0f, 0.25f), // LethalDashing
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.25f), // ShotgunThrow
	};

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
	HeadProjectile headProj;
	Transform projSpawn;
	Transform[] shotgunProjSpawns;
	Transform slamPoint;
	Light spotLight;
	GameManager gameMan;
	//List<GameObject> enemiesHit;
	public DefaultPlayerActions pActions;
	GetKnockbackInfo axeGetKnockbackInfo;
	SkinnedMeshRenderer model;
	Material[] materials;
	public Material hitflashMat;

	[Header("Movement:")]
	public Vector2 trueInput;							// movement vector read from left stick
	float trueAngle = 0f;								// movement angle float generated from trueInput
	public Vector2 mInput;								// processed movement vector read from input
	[SerializeField] Vector3 movement;					// actual movement vector used. mInput(x, y) = movement(x, z)
	public bool freeAim = false;
	public Vector2 rAimInput;                           // aiming vector read from right stick
	bool isGrounded;
	[Header("Speed:")]
	[SerializeField] AnimationCurve movementCurve;
	[SerializeField] float topSpeed = 10f;				// top player speed
	public float speedTime = 0f;				// how long has player been moving for?
	[SerializeField] float maxSpeedTime = 0.2f;			// how long does it take for player to reach max speed?
	[SerializeField] float attackDecelModifier = 5f;	// modifier that makes player decelerate slower when attacking (moves them out further)
	[SerializeField] float turnSpeed = 0.05f;
	[Header("Dashing:")]
	[SerializeField] AnimationCurve dashCurve;
	[SerializeField] float dashForce = 10f;				// dash strength (how far do you go)
	[SerializeField] float dashTime = 0f;				// how long has player been dashing for?
	[SerializeField] float maxDashCooldown = 1.5f;		// how long does it take for player to dash again after dashing?
	[SerializeField] float dashCooldown = 1f;
	public bool isWalking;
	[Header("Health/Damage:")]
	public bool vulnerable = true;
	public int healthMax = 20;
	public int health = 0;
	public float meter = 0;
	public float meterMax = 5;
	public float hitflashTimer = 0;

	KnockbackInfo knockbackInfo = new KnockbackInfo(Quaternion.identity, 0, 0);
	float remainingKnockbackTime;

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
		model = transform.Find("Lobber").GetComponent<SkinnedMeshRenderer>();
		materials = model.materials;

		headMesh = transform.Find("Weapon_Controller/Hitbox/StoredHead").GetComponent<MeshRenderer>();
		headMeshTrail = transform.Find("Weapon_Controller/Hitbox/StoredHead").GetComponent<TrailRenderer>();
		axeGetKnockbackInfo = transform.Find("Weapon_Controller/Hitbox").GetComponent<GetKnockbackInfo>();
		projSpawn = transform.Find("MainProjSpawn");
		shotgunProjSpawns = transform.Find("ShotgunSpawns").GetComponentsInChildren<Transform>();
		spotLight = transform.Find("Spot Light").GetComponent<Light>();
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		headProj = gameMan.SkullPrefab;
		slamPoint = transform.Find("SlamPoint");

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

	private void FixedUpdate() { // calculate movement here
		// accel/decel for movement
		if (mInput != Vector2.zero && currentState == States.Attacking) { speedTime -= (Time.fixedDeltaTime / attackDecelModifier); }
		// if attacking, reduce movement at half speed to produce sliding effect
		else if (mInput != Vector2.zero) { speedTime += Time.fixedDeltaTime; } // else build up speed while moving
		else { speedTime -= Time.fixedDeltaTime; }
		// if no movement input and not attacking, decelerate
		if (currentState != States.Attacking) { speedTime = Mathf.Clamp(speedTime, 0, maxSpeedTime); }
		// clamp accel value between 0 and a static maximum
		remainingKnockbackTime -= Time.fixedDeltaTime;

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
			if (currentAttack == Attacks.Dashing || currentAttack == Attacks.LethalDash) {
				dashTime += Time.fixedDeltaTime;
				this.transform.rotation = Quaternion.Euler(0f, trueAngle, 0f);
				Vector3 dashDirection = Quaternion.Euler(0f, trueAngle, 0f) * Vector3.forward;
				moveDelta = dashDirection.normalized * (dashForce * Mathf.Lerp(0, 1, dashCurve.Evaluate(dashTime /
					animationTimes[currentAttack == Attacks.LethalDash ? "Character_Lethal_Dash" : "Character_Roll"])));
				if (dashTime >= animationTimes[currentAttack == Attacks.LethalDash ? "Character_Lethal_Dash" : "Character_Roll"]) {
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
			float moveWeight = Mathf.Lerp(1, 0, Mathf.Clamp01(remainingKnockbackTime / knockbackInfo.time));
			float knockbackWeight = 1f - moveWeight;
			Vector3 knockbackDelta = (knockbackInfo.direction * Vector3.forward) * knockbackInfo.force;

			translationDelta = (moveDelta * moveWeight + knockbackDelta * knockbackWeight) * Time.fixedDeltaTime;
		}

		Util.PerformCheckedLateralMovement(gameObject, 0.75f, 0.5f, translationDelta);

		float fallingSpeed = 30.0f;
		if (currentAttack == Attacks.Dashing || currentAttack == Attacks.LethalDash) {
			fallingSpeed = 0.0f;
		}
		isGrounded = Util.PerformCheckedVerticalMovement(gameObject, 0.75f, 0.2f, 0.5f, fallingSpeed);

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
		AttackButton attackButtonPrep = AttackButton.None;

		AnimatorStateInfo Next = animr.GetNextAnimatorStateInfo(0);
		AnimatorStateInfo Current = animr.GetCurrentAnimatorStateInfo(0);

		bool isNextValid = Next.fullPathHash != 0;

		if (transform.position.y <= -20f) {
			transform.position = gameMan.eSpawns[Random.Range(0, gameMan.eSpawns.Length)].transform.position;
			Hit(1, null);
		}

		if (health > healthMax) { health = healthMax; }
		if (meter > meterMax) { meter = meterMax; }

		hitflashTimer -= Time.deltaTime;
		Material[] materialList = model.materials;
		for (int i = 0; i < materialList.Length; i++) {
			if (hitflashTimer > 0) {
				materialList[i] = hitflashMat;
			}
			else {
				materialList[i] = materials[i];
			}
		}
		model.materials = materialList;

		if (currentState != States.Death) {
			if (currentState != States.Attacking) {
				mInput = pActions.Player.Move.ReadValue<Vector2>();
				if (pActions.Player.Move.WasReleasedThisFrame()) {
					animr.SetBool("isWalking", false);
					isWalking = false;
					currentState = States.Idle;
				}
				else if (pActions.Player.Move.phase == InputActionPhase.Started) {
					currentState = States.Walking;
					animr.SetBool("isWalking", true);
					isWalking = true;
					movement = movement = new Vector3(mInput.x, 0, mInput.y);
				}
				else if (pActions.Player.Move.phase == InputActionPhase.Waiting) {
					
					animr.SetBool("isWalking", false);
					isWalking = false;
				}
			}

			if (pActions.Player.LightAttack.WasPerformedThisFrame()) {
				attackButtonPrep = AttackButton.LightAttack;
			}
			if (pActions.Player.HeavyAttack.WasPerformedThisFrame()) {
				attackButtonPrep = AttackButton.HeavyAttack;
			}
			if (meter >= 1f && pActions.Player.Throw.WasPerformedThisFrame()) {
				attackButtonPrep = AttackButton.Throw;
			}
			if (meter >= 1f && pActions.Player.LightAttack.WasPerformedThisFrame() && pActions.Player.MeterModifier.phase == InputActionPhase.Performed) {
				attackButtonPrep = AttackButton.ModLight;
			}
			if (meter >= 5f && pActions.Player.HeavyAttack.WasPerformedThisFrame() && pActions.Player.MeterModifier.phase == InputActionPhase.Performed) {
				attackButtonPrep = AttackButton.ModHeavy;
			}
			if (meter >= 3f && pActions.Player.Throw.WasPerformedThisFrame() && pActions.Player.MeterModifier.phase == InputActionPhase.Performed) {
				attackButtonPrep = AttackButton.ModThrow;
			}
		
			if (meter >= 1f && pActions.Player.Dash.WasPerformedThisFrame() && pActions.Player.MeterModifier.phase == InputActionPhase.Performed &&
				trueInput.sqrMagnitude >= 0.1f && dashCooldown <= 0f  && isGrounded) { //&& Util.PerformCheckedVerticalMovement == true
					attackButtonPrep = AttackButton.ModDash;
					dashTime = 0;
					dashCooldown = maxDashCooldown;
					trueAngle = Mathf.Atan2(trueInput.x, trueInput.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
			} else if (pActions.Player.Dash.WasPerformedThisFrame() && trueInput.sqrMagnitude >= 0.1f && dashCooldown <= 0f && isGrounded) {
				attackButtonPrep = AttackButton.Dash;
				dashTime = 0;
				dashCooldown = maxDashCooldown;
				trueAngle = Mathf.Atan2(trueInput.x, trueInput.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
			}

			if (attackButtonPrep != AttackButton.None) {
				QueueInfo queueInfo = QueueInfoTable[(int)currentAttack][(int)attackButtonPrep];
				float animationPercent = AnimatorNormalizedTimeOfNextOrCurrentAttackState() % 1.0f;
				if (queueInfo.attack != Attacks.None) {
					if (animationPercent >= queueInfo.startPercent && animationPercent <= queueInfo.endPercent) {
						queuedAttack = queueInfo.attack;
					}
				}
			}
			if (queuedAttack == Attacks.Dashing || queuedAttack == Attacks.LethalDash) {
				trueAngle = Mathf.Atan2(trueInput.x, trueInput.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
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
		if (other.gameObject.layer == (int)Layers.EnemyHitbox && vulnerable == true && remainingKnockbackTime <= 0) { // player is getting hit
			Hit(1, other);
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
		else if (other.gameObject.layer == (int)Layers.TrapHitbox && vulnerable == true && remainingKnockbackTime <= 0) {
			// @TODO(Roskuski): Do we want traps to hit the player?
			//knockbackInfo = other.GetComponent<GetKnockbackInfo>().GetInfo(this.gameObject);
		}
	}

	#region Combat functions
	public void Hit(int damageTaken, Collider other) {
		currentAttack = Attacks.None;
		animr.SetBool("isWalking", false);
		animr.SetInteger("currentAttack", (int)Attacks.None);
		animr.SetInteger("prepAttack", (int)AttackButton.None);
		health -= damageTaken;
		if (health <= 0) {
			health = 0;
			StartCoroutine(Death());
		}
		else {
			animr.SetTrigger("wasHurt");
			currentState = States.Idle;
			remainingKnockbackTime = knockbackInfo.time;
			float healthPercentage = (float)health / (float)healthMax;
			spotLight.intensity = 50f * (healthPercentage);
			hitflashTimer = 0.15f;
			if (other != null) {
				knockbackInfo = other.GetComponent<GetKnockbackInfo>().GetInfo(this.gameObject);
				Debug.Log("OWIE " + other.name + " JUST HIT ME! I have " + health + " health");
			}
		}
	}

	IEnumerator Death() {
		animr.SetBool("isDead", true);
		mInput = Vector2.zero; movement = Vector3.zero;
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
		if (meter >= 1) {
			headMesh.enabled = true;
			headMeshTrail.enabled = true;
		}
		else {
			headMesh.enabled = false;
			headMeshTrail.enabled = false;
		}
		if (meter < 0) { meter = 0; }
	}
	void setCurrentAttack(Attacks attack) {
		bool setupHoming = true;
		currentState = States.Attacking;

		tsr = targetSphereRadius;
		if (attack == Attacks.HeadThrow) {
			tsr = targetSphereRadius * 2.5f;
			speedTime = 0;
			freeAim = true;
			setupHoming = false;
		}
		else if (attack == Attacks.Chop) {
			freeAim = true;
		} else if (attack == Attacks.Spin) { ChangeMeter(-1); speedTime = 0.4f; }
		else if (attack == Attacks.Slam) { ChangeMeter(-5); } //-meterMax
		else if (attack == Attacks.ShotgunThrow) { ChangeMeter(-3);}
		else if (attack == Attacks.LethalDash) { setupHoming = false; ChangeMeter(-1); }
		else if (attack == Attacks.Dashing) { setupHoming = false; }

		animr.SetInteger("currentAttack", (int)attack);
		currentAttack = attack;
		if (meter >= meterMax/2.0f) {
			axeGetKnockbackInfo.constantInfo = FullAxeAttackKnockbackTable[(int)attack];
		}
		else {
			axeGetKnockbackInfo.constantInfo = EmptyAxeAttackKnockbackTable[(int)attack];
		}

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

	public void LobThrow() { // triggered in animator
		ChangeMeter(-1);
		SetupHoming();
		//freeAim = false;
		headProj.speed = 50f;
		headProj.canStun = true;
		Instantiate(headProj, projSpawn.position, transform.rotation);
	}

	public void ShotgunThrow() { // triggered in animator
		SetupHoming();
		headProj.speed = 50f;
		headProj.canStun = true;
		//freeAim = false;
		Instantiate(headProj, projSpawn.position, transform.rotation);
		for (int i = 1; i < shotgunProjSpawns.Length; i++) { // i starts at 1 to ignore the parent object
			Instantiate(headProj, shotgunProjSpawns[i].position, shotgunProjSpawns[i].localRotation * transform.rotation);
		}
		// BUG: Spawns an extra proj further behind?
	}

	public void StartHoming(float time) { // called in animator; starts the homing lerp in FixedUpdate()
		homingInitalPosition = this.transform.position;
		homingTimer = time;
		homingTimerMax = time;
		doHoming = true;
		homingPrevValue = Vector3.zero;
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

	public void SlamParticle() {
		gameMan.SpawnParticle(1, slamPoint.position, 1f);
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
