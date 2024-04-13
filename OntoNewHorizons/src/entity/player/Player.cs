using Rainfall;
using System.Reflection.Metadata;


internal class Player : Entity
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
		Ducked,
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


	const float MAX_GROUND_SPEED = 2.4f;
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

	const float SPRINT_SPEED_MULTIPLIER = 1.5f;
	const float WALK_SPEED_MULTIPLIER = 0.7f;
	const float DUCK_SPEED_MULTIPLIER = 0.5f;
	const float DUCK_TRANSITION_DURATION = 0.12f;

	public const float PLAYER_RADIUS = 0.35f;
	const float PLAYER_HEIGHT_STANDING = 1.75f - 2 * PLAYER_RADIUS;
	const float PLAYER_HEIGHT_DUCKED = 0.92f - 2 * PLAYER_RADIUS;
	public const float STEP_HEIGHT = 0.25f;

	const float CAMERA_HEIGHT_STANDING = 1.6f;
	const float CAMERA_HEIGHT_DUCKED = 1.0f;

	const float STEP_FREQUENCY = 0.8f;
	const float FALL_IMPACT_MIN_SPEED = -3.0f;
	const float FALL_DMG_THRESHHOLD = -8.0f;

	const float REACH_DISTANCE = 2.0f;

	const int MAX_ACTION_QUEUE_SIZE = 2;


	/* MOVEMENT VARIABLES */

	public Camera camera;
	public CharacterController controller;
	RigidBody detectionBody;

	public AudioSource audioMovement, audioAction;
	Sound[] sfxStep;
	Sound sfxJump, sfxLand;

	ParticleSystem hitParticles;
	Sound sfxHit;

	bool isCursorLocked = false;

	public MoveType moveType = MoveType.Walk;
	public bool noclip = false;
	public Ladder currentLadder = null;

	public Vector3 resetPoint;

	public bool isDucked = false;
	public float inDuckTimer = -1.0f;
	public WalkMode walkMode = WalkMode.Normal;

	public bool isGrounded = false;

	float distanceWalked = 0.0f;
	int lastStep = 0;

	long lastJumpInput = 0;

	long lastGroundedTime = 0;
	long lastJumpedTime = 0;
	long lastLandedTime = 0;

	public Vector3 velocity;
	float cameraHeight = 0.0f;
	public float pitch = 0.0f, yaw = 0.0f;
	public float viewmodelScale = 0.1f;

	float viewmodelSwayX = 0.0f, viewmodelSwayY = 0.0f;
	float viewmodelSwayPitch = 0.0f, viewmodelSwayYaw = 0.0f;
	Vector2 viewmodelWalkAnim = Vector2.Zero;
	float viewmodelVerticalSpeedAnim = 0.0f;
	float viewmodelLookSwayAnim = 0.0f;
	float cameraSwayY = 0.0f;

	List<Action> actionQueue = new List<Action>();


	/* ANIMATION VARIABLES */

	Model viewmodel;
	Animator moveAnimator;
	Animator animator0, animator1;
	Node rootNode, spine03Node, neckNode, clavicleRNode, clavicleLNode, cameraAnchorNode;
	Node rightItemNode, leftItemNode;
	AnimationState[]
		idleState = new AnimationState[3],
		runState = new AnimationState[3],
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


	/* INVENTORY VARIABLES */

	public Inventory inventory;
	public PlayerStats stats;


	/* UI VARIABLES */

	public HUD hud;
	InventoryUI inventoryUI;


	public Player(Camera camera)
	{
		this.camera = camera;

		cameraHeight = CAMERA_HEIGHT_STANDING;

		inventory = new Inventory();
		stats = new PlayerStats(this);

		hud = new HUD(this);
		inventoryUI = new InventoryUI(this);

		sfxStep = new Sound[] {
			Resource.GetSound("res/entity/player/sfx/step_walk1.ogg"),
			Resource.GetSound("res/entity/player/sfx/step_walk2.ogg"),
			Resource.GetSound("res/entity/player/sfx/step_walk3.ogg"),
		};
		/*
		sfxStep = new Sound[]
		{
			Resource.GetSound("res/entity/player/sfx/step_grass1.ogg"),
			Resource.GetSound("res/entity/player/sfx/step_grass2.ogg"),
		};
		*/
		sfxJump = Resource.GetSound("res/entity/player/sfx/step_jump.ogg");
		sfxLand = Resource.GetSound("res/entity/player/sfx/step_land.ogg");

		hitParticles = new ParticleSystem(1000);
		hitParticles.emissionRate = 0.0f;
		hitParticles.spawnOffset = new Vector3(0.0f, 1.2f, 0.0f);
		hitParticles.spriteTint = 0xff550000;

		sfxHit = Resource.GetSound("res/entity/player/sfx/hit.ogg");
	}

	public override void init()
	{
		yaw = rotation.eulers.y;

		controller = new CharacterController(this, PLAYER_RADIUS, Vector3.Zero, PLAYER_HEIGHT_STANDING, STEP_HEIGHT, new PlayerCollisionCallback(this));
		detectionBody = new RigidBody(this, RigidBodyType.Kinematic);
		detectionBody.addCapsuleTrigger(PLAYER_RADIUS - 0.2f, PLAYER_HEIGHT_STANDING, new Vector3(0.0f, PLAYER_RADIUS + 0.5f * PLAYER_HEIGHT_STANDING, 0.0f), Quaternion.Identity);

		audioMovement = Audio.CreateSource(position);
		audioAction = Audio.CreateSource(position);

		setCursorLocked(true);

		viewmodel = Resource.GetModel("res/entity/player/viewmodel.gltf");
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
			idleState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "idle", true) }, 0.2f);
			runState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "run", true) }, 0.2f) { animationSpeed = 1.6f };
			duckedState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "ducked", true) }, 0.2f);
			duckedWalkState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "ducked_walk", true) }, 0.2f) { animationSpeed = 1.6f };
			fallDuckedState[i] = new AnimationState(viewmodel, "idle", true, 0.0f) { transitionFromDuration = 0.0f };
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

		onItemPickup(Item.Get("axe"), 1);
	}

	public void hit(int damage, Entity from)
	{
		bool blocking = currentAction != null && (currentAction.type == ActionType.ShieldRaise || currentAction.type == ActionType.ShieldHit);
		if (blocking)
		{
			Item shield = null;
			int handID = -1;
			if (currentAction is ShieldRaiseAction)
			{
				ShieldRaiseAction shieldRaiseAction = currentAction as ShieldRaiseAction;
				shield = shieldRaiseAction.shield;
				handID = shieldRaiseAction.handID;
			}
			else if (currentAction is ShieldHitAction)
			{
				ShieldHitAction shieldHitAction = currentAction as ShieldHitAction;
				shield = shieldHitAction.shield;
				handID = shieldHitAction.handID;
			}
			else
			{
				Debug.Assert(false);
			}

			damage = (int)(damage * (1.0f - shield.shieldDamageAbsorption / 100.0f));
			cancelAllActions();
			queueAction(new ShieldHitAction(shield, handID));

			if (shield.sfxHit != null)
				handEntities[handID].audio.playSoundOrganic(shield.sfxHit);

			if (from is Creature)
			{
				Creature creature = from as Creature;
				creature.cancelAction();
				creature.queueAction(new MobStaggerAction(MobActionType.StaggerBlocked));
			}
		}
		else
		{
			Vector3 hitDirection = (position - from.position).normalized;
			hitParticles.randomVelocity = true;
			hitParticles.emitParticle(-hitDirection * 2.0f, 15);

			audioAction.playSoundOrganic(sfxHit, 0.4f);
		}

		if (damage > 0)
		{
			stats.applyDamage(damage);
			hud.onHit();
		}
	}

	Vector3 updateMovementInputs()
	{
		Vector3 fsu = Vector3.Zero;

		{
			if (InputManager.IsDown("Left"))
				fsu.x--;
			if (InputManager.IsDown("Right"))
				fsu.x++;
			if (InputManager.IsDown("Back"))
				fsu.z--;
			if (InputManager.IsDown("Forward"))
				fsu.z++;

			if (fsu.lengthSquared > 0.0f)
			{
				fsu = fsu.normalized;
				fsu *= MAX_GROUND_SPEED;
			}
		}

		if (InputManager.IsPressed("Jump"))
		{
			lastJumpInput = Time.currentTime;
		}

		if (InputManager.IsDown("Crouch"))
		{
			if (inDuckTimer == -1.0f)
				inDuckTimer = 0.0f;
			if (inDuckTimer >= 0.0f)
				inDuckTimer += Time.deltaTime;
			if (!isGrounded || inDuckTimer >= DUCK_TRANSITION_DURATION)
			{
				isDucked = true;
			}
		}
		else
		{
			if (isDucked)
			{
				Span<SweepHit> hits = stackalloc SweepHit[16];
				int numHits = Physics.SweepSphere(PLAYER_RADIUS, position + new Vector3(0.0f, PLAYER_RADIUS + PLAYER_HEIGHT_DUCKED, 0.0f), Vector3.Up, PLAYER_HEIGHT_STANDING - PLAYER_HEIGHT_DUCKED, hits, QueryFilterFlags.Static);

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

		if (isGrounded)
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
					float damage = (FALL_DMG_THRESHHOLD - velocity.y) * 5;
					//stats.applyDamage(damage);
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

	Vector3 updateVelocityLadder(Vector3 velocity, Vector3 wishdir, Vector3 ladderNormal, float frametime, Vector3 forward, Vector3 right, Vector3 up)
	{
		Vector3 u = wishdir.x * right + wishdir.y * up + wishdir.z * forward;
		Vector3 n = ladderNormal;

		Vector3 cu = Vector3.Cross(Vector3.Up, n);
		velocity = u - Vector3.Dot(u, n) * (n + Vector3.Cross(n, cu / cu.length));
		velocity *= LADDER_SPEED;

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

			if ((Time.currentTime - lastJumpInput) / 1e9f <= JUMP_BUFFER_TIME)
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
				if (currentAction != null)
					fsu *= currentAction.movementSpeedMultiplier;


				velocity = updateVelocityLadder(velocity, fsu, currentLadder.normal, Time.deltaTime, forward, right, up);

				Vector3 displacement = velocity * Time.deltaTime;

				isGrounded = false;
				controller.move(displacement);


				distanceWalked += MathF.Abs(velocity.y) * Time.deltaTime;
				int stepsWalked = (int)(distanceWalked * STEP_FREQUENCY);
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

			if (currentAction != null)
				fsu *= currentAction.movementSpeedMultiplier;


			if (isGrounded)
				velocity = updateVelocityGround(velocity, fsu, Time.deltaTime, walkMode == WalkMode.Sprint ? SPRINT_SPEED_MULTIPLIER * MAX_GROUND_SPEED : MAX_GROUND_SPEED, forward, right, up);
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
				if (flags.HasFlag(ControllerCollisionFlag.Down))
				{
					velocity.y = MathF.Max(velocity.y, -2.0f);
				}

				isGrounded = false;
				if (velocity.y < 0.5f)
				{
					Span<OverlapHit> hits = stackalloc OverlapHit[16];
					int numHits = Physics.OverlapSphere(PLAYER_RADIUS, position + new Vector3(0.0f, PLAYER_RADIUS - 0.1f, 0.0f), hits, QueryFilterFlags.Static | QueryFilterFlags.Dynamic);
					for (int i = 0; i < numHits; i++)
					{
						if (!hits[i].isTrigger && hits[i].body != null)
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
				int stepsWalked = (int)(distanceWalked * STEP_FREQUENCY);
				if (stepsWalked > lastStep)
				{
					if ((walkMode == WalkMode.Normal || walkMode == WalkMode.Sprint) && !isDucked)
					{
						audioMovement.playSoundOrganic(sfxStep, 0.5f);
						lastStep = stepsWalked;
					}
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

		detectionBody.setTransform(position, rotation);
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
					currentAction.duration = animationData.Value.duration / currentAction.animationSpeed;
			}
		}

		currentAction.onStarted(this);
	}

	void updateActions()
	{
		// Action inputs
		if (isCursorLocked)
		{
			// Hand items
			for (int i = 0; i < 2; i++)
			{
				int handID = i;
				ItemSlot handItemSlot = inventory.getSelectedHandSlot(handID);
				Item handItem = inventory.getSelectedHandItem(handID);
				Item otherItem = inventory.getSelectedHandItem(handID ^ 1);
				if (handItem != null && (otherItem == null || !otherItem.twoHanded || handItem.twoHanded))
				{
					switch (handItem.category)
					{
						case ItemCategory.Weapon:
							if (handID == 0 && InputManager.IsPressed("Action0") ||
								handID == 1 && InputManager.IsPressed("Action1"))
							{
								if (handItem.weaponType == WeaponType.Melee)
								{
									Attack attack = handItem.getAttack(AttackType.Light, 0).Value;
									if (currentAction != null && currentAction.type == ActionType.Attack)
									{
										AttackAction attackAction = (AttackAction)currentAction;
										if (attackAction.handID == handID)
										{
											attack = handItem.getNextAttack(attackAction.attack);
										}
									}

									if (attack.staminaCost == 0.0f || stats.stamina > 0.0f)
									{
										queueAction(new AttackAction(handItem, handID, attack));
									}
								}
								else if (handItem.weaponType == WeaponType.Bow)
								{
									// TODO uncomment, only for demo
									//if (inventory.arrows.Count > 0)
									queueAction(new BowDrawAction(handItem, handID));
								}
								else if (handItem.weaponType == WeaponType.Staff)
								{
									SpellSlot spell = inventory.getSpellSlot(handItemSlot);
									if (spell != null && spell.numCharges > 0)
									{
										queueAction(new SpellCastAction(handItem, spell.spell.item, handID));
									}
								}
							}
							if (handID == 0 && !InputManager.IsDown("Action0") ||
								handID == 1 && !InputManager.IsDown("Action1"))
							{
								if (handItem.weaponType == WeaponType.Bow)
								{
									if (currentAction != null && currentAction.type == ActionType.BowDraw && currentAction.elapsedTime >= currentAction.followUpCancelTime)
									{
										cancelAction();
										queueAction(new BowShootAction(handItem, handID));
									}
								}
							}
							if ((handID == 0 && InputManager.IsPressed("Action1") || handID == 1 && InputManager.IsPressed("Action0")) && handItem.weaponType == WeaponType.Bow && currentAction != null && currentAction.type == ActionType.BowDraw)
							{
								cancelAction();
							}
							break;
						case ItemCategory.Shield:
							if (currentAction == null && (
								handID == 0 && InputManager.IsDown("Action0") ||
								handID == 1 && InputManager.IsDown("Action1")
								))
							{
								queueAction(new ShieldRaiseAction(handItem, handID));
							}
							if (currentAction != null)
							{
								if (currentAction.type == ActionType.ShieldRaise)
								{
									ShieldRaiseAction shieldRaiseAction = currentAction as ShieldRaiseAction;
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
							//itemUseAction = new UtilityUseAction(handItem);
							break;
						case ItemCategory.Consumable:
							if (handID == 0 && InputManager.IsPressed("Action0") ||
								handID == 1 && InputManager.IsPressed("Action1"))
							{
								if (handItemSlot.stackSize > 0)
									queueAction(new ConsumableUseAction(handItemSlot, handID));
							}
							break;
						default:
							Debug.Assert(false);
							break;
					}
				}
			}

			// Quick Slot items
			int quickSlotItemID = inventory.quickSlots[0];
			if (quickSlotItemID != -1)
			{
				Item quickSlotItem = inventory.getQuickSlotItem(0);
				ItemSlot quickSlot = inventory.getQuickSlot(0);
				if (quickSlot != null)
				{
					switch (quickSlotItem.category)
					{
						case ItemCategory.Consumable:
							if (InputManager.IsPressed("Use"))
							{
								if (quickSlot.stackSize > 0)
									queueAction(new ConsumableUseAction(quickSlot, 1));
							}
							break;
						default:
							Debug.Assert(false);
							break;
					}
				}
			}
		}


		// Interactions
		if (isCursorLocked)
		{
			interactableInFocus = null;

			if (currentAction == null)
			{
				float closestDistance = float.MaxValue;

				Span<SweepHit> hits = stackalloc SweepHit[8];
				int numHits = Physics.SweepSphere(0.01f, camera.position, camera.rotation.forward, REACH_DISTANCE + 1.0f * MathF.Sin(MathF.Abs(pitch)), hits, QueryFilterFlags.Dynamic | QueryFilterFlags.NoBlock);
				for (int i = 0; i < numHits; i++)
				{
					RigidBody body = hits[i].body;
					if (body != null && body.entity is Interactable)
					{
						Interactable interactable = (Interactable)body.entity;
						if (interactable.canInteract())
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
						if (inventory.getSelectedHandSlot(0) != null)
						{
							dropItem(0);
							queueAction(new ItemThrowAction(0));
						}
						else if (inventory.getSelectedHandSlot(1) != null)
						{
							dropItem(1);
							queueAction(new ItemThrowAction(1));
						}
					}
				}
			}
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
		float currentSpeed = velocity.xz.length / (MAX_GROUND_SPEED * (isDucked ? DUCK_SPEED_MULTIPLIER : 1.0f));

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
				if (currentSpeed > 0.5f)
				{
					movementState0 = runState[0];
					runState[0].animationSpeed = currentSpeed;

					movementState1 = runState[1];
					runState[1].animationSpeed = currentSpeed;

					movementState2 = runState[2];
					runState[2].animationSpeed = currentSpeed;
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
					movementState0 = jumpState[0];
					movementState1 = jumpState[1];
					movementState2 = jumpState[2];
				}
				else
				{
					movementState0 = fallState[0];
					movementState1 = fallState[1];
					movementState2 = fallState[2];
				}
			}
		}


		// Testing procedural viewmodel animations
		movementState0 = idleState[0];
		movementState1 = idleState[1];


		movementAnimationTimerLooping += Time.deltaTime * movementState2.animationSpeed;

		if (currentAction != null)
		{
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
				animator0.setStateIfNot(movementState0, movementAnimationTimerLooping);
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
				animator1.setStateIfNot(movementState1, movementAnimationTimerLooping);
			}

			if (currentAction.fullBodyAnimation)
			{
				if (currentAction.startTime == Time.currentTime)
					moveAnimator.setState(currentActionState[2], movementAnimationTimerLooping);
				else
					moveAnimator.setStateIfNot(currentActionState[2], movementAnimationTimerLooping);
				moveAnimator.timer = currentAction.elapsedTime;
			}
			else
			{
				moveAnimator.setStateIfNot(movementState2, movementAnimationTimerLooping);
			}
		}
		else
		{
			animator0.setStateIfNot(movementState0, movementAnimationTimerLooping);
			animator1.setStateIfNot(movementState1, movementAnimationTimerLooping);
			moveAnimator.setStateIfNot(movementState2, movementAnimationTimerLooping);
		}


		animator0.update();
		animator1.update();
		moveAnimator.update();


		for (int i = 0; i < viewmodel.skeleton.nodes.Length; i++)
		{
			bool isArmBone =
				StringUtils.StartsWith(viewmodel.skeleton.nodes[i].name, "clavicle") ||
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
			viewmodelWalkAnim.x = 0.03f * MathF.Sin(distanceWalked * STEP_FREQUENCY * MathF.PI);
			viewmodelWalkAnim.y = 0.015f * -MathF.Abs(MathF.Cos(distanceWalked * STEP_FREQUENCY * MathF.PI));
			viewmodelWalkAnim *= currentSpeed;
			viewmodelSwayYaw += viewmodelWalkAnim.x;
			viewmodelSwayY += viewmodelWalkAnim.y;

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
			Matrix spineNodeTransform = moveAnimator.getNodeLocalTransform(clavicleRNode);
			//Vector3 spineNodePosition = spineNodeTransform.translation;
			//Quaternion spineNodeRotation = spineNodeTransform.rotation;
			//spineNodePosition += viewmodelOffset;
			//spineNodeRotation = Quaternion.FromAxisAngle(Vector3.UnitX, -pitch * 0.5f) * spineNodeRotation;
			//spineNodeTransform = Matrix.CreateTranslation(spineNodePosition) * Matrix.CreateRotation(spineNodeRotation);
			spineNodeTransform = viewmodelTransform * spineNodeTransform;
			moveAnimator.setNodeLocalTransform(clavicleRNode, spineNodeTransform);
		}
		{
			Matrix spineNodeTransform = moveAnimator.getNodeLocalTransform(clavicleLNode);
			//Vector3 spineNodePosition = spineNodeTransform.translation;
			//Quaternion spineNodeRotation = spineNodeTransform.rotation;
			//spineNodePosition += viewmodelOffset;
			//spineNodeRotation = Quaternion.FromAxisAngle(Vector3.UnitX, -pitch * 0.5f) * spineNodeRotation;
			//spineNodeTransform = Matrix.CreateTranslation(spineNodePosition) * Matrix.CreateRotation(spineNodeRotation);
			spineNodeTransform = viewmodelTransform * spineNodeTransform;
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
		return getModelMatrix() * Matrix.CreateRotation(Quaternion.FromAxisAngle(Vector3.Up, MathF.PI)) * moveAnimator.getNodeTransform(itemNode, 0) * Matrix.CreateRotation(Quaternion.FromAxisAngle(Vector3.UnitX, MathF.PI));
	}

	void updateMovesetLayer(Item item, int handID)
	{
		Item otherItem = inventory.getSelectedHandItem(handID ^ 1);

		if (item != null)
		{
			if ((otherItem == null || !otherItem.twoHanded) || item.twoHanded && otherItem != null && otherItem.twoHanded && handID == 0)
			{
				idleState[handID].layers[0].animationData = item.moveset;
				runState[handID].layers[0].animationData = item.moveset;
				duckedState[handID].layers[0].animationData = item.moveset;
				duckedWalkState[handID].layers[0].animationData = item.moveset;
				jumpState[handID].layers[0].animationData = item.moveset;
				fallState[handID].layers[0].animationData = item.moveset;

				idleState[handID].layers[0].mirrored = handID == 1;
				runState[handID].layers[0].mirrored = handID == 1;
				duckedState[handID].layers[0].mirrored = handID == 1;
				duckedWalkState[handID].layers[0].mirrored = handID == 1;
				jumpState[handID].layers[0].mirrored = handID == 1;
				fallState[handID].layers[0].mirrored = handID == 1;

				runState[handID].layers[0].timerOffset = handID == 1 ? 21 / 24.0f * 0.5f : 0.0f;
				duckedWalkState[handID].layers[0].timerOffset = handID == 1 ? 21 / 24.0f * 0.5f : 0.0f;

				handEntities[handID].setItem(item);
			}
			else if (!item.twoHanded && otherItem != null && otherItem.twoHanded || item.twoHanded && otherItem != null && otherItem.twoHanded && handID == 1)
			{
				idleState[handID].layers[0].animationData = otherItem.moveset;
				runState[handID].layers[0].animationData = otherItem.moveset;
				duckedState[handID].layers[0].animationData = otherItem.moveset;
				duckedWalkState[handID].layers[0].animationData = otherItem.moveset;
				jumpState[handID].layers[0].animationData = otherItem.moveset;
				fallState[handID].layers[0].animationData = otherItem.moveset;

				idleState[handID].layers[0].mirrored = handID != 1;
				runState[handID].layers[0].mirrored = handID != 1;
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
				Debug.Assert(!item.twoHanded && (otherItem == null || !otherItem.twoHanded));

				idleState[handID].layers[0].animationData = item.moveset;
				runState[handID].layers[0].animationData = item.moveset;
				duckedState[handID].layers[0].animationData = item.moveset;
				duckedWalkState[handID].layers[0].animationData = item.moveset;
				jumpState[handID].layers[0].animationData = item.moveset;
				fallState[handID].layers[0].animationData = item.moveset;

				idleState[handID].layers[0].mirrored = handID == 1;
				runState[handID].layers[0].mirrored = handID == 1;
				duckedState[handID].layers[0].mirrored = handID == 1;
				duckedWalkState[handID].layers[0].mirrored = handID == 1;
				jumpState[handID].layers[0].mirrored = handID == 1;
				fallState[handID].layers[0].mirrored = handID == 1;

				runState[handID].layers[0].timerOffset = handID == 1 ? 21 / 24.0f * 0.5f : 0.0f;
				duckedWalkState[handID].layers[0].timerOffset = handID == 1 ? 21 / 24.0f * 0.5f : 0.0f;

				handEntities[handID].setItem(item);
			}
		}
		else
		{
			if (otherItem != null && otherItem.twoHanded)
			{
				idleState[handID].layers[0].animationData = otherItem.moveset;
				runState[handID].layers[0].animationData = otherItem.moveset;
				duckedState[handID].layers[0].animationData = otherItem.moveset;
				duckedWalkState[handID].layers[0].animationData = otherItem.moveset;
				jumpState[handID].layers[0].animationData = otherItem.moveset;
				fallState[handID].layers[0].animationData = otherItem.moveset;

				idleState[handID].layers[0].mirrored = handID != 1;
				runState[handID].layers[0].mirrored = handID != 1;
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
				duckedState[handID].layers[0].animationData = viewmodel;
				duckedWalkState[handID].layers[0].animationData = viewmodel;
				jumpState[handID].layers[0].animationData = viewmodel;
				fallState[handID].layers[0].animationData = viewmodel;

				idleState[handID].layers[0].mirrored = false;
				runState[handID].layers[0].mirrored = false;
				duckedState[handID].layers[0].mirrored = false;
				duckedWalkState[handID].layers[0].mirrored = false;
				jumpState[handID].layers[0].mirrored = false;
				fallState[handID].layers[0].mirrored = false;

				runState[handID].layers[0].timerOffset = 0.0f;
				duckedWalkState[handID].layers[0].timerOffset = 0.0f;

				handEntities[handID].setItem(null);
			}
		}
	}

	void dropItem(int handID)
	{
		if (inventory.getSelectedHandItem(handID) != null)
		{
			ItemPickup pickup = new ItemPickup(inventory.getSelectedHandItem(handID));
			Vector3 startPosition = camera.position + camera.rotation.forward * 0.4f;
			startPosition.y = MathF.Max(startPosition.y, position.y + 0.2f); // prevent item from being thrown into the ground when crouching
			Quaternion startRotation = Quaternion.FromAxisAngle(new Vector3(1.0f).normalized, MathHelper.RandomFloat(0.0f, MathF.PI * 2.0f));
			OntoNewHorizons.instance.world.addEntity(pickup, startPosition, startRotation);
			float throwPower = isDucked ? 8.0f : 4.0f;
			pickup.body.setVelocity(camera.rotation.forward * throwPower);
			pickup.body.setRotationVelocity(new Vector3(MathHelper.RandomFloat(-3.0f, 3.0f), MathHelper.RandomFloat(-3.0f, 3.0f), MathHelper.RandomFloat(-3.0f, 3.0f)));

			inventory.removeItem(inventory.getSelectedHandSlot(handID));
			inventory.selectHandItem(handID, 0, null);

			updateMovesetLayer(inventory.getSelectedHandItem(0), 0);
			updateMovesetLayer(inventory.getSelectedHandItem(1), 1);
		}
	}

	public bool equipHandItem(int handID, int itemIdx, ItemSlot slot)
	{
		if (currentAction == null)
		{
			inventory.selectHandItem(handID, itemIdx, slot);
			updateMovesetLayer(inventory.getSelectedHandItem(0), 0);
			updateMovesetLayer(inventory.getSelectedHandItem(1), 1);

			if (slot != null)
			{
				if (slot.item.hasDrawAnim)
					queueAction(new WeaponDrawAction(slot.item, handID));
				else
					audioAction.playSoundOrganic(slot.item.sfxDraw);
			}

			return true;
		}
		return false;
	}

	public bool equipQuickSlotItem(ItemSlot slot)
	{
		if (currentAction == null)
		{
			return inventory.selectQuickSlotItem(slot);
		}
		return false;
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

	public void onItemPickup(Item item, int amount, Item[] equippedSpells = null)
	{
		ItemSlot slot = inventory.addItem(item, amount);
		if (item.category == ItemCategory.Weapon)
		{
			if (handEntities[0].item != null)
				dropItem(0);
			equipHandItem(0, 0, slot);
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
			if (handEntities[1].item != null)
				dropItem(1);
			equipHandItem(1, 0, slot);
		}
		else if (item.category == ItemCategory.Consumable)
		{
			equipQuickSlotItem(slot);
		}

		hud.collectedItems.Add(new ItemCollectedNotification() { item = item, amount = amount, timeCollected = Time.currentTime });

		//queueAction(new PickUpAction());
	}

	public void queueAction(Action action)
	{
		bool enoughStamina = action.staminaCost == 0.0f || stats.canDoAction;
		if (enoughStamina)
		{
			actionQueue.Add(action);
			actionQueue.Sort((Action a, Action b) => { return a.priority < b.priority ? -1 : a.priority > b.priority ? 1 : 0; });

			if (actionQueue.Count > MAX_ACTION_QUEUE_SIZE)
				actionQueue.RemoveRange(MAX_ACTION_QUEUE_SIZE, actionQueue.Count - MAX_ACTION_QUEUE_SIZE);
			if (actionQueue[0] == action)
				initializeAction(actionQueue[0]);
		}
	}

	/*
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
	*/

	public void cancelAction()
	{
		Debug.Assert(actionQueue.Count > 0);
		currentAction.onFinished(this);
		actionQueue.RemoveAt(0);
	}

	public void cancelAllActions()
	{
		while (actionQueue.Count > 0)
		{
			currentAction.onFinished(this); ;
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

		handEntities[0].draw(graphics);
		handEntities[1].draw(graphics);

		hud.draw(graphics);
		inventoryUI.draw(graphics);
	}

	public Action currentAction
	{
		get => actionQueue.Count > 0 && actionQueue[0].startTime != 0 ? actionQueue[0] : null;
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
}
