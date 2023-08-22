using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour {

	#region Combo tree
	readonly public static QueueInfo[][] QueueInfoTable = {
		// When in None
		new QueueInfo[]{ NoQueueInfo, // None
			               new QueueInfo(0.0f, 1.0f, 0.0f, 0.0f, 0.0f, Attacks.LAttack), // Light Attack
			               new QueueInfo(0.0f, 1.0f, 0.0f, 0.0f, 0.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.0f, 1.0f, 0.0f, 0.0f, 0.0f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 1.0f, 0.0f, 0.0f, 0.0f, Attacks.Dashing), // Dash
			               new QueueInfo(0.0f, 1.0f, 0.0f, 0.0f, 0.0f, Attacks.Spin), // Mod Light Attack
			               new QueueInfo(0.0f, 1.0f, 0.0f, 0.0f, 0.0f, Attacks.Slam), // Mod Heavy Attack
			               new QueueInfo(0.0f, 1.0f, 0.0f, 0.0f, 0.0f, Attacks.ShotgunThrow), // Mod Throw
			               new QueueInfo(0.0f, 1.0f, 0.0f, 0.0f, 0.24f, Attacks.LethalDash)}, // Mod Dash
		// When in LAttack
		new QueueInfo[]{   new QueueInfo(0.0f, 1.0f, 0.950f, 0.200f, 0.000f, Attacks.None),
						   new QueueInfo(0.4f, 1.0f, 0.750f, 0.000f, 0.000f, Attacks.LAttack2), // Light Attack
			               new QueueInfo(0.4f, 1.0f, 0.750f, 0.150f, 0.000f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.4f, 1.0f, 0.700f, 0.090f, 0.15f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.3f, 1.0f, 0.650f, 0.150f, 0.050f, Attacks.Dashing), // Dash
			               new QueueInfo(0.2f, 1.0f, 0.650f, 0.000f, 0.000f, Attacks.Spin), // Mod Light Attack
			               new QueueInfo(0.4f, 1.0f, 0.740f, 0.100f, 0.300f, Attacks.Slam), // Mod Heavy Attack
			               new QueueInfo(0.4f, 1.0f, 0.700f, 0.059f, 0.150f, Attacks.ShotgunThrow), // Mod Throw
			               new QueueInfo(0.3f, 1.0f, 0.650f, 0.150f, 0.300f, Attacks.LethalDash)}, // Mod Dash
		// When in LAttack2
		new QueueInfo[]{   new QueueInfo(0.0f, 1.0f, 0.950f, 0.200f, 0.000f, Attacks.None),
						   new QueueInfo(0.4f, 1.0f, 0.750f, 0.100f, 0.000f, Attacks.LAttack3), // Light Attack
			               new QueueInfo(0.3f, 1.0f, 0.670f, 0.073f, 0.000f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.3f, 1.0f, 0.652f, 0.163f, 0.150f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.3f, 1.0f, 0.650f, 0.150f, 0.050f, Attacks.Dashing), // Dash
			               new QueueInfo(0.3f, 1.0f, 0.650f, 0.000f, 0.000f, Attacks.Spin), // Mod Light Attack
			               new QueueInfo(0.3f, 1.0f, 0.700f, 0.100f, 0.300f, Attacks.Slam), // Mod Heavy Attack
			               new QueueInfo(0.3f, 1.0f, 0.700f, 0.057f, 0.150f, Attacks.ShotgunThrow), // Mod Throw
			               new QueueInfo(0.3f, 1.0f, 0.650f, 0.100f, 0.300f, Attacks.LethalDash)}, // Mod Dash
		// When in LAttack3
		new QueueInfo[]{   new QueueInfo(0.0f, 1.0f, 0.950f, 0.200f, 0.000f, Attacks.None),
						   NoQueueInfo, // Light Attack
			               NoQueueInfo, // Heavy Attack
			               NoQueueInfo, // Throw
			               new QueueInfo(0.4f, 1.0f, 0.800f, 0.189f, 0.000f, Attacks.Dashing), // Dash
			               new QueueInfo(0.4f, 1.0f, 0.800f, 0.000f, 0.000f, Attacks.Spin), // Mod Light Attack
			               new QueueInfo(0.4f, 1.0f, 0.810f, 0.100f, 0.300f, Attacks.Slam), // Mod Heavy Attack
			               NoQueueInfo, // Mod Throw
			               new QueueInfo(0.4f, 1.0f, 0.800f, 0.100f, 0.300f, Attacks.LethalDash)}, // Mod Dash
		// When in Chop
		new QueueInfo[]{   new QueueInfo(0.0f, 1.0f, 0.950f, 0.200f, 0.000f, Attacks.None),
						   new QueueInfo(0.1f, 1.0f, 0.370f, 0.044f, 0.000f, Attacks.LAttack), // Light Attack
			               new QueueInfo(0.4f, 1.0f, 0.622f, 0.030f, 0.000f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.1f, 1.0f, 0.400f, 0.045f, 0.15f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.1f, 1.0f, 0.400f, 0.041f, 0.000f, Attacks.Dashing), // Dash
			               new QueueInfo(0.1f, 1.0f, 0.400f, 0.000f, 0.000f, Attacks.Spin), // Mod Light Attack
			               new QueueInfo(0.1f, 1.0f, 0.400f, 0.050f, 0.300f, Attacks.Slam), // Mod Heavy Attack
			               new QueueInfo(0.1f, 1.0f, 0.400f, 0.088f, 0.154f, Attacks.ShotgunThrow), // Mod Throw
			               new QueueInfo(0.1f, 1.0f, 0.400f, 0.000f, 0.240f, Attacks.LethalDash)}, // Mod Dash
		// When in Slam
		new QueueInfo[]{   new QueueInfo(0.0f, 1.0f, 0.950f, 0.200f, 0.000f, Attacks.None),
						   NoQueueInfo, // Light Attack
			               NoQueueInfo, // Heavy Attack
			               NoQueueInfo, // Throw
			               NoQueueInfo, // Dash
			               NoQueueInfo, // Mod Light Attack
			               new QueueInfo(0.4f, 1.0f, 0.683f, 0.048f, 0.367f, Attacks.Slam), // Mod Heavy Attack
			               NoQueueInfo, // Mod Throw
			               NoQueueInfo}, // Mod Dash
		// When in Spin
		new QueueInfo[]{   new QueueInfo(0.0f, 1.0f, 0.950f, 0.200f, 0.000f, Attacks.None),
						   new QueueInfo(0.0f, 1.0f, 0.350f, 0.050f, 0.100f, Attacks.LAttack), // Light Attack
			               new QueueInfo(0.0f, 1.0f, 0.350f, 0.100f, 0.000f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.0f, 1.0f, 0.320f, 0.050f, 0.250f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.0f, 1.0f, 0.320f, 0.050f, 0.000f, Attacks.Dashing), // Dash
			               new QueueInfo(0.0f, 0.4f, 0.250f, 0.100f, 0.000f, Attacks.Spin), // Mod Light Attack
			               new QueueInfo(0.0f, 1.0f, 0.350f, 0.102f, 0.300f, Attacks.Slam), // Mod Heavy Attack
			               new QueueInfo(0.0f, 1.0f, 0.350f, 0.050f, 0.200f, Attacks.ShotgunThrow), // Mod Throw
			               new QueueInfo(0.0f, 1.0f, 0.350f, 0.100f, 0.300f, Attacks.LethalDash)}, // Mod Dash
		// When in HeadThrow
		new QueueInfo[]{   new QueueInfo(0.0f, 1.0f, 0.950f, 0.200f, 0.000f, Attacks.None),
						   new QueueInfo(0.3f, 1.0f, 0.750f, 0.100f, 0.000f, Attacks.LAttack), // Light Attack
			               new QueueInfo(0.3f, 1.0f, 0.750f, 0.100f, 0.000f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.5f, 1.0f, 0.750f, 0.150f, 0.150f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.3f, 1.0f, 0.650f, 0.250f, 0.000f, Attacks.Dashing), // Dash
			               new QueueInfo(0.3f, 1.0f, 0.650f, 0.000f, 0.000f, Attacks.Spin), // Mod Light Attack
			               new QueueInfo(0.3f, 1.0f, 0.750f, 0.100f, 0.300f, Attacks.Slam), // Mod Heavy Attack
			               new QueueInfo(0.3f, 1.0f, 0.750f, 0.050f, 0.200f, Attacks.ShotgunThrow), // Mod Throw
			               new QueueInfo(0.3f, 1.0f, 0.650f, 0.100f, 0.150f, Attacks.LethalDash)}, // Mod Dash
		// When in Dashing
		new QueueInfo[]{   new QueueInfo(0.0f, 1.0f, 0.950f, 0.200f, 0.000f, Attacks.None),
						   new QueueInfo(0.1f, 1.0f, 0.400f, 0.09326167f, 0.000f, Attacks.LAttack), // Light Attack
			               new QueueInfo(0.1f, 1.0f, 0.400f, 0.2691187f, 0.0f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.1f, 1.0f, 0.400f, 0.3544056f, 0.25f, Attacks.HeadThrow), // Throw
			               NoQueueInfo, // Dash
			               new QueueInfo(0.1f, 1.0f, 0.400f, 0.1125348f, 0.000f, Attacks.Spin), // Mod Light Attack
			               new QueueInfo(0.1f, 1.0f, 0.680f, 0.2803563f, 0.300f, Attacks.Slam), // Mod Heavy Attack
			               new QueueInfo(0.1f, 1.0f, 0.400f, 0.189f, 0.200f, Attacks.ShotgunThrow), // Mod Throw
			               new QueueInfo(0.1f, 1.0f, 0.904f, 0.086f, 0.129f, Attacks.LethalDash)}, // Mod Dash
		// When in LethalDash
		new QueueInfo[]{   new QueueInfo(0.0f, 1.0f, 0.950f, 0.200f, 0.000f, Attacks.None),
						   new QueueInfo(0.3f, 1.0f, 0.700f, 0.100f, 0.200f, Attacks.LAttack3), // Light Attack
			               new QueueInfo(0.3f, 1.0f, 0.700f, 0.100f, 0.000f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.3f, 1.0f, 0.700f, 0.100f, 0.150f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.4f, 1.0f, 0.916f, 0.074f, 0.000f, Attacks.Dashing), // Dash
			               new QueueInfo(0.3f, 1.0f, 0.700f, 0.000f, 0.000f, Attacks.Spin), // Mod Light Attack
			               new QueueInfo(0.3f, 1.0f, 0.700f, 0.259f, 0.300f, Attacks.Slam), // Mod Heavy Attack
			               new QueueInfo(0.3f, 1.0f, 0.700f, 0.055f, 0.200f, Attacks.ShotgunThrow), // Mod Throw
			               new QueueInfo(0.3f, 1.0f, 0.907f, 0.091f, 0.157f, Attacks.LethalDash)}, // Mod Dash
		// When in ShotgunThrow
		new QueueInfo[]{   new QueueInfo(0.0f, 1.0f, 0.950f, 0.200f, 0.000f, Attacks.None),
						   new QueueInfo(0.3f, 1.0f, 0.600f, 0.100f, 0.000f, Attacks.LAttack), // Light Attack
			               new QueueInfo(0.3f, 1.0f, 0.600f, 0.040f, 0.000f, Attacks.Chop), // Heavy Attack
			               new QueueInfo(0.2f, 1.0f, 0.500f, 0.045f, 0.150f, Attacks.HeadThrow), // Throw
			               new QueueInfo(0.2f, 1.0f, 0.500f, 0.100f, 0.000f, Attacks.Dashing), // Dash
			               new QueueInfo(0.2f, 1.0f, 0.500f, 0.000f, 0.000f, Attacks.Spin), // Mod Light Attack
			               new QueueInfo(0.3f, 1.0f, 0.600f, 0.050f, 0.300f, Attacks.Slam), // Mod Heavy Attack
			               new QueueInfo(0.2f, 1.0f, 0.500f, 0.035f, 0.200f, Attacks.ShotgunThrow), // Mod Throw
			               new QueueInfo(0.2f, 1.0f, 0.500f, 0.100f, 0.240f, Attacks.LethalDash)}, // Mod Dash
	};

	readonly static QueueInfo NoQueueInfo = new QueueInfo(0, 0, 0, 0, 0, Attacks.None);

	public struct QueueInfo {
		// When queueing is allowed
		public float startQueuePercent;
		public float endQueuePercent;
		// After this percent we'll start the next animation.
		public float transitionStartPercent;
		// How long the transition will take of the next animation
		public float transitionDurationPercent;
		// At what percent to start the next animation
		public float nextOffset;
		// Attack to trigger
		public Attacks nextAttack;

		public QueueInfo(float startQueuePercent, float endQueuePercent, float transitionStartPercent, float transitionDurationPercent, float nextOffset, Attacks nextAttack) {
			this.startQueuePercent = startQueuePercent;
			this.endQueuePercent = endQueuePercent;
			this.transitionStartPercent = transitionStartPercent;
			this.transitionDurationPercent = transitionDurationPercent;
			this.nextOffset = nextOffset;
			this.nextAttack = nextAttack;
		}
	}

	static readonly KnockbackInfo[] AttackKnockbackTable = new KnockbackInfo[] {
		//                Set Direction,      force , time 
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.0f), // None
		new KnockbackInfo(Quaternion.identity, 30.0f, 0.25f), // LAttack
		new KnockbackInfo(Quaternion.identity, 30.0f, 0.25f), // LAttack2
		new KnockbackInfo(Quaternion.identity, 60.0f, 0.25f), // LAttack3
		new KnockbackInfo(Quaternion.identity, 10.0f, 0.25f), // Chop
		new KnockbackInfo(Quaternion.identity, 70.0f, 0.35f), // Slam
		new KnockbackInfo(Quaternion.identity, 20.0f, 0.25f), // Spin
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.25f), // HeadThrow
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.25f), // Dashing
		new KnockbackInfo(Quaternion.identity, 20.0f, 0.25f), // LethalDashing
		new KnockbackInfo(Quaternion.identity,  0.0f, 0.25f), // ShotgunThrow
	};
	
	public static readonly float[] AttackDamageTable = {
		0f, // None
		1f, // LAttack
		1f, // LAttack2
		3f, // LAttack3
		5f, // Chop (Should be enough to kill a basic in one hit)
		0f, // Slam (Special case, Slam does different damages at different radii)
		2f, // Spin
		0f, // HeadThrow (Hit + Damage is handled by the projectile, we shouldn't even get a hit while in this attack)
		0f, // Dashing
		4f, // LethalDash
		0f, // ShotgunThrow (Hit + Damage is handled by the projectile, we shouldn't even get a hit while in this attack)
	};

	public static readonly float[] AttackMeterCost = {
		100.0f, // None
		0.0f, // LAttack
		0.0f, // LAttack2
		0.0f, // LAttack3
		0.0f, // Chop
		4.0f, // Slam
		0.5f, // Spin
		1.0f, // HeadThrow
		0.0f, // Dashing
		1.0f, // LethalDashing
		3.0f, // ShotgunThrow
	};

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

	static bool animationTimesPopulated = false;
	static Dictionary<string, float> animationTimes;
	#endregion

	#region State machines

	public enum States { Idle = 0, Walking, Attacking, Death, Win };
	public States currentState = 0;

	public enum Attacks {
		None = 0,
		LAttack,
		LAttack2,
		LAttack3,
		Chop,
		Slam,
		Spin,
		HeadThrow,
		Dashing,
		LethalDash,
		ShotgunThrow
	};
	
	public enum AttackBitMask : int {
		None = 1 << Attacks.None,
		LAttack = 1 << Attacks.LAttack,
		LAttack2 = 1 << Attacks.LAttack2,
		LAttack3 = 1 << Attacks.LAttack3,
		Chop = 1 << Attacks.Chop,
		Slam = 1 << Attacks.Slam,
		Spin = 1 << Attacks.Spin,
		HeadThrow = 1 << Attacks.HeadThrow,
		Dashing = 1 << Attacks.Dashing,
		LethalDash = 1 << Attacks.LethalDash,
		ShotgunThrow = 1 << Attacks.ShotgunThrow
	}
	public Attacks currentAttack = 0;
	public QueueInfo queuedAttackInfo = NoQueueInfo;

	public enum AttackButton { None = 0, LightAttack, HeavyAttack, Throw, Dash, ModLight, ModHeavy, ModThrow, ModDash };

	[Space]
	#endregion

	[Header("Object assignments:")]
	public DefaultPlayerActions pActions;
	GetKnockbackInfo axeGetKnockbackInfo;
	SkinnedMeshRenderer model;
	Material[] materials;
	public Material hitflashMat;

	CapsuleCollider capCol;
	Rigidbody rb;
	public Animator animr;
	public MeshRenderer[] headMesh;
	Transform mainJoint;
	HeadProjectile headProj;
	public Transform projSpawn;
	Transform[] shotgunProjSpawns;
	Transform shotgunPoint;
	Transform slamPoint;
	Transform dashFlashPoint;
	public Transform crystalHolster;
	Light spotLight;
	GameManager gameMan;
	MotionAudio_Player sounds;
	public float footStepCounter;
	Transform footStepParticlePoint;
	public Transform portalPoint;

	public HapticEffect[] hapticEffects;

	[Header("Movement:")]
	public Vector2 trueInput;							// movement vector read from left stick
	float trueAngle = 0f;								// movement angle float generated from trueInput
	public Vector2 mInput;								// processed movement vector read from input
	public Vector3 movement;					// actual movement vector used. mInput(x, y) = movement(x, z)
	public bool freeAim = false;
	public Vector2 rAimInput;                           // aiming vector read from right stick
	bool isGrounded;
	public bool canMove = true;
	[Header("Speed:")]
	[SerializeField] AnimationCurve movementCurve;
	public float topSpeed = 10f;				// top player speed
	public float speedTime = 0f;				// how long has player been moving for?
	[SerializeField] float maxSpeedTime = 0.2f;			// how long does it take for player to reach max speed?
	[SerializeField] float attackDecelModifier = 5f;	// modifier that makes player decelerate slower when attacking (moves them out further)
	[SerializeField] float turnSpeed = 0.05f;
	[Header("Dashing:")]
	[SerializeField] AnimationCurve dashCurve;
	[SerializeField] float dashForce = 10f;             // dash strength (how far do you go)
	[SerializeField] float chopMovementMultiplier = 6f; // what to multiply dashForce by when using Chop
	[SerializeField] float meterDashMultiplier = 2f;    // what to multiply dashForce by when using Lethal Dash
	[SerializeField] float dashTime = 0f;				// how long has player been dashing for?
	[SerializeField] float maxDashCooldown = 1.5f;		// how long does it take for player to dash again after dashing?
	public float dashCooldown = 1f;
	public bool isWalking;
	[Header("Health/Damage:")]
	float immunityTime;
	public bool godMode = false;
	public int healthMax = 20;
	public int health = 0;
	public float meter = 0;
	public float meterMax = 5;
	public float hitflashTimer = 0;
	public float frenzyTimer = 0f;
	public int crystalCount = 0;
	public bool deathResist;

	KnockbackInfo knockbackInfo = new KnockbackInfo(Quaternion.identity, 0, 0);
	public float remainingKnockbackTime;
	[Header("Homing:")]
	[SerializeField] float homingDistance = 3f;			// how far away the player can home (this x2 is the length of the capsule)
	[SerializeField] float homingCapsuleRadius = 3f;	// how wide the homing zone is
	Vector3 homingInitalPosition;						// where player starts when homing
	Vector3 homingTargetDelta;							// where player is going at end of homing
	Vector3 homingPrevValue;
	float homingTimer;									// keeps track of how long homing has been happening
	float homingTimerMax;								// how long homing will happen
	bool doHoming = false;								// bool that controls homing function in FixedUpdate

	private void Awake() {
		capCol = GetComponent<CapsuleCollider>();
		rb = GetComponent<Rigidbody>();
		sounds = GetComponent<MotionAudio_Player>();
		animr = GetComponent<Animator>();
		pActions = new DefaultPlayerActions();
		model = transform.Find("Lobber").GetComponent<SkinnedMeshRenderer>();
		materials = model.materials;

		axeGetKnockbackInfo = transform.Find("Weapon_Controller/Hitbox").GetComponent<GetKnockbackInfo>();
		projSpawn = transform.Find("MainProjSpawn");
		shotgunProjSpawns = transform.Find("ShotgunSpawns").GetComponentsInChildren<Transform>();
		spotLight = transform.Find("Spot Light").GetComponent<Light>();
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		headProj = gameMan.SkullPrefab;
		slamPoint = transform.Find("SlamPoint");
		shotgunPoint = transform.Find("ShotgunPoint");
		dashFlashPoint = transform.Find("DashFlashPoint");
		footStepParticlePoint = transform.Find("FootstepParticlePoint");
		crystalHolster = transform.Find("MAIN_JOINT/MidTorso_Joint/Chest_Joint/CrystalHipSpawn");
		mainJoint = transform.Find("MAIN_JOINT");

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
		health = GameManager.storedPlayerHealth;
		meter = GameManager.storedPlayerMeter;
		footStepCounter = 0;
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
		if (currentState == States.Attacking && currentAttack == Attacks.Spin) {
			speedTime = maxSpeedTime * 1.0f;
		}

		Vector3 translationDelta = Vector3.zero;
		if (doHoming) { // doing homing takes priority over normal movement
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
				//gameMan.crystalCountText.text = "";
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
				if (currentAttack == Attacks.LethalDash) moveDelta *= meterDashMultiplier;
				if (dashTime >= animationTimes[currentAttack == Attacks.LethalDash ? "Character_Lethal_Dash" : "Character_Roll"] && currentState != States.Win) {
					currentState = States.Idle;
					trueAngle = 0;
					currentAttack = 0;
					dashTime = 0;
				}
			}
			else {
				float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + UnityEngine.Camera.main.transform.eulerAngles.y;
				float turnVelocity = 0f;  // annoying float that is only referenced and has to exist for movement math to work
				float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnSpeed);
				if (currentAttack == Attacks.None) {
					transform.rotation = Quaternion.Euler(0f, angle, 0f);
				}
				Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
				moveDelta = moveDirection.normalized * (topSpeed * Mathf.Lerp(0, 1, movementCurve.Evaluate(speedTime / maxSpeedTime)));
				if (currentAttack == Attacks.Chop) moveDelta *= chopMovementMultiplier;
			}
			Vector3 knockbackDelta = Util.ProcessKnockback(ref remainingKnockbackTime, knockbackInfo);

			translationDelta = (moveDelta + knockbackDelta) * Time.fixedDeltaTime;
		}

		float fallingSpeed = 30.0f;
		float stepUp = 0.75f;
		int layerMask = ~Mask.Get(Layers.Corpses);
		if (currentAttack == Attacks.Dashing || currentAttack == Attacks.LethalDash || currentState == States.Win) {
			fallingSpeed = 0.0f;
			stepUp = 1.5f;
			layerMask &= ~Mask.Get(new Layers[] {Layers.EnemyHitbox, Layers.EnemyHurtbox, Layers.StickyLedge});
		}

		if (remainingKnockbackTime > 0) {
			layerMask &= ~Mask.Get(Layers.StickyLedge);
		}

		if (currentState != States.Death && currentState != States.Win) {
			Util.PerformCheckedLateralMovement(gameObject, stepUp, 0.5f, translationDelta, layerMask);
			isGrounded = Util.PerformCheckedVerticalMovement(gameObject, stepUp, 0.2f, 0.5f, fallingSpeed);
		}

		if (freeAim) { // assigned in animator
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

		if (health > healthMax) { health = healthMax; }
		if (meter > meterMax) { meter = meterMax; }
		UpdateAxeMesh();

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

		immunityTime -= Time.deltaTime;

		if (frenzyTimer > 0f) {
			frenzyTimer -= Time.deltaTime;
		}

		CheckDeathResist();

		//Input
		if (currentState != States.Death && currentState != States.Win && gameMan.pauseBG.enabled == false && canMove && Application.isFocused) {
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
					movement = new Vector3(mInput.x, 0, mInput.y);
				}
				else if (pActions.Player.Move.phase == InputActionPhase.Waiting) {
					
					animr.SetBool("isWalking", false);
					isWalking = false;
				}
			}

			if (pActions.Player.LightAttack.WasPerformedThisFrame()) {
				if (pActions.Player.MeterModifier.phase == InputActionPhase.Performed) {
					attackButtonPrep = AttackButton.ModLight;
					if (meter < 0.2f) gameMan.MeterSpendFail(0);
				}
				else { 
					attackButtonPrep = AttackButton.LightAttack;
				}
			}

			if (pActions.Player.HeavyAttack.WasPerformedThisFrame()) {
				if (pActions.Player.MeterModifier.phase == InputActionPhase.Performed) {
					attackButtonPrep = AttackButton.ModHeavy;
					if (meter < 3.7f) gameMan.MeterSpendFail(3);
				}
				else {
					attackButtonPrep = AttackButton.HeavyAttack;
				}
			}

			if (pActions.Player.Throw.WasPerformedThisFrame()) {
				if (pActions.Player.MeterModifier.phase == InputActionPhase.Performed) {
					attackButtonPrep = AttackButton.ModThrow;
					if (meter < 2.7f) gameMan.MeterSpendFail(2);
				}
				else {
					attackButtonPrep = AttackButton.Throw;
					if (meter < 0.7f) gameMan.MeterSpendFail(1);
				}
			}

			if (pActions.Player.Dash.WasPerformedThisFrame() && trueInput.sqrMagnitude >= 0.1f && dashCooldown <= 0f && isGrounded) {
				// NOTE(Roskuski): Common variables for dashing.

				if (pActions.Player.MeterModifier.phase == InputActionPhase.Performed) {
					attackButtonPrep = AttackButton.ModDash;
					if (meter < 0.7f) gameMan.MeterSpendFail(1);
				}
				else {
					attackButtonPrep = AttackButton.Dash;
				}
			}

			if (attackButtonPrep != AttackButton.None) {
				QueueInfo queueInfo = QueueInfoTable[(int)currentAttack][(int)attackButtonPrep];
				float animationPercent = Current.normalizedTime % 1.0f;
				if (queueInfo.nextAttack != Attacks.None && CanAffordMove(queueInfo.nextAttack)) {
					if (animationPercent >= queueInfo.startQueuePercent && animationPercent <= queueInfo.endQueuePercent) {
						queuedAttackInfo = queueInfo;
						if (queueInfo.nextAttack == Attacks.Dashing) {
							dashTime = 0;
							dashCooldown = maxDashCooldown;
						}

						if (queueInfo.nextAttack == Attacks.LethalDash) {
							dashTime = 0;
							dashCooldown = 0.4f;
						}
					}
				}
			}

			// NOTE(Roskuski): Update the angle we'll dash in every frame that a dash is queued.
			if (queuedAttackInfo.nextAttack == Attacks.Dashing || queuedAttackInfo.nextAttack == Attacks.LethalDash) {
				trueAngle = Mathf.Atan2(trueInput.x, trueInput.y) * Mathf.Rad2Deg + UnityEngine.Camera.main.transform.eulerAngles.y;
			}

			// animator controller
			{
				if (queuedAttackInfo.nextAttack != Attacks.None && queuedAttackInfo.transitionStartPercent < Current.normalizedTime) {
					bool setupHoming = true;                    
					currentState = States.Attacking;
                    doHoming = false;  // NOTE(Roskuski): Stop homing from the previous attack

                    animr.CrossFade(AttackToStateName[(int)queuedAttackInfo.nextAttack], queuedAttackInfo.transitionDurationPercent, -1, queuedAttackInfo.nextOffset);
					currentAttack = queuedAttackInfo.nextAttack;
					queuedAttackInfo = NoQueueInfo;
					ChangeMeter(-AttackMeterCost[(int)currentAttack]);

					switch (currentAttack) {
						case Attacks.HeadThrow:
						case Attacks.ShotgunThrow:
						speedTime = 0;
							break;

						case Attacks.Chop:
							speedTime = 0f;
							break;

						case Attacks.Spin:
							speedTime = 0.4f;
							setupHoming = false;
							this.transform.rotation = Quaternion.LookRotation(movement, Vector3.up);
							break;

						case Attacks.LethalDash:
							setupHoming = false;
							break;

						case Attacks.Dashing:
							setupHoming = false;
							immunityTime = 0.25f;
							break;
					}

					axeGetKnockbackInfo.constantInfo = AttackKnockbackTable[(int)currentAttack];

					if (pActions.Player.Aim.ReadValue<Vector2>().sqrMagnitude >= 0.02) {
						movement = new Vector3(rAimInput.x, 0, rAimInput.y); // this and next line allow for movement between hits
					} else if (pActions.Player.Move.ReadValue<Vector2>().sqrMagnitude >= 0.02) {
						movement = new Vector3(trueInput.x, 0, trueInput.y);
					}

                    if (setupHoming) {
						SetupHoming();
					} else {
						speedTime = 0; // stops player movement when throwing
					}
				}
				else {
					QueueInfo queueInfo = QueueInfoTable[(int)currentAttack][(int)AttackButton.None];
					if (currentAttack != Attacks.None && IsAttackState(Current) && Current.normalizedTime >= queueInfo.transitionStartPercent && Next.normalizedTime == 0f) {
						animr.CrossFade("Base.Idle", queueInfo.transitionDurationPercent, -1, queueInfo.nextOffset);
						currentAttack = Attacks.None;
						currentState = States.Idle;
					}
				}
			}
		}

		if ((currentState == States.Idle || currentState == States.Walking) && IsAttackState(Current)) {
			animr.Play("Base.Idle");
		}

		if (currentState == States.Win) {
			speedTime = 0f;
			movement = new Vector3(0, 0, 0);
			if (portalPoint != null) {
				transform.position = portalPoint.position;
            }
		}

	}

	private void OnTriggerEnter(Collider other) {
		if (immunityTime <= 0 && currentAttack != Attacks.Slam && currentAttack != Attacks.Spin && currentAttack != Attacks.LethalDash&& currentAttack != Attacks.ShotgunThrow && !godMode) {
			if (other.gameObject.layer == (int)Layers.EnemyHitbox && remainingKnockbackTime <= 0) { // player is getting hit
				Basic otherBasic = other.GetComponentInParent<Basic>();
				NecroProjectile otherNecroProjectile = other.GetComponent<NecroProjectile>();
				MiniFireball mini = other.GetComponent<MiniFireball>();

				if (otherBasic != null) {
					int damage = 0;
					switch (otherBasic.currentAttack) {
						case Basic.Attack.Slash:
							damage = 1;
							DropCrystals();
							break;

						case Basic.Attack.Lunge:
							damage = 2;
                            DropCrystals();
                            break;

						default:
							Debug.Assert(false);
							break;
					}
					Hit(damage, other);
				}
				else if (otherNecroProjectile != null) {
					Hit(2, other);
                    DropCrystals();
                }
				else if (mini != null) {
					mini.Grow();
					Hit(1, other);
					DropCrystals();
				}
			}
			else if (other.gameObject.layer == (int)Layers.AgnosticHitbox) {
				if (other.GetComponentInParent<Exploding>() != null) {
					// NOTE(Roskuski): Explosive enemy
					Hit(3, other);
                    DropCrystals();
                }
            }
			else if (other.gameObject.layer == (int)Layers.TransparentFX) {
				Burn burn = other.GetComponent<Burn>();
				if (burn != null) {
					Hit(1, other);
				}
			}
		}

		if (other.gameObject.layer == (int)Layers.Pickup) {
			Pickup headPickup = other.gameObject.GetComponent<Pickup>();
			if (headPickup.lifetime <= headPickup.timeUntilCollect) {
				headPickup.collected = true;
				if (headPickup.pickupType == Pickup.Type.Skull) sounds.Sound_HeadPickup();
				if (headPickup.pickupType == Pickup.Type.RedSkull) sounds.Sound_HeadPickup();
				if (headPickup.pickupType == Pickup.Type.Health) sounds.Sound_HealthPickup();
				if (headPickup.pickupType == Pickup.Type.Crystal) {
					sounds.Sound_HeadPickup();
					sounds.Sound_CrystalPickup();
					if (!Initializer.save.versionLatest.hasCompletedCrystalTaskOnce) gameMan.helperText.text = "- Bring them to the Cart";
				}
				if (headPickup.pickupType == Pickup.Type.GoldenSkull) {
					sounds.Sound_HeadPickup();
					sounds.Sound_CrystalPickup();
				}
			}
		}
	}

	void DropCrystals() {
		if (crystalCount > 0) {
			for (int i = 0; i < crystalCount; i++) {
				Vector3 pos = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
				gameMan.DeterminePickups(pos, true, false);
			}
			crystalCount = 0;
			gameMan.crystalCountText.text = "";
			gameMan.crystalPickupImage.enabled = false;
			for (var i = crystalHolster.childCount - 1; i >= 0; i--) {
				Destroy(crystalHolster.GetChild(i).gameObject);
			}
			CrystalDropoff.indicator.enabled = false;
			if (!Initializer.save.versionLatest.hasCompletedCrystalTaskOnce) { gameMan.helperText.text = "- Grab the Crystals" + "\n- Bring them to the Cart"; }

            sounds.Sound_CrystalDrop();
			sounds.Sound_CrystalPickup();
			Vector3 newPos = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
			Util.SpawnFlash(gameMan, 5, newPos, true);
		}
    }

    #region Combat functions
    public void Hit(int damageTaken, Collider other) {
		doHoming = false;
		//currentAttack = Attacks.None;
		//animr.SetBool("isWalking", false);
		if (gameMan.barDrainTime <= 0) gameMan.previousHealth = health;
		if (gameMan.hardModeActive && damageTaken > 0) damageTaken += 1;
		health -= damageTaken;
		if (health == 1) deathResist = false;
		if (health <= 0) {
			if (deathResist) {
				health = 1;
				deathResist = false;
			}
			else {
				health = 0;
				StartCoroutine(Death());
			}
		}
		else {
			//animr.SetTrigger("wasHurt");
			//currentState = States.Idle;
			float healthPercentage = (float)health / (float)healthMax;
			spotLight.intensity = 50f * (healthPercentage);
			hitflashTimer = 0.15f;
			if (other != null) {
				knockbackInfo = other.GetComponent<GetKnockbackInfo>().GetInfo(this.gameObject);
				remainingKnockbackTime = knockbackInfo.time;
				Vector3 directionDelta = transform.position - other.transform.position;
				movement = new Vector3(directionDelta.x, 0, directionDelta.z);
				Debug.Log("OWIE " + other.name + " JUST HIT ME! I have " + health + " health");
				gameMan.SpawnParticle(15, transform.position, 1f);
				SmokeParticleSmall();
			}
		}
		//Lose some killStreak Time
		float timeLoss = -damageTaken;
		gameMan.AddToKillStreak(0, timeLoss);

		if (damageTaken >= 1) {
			gameMan.HealthDialGrow(0.5f);
        }

		//trigger haptics here
		if (GameObject.Find("HapticManager") != null && Initializer.save.versionLatest.rumble) {
			HapticManager.PlayEffect(hapticEffects[0], this.transform.position);
		}
		gameMan.ShakeCamera(5f, 0.1f);
		gameMan.HurtVignette(0.8f, damageTaken);
		gameMan.barDrainTime += damageTaken / 2f;
		if (gameMan.barDrainTime > 1.5f) gameMan.barDrainTime = 1.5f;
		sounds.CharacterGetHit();
		immunityTime = 0.25f;
	}

	IEnumerator Death() {
		Die();
		animr.SetBool("isDead", true);
		mInput = Vector2.zero; movement = Vector3.zero;
		currentState = States.Death;
		gameMan.mainUI.enabled = false;
		GameManager.ResetSpawnerValues();
		capCol.enabled = false;
		rb.useGravity = false;
		spotLight.intensity = 0;
		float deathTimer = animationTimes["Character_Death_Test"];
		deathTimer -= Time.deltaTime;
		Debug.Log("Player died, game over");
		gameMan.statusTextboxText.text = "GAME OVER \nLevel " + GameManager.levelCount + ", " + GameManager.enemiesKilledInRun + " enemies killed"
			+ (GameManager.levelCount > Initializer.save.versionLatest.longestRun ? "\nNew Longest Run!" : "");
		GameManager.enemiesKilledInRun = 0;
		yield return new WaitForSeconds(deathTimer + 1);
		StartCoroutine(gameMan.QuitTransition(true));
	}

	public void Die() {
		foreach (Transform child in this.transform) {
			child.gameObject.SetActive(false);
        }

		gameMan.SpawnCorpse(2, transform.position, Quaternion.identity, 1f, true);
    }

	public void Win() {
		animr.SetBool("isTeleporting", true);
		currentState = States.Win;
		mInput = Vector2.zero; movement = Vector3.zero;
		capCol.enabled = false;
		rb.useGravity = false;
		rb.isKinematic = true;
	}

	public void ChangeMeter(float Amount) {
		if (frenzyTimer > 0) return;
		if (currentAttack == Attacks.HeadThrow) return;
		meter += Amount;
		if (Amount >= 1) {
			gameMan.MeterDialGrow(0.5f);
		}
		else if (Amount >= 0.1) {
			gameMan.MeterDialGrow(0.2f);
		}
		if (meter < 0) { meter = 0; }
	}

	public void ChangeMeterThrow() {
		if (frenzyTimer > 0) return;
		meter -= 1.0f;
		if (meter < 0) meter = 0;
	}

    public void StartHoming(float time) { // called in animator; starts the homing lerp in FixedUpdate()
        if (trueInput.magnitude > 0.1f) {
            movement = new Vector3(trueInput.x, 0, trueInput.y);
        }
        homingInitalPosition = transform.position;
        homingTimer = time;
        homingTimerMax = time;
        doHoming = true;
        homingPrevValue = Vector3.zero;
    }

	void SetupHoming() { // attack homing function
		// NOTE(Roskuski): If homing is currently taking place, cancel it.
		doHoming = false;

		Vector3 targetCapsuleStart = GetTargetCapsuleStartSphere(); // find the ball with all the enemies in it
		Vector3 targetCapsuleEnd = GetTargetCapsuleEndSphere(); // find the ball with all the enemies in it
		Vector3 targetingDirection = targetCapsuleStart - transform.position;
		RaycastHit[] eColliders = Physics.CapsuleCastAll(targetCapsuleStart, targetCapsuleEnd, homingCapsuleRadius, targetingDirection, 0, Mask.Get(Layers.EnemyHurtbox)); // scan for enemies in a cone in front of player

		// homingTargetDelta is the delta (change) in location between the player and what's being homed in on.
		// function below attempts to find the smallest delta (enemy closest to player) and home in on that
		homingTargetDelta = Vector3.forward * 10;                                                   //set to a really high value that anything in the sphere would beat
		int savedEnemy = 0;
        for (int index = 0; index < eColliders.Length; index += 1) {                                // for every collider found...
																									// TODO: try adding weights to both ends of the capsule and see how it feels
			if (eColliders[index].transform.position.y < -5) return;
			Vector3 pointDelta = eColliders[index].transform.position - targetCapsuleStart;      // calculate the delta between player and the enemy collider
			pointDelta.y = 0;
			//var target = eColliders[index].transform.gameObject.GetComponent<Basic>();			// debug ref to collider's enemy script
			if (pointDelta.magnitude < homingTargetDelta.magnitude) {							// if current delta is lower than the previous one...
				savedEnemy = index;
				homingTargetDelta = pointDelta;													// make it the new delta
				//if (target != null && target.debugHoming == true) target.debugTargetTimer = 0.3f;	// (debug) if target is being homed, make them flash
			}
		}
		if (eColliders.Length > 0) {
            //sounds.Sound_CrystalPickup();
            homingTargetDelta = eColliders[savedEnemy].transform.position - transform.position;	// correct the htd to be based on where the player is instead of the capsule start point
        }

        if (homingTargetDelta != Vector3.forward * 10) {											// if homingtargetdelta is not the default value...
			switch (currentAttack) {																// multiply the target delta/distance amount specific to the attack
				case Attacks.None:
				default:
					Debug.Assert(false, "currentAttack == " + currentAttack.ToString());
					break;

				case Attacks.LAttack:
                case Attacks.LAttack3:
                    homingTargetDelta *= 1f;
					break;

				case Attacks.LAttack2:
					homingTargetDelta *= 1.2f;
					break;

				case Attacks.Chop:
                case Attacks.Slam:
                    homingTargetDelta *= 0.2f;
					break;

				case Attacks.HeadThrow:
				case Attacks.ShotgunThrow:
					homingTargetDelta *= 0f;
					break;
			}
		}
		else {																						// if nothing was detected, just go forward using position of the orb
			Vector3 Location = GetTargetCapsuleEndSphere();
			Location = new Vector3(Location.x, transform.position.y, Location.z);
			homingTargetDelta = Quaternion.LookRotation(Location - transform.position, Vector3.up) * Vector3.forward * 2;
		}

		transform.LookAt(homingTargetDelta + transform.position);									// finally, look at the enemy
	}

	Vector3 GetTargetCapsuleStartSphere() {
		if (trueInput == Vector2.zero) {
			return transform.position + transform.rotation * new Vector3(0, 0, homingDistance);
		}
		else {
			return transform.position + Quaternion.LookRotation(new Vector3(trueInput.x, 0, trueInput.y), Vector3.up) * new Vector3(0, 0, homingDistance * 0.95f);
		}
	}

	Vector3 GetTargetCapsuleEndSphere() {
		if (trueInput == Vector2.zero) {
			return transform.position + transform.rotation * new Vector3(0, 0, homingDistance * 2);
		}
		else {
			return transform.position + Quaternion.LookRotation(new Vector3(trueInput.x, 0, trueInput.y), Vector3.up) * new Vector3(0, 0, homingDistance * 1.9f);
		}
	}

	public void LobThrow() { // triggered in animator
		SetupHoming();
        headProj.speed = 50f;
		headProj.canStun = true;
		Instantiate(headProj, projSpawn.position, transform.rotation);
	}

	public void ShotgunThrow() { // triggered in animator
		SetupHoming();
		headProj.speed = 50f;
		headProj.canStun = true;
		//headProj.canPierce = true; disabled for now
		Instantiate(headProj, projSpawn.position, transform.rotation);
		for (int i = 1; i < shotgunProjSpawns.Length; i++) { // i starts at 1 to ignore the parent object
			Instantiate(headProj, shotgunProjSpawns[i].position, shotgunProjSpawns[i].localRotation * transform.rotation);
		}
		// BUG: Spawns an extra proj further behind?
	}

	bool IsAttackState(AnimatorStateInfo stateInfo) {
		// NOTE(Roskuski): If we're NOT any of these states
		return !(
				stateInfo.IsName("Base.Run") ||
				stateInfo.IsName("Base.Idle") ||
				stateInfo.IsName("Base.Hit") ||
				stateInfo.IsName("Base.Death") ||
				stateInfo.IsName("Base.Win")
			);
	}

	public bool CanAffordMove(Attacks attack) {
		if (frenzyTimer > 0) return true;
		else if (meter >= AttackMeterCost[(int)attack] - 0.3f) return true;
		else return false;
	}

	public void Slam() {
		gameMan.SpawnParticle(1, slamPoint.position, 1f);
		Util.SpawnFlash(gameMan, 10, slamPoint.position, false);
		if (GameObject.Find("HapticManager") != null) HapticManager.PlayEffect(hapticEffects[1], this.transform.position);
		gameMan.ShakeCamera(5f, 0.75f);
	}

	public void Shotgun() {
		gameMan.SpawnParticle(0, shotgunPoint.position, 1.5f);
		sounds.CharacterSpin();
		if (GameObject.Find("HapticManager") != null) HapticManager.PlayEffect(hapticEffects[2], this.transform.position);
		gameMan.ShakeCamera(3f, 0.25f);
	}

    void CheckDeathResist() {
        if (health > 3) deathResist = true;
    }

    public void UpdateAxeMesh() {
        for (int i = 0; i < headMesh.Length; i++) {
            if (meter >= i + 1) headMesh[i].enabled = true;
            else headMesh[i].enabled = false;

			if (currentState == States.Death) {
				headMesh[i].enabled = false;
			}
		}
    }
    #endregion

    public void DashFlash() {
		var flash = gameMan.flashes[9];
		Vector3 pos = new Vector3(dashFlashPoint.position.x, dashFlashPoint.position.y + 1.5f, dashFlashPoint.position.z);
		Instantiate(flash, pos, transform.rotation);
    }

	public void FootstepParticles() {
		if (isGrounded) footStepCounter += 1;
		if (footStepCounter >= 2) {
			var puff = gameMan.particles[17];
			Instantiate(puff, footStepParticlePoint.position, transform.rotation);
			footStepCounter = 0;
        }
    }

	public void SmokeParticleSmall() {
		if (isGrounded) {
			var puff = gameMan.particles[17];
			Instantiate(puff, footStepParticlePoint.position, transform.rotation);
		}
    }

	#region Minor utility functions
	void OnEnable() { pActions.Enable(); }
	void OnDisable() { pActions.Disable(); }

	private void OnDrawGizmos() {
		Gizmos.color = Color.red; // sphere base of cone
		Vector3 capsuleStartPosition = GetTargetCapsuleStartSphere();
		Vector3 capsuleEndPosition = GetTargetCapsuleEndSphere();
		float capsuleLength = homingDistance * 2;
        Gizmos.DrawWireSphere(capsuleStartPosition, homingCapsuleRadius);
        Gizmos.DrawWireSphere(capsuleEndPosition, homingCapsuleRadius);

        Gizmos.color = Color.blue; // bounds of the capsule
        Gizmos.DrawLine(new Vector3(capsuleStartPosition.x + capsuleLength, capsuleStartPosition.y, capsuleStartPosition.z), new Vector3(capsuleEndPosition.x + capsuleLength, capsuleEndPosition.y, capsuleEndPosition.z));
        Gizmos.DrawLine(new Vector3(capsuleStartPosition.x - capsuleLength, capsuleStartPosition.y, capsuleStartPosition.z), new Vector3(capsuleEndPosition.x - capsuleLength, capsuleEndPosition.y, capsuleEndPosition.z));
        Gizmos.DrawLine(new Vector3(capsuleStartPosition.x, capsuleStartPosition.y + capsuleLength, capsuleStartPosition.z), new Vector3(capsuleEndPosition.x, capsuleEndPosition.y + capsuleLength, capsuleEndPosition.z));
        Gizmos.DrawLine(new Vector3(capsuleStartPosition.x, capsuleStartPosition.y - capsuleLength, capsuleStartPosition.z), new Vector3(capsuleEndPosition.x, capsuleEndPosition.y - capsuleLength, capsuleEndPosition.z));

        Gizmos.color = Color.yellow; // length + radius of capsule
        Gizmos.DrawLine(capsuleStartPosition, capsuleEndPosition);
        Gizmos.DrawLine(capsuleEndPosition, new Vector3(capsuleEndPosition.x + homingCapsuleRadius, capsuleEndPosition.y, capsuleEndPosition.z));
    }
    #endregion
}


