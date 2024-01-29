using Rainfall;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

public class Player : Entity
{
	public enum MoveType
	{
		Walk,
		Fly,
		Ladder,
	}

	public enum WalkMode
	{
		Normal,
		Walk,
		Sprint,
	}

	class PlayerCollisionCallback : ControllerHitCallback
	{
		Player player;


		public PlayerCollisionCallback(Player player)
		{
			this.player = player;
		}

		public void onShapeHit(ControllerHit hit)
		{
			player.onCollision(hit.position, hit.normal);
		}
	}


	public const float MAX_GROUND_SPEED = 2.8f;
	const float MAX_AIR_SPEED = 0.3f;
	const float LADDER_SPEED = 1.5f;
	const float ACCELERATION = 10.0f;
	const float AIR_ACCELERATION = 10.0f;
	const float FRICTION = 6.0f;
	const float STOP_SPEED = 1.0f;

	const float GRAVITY = -12.0f;
	const float JUMP_HEIGHT = 1.0f;
	const float JUMP_POWER = 4.89898f; // sqrt(2*-gravity*jumpHeight)
	const float JUMP_PEAK_TIME = 0.387f; // jumpPower / gravity
	const float JUMP_BUFFER_TIME = 0.3f;
	const float JUMP_COYOTE_TIME = 0.2f;
	const float JUMP_STAMINA_COST = 4.0f;
	const float JUMP_POWER_LADDER = 2.7f;
	const float DODGE_RELEASE_WINDOW = 0.3f;

	const float SPRINT_SPEED_MULTIPLIER = 1.5f;
	const float WALK_SPEED_MULTIPLIER = 0.5f;
	const float DUCK_SPEED_MULTIPLIER = 0.5f;
	const float DUCK_TRANSITION_DURATION = 0.12f;

	public const float PLAYER_RADIUS = 0.35f;
	const float PLAYER_HEIGHT_STANDING = 1.75f;
	const float PLAYER_HEIGHT_DUCKED = 0.92f;
	public const float STEP_HEIGHT = 0.25f;

	const float CAMERA_HEIGHT_STANDING = 1.6f;
	const float CAMERA_HEIGHT_DUCKED = 1.0f;
	public const float DEFAULT_VIEWMODEL_SCALE = 0.25f;

	const float STEP_FREQUENCY = 0.8f;
	const float FALL_IMPACT_MIN_SPEED = -3.0f;
	const float FALL_DMG_THRESHHOLD = -8.0f;

	const float REACH_DISTANCE = 2.0f;

	const int MAX_ACTION_QUEUE_SIZE = 2;


	/* MOVEMENT VARIABLES */

	public Camera camera;
	public CharacterController controller;
	RigidBody kinematicBody;

	public AudioSource audioMovement, audioAction;
	Sound[] sfxStep;
	Sound sfxJump, sfxLand;
	public Sound sfxExhaust;

	ParticleSystem hitParticles;
	Sound sfxHit;

	bool isCursorLocked = false;

	public MoveType moveType = MoveType.Walk;
	public bool noclip = false;
	public LadderRegion currentLadder = null;

	public Matrix resetPoint;

	public bool isDucked = false;
	public float inDuckTimer = -1.0f;
	public WalkMode walkMode = WalkMode.Normal;

	public bool isGrounded = false;

	float distanceWalked = 0.0f;
	int lastStep = 0;

	long lastJumpInput = 0;

	long lastDodgePressedInput = 0;

	long lastGroundedTime = 0;
	long lastJumpedTime = 0;
	long lastLandedTime = 0;

	public Vector3 velocity;
	float cameraHeight = 0.0f;
	public float pitch = 0.0f, yaw = 0.0f;
	public float viewmodelScale = DEFAULT_VIEWMODEL_SCALE;

	float viewmodelSwayX = 0.0f, viewmodelSwayY = 0.0f;
	float viewmodelSwayPitch = 0.0f, viewmodelSwayYaw = 0.0f;
	Vector2 viewmodelWalkAnim = Vector2.Zero;
	float viewmodelVerticalSpeedAnim = 0.0f;
	float viewmodelLookSwayAnim = 0.0f;
	float cameraSwayY = 0.0f;

	List<Action> actionQueue = new List<Action>();
	Action lastAction = null;


	/* ANIMATION VARIABLES */

	public Model viewmodel { get; private set; }
	Animator moveAnimator;
	Animator animator0, animator1;
	Node rootNode, spine03Node, neckNode, clavicleRNode, clavicleLNode, cameraAnchorNode;
	Node rightItemNode, leftItemNode;
	AnimationState[]
		idleState = new AnimationState[3],
		runState = new AnimationState[3],
		sprintState = new AnimationState[3],
		duckedState = new AnimationState[3],
		duckedWalkState = new AnimationState[3],
		fallDuckedState = new AnimationState[3],
		jumpState = new AnimationState[3],
		fallState = new AnimationState[3],
		actionState1 = new AnimationState[3],
		actionState2 = new AnimationState[3];
	AnimationState[] currentActionState;

	float movementAnimationTimerLooping = 0.0f;

	public ItemEntity[] handEntities = new ItemEntity[2];


	/* INTERACTION VARIABLES */

	public Interactable interactableInFocus = null;

	public long wonTime = long.MaxValue;
	public bool hasWon { get => Time.currentTime > wonTime; }


	/* INVENTORY VARIABLES */

	public Inventory inventory;
	public PlayerStats stats;


	/* UI VARIABLES */

	public HUD hud;
	public readonly InventoryUI inventoryUI;


	public Player(Camera camera, GraphicsDevice graphics)
	{
		this.camera = camera;

		cameraHeight = CAMERA_HEIGHT_STANDING;

		inventory = new Inventory(this);
		stats = new PlayerStats(this);

		hud = new HUD(this, graphics);
		inventoryUI = new InventoryUI(this);

		sfxStep = new Sound[]
		{
			Resource.GetSound("res/entity/player/sfx/step_stone.ogg")
		};
		/*
		sfxStep = new Sound[] {
			Resource.GetSound("res/entity/player/sfx/step_walk1.ogg"),
			Resource.GetSound("res/entity/player/sfx/step_walk2.ogg"),
			Resource.GetSound("res/entity/player/sfx/step_walk3.ogg"),
		};
		*/
		/*
		sfxStep = new Sound[]
		{
			Resource.GetSound("res/entity/player/sfx/step_grass1.ogg"),
			Resource.GetSound("res/entity/player/sfx/step_grass2.ogg"),
		};
		*/
		sfxJump = Resource.GetSound("res/entity/player/sfx/step_jump.ogg");
		sfxLand = Resource.GetSound("res/entity/player/sfx/step_land.ogg");
		sfxExhaust = Resource.GetSound("res/entity/player/sfx/exhaust.ogg");

		hitParticles = new ParticleSystem(1000);
		hitParticles.emissionRate = 0.0f;
		hitParticles.spawnOffset = new Vector3(0.0f, 1.2f, 0.0f);
		hitParticles.spriteTint = new Vector4(0.3f, 0.0f, 0.0f, 1.0f);

		sfxHit = Resource.GetSound("res/entity/player/sfx/hit.ogg");
	}

	public override void init()
	{
		controller = new CharacterController(this, PLAYER_RADIUS, Vector3.Zero, PLAYER_HEIGHT_STANDING, STEP_HEIGHT, (uint)PhysicsFilterGroup.PlayerController, (uint)PhysicsFilterMask.PlayerController, new PlayerCollisionCallback(this));
		kinematicBody = new RigidBody(this, RigidBodyType.Kinematic);
		//kinematicBody.addCapsuleTrigger(PLAYER_RADIUS - 0.2f, PLAYER_HEIGHT_STANDING, new Vector3(0.0f, 0.5f * PLAYER_HEIGHT_STANDING, 0.0f), Quaternion.Identity);
		kinematicBody.addCapsuleCollider(PLAYER_RADIUS - 0.15f, PLAYER_HEIGHT_STANDING, new Vector3(0.0f, 0.5f * PLAYER_HEIGHT_STANDING, 0.0f), Quaternion.Identity, (uint)PhysicsFilterGroup.PlayerControllerKinematicBody, (uint)PhysicsFilterMask.PlayerControllerKinematicBody);

		audioMovement = new AudioSource(position);
		audioAction = new AudioSource(position);

		setCursorLocked(true);

		viewmodel = Resource.GetModel("res/entity/player/viewmodel.gltf");
		viewmodel.isStatic = false;

		rootNode = viewmodel.skeleton.getNode("Root");
		spine03Node = viewmodel.skeleton.getNode("spine_03");
		neckNode = viewmodel.skeleton.getNode("neck_01");
		clavicleRNode = viewmodel.skeleton.getNode("clavicle_r");
		clavicleLNode = viewmodel.skeleton.getNode("clavicle_l");
		cameraAnchorNode = viewmodel.skeleton.getNode("camera_anchor");
		rightItemNode = viewmodel.skeleton.getNode("weapon_r");
		leftItemNode = viewmodel.skeleton.getNode("weapon_l");

		moveAnimator = new Animator(viewmodel);
		animator0 = new Animator(viewmodel);
		animator1 = new Animator(viewmodel);

		for (int i = 0; i < 3; i++)
		{
			idleState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "idle", true) }, 0.5f);
			runState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "run", true) }, 0.2f) { animationSpeed = 1.6f };
			sprintState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "sprint", true) }, 1.0f) { animationSpeed = 1.6f };
			duckedState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "ducked", true) }, 0.2f);
			duckedWalkState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "ducked_walk", true) }, 0.2f) { animationSpeed = 1.6f };
			fallDuckedState[i] = new AnimationState(viewmodel, "idle", true) { transitionFromDuration = 0.0f };
			jumpState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "jump", false) }, 0.1f);
			fallState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "fall", false) }, 0.2f);
			actionState1[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "default", false) }, 0.2f);
			actionState2[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "default", false) }, 0.2f);
		}

		animator0.setState(idleState[0]);
		animator1.setState(idleState[1]);
		moveAnimator.setState(idleState[2]);

		handEntities[0] = new ItemEntity(this, 0);
		handEntities[1] = new ItemEntity(this, 1);

		yaw = rotation.eulers.y;

		giveItem(Item.Get("map"), 1);

		//onItemPickup(Item.Get("axe"), 1);
		//onItemPickup(Item.Get("shortsword"), 1);
		//equipHandItem(1, 0, inventory.findSlot(Item.Get("axe")));
		//giveItem(Item.Get("wooden_round_shield"), 1);
		//giveItem(Item.Get("torch"), 1);
		//giveItem(Item.Get("longbow"), 1);
		//giveItem(Item.Get("firebomb"), 10);

		//giveItem(Item.Get("oak_staff"), 1);
		//giveItem(Item.Get("magic_orb"), 1);
		//giveItem(Item.Get("magic_arrow"), 1);
		//giveItem(Item.Get("homing_orbs"), 1);
		//giveItem(Item.Get("torch"), 1);
		//giveItem(Item.Get("longbow"));
		//giveItem(Item.Get("arrow"), 50);
		//giveItem(Item.Get("flask"), 3);

		//giveItem(Item.Get("leather_chestplate"));
	}

	public override void destroy()
	{
		audioMovement.destroy();
		audioAction.destroy();
	}

	public void hit(int damage, Entity from)
	{
		bool invincible = currentAction != null && currentAction.elapsedTime >= currentAction.iframesStartTime && currentAction.elapsedTime < currentAction.iframesEndTime;
		if (invincible)
			damage = 0;

		bool parry = currentAction != null && currentAction.elapsedTime >= currentAction.parryFramesStartTime && currentAction.elapsedTime < currentAction.parryFramesEndTime;
		if (parry)
		{
			Item shield = null;
			int handID = -1;
			if (currentAction.type == ActionType.Parry)
			{
				ParryAction shieldParryAction = currentAction as ParryAction;
				shield = shieldParryAction.item;
				handID = shieldParryAction.handID;
			}
			else if (currentAction.type == ActionType.ShieldStance)
			{
				ShieldStanceAction blockAction = currentAction as ShieldStanceAction;
				shield = blockAction.item;
				handID = blockAction.handID;
			}

			if (from is Creature)
			{
				Creature creature = from as Creature;
				Debug.Assert(creature.currentAction != null && creature.currentAction.type == MobActionType.Attack);
				creature.cancelAllActions();
				creature.queueAction(new MobStaggerAction(MobActionType.StaggerBlocked));

				if (shield.category == ItemCategory.Weapon)
				{
					cancelAllActions();
					queueAction(new BlockingHitAction(shield, handID, isTwoHanded(handID), true));
				}

				if (shield.sfxParry != null)
					handEntities[handID].audio.playSound(shield.sfxParry);
			}
			damage = 0;
		}

		bool blocking = isBlocking;
		if (blocking && !parry)
		{
			Item shield = null;
			int handID = -1;
			if (currentAction is ShieldStanceAction)
			{
				ShieldStanceAction shieldRaiseAction = currentAction as ShieldStanceAction;
				shield = shieldRaiseAction.item;
				handID = shieldRaiseAction.handID;
			}
			else if (currentAction is BlockingHitAction)
			{
				BlockingHitAction shieldHitAction = currentAction as BlockingHitAction;
				shield = shieldHitAction.item;
				handID = shieldHitAction.handID;
			}
			else
			{
				Debug.Assert(false);
			}

			damage = (int)(damage * (1.0f - shield.blockDamageAbsorption / 100.0f));

			if (stats.stamina >= shield.shieldHitStaminaCost)
			{
				//if (currentAction.type == ActionType.ShieldHit)
				//	cancelAction();
				cancelAllActions();
				queueAction(new BlockingHitAction(shield, handID, isTwoHanded(handID)));

				if (shield.sfxBlock != null)
					handEntities[handID].audio.playSoundOrganic(shield.sfxBlock, 1, 1, 0.1f, 0.1f);

				if (from is Creature && shield.category == ItemCategory.Shield) // Dont stagger if blocking with weapon
				{
					Creature creature = from as Creature;
					creature.cancelAction();
					creature.queueAction(new MobStaggerAction(MobActionType.StaggerBlocked));
				}
			}
			else
			{
				cancelAllActions();
				queueAction(new GuardBreakAction(shield, handID, isTwoHanded(handID)));

				if (shield.sfxGuardBreak != null)
					handEntities[handID].audio.playSoundOrganic(shield.sfxGuardBreak, 1, 1, 0.1f, 0.1f);
			}
		}

		// Armor damage absorption
		{
			float damageMultiplier = 1.0f;
			for (int i = 0; i < inventory.armor.Length; i++)
			{
				ItemSlot armor = inventory.armor[i];
				if (armor.item != null)
				{
					damageMultiplier *= 1.0f - armor.item.blockDamageAbsorption / 100.0f;
				}
			}
			damage = (int)(damage * damageMultiplier);
		}

		if (!blocking && !invincible && !parry)
		{
			Vector3 hitDirection = (position - from.position).normalized;
			hitParticles.randomVelocity = true;
			hitParticles.emitParticle(-hitDirection * 2.0f, 15);

			audioAction.playSoundOrganic(sfxHit, 0.4f);
		}

		if (damage > 0)
		{
			stats.applyDamage(damage);
			if (!blocking)
				hud.onHit();
		}
	}

	Vector3 updateMovementInputs()
	{
		Vector3 fsu = Vector3.Zero;

		{
			if (currentAction == null || !currentAction.lockMovement)
			{
				if (InputManager.IsDown("Left"))
					fsu.x--;
				if (InputManager.IsDown("Right"))
					fsu.x++;
				if (InputManager.IsDown("Back"))
					fsu.z--;
				if (InputManager.IsDown("Forward"))
					fsu.z++;
			}

			{
				if (currentAction != null)
				{
					fsu += currentAction.movementInput;
				}

				if (fsu.lengthSquared > 0.0f)
				{
					fsu = fsu.normalized;
					fsu *= MAX_GROUND_SPEED;

					if (currentAction != null)
					{
						fsu *= currentAction.movementSpeedMultiplier;
					}
				}
			}
		}

		if (currentAction == null || currentAction.movementSpeedMultiplier > 0.0f)
		{
			if (InputManager.IsPressed("Jump"))
			{
				lastJumpInput = Time.currentTime;
			}

			if (InputManager.IsPressed("Dodge"))
				lastDodgePressedInput = Time.currentTime;
			if (InputManager.IsReleased("Dodge"))
			{
				if ((Time.currentTime - lastDodgePressedInput) / 1e9f < DODGE_RELEASE_WINDOW)
				{
					Vector3 dir = fsu;
					if (dir.lengthSquared == 0.0f)
						dir = new Vector3(0.0f, 0.0f, -1.0f);
					queueAction(new DodgeAction(dir));

					isDucked = false;
					inDuckTimer = -1;
				}
			}

			if (InputManager.IsPressed("Crouch"))
			{
				if (inDuckTimer == -1.0f)
				{
					inDuckTimer = 0.0f;
				}
				else
				{
					if (isDucked)
					{
						Span<HitData> hits = stackalloc HitData[16];
						int numHits = Physics.SweepSphere(PLAYER_RADIUS, position + new Vector3(0.0f, PLAYER_HEIGHT_DUCKED - PLAYER_RADIUS, 0.0f), Vector3.Up, PLAYER_HEIGHT_STANDING - PLAYER_HEIGHT_DUCKED, hits, QueryFilterFlags.Static);

						bool headBlocked = false;
						for (int i = 0; i < numHits; i++)
						{
							if (!hits[i].isTrigger)
							{
								headBlocked = true;
								break;
							}
						}

						if (!headBlocked)
						{
							isDucked = false;
							inDuckTimer = -1.0f;
						}
					}

					if (!isDucked)
					{
						inDuckTimer = -1.0f;
					}
				}
			}
			if (inDuckTimer >= 0.0f)
			{
				inDuckTimer += Time.deltaTime;
				if (!isGrounded || inDuckTimer >= DUCK_TRANSITION_DURATION)
				{
					isDucked = true;
				}

				if (InputManager.IsDown("Walk") || InputManager.IsDown("Sprint"))
				{
					isDucked = false;
					inDuckTimer = -1;
				}
			}
			else
			{
				if (fsu.lengthSquared > 0.0f && InputManager.IsDown("Walk"))
				{
					walkMode = WalkMode.Walk;
				}
				else if (fsu.lengthSquared > 0.0f && InputManager.IsDown("Sprint") && stats.canSprint)
				{
					walkMode = WalkMode.Sprint;
				}
				else
				{
					walkMode = WalkMode.Normal;
				}
			}
		}


		if (currentAction != null && currentAction.maxSpeed != 0.0f)
			fsu *= currentAction.maxSpeed / MAX_GROUND_SPEED;
		else if (isGrounded)
		{
			if (isDucked)
			{
				fsu *= DUCK_SPEED_MULTIPLIER;
			}
			else
			{
				switch (walkMode)
				{
					case WalkMode.Walk:
						fsu *= WALK_SPEED_MULTIPLIER;
						break;
					case WalkMode.Sprint:
						fsu *= SPRINT_SPEED_MULTIPLIER;
						break;
					default:
						break;
				}
			}
		}


		if (isCursorLocked && (currentAction == null || !currentAction.lockRotation))
		{
			Vector2 lookVector = InputManager.lookVector;
			yaw -= lookVector.x;
			pitch = Math.Clamp(pitch - lookVector.y, -MathHelper.PiOver2, MathHelper.PiOver2);
		}


		return fsu;
	}

	void onCollision(Vector3 position, Vector3 normal)
	{
		if (Vector3.Dot(velocity, normal) < 0.0f)
		{
			bool groundHit = normal.y > 0.5f && velocity.y < FALL_IMPACT_MIN_SPEED;
			if (groundHit)
			{
				lastLandedTime = Time.currentTime;

				if (sfxLand != null)
					audioMovement.playSoundOrganic(sfxLand);

				if (velocity.y < FALL_DMG_THRESHHOLD)
				{
					float fallDamage = MathF.Pow(FALL_DMG_THRESHHOLD - velocity.y, 2);
					stats.applyDamage(fallDamage);
				}
			}

			// If this is a slope, don't modify velocity to allow for smooth climbing
			if (MathF.Abs(normal.x) > 0.999f || MathF.Abs(normal.y) > 0.999f || MathF.Abs(normal.z) > 0.999f || normal.y < 0.001f)
			{
				float bounceCoefficient = 1.0f;
				Vector3 newVelocity = velocity - bounceCoefficient * Vector3.Dot(velocity, normal) * normal;
				velocity = newVelocity;
			}
		}
	}

	static Vector3 friction(Vector3 velocity, float frametime)
	{
		float entityFriction = 1.0f;
		float edgeFriction = 1.0f;
		float fric = FRICTION * entityFriction * edgeFriction; // sv_friction * ke * ef

		float l = velocity.length;
		Vector3 vn = velocity / l;

		if (l >= STOP_SPEED)
			return (1.0f - frametime * fric) * velocity;
		else if (l >= MathF.Max(0.01f, frametime * STOP_SPEED * fric) && l < STOP_SPEED)
			return velocity - frametime * STOP_SPEED * fric * vn;
		else // if (l < MathHelper.Max(0.1f, frametime * STOP_SPEED * fric)
			return Vector3.Zero;
	}

	static Vector3 updateVelocityGround(Vector3 velocity, Vector3 wishdir, float frametime, float maxSpeed, Vector3 forward, Vector3 right, Vector3 up)
	{
		velocity.y = velocity.y + 0.5f * GRAVITY * Time.deltaTime;

		Vector3 accel = wishdir.x * right + wishdir.y * up + wishdir.z * forward;
		float accelMag = accel.length;
		Vector3 accelDir = accelMag > 0.0f ? accel / accelMag : Vector3.Zero;

		float entityFriction = 1.0f;

		velocity = friction(velocity, frametime);
		float m = MathF.Min(maxSpeed, wishdir.length);
		float currentSpeed = Vector3.Dot(velocity, accelDir);
		float l = m;
		float addSpeed = Math.Clamp(l - currentSpeed, 0.0f, entityFriction * frametime * m * ACCELERATION);

		velocity = velocity + accelDir * addSpeed;

		velocity.y = velocity.y + 0.5f * GRAVITY * Time.deltaTime;

		return velocity;
	}

	Vector3 updateVelocityAir(Vector3 velocity, Vector3 wishdir, float frametime, Vector3 forward, Vector3 right, Vector3 up)
	{
		velocity.y = velocity.y + 0.5f * GRAVITY * Time.deltaTime;

		Vector3 accel = wishdir.x * right + wishdir.y * up + wishdir.z * forward;
		float accelMag = accel.length;
		Vector3 accelDir = accelMag > 0.0f ? accel / accelMag : Vector3.Zero;

		float entityFriction = 1.0f;

		float m = MathF.Min(MAX_GROUND_SPEED, wishdir.length);
		float currentSpeed = Vector3.Dot(velocity, accelDir);
		float l = MathF.Min(m, MAX_AIR_SPEED);
		float addSpeed = Math.Clamp(l - currentSpeed, 0.0f, entityFriction * frametime * m * AIR_ACCELERATION);

		velocity = velocity + accelDir * addSpeed;

		velocity.y = velocity.y + 0.5f * GRAVITY * Time.deltaTime;

		return velocity;
	}

	Vector3 updateVelocityFly(Vector3 velocity, Vector3 wishdir, float frametime, Vector3 forward, Vector3 right, Vector3 up)
	{
		Vector3 accel = wishdir.x * right + wishdir.y * up + wishdir.z * forward;
		float accelMag = accel.length;
		Vector3 accelDir = accelMag > 0.0f ? accel / accelMag : Vector3.Zero;

		float entityFriction = 1.0f;

		float m = MathF.Min(MAX_GROUND_SPEED, wishdir.length);
		float currentSpeed = Vector3.Dot(velocity, accelDir);
		float l = MathF.Min(m, MAX_AIR_SPEED);
		float addSpeed = Math.Clamp(l - currentSpeed, 0.0f, entityFriction * frametime * m * AIR_ACCELERATION);

		velocity = velocity + accelDir * addSpeed;

		return velocity;
	}

	Vector3 updateVelocityLadder(Vector3 velocity, Vector3 wishdir, Vector3 ladderNormal, float frametime, Vector3 forward, Vector3 right, Vector3 up, bool topEdge, bool bottomEdge)
	{
		Vector3 u = wishdir.x * right + wishdir.y * up + wishdir.z * forward;
		Vector3 n = ladderNormal;

		if (topEdge || bottomEdge && Vector3.Dot(u, n) > 0.0f)
		{
			Vector3 cu = Vector3.Cross(Vector3.Up, n);
			velocity = u - Vector3.Dot(u, n) * (Vector3.Cross(n, cu / cu.length));
			velocity *= LADDER_SPEED;
		}
		else
		{
			Vector3 cu = Vector3.Cross(Vector3.Up, n);
			velocity = u - Vector3.Dot(u, n) * (n + Vector3.Cross(n, cu / cu.length));
			velocity *= LADDER_SPEED;
		}

		return velocity;
	}

	void updateMovement(Vector3 fsu)
	{
		Vector3 forward = rotation.forward;
		Vector3 right = rotation.right;
		Vector3 up = rotation.up;


		if (moveType == MoveType.Ladder)
		{
			Debug.Assert(currentLadder != null);

			//if ((Time.currentTime - lastJumpInput) / 1e9f <= JUMP_BUFFER_TIME)
			if (InputManager.IsPressed("Dodge"))
			{
				if (stats.canJump)
				{
					velocity += JUMP_POWER_LADDER * currentLadder.normal;
					lastJumpInput = 0;
					stats.consumeStamina(JUMP_STAMINA_COST);

					lastJumpedTime = Time.currentTime;

					moveType = MoveType.Walk;
					currentLadder = null;

					if (sfxJump != null)
						audioMovement.playSoundOrganic(sfxJump);
				}
			}

			if (moveType == MoveType.Ladder)
			{
				bool topEdge = position.y > currentLadder.position.y + currentLadder.offset.y + currentLadder.halfExtents.y - PLAYER_RADIUS;
				bool bottomEdge = position.y < currentLadder.position.y + currentLadder.offset.y - currentLadder.halfExtents.y + PLAYER_RADIUS;
				velocity = updateVelocityLadder(velocity, fsu, currentLadder.normal, Time.deltaTime, forward, right, up, topEdge, bottomEdge);

				Vector3 displacement = velocity * Time.deltaTime;

				isGrounded = false;
				controller.move(displacement);


				distanceWalked += MathF.Abs(velocity.y) * Time.deltaTime;
				int stepsWalked = (int)(distanceWalked * STEP_FREQUENCY + 0.5f);
				if (stepsWalked > lastStep)
				{
					audioMovement.playSoundOrganic(currentLadder.sfxStep);
					lastStep = stepsWalked;
				}
			}
		}
		if (moveType == MoveType.Fly)
		{
			//velocity = updateVelocityFly(velocity, fsu, Time.deltaTime, forward, right, up);

			Vector3 displacement = Vector3.Zero;

			// Root Motion
			if (currentAction != null && currentAction.rootMotion)
			{
				Vector3 rootMotionDisplacement = currentActionState[2].layers[0].rootMotionDisplacement;
				rootMotionDisplacement = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI + yaw) * rootMotionDisplacement;
				displacement += rootMotionDisplacement;
			}

			if (noclip)
				controller.setPosition(position + displacement);
			else
				controller.move(displacement);

			isGrounded = true;
		}
		if (moveType == MoveType.Walk)
		{
			if ((Time.currentTime - lastJumpInput) / 1e9f <= JUMP_BUFFER_TIME)
			{
				if ((isGrounded || (Time.currentTime - lastGroundedTime) / 1e9f <= JUMP_COYOTE_TIME) && velocity.y < 0.5f * JUMP_POWER && stats.canJump)
				{
					velocity.y = JUMP_POWER;
					isGrounded = false;
					lastJumpInput = 0;
					stats.consumeStamina(JUMP_STAMINA_COST);

					lastJumpedTime = Time.currentTime;

					if (sfxJump != null)
						audioMovement.playSoundOrganic(sfxJump);
				}
			}


			if (isGrounded)
			{
				float currentMaxSpeed = currentAction != null && currentAction.maxSpeed != 0.0f ? currentAction.maxSpeed : walkMode == WalkMode.Sprint ? SPRINT_SPEED_MULTIPLIER * MAX_GROUND_SPEED : MAX_GROUND_SPEED;
				velocity = updateVelocityGround(velocity, fsu, Time.deltaTime, currentMaxSpeed, forward, right, up);
			}
			else
				velocity = updateVelocityAir(velocity, fsu, Time.deltaTime, forward, right, up);


			// Position update
			{
				Vector3 displacement = velocity * Time.deltaTime;

				// Root Motion
				if (isGrounded && currentAction != null && currentAction.rootMotion)
				{
					Vector3 rootMotionDisplacement = currentActionState[2].layers[0].rootMotionDisplacement;
					rootMotionDisplacement = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI + yaw) * rootMotionDisplacement;
					displacement += rootMotionDisplacement;
				}

				ControllerCollisionFlag flags = controller.move(displacement);
				if ((flags & ControllerCollisionFlag.Down) != 0)
				{
					velocity.y = MathF.Max(velocity.y, -2.0f);
				}

				isGrounded = false;
				if (velocity.y < 0.5f)
				{
					Span<HitData> hits = stackalloc HitData[16];
					int numHits = Physics.OverlapSphere(PLAYER_RADIUS, position + new Vector3(0.0f, PLAYER_RADIUS - 0.1f, 0.0f), hits, QueryFilterFlags.Static | QueryFilterFlags.Dynamic);
					for (int i = 0; i < numHits; i++)
					{
						if (!hits[i].isTrigger && hits[i].body != null && hits[i].body.entity != this)
						{
							Debug.Assert(hits[i].controller == null);
							isGrounded = true;
							lastGroundedTime = Time.currentTime;
							break;
						}
					}
				}
			}

			if (isGrounded)
			{
				distanceWalked += velocity.xz.length * Time.deltaTime;
				int stepsWalked = (int)(distanceWalked * STEP_FREQUENCY + 0.5f);
				if (stepsWalked > lastStep)
				{
					if ((walkMode == WalkMode.Normal || walkMode == WalkMode.Sprint) && !isDucked)
					{
						audioMovement.playSoundOrganic(sfxStep, 0.04f);
						AIManager.NotifySound(position, 3.0f);
					}
					lastStep = stepsWalked;
				}
			}
		}


		// adding forward vector to make it easier on the ears
		audioMovement.updateTransform(position + camera.rotation.forward);
		audioAction.updateTransform(position + camera.rotation.forward);
	}

	void updatePhysics()
	{
		if (isDucked && controller.height != PLAYER_HEIGHT_DUCKED)
		{
			controller.resize(PLAYER_HEIGHT_DUCKED);
			if (!isGrounded)
			{
				controller.move(new Vector3(0.0f, PLAYER_HEIGHT_STANDING - PLAYER_HEIGHT_DUCKED, 0.0f));
				cameraHeight = CAMERA_HEIGHT_DUCKED;
			}
		}
		else if (!isDucked && controller.height != PLAYER_HEIGHT_STANDING)
		{
			controller.resize(PLAYER_HEIGHT_STANDING);
			if (!isGrounded)
			{
				controller.move(-new Vector3(0.0f, PLAYER_HEIGHT_STANDING - PLAYER_HEIGHT_DUCKED, 0.0f));
				cameraHeight = CAMERA_HEIGHT_STANDING;
			}
		}

		kinematicBody.setTransform(position, rotation);
	}

	void initializeAction(Action currentAction)
	{
		// Initialize action
		currentAction.startTime = Time.currentTime;

		currentActionState = currentActionState == actionState1 ? actionState2 : currentActionState == actionState2 ? actionState1 : actionState1;

		for (int i = 0; i < 2; i++)
		{
			Item handItem = inventory.getSelectedHandItem(i);

			AnimationData? animationData = null;
			if (currentAction.animationName[i] != null)
			{
				if (currentAction.animationSet[i] != null)
				{
					animationData = currentAction.animationSet[i].getAnimationData(currentAction.animationName[i]);
					currentActionState[i].layers[0].animationData = currentAction.animationSet[i];
				}
				else
				{
					if (animationData == null && handItem != null)
					{
						animationData = handItem.moveset.getAnimationData(currentAction.animationName[i]);
						currentActionState[i].layers[0].animationData = handItem.moveset;
					}
					if (animationData == null)
					{
						animationData = viewmodel.getAnimationData(currentAction.animationName[i]);
						currentActionState[i].layers[0].animationData = viewmodel;
					}
				}
			}
			if (animationData != null)
			{
				currentActionState[i].layers[0].animationName = currentAction.animationName[i];
				currentActionState[i].layers[0].looping = false;
				currentActionState[i].layers[0].mirrored = currentAction.mirrorAnimation;
				currentActionState[i].layers[0].rootMotion = currentAction.rootMotion;
				currentActionState[i].layers[0].rootMotionNode = rootNode;
				currentActionState[i].animationSpeed = currentAction.animationSpeed;
				currentActionState[i].transitionDuration = currentAction.animationTransitionDuration;

				if (currentAction.fullBodyAnimation)
				{
					currentActionState[2].layers[0].animationName = currentActionState[i].layers[0].animationName;
					currentActionState[2].layers[0].animationData = currentActionState[i].layers[0].animationData;
					currentActionState[2].layers[0].looping = currentActionState[i].layers[0].looping;
					currentActionState[2].layers[0].mirrored = currentActionState[i].layers[0].mirrored;
					currentActionState[2].layers[0].rootMotion = currentActionState[i].layers[0].rootMotion;
					currentActionState[2].layers[0].rootMotionNode = currentActionState[i].layers[0].rootMotionNode;
					currentActionState[2].animationSpeed = currentActionState[i].animationSpeed;
					currentActionState[2].transitionDuration = currentActionState[i].transitionDuration;
				}

				if (currentAction.duration == 0.0f)
					currentAction.duration = animationData.Value.duration;
			}
		}

		currentAction.onStarted(this);
	}

	void updateWeaponActions()
	{
		if (currentAction == null)
		{
			if (InputManager.IsPressed("SwitchWeaponRight"))
			{
				inventory.rightHandSlotIdx = (inventory.rightHandSlotIdx + 1) % inventory.rightHand.Length;
				onHandItemUpdate(inventory.getSelectedHandSlot(0), 0);
			}
			if (InputManager.IsPressed("SwitchWeaponLeft"))
			{
				inventory.leftHandSlotIdx = (inventory.leftHandSlotIdx + 1) % inventory.leftHand.Length;
				onHandItemUpdate(inventory.getSelectedHandSlot(1), 1);
			}
		}

		for (int i = 0; i < 2; i++)
		{
			int handID = i;
			ItemSlot handItemSlot = inventory.getSelectedHandSlot(handID);
			Item handItem = inventory.getSelectedHandItem(handID);
			Item otherItem = inventory.getSelectedHandItem(handID ^ 1);
			if (handItem != null && (otherItem == null || !isTwoHanded(handID ^ 1) || isTwoHanded(handID)))
			{
				switch (handItem.category)
				{
					case ItemCategory.Weapon:
						if (handItem.weaponType == WeaponType.Melee)
						{
							if (handID == 0 && InputManager.IsPressed("Action0") ||
								handID == 1 && InputManager.IsPressed("Action1"))
							{
								Attack? attack = handItem.getAttack(AttackType.Light, 0);
								if (InputManager.IsDown("ActionModifier") && handItem.getNumAttacksForType(AttackType.Heavy) > 0)
								{
									Attack? heavyAttack = handItem.getAttack(AttackType.Heavy, 0);
									if (heavyAttack.HasValue)
										attack = heavyAttack.Value;
								}
								else if (currentAction != null && currentAction.type == ActionType.BlockingHit && ((BlockingHitAction)currentAction).parry)
								{
									Attack? riposteAttack = handItem.getAttack(AttackType.Riposte, 0);
									if (riposteAttack.HasValue)
									{
										attack = riposteAttack.Value;
										cancelAction();
									}
								}
								if (currentAction != null && currentAction.type == ActionType.Attack)
								{
									AttackAction attackAction = (AttackAction)currentAction;
									if (/*attackAction.handID == handID &&*/
										(attackAction.attack.type == AttackType.Light || attackAction.attack.type == AttackType.Heavy))
									{
										attack = handItem.getNextAttack(attackAction.attack, attack.Value.type);
									}
								}

								if (attack != null && (attack.Value.staminaCost == 0.0f || stats.stamina > 0.0f))
								{
									queueAction(new AttackAction(handItem, handID, attack.Value, isTwoHanded(handID)));
								}
							}

							if (currentAction == null && (
								handID == 0 && InputManager.IsDown("Action1") && (inventory.getSelectedHandItem(1) == null || !inventory.getSelectedHandItem(1).hasPrimaryAction) ||
								handID == 1 && InputManager.IsDown("Action0") && (inventory.getSelectedHandItem(0) == null || !inventory.getSelectedHandItem(0).hasPrimaryAction)
							))
							{
								queueAction(new ShieldStanceAction(handItem, handID, isTwoHanded(handID)));

								/*
								if (InputManager.IsDown("ActionModifier"))
								{
									if (handID == 0 && InputManager.IsPressed("Action1") ||
										handID == 1 && InputManager.IsPressed("Action0"))
										queueAction(new ParryAction(handItem, handID, true));
								}
								else
								{
									queueAction(new ShieldRaiseAction(handItem, handID, true));
								}
								*/
							}
							if (currentAction != null)
							{
								if (currentAction.type == ActionType.ShieldStance)
								{
									ShieldStanceAction shieldRaiseAction = currentAction as ShieldStanceAction;
									if (shieldRaiseAction.handID == handID && shieldRaiseAction.handID == 0 && !InputManager.IsDown("Action1") ||
										shieldRaiseAction.handID == handID && shieldRaiseAction.handID == 1 && !InputManager.IsDown("Action0") ||
										actionQueue.Count > 1)
									{
										cancelAction();
									}
								}
							}
						}
						else if (handItem.weaponType == WeaponType.Bow)
						{
							if (handID == 0 && InputManager.IsPressed("Action0") ||
								handID == 1 && InputManager.IsPressed("Action1"))
							{
								if (inventory.totalArrowCount > 0 && (currentAction == null || currentAction.type != ActionType.BowDraw))
									queueAction(new BowDrawAction(handItem, handID));
							}
							if (handID == 0 && !InputManager.IsDown("Action0") ||
								handID == 1 && !InputManager.IsDown("Action1"))
							{
								if (currentAction != null && currentAction.type == ActionType.BowDraw && currentAction.elapsedTime >= currentAction.followUpCancelTime)
								{
									cancelAction();
									queueAction(new BowShootAction(handItem, handID));
								}
							}
							if ((handID == 0 && InputManager.IsPressed("Action1") || handID == 1 && InputManager.IsPressed("Action0")) &&
								currentAction != null && currentAction.type == ActionType.BowDraw)
							{
								cancelAction();
							}
						}
						else if (handItem.weaponType == WeaponType.Staff)
						{
							if (handID == 0 && InputManager.IsPressed("Action0") ||
								handID == 1 && InputManager.IsPressed("Action1"))
							{
								ItemSlot spell = inventory.getSpellSlot(handItemSlot);
								if (spell != null /*&& spell.numCharges > 0*/)
								{
									queueAction(new SpellCastAction(handItem, spell.item, handID));
								}
							}
						}

						/*
						if (handID == 0 && handItem != null && !handItem.twoHanded || handID == 1 && otherItem != null && !otherItem.twoHanded && handItem == null)
						{
							if (InputManager.IsPressed("WieldTwoHanded"))
							{
								if (!isTwoHanded(handID))
									queueAction(new ItemWieldTwoHandAction(inventory.getSelectedHandItem(handID), handID, this));
								else
								{
									queueAction(new WeaponDrawAction(inventory.getSelectedHandItem(handID ^ 1), handID ^ 1));
									inventory.twoHandedWeapon = -1;
									updateMovesetLayer(inventory.getSelectedHandItem(handID ^ 1), handID ^ 1);
								}
							}
						}
						*/

						break;
					case ItemCategory.Shield:
						if (InputManager.IsDown("ActionModifier") &&
								(handID == 0 && InputManager.IsPressed("Action0") ||
								handID == 1 && InputManager.IsPressed("Action1")))
						{
							queueAction(new ParryAction(handItem, handID, false));
						}
						else if (currentAction == null && (
							handID == 0 && InputManager.IsDown("Action0") ||
							handID == 1 && InputManager.IsDown("Action1")
						))
						{
							queueAction(new ShieldStanceAction(handItem, handID, false));
						}
						if (currentAction != null)
						{
							if (currentAction.type == ActionType.ShieldStance)
							{
								ShieldStanceAction shieldRaiseAction = currentAction as ShieldStanceAction;
								if (shieldRaiseAction.handID == handID && shieldRaiseAction.handID == 0 && !InputManager.IsDown("Action0") ||
									shieldRaiseAction.handID == handID && shieldRaiseAction.handID == 1 && !InputManager.IsDown("Action1") ||
									actionQueue.Count > 1)
								{
									cancelAction();
								}
							}
						}
						break;
					case ItemCategory.Utility:
						/*
						if (currentAction == null && (
							handID == 0 && InputManager.IsDown("Action0") ||
							handID == 1 && InputManager.IsDown("Action1")
						))
						{
							if (InputManager.IsDown("ActionModifier") &&
								(handID == 0 && InputManager.IsPressed("Action0") ||
								handID == 1 && InputManager.IsPressed("Action1")))
							{
								//queueAction(new ParryAction(handItem, handID, false));
							}
							else
							{
								queueAction(new TorchRaiseAction(handItem, handID, false));
							}
						}
						if (currentAction != null)
						{
							if (currentAction.type == ActionType.TorchRaise)
							{
								TorchRaiseAction torchRaiseAction = currentAction as TorchRaiseAction;
								if (torchRaiseAction.handID == handID && torchRaiseAction.handID == 0 && !InputManager.IsDown("Action0") ||
									torchRaiseAction.handID == handID && torchRaiseAction.handID == 1 && !InputManager.IsDown("Action1") ||
									actionQueue.Count > 1)
								{
									cancelAction();
								}
							}
						}
						*/
						break;
					case ItemCategory.Consumable:
						if (handID == 0 && InputManager.IsPressed("Action0") ||
							handID == 1 && InputManager.IsPressed("Action1"))
						{
							if (handItemSlot.stackSize > 0)
							{
								bool followUp = currentAction != null && currentAction.type == ActionType.ConsumableUse && ((ConsumableUseAction)currentAction).item == handItemSlot.item;
								queueAction(new ConsumableUseAction(handItemSlot, handID, followUp));
							}
						}
						break;
					case ItemCategory.Artifact:
						break;
					default:
						Debug.Assert(false);
						break;
				}
			}
			else if (handItem == null && (otherItem == null || !isTwoHanded(handID ^ 1)))
			{
				Item fistItem = Item.Get("default");
				bool twoHanded = otherItem == null;
				if (twoHanded)
				{
					if (handID == 0 && InputManager.IsPressed("Action0"))
					{
						AttackType attackType = AttackType.Light;
						if (InputManager.IsDown("ActionModifier") && fistItem.getNumAttacksForType(AttackType.Heavy) > 0)
							attackType = AttackType.Heavy;
						Attack attack = fistItem.getAttack(attackType, 0).Value;
						int attackHand = 0;
						if (currentAction != null && currentAction.type == ActionType.Attack)
						{
							AttackAction attackAction = (AttackAction)currentAction;
							if (attackAction.item == fistItem)
							{
								attackHand = attackAction.handID ^ 1;
							}
						}

						if (attack.staminaCost == 0.0f || stats.stamina > 0.0f)
						{
							queueAction(new AttackAction(fistItem, attackHand, attack, false));
						}
					}
					else if (currentAction == null && handID == 1)
					{
						if (InputManager.IsDown("Action1"))
						{
							/*
							if (InputManager.IsDown("ActionModifier"))
							{
								if (InputManager.IsPressed("Action1"))
									queueAction(new ShieldParryAction(fistItem, handID, true));
							}
							else
							*/
							{
								queueAction(new ShieldStanceAction(fistItem, handID, true));
							}
						}
					}
					else if (currentAction != null && handID == 1)
					{
						if (currentAction.type == ActionType.ShieldStance)
						{
							ShieldStanceAction shieldRaiseAction = currentAction as ShieldStanceAction;
							if (shieldRaiseAction.handID == handID && shieldRaiseAction.handID == 1 && !InputManager.IsDown("Action1") ||
								actionQueue.Count > 1)
							{
								cancelAction();
							}
						}
					}
				}
				else
				{
					if (handID == 0 && InputManager.IsPressed("Action0") && (inventory.getSelectedHandItem(1) != null && !inventory.getSelectedHandItem(1).hasSecondaryAction) ||
						handID == 1 && InputManager.IsPressed("Action1") && (inventory.getSelectedHandItem(0) != null && !inventory.getSelectedHandItem(0).hasSecondaryAction))
					{
						AttackType attackType = AttackType.Light;
						if (InputManager.IsDown("ActionModifier") && fistItem.getNumAttacksForType(AttackType.Heavy) > 0)
							attackType = AttackType.Heavy;
						Attack attack = fistItem.getAttack(attackType, 0).Value;

						if (attack.staminaCost == 0.0f || stats.stamina > 0.0f)
						{
							queueAction(new AttackAction(fistItem, handID, attack, false));
						}
					}
				}
			}
		}
	}

	void updateQuickSlots()
	{
		{
			if (InputManager.IsPressed("SwitchHotbarItem"))
			{
				int nextSlotWithItem = -1;
				for (int i = 1; i < inventory.hotbar.Length; i++)
				{
					int idx = (inventory.quickSlotIdx + i) % inventory.hotbar.Length;
					if (inventory.hotbar[idx].item != null)
						nextSlotWithItem = idx;
				}
				if (nextSlotWithItem != -1)
					inventory.quickSlotIdx = nextSlotWithItem;
			}
		}

		ItemSlot quickSlot = inventory.getCurrentQuickSlot();
		if (quickSlot.item != null)
		{
			Item quickSlotItem = quickSlot.item;
			switch (quickSlotItem.category)
			{
				case ItemCategory.Consumable:
					if (InputManager.IsPressed("Use"))
					{
						if (quickSlot.stackSize > 0)
						{
							bool followUp = currentAction != null && currentAction.type == ActionType.ConsumableUse && ((ConsumableUseAction)currentAction).item == quickSlot.item;
							queueAction(new ConsumableUseAction(quickSlot, 1, followUp));
						}
					}
					break;
				default:
					Debug.Assert(false);
					break;
			}
		}
	}

	void updateCombat()
	{
		updateWeaponActions();
		updateQuickSlots();
	}

	void updateInteractions()
	{
		interactableInFocus = null;

		if (currentAction == null)
		{
			float closestDistance = float.MaxValue;

			Span<HitData> hits = stackalloc HitData[16];
			int numHits = Physics.SweepSphere(0.1f, camera.position, camera.rotation.forward, REACH_DISTANCE, hits, QueryFilterFlags.Default | QueryFilterFlags.NoBlock);
			for (int i = 0; i < numHits; i++)
			{
				RigidBody body = hits[i].body;
				if (body != null && (body.filterGroup & (uint)PhysicsFilterGroup.Interactable) != 0 && body.entity is Interactable)
				{
					Interactable interactable = (Interactable)body.entity;
					if (interactable.canInteract(this))
					{
						if (hits[i].distance < closestDistance)
						{
							interactableInFocus = interactable;
							closestDistance = hits[i].distance;
						}
					}
				}
			}

			if (interactableInFocus != null)
			{
				if (InputManager.IsPressed("Interact"))
				{
					interactableInFocus.interact(this);
				}
			}

			// Dropping item
			{
				if (InputManager.IsPressed("Drop"))
				{
					if (InputManager.IsDown("ActionModifier"))
					{
						if (inventory.getSelectedHandSlot(1) != null)
						{
							dropItem(1);
							queueAction(new ItemThrowAction(1));
						}
					}
					else
					{
						if (inventory.getSelectedHandSlot(0) != null)
						{
							dropItem(0);
							queueAction(new ItemThrowAction(0));
						}
					}
				}
			}
		}
	}

	void updateActions()
	{
		if (isCursorLocked)
		{
			updateCombat();
			updateInteractions();
		}

		// Actions
		if (actionQueue.Count > 0)
		{
			Action currentAction = actionQueue[0];
			if (currentAction.hasStarted)
			{
				bool actionShouldFinish = currentAction.hasFinished ||
					(currentAction.elapsedTime >= currentAction.followUpCancelTime && actionQueue.Count > 1 && currentAction.type == actionQueue[1].type);
				if (actionShouldFinish)
				{
					currentAction.onFinished(this);
					lastAction = currentAction;
					actionQueue.RemoveAt(0);
					currentAction = actionQueue.Count > 0 ? actionQueue[0] : null;
				}
			}

			if (currentAction != null)
			{
				if (!currentAction.hasStarted)
				{
					initializeAction(currentAction);
				}

				currentAction.update(this);
			}
		}
	}

	void updateAnimations()
	{
		float movementSpeed = velocity.xz.length / MAX_GROUND_SPEED; // (MAX_GROUND_SPEED * (isDucked ? DUCK_SPEED_MULTIPLIER : 1.0f));

		AnimationState movementState0, movementState1, movementState2;

		if (isGrounded)
		{
			/*
			if (isDucked)
			{
				if (currentSpeed > 0.5f)
				{
					movementState0 = duckedWalkState[0];
					movementState1 = duckedWalkState[1];
					movementState2 = duckedWalkState[2];
				}
				else
				{
					movementState0 = duckedState[0];
					movementState1 = duckedState[1];
					movementState2 = duckedState[2];
				}
			}
			else
			*/
			{
				if (movementSpeed > SPRINT_SPEED_MULTIPLIER - 0.1f && currentAction == null)
				{
					movementState0 = sprintState[0];
					movementState0.animationSpeed = movementSpeed;

					movementState1 = sprintState[1];
					movementState1.animationSpeed = movementSpeed;

					movementState2 = runState[2];
					runState[2].animationSpeed = movementSpeed;
				}
				else if (movementSpeed > 0.25f && currentAction == null)
				{
					movementState0 = runState[0];
					runState[0].animationSpeed = movementSpeed;

					movementState1 = runState[1];
					runState[1].animationSpeed = movementSpeed;

					movementState2 = runState[2];
					runState[2].animationSpeed = movementSpeed;
				}
				else
				{
					movementState0 = idleState[0];
					movementState1 = idleState[1];
					movementState2 = idleState[2];
				}
			}
		}
		else
		{
			/*
			if (isDucked)
			{
				movementState0 = fallDuckedState[0];
				movementState1 = fallDuckedState[1];
				movementState2 = fallDuckedState[2];
			}
			else
			*/
			{

				if (velocity.y > 0.0f)
				{
					//movementState0 = jumpState[0];
					//movementState1 = jumpState[1];
					//movementState2 = jumpState[2];
					movementState0 = idleState[0];
					movementState1 = idleState[1];
					movementState2 = idleState[2];
				}
				else
				{
					//movementState0 = fallState[0];
					//movementState1 = fallState[1];
					//movementState2 = fallState[2];
					movementState0 = idleState[0];
					movementState1 = idleState[1];
					movementState2 = idleState[2];
				}
			}
		}


		// Testing procedural viewmodel animations
		//movementState0 = idleState[0];
		//movementState1 = idleState[1];


		//movementAnimationTimerLooping += Time.deltaTime * movementState2.animationSpeed;
		movementAnimationTimerLooping = distanceWalked * STEP_FREQUENCY * 0.5f;

		if (currentAction != null)
		{
			currentActionState[0].animationSpeed = currentAction.animationSpeed;
			currentActionState[1].animationSpeed = currentAction.animationSpeed;
			currentActionState[2].animationSpeed = currentAction.animationSpeed;

			if (currentAction.animationName[0] != null)
			{
				if (currentAction.startTime == Time.currentTime)
					animator0.setState(currentActionState[0]);
				else
					animator0.setStateIfNot(currentActionState[0]);
				animator0.timer = currentAction.elapsedTime;
			}
			else
			{
				animator0.setStateIfNot(movementState0);
				animator0.setTimer(movementAnimationTimerLooping);
			}
			if (currentAction.animationName[1] != null)
			{
				if (currentAction.startTime == Time.currentTime)
					animator1.setState(currentActionState[1]);
				else
					animator1.setStateIfNot(currentActionState[1]);
				animator1.timer = currentAction.elapsedTime;
			}
			else
			{
				animator1.setStateIfNot(movementState1);
				animator1.setTimer(movementAnimationTimerLooping);
			}

			if (currentAction.fullBodyAnimation)
			{
				if (currentAction.startTime == Time.currentTime)
				{
					moveAnimator.setState(currentActionState[2]);
					moveAnimator.setTimer(movementAnimationTimerLooping);
				}
				else
				{
					moveAnimator.setStateIfNot(currentActionState[2]);
					moveAnimator.setTimer(movementAnimationTimerLooping);
				}
				moveAnimator.timer = currentAction.elapsedTime;
			}
			else
			{
				moveAnimator.setStateIfNot(movementState2);
				moveAnimator.setTimer(movementAnimationTimerLooping);
			}
		}
		else
		{
			animator0.setStateIfNot(movementState0);
			animator1.setStateIfNot(movementState1);
			moveAnimator.setStateIfNot(movementState2);

			if (movementState0 != idleState[0]) animator0.setTimer(movementAnimationTimerLooping);
			if (movementState1 != idleState[1]) animator1.setTimer(movementAnimationTimerLooping);
			if (movementState2 != idleState[2]) moveAnimator.setTimer(movementAnimationTimerLooping);
		}


		animator0.update();
		animator1.update();
		moveAnimator.update();


		for (int i = 0; i < viewmodel.skeleton.nodes.Length; i++)
		{
			bool isArmBone = StringUtils.StartsWith(viewmodel.skeleton.nodes[i].name, "clavicle") ||
				StringUtils.StartsWith(viewmodel.skeleton.nodes[i].name, "upperarm") ||
				StringUtils.StartsWith(viewmodel.skeleton.nodes[i].name, "lowerarm") ||
				StringUtils.StartsWith(viewmodel.skeleton.nodes[i].name, "hand") ||
				StringUtils.StartsWith(viewmodel.skeleton.nodes[i].name, "thumb") ||
				StringUtils.StartsWith(viewmodel.skeleton.nodes[i].name, "index") ||
				StringUtils.StartsWith(viewmodel.skeleton.nodes[i].name, "middle") ||
				StringUtils.StartsWith(viewmodel.skeleton.nodes[i].name, "ring") ||
				StringUtils.StartsWith(viewmodel.skeleton.nodes[i].name, "pinky") ||
				StringUtils.StartsWith(viewmodel.skeleton.nodes[i].name, "weapon");

			if (isArmBone && StringUtils.EndsWith(viewmodel.skeleton.nodes[i].name, "_r"))
				moveAnimator.nodeLocalTransforms[i] = animator0.nodeLocalTransforms[i];
			if (isArmBone && StringUtils.EndsWith(viewmodel.skeleton.nodes[i].name, "_l"))
				moveAnimator.nodeLocalTransforms[i] = animator1.nodeLocalTransforms[i];
		}


		// Procedural viewmodel animations
		{
			viewmodelSwayX = 0.0f;
			viewmodelSwayY = 0.0f;
			viewmodelSwayPitch = 0.0f;
			viewmodelSwayYaw = 0.0f;
			cameraSwayY = 0.0f;

			// Walk animation
			/*
			viewmodelWalkAnim.x = 0.03f * MathF.Sin(distanceWalked * STEP_FREQUENCY * MathF.PI);
			viewmodelWalkAnim.y = 0.015f * -MathF.Abs(MathF.Cos(distanceWalked * STEP_FREQUENCY * MathF.PI));
			viewmodelWalkAnim *= currentSpeed;
			viewmodelSwayYaw += viewmodelWalkAnim.x;
			viewmodelSwayY += viewmodelWalkAnim.y;
			*/

			// Vertical speed animation
			float verticalSpeedAnimDst = velocity.y;
			verticalSpeedAnimDst = Math.Clamp(verticalSpeedAnimDst, -5.0f, 5.0f);
			viewmodelVerticalSpeedAnim = MathHelper.Lerp(viewmodelVerticalSpeedAnim, verticalSpeedAnimDst * 0.02f, 5.0f * Time.deltaTime);
			viewmodelSwayPitch += viewmodelVerticalSpeedAnim;

			// Jump bob animation
			float timeSinceJump = (Time.currentTime - lastJumpedTime) / 1e9f * 2.0f;
			float jumpBob = (1.0f - MathF.Pow(0.5f, timeSinceJump)) * 1.0f * MathF.Pow(0.1f, timeSinceJump);
			//viewmodelSwayY += jumpBob;

			// Land bob animation
			float timeSinceLanding = (Time.currentTime - lastLandedTime) / 1e9f;
			float landBob = (1.0f - MathF.Pow(0.5f, timeSinceLanding * 4.0f)) * 1.0f * MathF.Pow(0.1f, timeSinceLanding * 4.0f);
			viewmodelSwayY -= landBob;

			// Land camera animation
			float landCameraBob = (1.0f - MathF.Pow(0.5f, timeSinceLanding * 2.0f)) * 4.0f * MathF.Pow(0.1f, timeSinceLanding * 2.0f);
			landCameraBob = MathF.Max(landCameraBob, 0.0f);
			cameraSwayY -= landCameraBob;

			// Look sway
			float swayYawDst = -0.5f * InputManager.lookVector.x;
			viewmodelLookSwayAnim = MathHelper.Lerp(viewmodelLookSwayAnim, swayYawDst, 5.0f * Time.deltaTime);
			viewmodelSwayYaw += viewmodelLookSwayAnim;


			float fastFOV = MathF.Min(MathHelper.ToDegrees(MathF.Atan2(1 + (movementSpeed - 1) * 0.5f, 1.0f) * 2), 100);
			float cameraTargetFOV = movementSpeed > 1.25f ? fastFOV : 90;
			camera.fov = MathHelper.Lerp(camera.fov, cameraTargetFOV, 3 * Time.deltaTime);
		}


		Matrix neckTransform = moveAnimator.getNodeLocalTransform(neckNode);
		Matrix viewmodelTransform = neckTransform
			* Matrix.CreateTranslation(viewmodelSwayX, viewmodelSwayY, 0.0f)
			* Matrix.CreateRotation(Vector3.Up, viewmodelSwayYaw)
			* Matrix.CreateRotation(Vector3.Right, -pitch * 0.5f + viewmodelSwayPitch)
			* neckTransform.inverted;
		{
			Vector3 spineNodePosition = neckTransform.translation;
			Quaternion spineNodeRotation = neckTransform.rotation;
			spineNodeRotation = Quaternion.FromAxisAngle(Vector3.UnitX, -pitch * 0.5f) * spineNodeRotation;
			Matrix newNeckTransform = Matrix.CreateTranslation(spineNodePosition) * Matrix.CreateRotation(spineNodeRotation);
			moveAnimator.setNodeLocalTransform(neckNode, newNeckTransform);
		}
		{
			Item rightItem = getHandItem(0);
			float pitchFactor = rightItem != null ? rightItem.pitchFactor : 1.0f;

			Matrix viewmodelTransform_ = neckTransform
			* Matrix.CreateTranslation(viewmodelSwayX, viewmodelSwayY, 0.0f)
			* Matrix.CreateRotation(Vector3.Up, viewmodelSwayYaw)
			* Matrix.CreateRotation(Vector3.Right, -(-pitch * 0.5f + pitch * pitchFactor) + viewmodelSwayPitch)
			* neckTransform.inverted;

			Matrix spineNodeTransform = moveAnimator.getNodeLocalTransform(clavicleRNode);
			//Vector3 spineNodePosition = spineNodeTransform.translation;
			//Quaternion spineNodeRotation = spineNodeTransform.rotation;
			//spineNodePosition += viewmodelOffset;
			//spineNodeRotation = Quaternion.FromAxisAngle(Vector3.UnitX, -pitch * 0.5f) * spineNodeRotation;
			//spineNodeTransform = Matrix.CreateTranslation(spineNodePosition) * Matrix.CreateRotation(spineNodeRotation);
			spineNodeTransform = viewmodelTransform_ * spineNodeTransform;
			moveAnimator.setNodeLocalTransform(clavicleRNode, spineNodeTransform);
		}
		{
			Item leftItem = getHandItem(1);
			float pitchFactor = leftItem != null ? leftItem.pitchFactor : 1.0f;

			Matrix viewmodelTransform_ = neckTransform
			* Matrix.CreateTranslation(viewmodelSwayX, viewmodelSwayY, 0.0f)
			* Matrix.CreateRotation(Vector3.Up, viewmodelSwayYaw)
			* Matrix.CreateRotation(Vector3.Right, -(-pitch * 0.5f + pitch * pitchFactor) + viewmodelSwayPitch)
			* neckTransform.inverted;

			Matrix spineNodeTransform = moveAnimator.getNodeLocalTransform(clavicleLNode);
			//Vector3 spineNodePosition = spineNodeTransform.translation;
			//Quaternion spineNodeRotation = spineNodeTransform.rotation;
			//spineNodePosition += viewmodelOffset;
			//spineNodeRotation = Quaternion.FromAxisAngle(Vector3.UnitX, -pitch * 0.5f) * spineNodeRotation;
			//spineNodeTransform = Matrix.CreateTranslation(spineNodePosition) * Matrix.CreateRotation(spineNodeRotation);
			spineNodeTransform = viewmodelTransform_ * spineNodeTransform;
			moveAnimator.setNodeLocalTransform(clavicleLNode, spineNodeTransform);
		}
		{
			Matrix spineNodeTransform = moveAnimator.getNodeLocalTransform(spine03Node);
			Matrix spineNodeGlobalTransform = moveAnimator.getNodeTransform(spine03Node, 0);
			Vector3 spineNodePosition = spineNodeTransform.translation;
			Quaternion spineNodeRotation = spineNodeTransform.rotation;
			spineNodePosition += spineNodeGlobalTransform.rotation.conjugated * new Vector3(0.0f, cameraSwayY + (cameraHeight - CAMERA_HEIGHT_STANDING), 0.0f);
			spineNodeRotation = Quaternion.FromAxisAngle(Vector3.UnitX, -pitch * 0.5f) * spineNodeRotation;
			spineNodeTransform = Matrix.CreateTranslation(spineNodePosition) * Matrix.CreateRotation(spineNodeRotation);
			moveAnimator.setNodeLocalTransform(spine03Node, spineNodeTransform);
		}


		moveAnimator.applyAnimation();
	}

	void updateCamera()
	{
		if (isGrounded)
		{
			if (inDuckTimer >= 0.0f)
			{
				if (cameraHeight > CAMERA_HEIGHT_DUCKED)
					cameraHeight = MathF.Max(cameraHeight - (CAMERA_HEIGHT_STANDING - CAMERA_HEIGHT_DUCKED) / DUCK_TRANSITION_DURATION * Time.deltaTime, CAMERA_HEIGHT_DUCKED);
			}
			else
			{
				if (cameraHeight < CAMERA_HEIGHT_STANDING)
					cameraHeight = MathF.Min(cameraHeight + (CAMERA_HEIGHT_STANDING - CAMERA_HEIGHT_DUCKED) / DUCK_TRANSITION_DURATION * Time.deltaTime, CAMERA_HEIGHT_STANDING);
			}
		}
		/*
		else
		{
			if (isDucked)
			{
				cameraHeight = CAMERA_HEIGHT_DUCKED;
				//if (cameraHeight > CAMERA_HEIGHT_DUCKED)
				//	cameraHeight = MathF.Max(cameraHeight - (CAMERA_HEIGHT_STANDING - CAMERA_HEIGHT_DUCKED) / DUCK_TRANSITION_DURATION * Time.deltaTime, CAMERA_HEIGHT_DUCKED);
			}
			else
			{
				cameraHeight = CAMERA_HEIGHT_STANDING;
				//if (cameraHeight < CAMERA_HEIGHT_STANDING)
				//	cameraHeight = MathF.Min(cameraHeight + (CAMERA_HEIGHT_STANDING - CAMERA_HEIGHT_DUCKED) / DUCK_TRANSITION_DURATION * Time.deltaTime, CAMERA_HEIGHT_STANDING);
			}
		}
		*/

		{
			bool animateCameraRotation = currentAction != null ? currentAction.animateCameraRotation : false;

			Matrix cameraAnchorTransform = Matrix.CreateRotation(Vector3.Up, MathF.PI) * moveAnimator.getNodeTransform(cameraAnchorNode, 0) * Matrix.CreateRotation(Vector3.Up, MathF.PI) * Matrix.CreateRotation(Vector3.Right, MathHelper.PiOver2);
			Matrix cameraTransform = Matrix.CreateTranslation(position) * Matrix.CreateRotation(Vector3.Up, yaw) * cameraAnchorTransform;
			camera.position = cameraTransform.translation; // + new Vector3(0.0f, cameraHeight, 0.0f);
			camera.rotation = animateCameraRotation ? cameraTransform.rotation : Quaternion.FromAxisAngle(Vector3.UnitY, yaw) * Quaternion.FromAxisAngle(Vector3.UnitX, pitch);
		}

		rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw);



	}

	public override void update()
	{
		base.update();

		Vector3 fsu = updateMovementInputs();
		updateMovement(fsu);
		updatePhysics();
		updateActions();
		updateAnimations();
		updateCamera();

		Matrix scaleTowardsCamera = Matrix.CreateTranslation(camera.position) * Matrix.CreateScale(viewmodelScale) * Matrix.CreateTranslation(-camera.position);
		handEntities[0].setTransform(getWeaponTransform(0), scaleTowardsCamera);
		handEntities[1].setTransform(getWeaponTransform(1), scaleTowardsCamera);

		handEntities[0].update();
		handEntities[1].update();

		hitParticles.transform = getModelMatrix();
		hitParticles.update();

		stats.update();

		inventoryUI.update();
	}

	public Matrix getWeaponTransform(int handID)
	{
		Node itemNode = handID == 0 ? rightItemNode : leftItemNode;
		Matrix transform = getModelMatrix()
			* Matrix.CreateRotation(Quaternion.FromAxisAngle(Vector3.Up, MathF.PI))
			* moveAnimator.getNodeTransform(itemNode, 0);
		return transform;
	}

	Item getHandItem(int handID)
	{
		if (currentAction != null)
		{
			if (currentAction.overrideHandModels[handID])
				return currentAction.handItemModels[handID];
		}

		Item item = inventory.getSelectedHandItem(handID);
		Item otherItem = inventory.getSelectedHandItem(handID ^ 1);

		if (item != null)
		{
			if ((otherItem == null || !isTwoHanded(handID ^ 1)) || isTwoHanded(handID) && otherItem != null && isTwoHanded(handID ^ 1) && handID == 0)
			{
				return item;
			}
			else if (!isTwoHanded(handID) && otherItem != null && isTwoHanded(handID ^ 1) || isTwoHanded(handID) && otherItem != null && isTwoHanded(handID ^ 1) && handID == 1)
			{
				return otherItem;
			}
			else
			{
				Debug.Assert(!isTwoHanded(handID) && (otherItem == null || !isTwoHanded(handID ^ 1)));

				return item;
			}
		}
		else
		{
			if (otherItem != null && isTwoHanded(handID ^ 1))
			{
				return otherItem;
			}
			else
			{
				return null;
			}
		}
	}

	bool isTwoHanded(int handID)
	{
		return inventory.getSelectedHandItem(handID) != null && inventory.getSelectedHandItem(handID).twoHanded || inventory.twoHandedWeapon == handID;
	}

	public void updateMovesetLayer(Item item, int handID)
	{
		Item otherItem = inventory.getSelectedHandItem(handID ^ 1);

		if (item != null)
		{
			if ((otherItem == null || !isTwoHanded(handID ^ 1)) || isTwoHanded(handID) && otherItem != null && isTwoHanded(handID ^ 1) && handID == 0)
			{
				idleState[handID].layers[0].animationData = item.moveset;
				runState[handID].layers[0].animationData = item.moveset;
				sprintState[handID].layers[0].animationData = item.moveset;
				duckedState[handID].layers[0].animationData = item.moveset;
				duckedWalkState[handID].layers[0].animationData = item.moveset;
				jumpState[handID].layers[0].animationData = item.moveset;
				fallState[handID].layers[0].animationData = item.moveset;

				idleState[handID].layers[0].mirrored = handID == 1;
				runState[handID].layers[0].mirrored = handID == 1;
				sprintState[handID].layers[0].mirrored = handID == 1;
				duckedState[handID].layers[0].mirrored = handID == 1;
				duckedWalkState[handID].layers[0].mirrored = handID == 1;
				jumpState[handID].layers[0].mirrored = handID == 1;
				fallState[handID].layers[0].mirrored = handID == 1;

				runState[handID].layers[0].timerOffset = handID == 1 ? 21 / 24.0f * 0.5f : 0.0f;
				sprintState[handID].layers[0].timerOffset = handID == 1 ? 21 / 24.0f * 0.5f : 0.0f;
				duckedWalkState[handID].layers[0].timerOffset = handID == 1 ? 21 / 24.0f * 0.5f : 0.0f;

				handEntities[handID].setItem(item);
			}
			else if (!isTwoHanded(handID) && otherItem != null && isTwoHanded(handID ^ 1) || isTwoHanded(handID) && otherItem != null && isTwoHanded(handID ^ 1) && handID == 1)
			{
				idleState[handID].layers[0].animationData = otherItem.moveset;
				runState[handID].layers[0].animationData = otherItem.moveset;
				sprintState[handID].layers[0].animationData = otherItem.moveset;
				duckedState[handID].layers[0].animationData = otherItem.moveset;
				duckedWalkState[handID].layers[0].animationData = otherItem.moveset;
				jumpState[handID].layers[0].animationData = otherItem.moveset;
				fallState[handID].layers[0].animationData = otherItem.moveset;

				idleState[handID].layers[0].mirrored = handID != 1;
				runState[handID].layers[0].mirrored = handID != 1;
				sprintState[handID].layers[0].mirrored = handID != 1;
				duckedState[handID].layers[0].mirrored = handID != 1;
				duckedWalkState[handID].layers[0].mirrored = handID != 1;
				jumpState[handID].layers[0].mirrored = handID != 1;
				fallState[handID].layers[0].mirrored = handID != 1;

				runState[handID].layers[0].timerOffset = handID != 1 ? 21 / 24.0f * 0.5f : 0.0f;
				sprintState[handID].layers[0].timerOffset = handID != 1 ? 21 / 24.0f * 0.5f : 0.0f;
				duckedWalkState[handID].layers[0].timerOffset = handID != 1 ? 21 / 24.0f * 0.5f : 0.0f;

				handEntities[handID].setItem(null);
			}
			else
			{
				Debug.Assert(!isTwoHanded(handID) && (otherItem == null || !isTwoHanded(handID ^ 1)));

				idleState[handID].layers[0].animationData = item.moveset;
				runState[handID].layers[0].animationData = item.moveset;
				sprintState[handID].layers[0].animationData = item.moveset;
				duckedState[handID].layers[0].animationData = item.moveset;
				duckedWalkState[handID].layers[0].animationData = item.moveset;
				jumpState[handID].layers[0].animationData = item.moveset;
				fallState[handID].layers[0].animationData = item.moveset;

				idleState[handID].layers[0].mirrored = handID == 1;
				runState[handID].layers[0].mirrored = handID == 1;
				sprintState[handID].layers[0].mirrored = handID == 1;
				duckedState[handID].layers[0].mirrored = handID == 1;
				duckedWalkState[handID].layers[0].mirrored = handID == 1;
				jumpState[handID].layers[0].mirrored = handID == 1;
				fallState[handID].layers[0].mirrored = handID == 1;

				runState[handID].layers[0].timerOffset = handID == 1 ? 21 / 24.0f * 0.5f : 0.0f;
				sprintState[handID].layers[0].timerOffset = handID == 1 ? 21 / 24.0f * 0.5f : 0.0f;
				duckedWalkState[handID].layers[0].timerOffset = handID == 1 ? 21 / 24.0f * 0.5f : 0.0f;

				handEntities[handID].setItem(item);
			}
		}
		else
		{
			if (otherItem != null && isTwoHanded(handID ^ 1))
			{
				idleState[handID].layers[0].animationData = otherItem.moveset;
				runState[handID].layers[0].animationData = otherItem.moveset;
				sprintState[handID].layers[0].animationData = otherItem.moveset;
				duckedState[handID].layers[0].animationData = otherItem.moveset;
				duckedWalkState[handID].layers[0].animationData = otherItem.moveset;
				jumpState[handID].layers[0].animationData = otherItem.moveset;
				fallState[handID].layers[0].animationData = otherItem.moveset;

				idleState[handID].layers[0].mirrored = handID != 1;
				runState[handID].layers[0].mirrored = handID != 1;
				sprintState[handID].layers[0].mirrored = handID != 1;
				duckedState[handID].layers[0].mirrored = handID != 1;
				duckedWalkState[handID].layers[0].mirrored = handID != 1;
				jumpState[handID].layers[0].mirrored = handID != 1;
				fallState[handID].layers[0].mirrored = handID != 1;

				runState[handID].layers[0].timerOffset = handID != 1 ? 21 / 24.0f * 0.5f : 0.0f;
				duckedWalkState[handID].layers[0].timerOffset = handID != 1 ? 21 / 24.0f * 0.5f : 0.0f;

				handEntities[handID].setItem(null);
			}
			else
			{
				idleState[handID].layers[0].animationData = viewmodel;
				runState[handID].layers[0].animationData = viewmodel;
				sprintState[handID].layers[0].animationData = viewmodel;
				duckedState[handID].layers[0].animationData = viewmodel;
				duckedWalkState[handID].layers[0].animationData = viewmodel;
				jumpState[handID].layers[0].animationData = viewmodel;
				fallState[handID].layers[0].animationData = viewmodel;

				idleState[handID].layers[0].mirrored = false;
				runState[handID].layers[0].mirrored = false;
				sprintState[handID].layers[0].mirrored = false;
				duckedState[handID].layers[0].mirrored = false;
				duckedWalkState[handID].layers[0].mirrored = false;
				jumpState[handID].layers[0].mirrored = false;
				fallState[handID].layers[0].mirrored = false;

				runState[handID].layers[0].timerOffset = 0.0f;
				sprintState[handID].layers[0].timerOffset = 0.0f;
				duckedWalkState[handID].layers[0].timerOffset = 0.0f;

				handEntities[handID].setItem(null);
			}
		}

		bool hasSprintAnim = sprintState[handID].layers[0].animationData.getAnimationData("sprint") != null;
		sprintState[handID].layers[0].animationName = hasSprintAnim ? "sprint" : "run";
	}

	public void throwItem(Item item, int amount)
	{
		ItemPickup pickup = new ItemPickup(item, amount);
		Vector3 startPosition = camera.position + camera.rotation.forward * 0.4f;
		startPosition.y = MathF.Max(startPosition.y, position.y + 0.2f); // prevent item from being thrown into the ground when crouching
		Quaternion startRotation = Quaternion.FromAxisAngle(new Vector3(1.0f).normalized, MathHelper.RandomFloat(0.0f, MathF.PI * 2.0f));
		DungeonGame.instance.level.addEntity(pickup, startPosition, startRotation);
		float throwPower = 4.0f;
		pickup.body.setVelocity(camera.rotation.forward * throwPower);
		pickup.body.setRotationVelocity(new Vector3(MathHelper.RandomFloat(-3.0f, 3.0f), MathHelper.RandomFloat(-3.0f, 3.0f), MathHelper.RandomFloat(-3.0f, 3.0f)));
	}

	void dropItem(int handID)
	{
		if (inventory.getSelectedHandItem(handID) != null)
		{
			ItemSlot slot = inventory.getSelectedHandSlot(handID);
			throwItem(slot.item, 1);
			inventory.removeItem(slot);

			updateMovesetLayer(inventory.getSelectedHandItem(0), 0);
			updateMovesetLayer(inventory.getSelectedHandItem(1), 1);
		}
	}

	public void onHandItemUpdate(ItemSlot slot, int handID)
	{
		updateMovesetLayer(inventory.getSelectedHandItem(0), 0);
		updateMovesetLayer(inventory.getSelectedHandItem(1), 1);

		if (inventory.getSelectedHandSlot(handID) == slot)
		{
			if (slot.item != null)
			{
				if (slot.item.moveset.getAnimationData("draw") != null)
				{
					if (currentAction is WeaponDrawAction)
						cancelAction();
					queueAction(new WeaponDrawAction(slot.item, handID));
				}
				else
				{
					audioAction.playSoundOrganic(slot.item.sfxDraw);
				}
			}
			else
			{
				if (currentAction is WeaponDrawAction)
					cancelAction();
				queueAction(new WeaponDrawAction(null, handID));
			}
		}
	}

	public void equipSpells(ItemSlot staff, ItemSlot[] spells)
	{
		SpellSet spellSet = new SpellSet();
		SpellSlot[] slots = new SpellSlot[spells.Length];
		spellSet.slots = slots;
		spellSet.selectedSlot = 0;

		for (int i = 0; i < spells.Length; i++)
		{
			SpellSlot spellSlot = new SpellSlot();
			spellSlot.spell = spells[i];
			spellSlot.numCharges = 10;
			slots[i] = spellSlot;
		}

		inventory.spellSlots.Add(staff, spellSet);
	}

	public void giveItem(Item item, int amount = 1, Item[] equippedSpells = null)
	{
		ItemSlot slot;
		bool firstOfType = inventory.findItemOfType(item.category) == null;

		if (item.category == ItemCategory.Weapon)
		{
			//if (handEntities[0].item != null)
			//	dropItem(0);
			//ItemSlot slot = inventory.addHandItem(0, 0, item, 1);
			slot = inventory.addItem(item, amount);
			if (equippedSpells != null)
			{
				ItemSlot[] spellSlots = new ItemSlot[equippedSpells.Length];
				for (int i = 0; i < equippedSpells.Length; i++)
				{
					ItemSlot spellSlot = inventory.addItem(equippedSpells[i]);
					spellSlots[i] = spellSlot;
				}
				equipSpells(slot, spellSlots);
			}
		}
		else if (item.category == ItemCategory.Shield || item.category == ItemCategory.Utility)
		{
			//if (handEntities[1].item != null)
			//	dropItem(1);
			//inventory.addHandItem(1, 0, item, amount);
			slot = inventory.addItem(item, amount);
		}
		else if (item.category == ItemCategory.Consumable)
		{
			//inventory.addHotbarItem(0, item, amount);
			slot = inventory.addHotbarItem(item, amount);
			if (slot == null)
				slot = inventory.addItem(item, amount);
		}
		else
		{
			slot = inventory.addItem(item, amount);
		}

		if (false) //if (firstOfType)
		{
			ItemSlot newSlot = null;
			if (item.category == ItemCategory.Weapon)
				newSlot = inventory.addHandItem(0, item, amount);
			else if (item.category == ItemCategory.Shield)
				newSlot = inventory.addHandItem(1, item, amount);
			else if (item.category == ItemCategory.Utility)
				newSlot = inventory.addHandItem(1, item, amount);
			else if (item.category == ItemCategory.Consumable)
				newSlot = inventory.addHotbarItem(item, amount);

			if (newSlot != null)
				inventory.removeItem(slot, amount);
		}

		hud.onItemCollected(item, amount, Time.currentTime);

		//queueAction(new PickUpAction());
	}

	public void queueAction(Action action)
	{
		bool enoughStamina = action.staminaCost == 0.0f || stats.canDoAction;
		if (enoughStamina)
		{
			actionQueue.Add(action);
			actionQueue.Sort((Action a, Action b) => { return a.priority < b.priority ? 1 : a.priority > b.priority ? -1 : 0; });

			if (actionQueue.Count > MAX_ACTION_QUEUE_SIZE)
				actionQueue.RemoveRange(MAX_ACTION_QUEUE_SIZE, actionQueue.Count - MAX_ACTION_QUEUE_SIZE);
			if (actionQueue[0] == action)
				initializeAction(actionQueue[0]);
		}
	}

	public void insertAction(Action action, int index)
	{
		bool enoughStamina = action.staminaCost == 0.0f || stats.canDoAction;
		if (enoughStamina)
		{
			actionQueue.Insert(index, action);
			actionQueue.Sort((Action a, Action b) => { return a.priority < b.priority ? -1 : a.priority > b.priority ? 1 : 0; });

			if (actionQueue.Count > MAX_ACTION_QUEUE_SIZE)
				actionQueue.RemoveRange(MAX_ACTION_QUEUE_SIZE, actionQueue.Count - MAX_ACTION_QUEUE_SIZE);
			if (actionQueue[0] == action)
				initializeAction(actionQueue[0]);
		}
	}

	public void cancelAction()
	{
		Debug.Assert(actionQueue.Count > 0);
		currentAction.onFinished(this);
		lastAction = currentAction;
		actionQueue.RemoveAt(0);
	}

	public void cancelAllActions()
	{
		while (actionQueue.Count > 0)
		{
			if (actionQueue[0].hasStarted)
				currentAction.onFinished(this);
			lastAction = currentAction;
			actionQueue.RemoveAt(0);
		}
	}

	public new void setPosition(Vector3 position)
	{
		controller.setPosition(position);
	}

	public new void setRotation(Quaternion rotation)
	{
		this.rotation = rotation;
		yaw = rotation.angle;
	}

	public void setRotation(float yaw)
	{
		this.yaw = yaw;
		rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw);
	}

	public void moveTo(Vector3 position)
	{
		controller.move(position - this.position);
	}

	public void setCursorLocked(bool locked)
	{
		isCursorLocked = locked;
		Input.mouseLocked = locked;
	}

	public override void draw(GraphicsDevice graphics)
	{
		/*
		{ // DEBUG MODEL
			Renderer.DrawModel(viewmodel, Matrix.CreateTranslation(0.0f, 0.0f, -3.0f) * Matrix.CreateRotation(Vector3.Up, MathF.PI));
			if (inventory.getSelectedHandItem(0) != null)
				Renderer.DrawModel(inventory.getSelectedHandItem(0).model, Matrix.CreateTranslation(0.0f, 0.0f, -3.0f) * Matrix.CreateRotation(Vector3.Up, MathF.PI) * moveAnimator.getNodeTransform(rightItemNode, 0) * Matrix.CreateRotation(Vector3.UnitX, MathF.PI));
			if (inventory.getSelectedHandItem(1) != null)
				Renderer.DrawModel(inventory.getSelectedHandItem(1).model, Matrix.CreateTranslation(0.0f, 0.0f, -3.0f) * Matrix.CreateRotation(Vector3.Up, MathF.PI) * moveAnimator.getNodeTransform(leftItemNode, 0) * Matrix.CreateRotation(Vector3.UnitX, MathF.PI));
		}
		*/


		Matrix scaleTowardsCamera = Matrix.CreateTranslation(camera.position) * Matrix.CreateScale(viewmodelScale) * Matrix.CreateTranslation(-camera.position);
		Matrix transform = scaleTowardsCamera * getModelMatrix() * Matrix.CreateRotation(Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));
		Renderer.DrawModel(viewmodel, transform, moveAnimator);

		// Camera light
		if (inventory.hasItemInOffhand(Item.Get("torch")))
			Renderer.DrawLight(camera.position, new Vector3(2.7738395f, 0.9894696f, 0.25998735f) * 2.0f);
		else
			Renderer.DrawLight(camera.position, new Vector3(2.7738395f, 0.9894696f, 0.25998735f) * 0.2f + 1.0f);
		//Renderer.DrawLight(camera.position, new Vector3(1.0f) * 0.2f);

		handEntities[0].draw(graphics);
		handEntities[1].draw(graphics);

		hud.draw(graphics);
		inventoryUI.draw(graphics);
	}

	public Action currentAction
	{
		get => actionQueue.Count > 0 ? actionQueue[0] : null;
	}

	public Vector3 lookOrigin
	{
		get => camera.position;
	}

	public Vector3 lookDirection
	{
		get => camera.rotation.forward;
	}

	public Quaternion lookRotation
	{
		get => camera.rotation;
	}

	public bool isAlive
	{
		get => stats.health > 0;
	}

	public bool isBlocking
	{
		get
		{
			if (currentAction != null)
			{
				if (currentAction.type == ActionType.BlockingHit)
					return true;
				else if (currentAction.type == ActionType.ShieldStance)
				{
					ShieldStanceAction shieldStance = currentAction as ShieldStanceAction;
					return shieldStance.isBlocking;
				}
			}
			return false;
		}
	}
}
