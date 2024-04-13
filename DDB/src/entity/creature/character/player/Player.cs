using Rainfall;
using System.ComponentModel.Design;

internal class Player : Entity
{
	public enum MoveMode
	{
		Normal,
		Walk,
		Sprint
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


	const float MAX_GROUND_SPEED = 3.6f;
	const float MAX_AIR_SPEED = 0.3f;
	const float ACCELERATION = 20.0f;
	const float AIR_ACCELERATION = 10.0f;
	const float FRICTION = 16.0f;
	const float STOP_SPEED = 1.0f;

	const float GRAVITY = -10.0f; //-JUMP_POWER / JUMP_PEAK_TIME;
	const float JUMP_HEIGHT = 0.7f;
	//const float JUMP_DURATION = 0.67f;
	//const float JUMP_PEAK_TIME = 0.5f * JUMP_DURATION;
	//const float JUMP_POWER = 2.0f * JUMP_HEIGHT / JUMP_PEAK_TIME;
	static readonly float JUMP_POWER = MathF.Sqrt(2.0f * -GRAVITY * JUMP_HEIGHT);
	const float JUMP_BUFFER_TIME = 0.1f;
	const float JUMP_STAMINA_COST = 4.0f;

	const float SPRINT_SPEED_MULTIPLIER = 1.5f;

	const float WALK_SPEED_MULTIPLIER = 5.5f;

	const float DUCK_SPEED_MULTIPLIER = 0.5f;
	const float DUCK_TRANSITION_DURATION = 0.24f;

	const float MOUSE_SENSITIVITY = 0.0015f;

	const float PLAYER_HEIGHT_STANDING = 1.64f;
	const float PLAYER_HEIGHT_DUCKED = 0.92f;
	const float PLAYER_RADIUS = 0.15f;
	const float STEP_HEIGHT = 0.15f;

	const float STEP_FREQUENCY = 0.8f;
	const float FALL_IMPACT_MIN_SPEED = -3.0f;

	const float CAMERA_HEIGHT_STANDING = 1.6f;
	const float CAMERA_HEIGHT_DUCKED = 1.1f;

	const float REACH_DISTANCE = 1.0f;

	const int MAX_ACTION_QUEUE_SIZE = 2;

	public static readonly Vector3 DEFAULT_VIEWMODEL_OFFSET = new Vector3(0.0f); //new Vector3(0.0f, -0.05f, 0.1f);


	/* MOVEMENT VARIABLES */

	public PlayerCamera camera;
	public CharacterController controller;
	RigidBody detectionBody;

	public AudioSource audioMovement, audioAction;
	Sound[] sfxStep;
	Sound sfxJump, sfxLand;

	bool isCursorLocked = false;
	bool physicsEnabled = true;

	long lastJumpInput = 0;

	Vector2 velocity;
	float verticalVelocity;
	public float yaw;
	public float cameraHeight;

	public bool isDucked = false;
	public float inDuckTimer = -1.0f;
	public MoveMode moveMode = MoveMode.Normal;

	public bool isGrounded = false;

	float stepsWalked = 0.0f;
	int lastStep = 0;

	List<Action> actionQueue = new List<Action>();


	/* ANIMATION VARIABLES */

	Model viewmodel;
	Animator moveAnimator;
	Animator animator0, animator1;
	Node rootNode;
	Node rightItemNode, leftItemNode;
	Node headNode;
	AnimationState[]
		idleState = new AnimationState[3],
		runState = new AnimationState[3],
		duckedState = new AnimationState[3],
		duckedWalkState = new AnimationState[3],
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

	HUD hud;
	InventoryUI inventoryUI;


	public Player()
	{
		cameraHeight = CAMERA_HEIGHT_STANDING;

		inventory = new Inventory();
		stats = new PlayerStats(this);

		hud = new HUD(this);
		inventoryUI = new InventoryUI(this);

		sfxStep = new Sound[] {
			Resource.GetSound("res/entity/creature/character/player/sfx/step_walk1.ogg"),
			Resource.GetSound("res/entity/creature/character/player/sfx/step_walk2.ogg"),
			Resource.GetSound("res/entity/creature/character/player/sfx/step_walk3.ogg"),
		};
		/*
		sfxStep = new Sound[]
		{
			Resource.GetSound("res/entity/player/sfx/step_grass1.ogg"),
			Resource.GetSound("res/entity/player/sfx/step_grass2.ogg"),
		};
		*/
		sfxJump = Resource.GetSound("res/entity/creature/character/player/sfx/step_jump.ogg");
		sfxLand = Resource.GetSound("res/entity/creature/character/player/sfx/step_land.ogg");


		//viewmodel = Resource.CreateModel("res/entity/player/viewmodel.gltf");
		viewmodel = Resource.GetModel("res/entity/creature/character/player/player.gltf");
		rootNode = viewmodel.skeleton.getNode("Root");
		rightItemNode = viewmodel.skeleton.getNode("Weapon.R");
		leftItemNode = viewmodel.skeleton.getNode("Weapon.L");
		headNode = viewmodel.skeleton.getNode("Head");
		//spineNode = viewmodel.skeleton.getNode("Spine");
		//chestNode = viewmodel.skeleton.getNode("Chest");
		//cameraAnchorNode = viewmodel.skeleton.getNode("cameraHold");
		//rightItemNode = viewmodel.skeleton.getNode("handHold.R");
		//leftItemNode = viewmodel.skeleton.getNode("handHold.L");

		unsafe
		{
			viewmodel.getMaterialData(1)->emissiveStrength = 100.0f;
		}

		/*
		bool[] leftHandAnimMask = new bool[viewmodel.skeleton.nodes.Length];
		Array.Fill(leftHandAnimMask, false);
		for (int i = 0; i < viewmodel.skeleton.nodes.Length; i++)
		{
			if (viewmodel.skeleton.nodes[i].name.EndsWith(".L"))
				leftHandAnimMask[viewmodel.skeleton.nodes[i].id] = true;
		}
		*/

		moveAnimator = new Animator(viewmodel);
		animator0 = new Animator(viewmodel);
		animator1 = new Animator(viewmodel);

		for (int i = 0; i < 3; i++)
		{
			idleState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "idle", true) }, 0.2f);
			runState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "run", true) }, 0.2f) { animationSpeed = 1.6f };
			duckedState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "ducked", true) }, 0.2f);
			duckedWalkState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "ducked_walk", true) }, 0.2f) { animationSpeed = 1.6f };
			jumpState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "jump", false) }, 0.1f);
			fallState[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "fall", false) }, 0.2f);
			actionState1[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "default", false) }, 0.2f);
			actionState2[i] = new AnimationState(viewmodel, new AnimationLayer[] { new AnimationLayer(viewmodel, "default", false) }, 0.2f);
		}

		//runState0.layers[1].timerOffset = 21 / 24.0f * 0.5f;
		//duckedWalkState0.layers[1].timerOffset = 21 / 24.0f * 0.5f;

		//animator.transitions.Add(new AnimationTransition(fallState, fallDuckedState, 0.0f));
		//animator.transitions.Add(new AnimationTransition(jumpState, fallDuckedState, 0.0f));
		//animator.transitions.Add(new AnimationTransition(fallDuckedState, fallState, 0.0f));
		//animator.transitions.Add(new AnimationTransition(fallDuckedState, jumpState, 0.0f));

		animator0.setState(idleState[0]);
		animator1.setState(idleState[1]);
		moveAnimator.setState(idleState[2]);

		handEntities[0] = new ItemEntity(this, 0);
		handEntities[1] = new ItemEntity(this, 1);


		// TODO remove
		//inventory.selectHandItem(0, 0, inventory.addItem(Item.Get("longsword")));
		//updateMovesetLayer(Item.Get("longsword"), 0);
		//updateMovesetLayer(null, 1);

		inventory.equipArmorPiece(inventory.addItem(Item.Get("soldier_helmet")));

		inventory.selectHandItem(0, 0, inventory.addItem(Item.Get("shortsword")));
		updateMovesetLayer(Item.Get("shortsword"), 0);

		inventory.selectHandItem(1, 0, inventory.addItem(Item.Get("wooden_round_shield")));
		updateMovesetLayer(Item.Get("wooden_round_shield"), 1);
	}

	public override void init()
	{
		controller = new CharacterController(this, PLAYER_RADIUS, Vector3.Zero, PLAYER_HEIGHT_STANDING, STEP_HEIGHT, new PlayerCollisionCallback(this));
		detectionBody = new RigidBody(this, RigidBodyType.Kinematic);
		detectionBody.addCapsuleTrigger(PLAYER_RADIUS, PLAYER_HEIGHT_STANDING, new Vector3(0.0f, 0.5f * PLAYER_HEIGHT_STANDING, 0.0f), Quaternion.Identity);

		audioMovement = Audio.CreateSource(position);
		audioAction = Audio.CreateSource(position);

		setCursorLocked(true);
	}

	public void hit(int damage, Entity from)
	{
		bool blocking = currentAction != null && currentAction.type == ActionType.ShieldRaise;
		if (blocking)
		{
			ShieldRaiseAction shieldRaiseAction = currentAction as ShieldRaiseAction;
			Item shield = shieldRaiseAction.shield;
			damage = (int)(damage * (1.0f - shield.shieldDamageAbsorption / 100.0f));
			cancelAction();
			//actionQueue.Clear();
			insertAction(new ShieldHitAction(shield, shieldRaiseAction.handID), 0);

			if (shield.sfxHit != null)
				handEntities[shieldRaiseAction.handID].audio.playSoundOrganic(shield.sfxHit);

			if (from is Creature)
			{
				Creature creature = from as Creature;
				creature.cancelAction();
				creature.queueAction(new MobStaggerAction(MobActionType.StaggerBlocked));
			}
		}

		stats.applyDamage(damage);

		if (stats.health == 0)
		{
			/*
			state = CreatureState.Dead;
			body.clearColliders();
			if (from is Player)
			{
				Player otherPlayer = (Player)from;
				otherPlayer.stats.awardXP(200);
			}
			onDeath();

			actionQueue.Clear();
			*/
		}
		else
		{
			/*
			actionQueue.Clear();
			queueAction(new MobStaggerAction());
			*/
		}

		//audio.playSound(hitSound, 0.25f, MathHelper.RandomFloat(0.75f, 1.25f));

		/*
		if (hitParticles != null)
		{
			Vector3 hitDirection = (position - from.position).normalized;
			int numBloodParticles = MathHelper.RandomInt(10, 24);
			for (int i = 0; i < numBloodParticles; i++)
			{
				Vector3 particleDirection = -hitDirection.normalized * 2.0f;
				Vector3 randomVector = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle()) * 2.0f - 1.0f;
				particleDirection += randomVector * Vector3.Dot(randomVector.normalized, particleDirection);
				hitParticles.emitParticle(particleDirection);
			}
		}
		*/

		//onHit(damage, from);
	}

	Vector3 updateMovementInputs()
	{
		Vector3 fsu = Vector3.Zero;

		{
			if (Input.IsKeyDown(KeyCode.KeyA))
				fsu.x -= 1.0f;
			if (Input.IsKeyDown(KeyCode.KeyD))
				fsu.x += 1.0f;
			if (Input.IsKeyDown(KeyCode.KeyS))
				fsu.z -= 1.0f;
			if (Input.IsKeyDown(KeyCode.KeyW))
				fsu.z += 1.0f;
			//if (Input.GetInput("Up"))
			//	fsu.Y += UP_SPEED;
			//if (Input.GetInput("Down"))
			//	fsu.Y -= UP_SPEED;


			if (fsu.lengthSquared > 0.0f)
			{
				fsu = fsu.normalized;
				fsu *= MAX_GROUND_SPEED;
			}
		}

		if (Input.IsKeyPressed(KeyCode.Space))
		{
			lastJumpInput = Time.currentTime;
		}

		if (Input.IsKeyDown(KeyCode.LeftCtrl))
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
				bool headBlocked = Physics.Sweep(new Vector3(PLAYER_RADIUS - 0.1f, 0.1f, PLAYER_RADIUS - 0.1f), position + new Vector3(0.0f, PLAYER_HEIGHT_DUCKED - 0.1f, 0.0f), Vector3.Up, PLAYER_HEIGHT_STANDING - PLAYER_HEIGHT_DUCKED, QueryFilterFlags.Static);

				if (!headBlocked)
				{
					isDucked = false;
					inDuckTimer = -1.0f;
				}
			}

			if (!isDucked)
			{
				inDuckTimer = -1.0f;

				if (Input.IsKeyDown(KeyCode.LeftAlt))
				{
					moveMode = MoveMode.Walk;
				}
				else if (Input.IsKeyDown(KeyCode.LeftShift) && stats.canSprint)
				{
					moveMode = MoveMode.Sprint;
				}
				else
				{
					moveMode = MoveMode.Normal;
				}
			}
		}

		if (isDucked)
		{
			fsu *= DUCK_SPEED_MULTIPLIER;
		}
		else
		{
			switch (moveMode)
			{
				case MoveMode.Walk:
					fsu *= WALK_SPEED_MULTIPLIER;
					break;
				case MoveMode.Sprint:
					fsu *= SPRINT_SPEED_MULTIPLIER;
					break;
				default:
					break;
			}
		}


		return fsu;
	}

	static Vector2 friction(Vector2 velocity, float frametime)
	{
		float entityFriction = 1.0f;
		float edgeFriction = 1.0f;
		float fric = FRICTION * entityFriction * edgeFriction; // sv_friction * ke * ef

		float l = velocity.length;
		Vector2 vn = velocity / l;

		if (l >= STOP_SPEED)
			return (1.0f - frametime * fric) * velocity;
		else if (l >= MathF.Max(0.01f, frametime * STOP_SPEED * fric) && l < STOP_SPEED)
			return velocity - frametime * STOP_SPEED * fric * vn;
		else // if (l < MathHelper.Max(0.1f, frametime * STOP_SPEED * fric)
			return Vector2.Zero;
	}

	static Vector2 updateVelocityGround(Vector2 velocity, Vector2 wishdir, float frametime, float maxSpeed, Vector2 forward, Vector2 right)
	{
		Vector2 accel = wishdir.x * right + wishdir.y * forward;
		float accelMag = accel.length;
		Vector2 accelDir = accelMag > 0.0f ? accel / accelMag : Vector2.Zero;

		float entityFriction = 1.0f;

		velocity = friction(velocity, frametime);
		float m = MathF.Min(maxSpeed, wishdir.length);
		float currentSpeed = Vector2.Dot(velocity, accelDir);
		float l = m;
		float addSpeed = Math.Clamp(l - currentSpeed, 0.0f, entityFriction * frametime * m * ACCELERATION);

		return velocity + accelDir * addSpeed;
	}

	Vector2 updateVelocityAir(Vector2 velocity, Vector2 wishdir, float frametime, Vector2 forward, Vector2 right)
	{
		Vector2 accel = wishdir.x * right + wishdir.y * forward;
		float accelMag = accel.length;
		Vector2 accelDir = accelMag > 0.0f ? accel / accelMag : Vector2.Zero;

		float entityFriction = 1.0f;

		float m = MathF.Min(MAX_GROUND_SPEED, wishdir.length);
		float currentSpeed = Vector2.Dot(velocity, accelDir);
		float l = MathF.Min(m, MAX_AIR_SPEED);
		float addSpeed = Math.Clamp(l - currentSpeed, 0.0f, entityFriction * frametime * m * AIR_ACCELERATION);

		return velocity + accelDir * addSpeed;
	}

	void onCollision(Vector3 position, Vector3 normal)
	{
		Vector3 v = new Vector3(velocity.x, verticalVelocity, velocity.y);
		if (Vector3.Dot(v, normal) < 0.0f)
		{
			if (normal.y >= 0.999f && verticalVelocity < FALL_IMPACT_MIN_SPEED)
			{
				// Land
				//sfxStep.Play(0.4f, 0.0f, 0.0f);
			}

			// If this is a slope, don't modify velocity to allow for smooth climbing
			if (MathF.Abs(normal.x) > 0.999f || MathF.Abs(normal.y) > 0.999f || MathF.Abs(normal.z) > 0.999f || normal.y < 0.001f)
			{
				bool groundHit = normal.y > 0.5f && verticalVelocity < -2.0f;
				if (groundHit && sfxLand != null)
					audioMovement.playSoundOrganic(sfxLand);

				float bounceCoefficient = 1.0f;
				Vector3 newVelocity = v - bounceCoefficient * Vector3.Dot(v, normal) * normal;
				velocity.x = newVelocity.x;
				verticalVelocity = newVelocity.y;
				velocity.y = newVelocity.z;
			}
		}
	}

	void updateMovement(Vector3 fsu)
	{
		Vector2 forward = new Vector2(MathF.Sin(-yaw), -MathF.Cos(-yaw));
		Vector2 right = new Vector2(MathF.Cos(yaw), -MathF.Sin(yaw));

		if ((Time.currentTime - lastJumpInput) / 1e9f <= JUMP_BUFFER_TIME)
		{
			if (isGrounded && stats.canJump)
			{
				verticalVelocity = JUMP_POWER;
				isGrounded = false;
				lastJumpInput = 0;
				stats.consumeStamina(JUMP_STAMINA_COST);
				if (sfxJump != null)
					audioMovement.playSoundOrganic(sfxJump);
			}
		}

		if (fsu.lengthSquared > 0.0f)
		{
			bool combatMode = false;
			if (combatMode)
			{
				float targetYaw = camera.yaw;
				float rotationSpeed = 20.0f;
				if (currentAction != null)
					rotationSpeed *= currentAction.rotationSpeedMultiplier;
				yaw = MathHelper.LerpAngle(yaw, targetYaw, Time.deltaTime * rotationSpeed);
				//fsu = new Vector3(0.0f, 0.0f, 1.0f) * fsu.length;
			}
			else
			{
				float targetYaw = MathF.Atan2(fsu.z, fsu.x) - MathF.PI * 0.5f + camera.yaw;
				float rotationSpeed = 20.0f;
				if (currentAction != null)
					rotationSpeed *= currentAction.rotationSpeedMultiplier;
				yaw = MathHelper.LerpAngle(yaw, targetYaw, Time.deltaTime * rotationSpeed);
				fsu = new Vector3(0.0f, 0.0f, 1.0f) * fsu.length;
			}
		}

		if (currentAction != null)
			fsu *= currentAction.movementSpeedMultiplier;

		if (isGrounded)
			velocity = updateVelocityGround(velocity, new Vector2(fsu.x, fsu.z), Time.deltaTime, moveMode == MoveMode.Sprint ? SPRINT_SPEED_MULTIPLIER * MAX_GROUND_SPEED : MAX_GROUND_SPEED, forward, right);
		else
			velocity = updateVelocityAir(velocity, new Vector2(fsu.x, fsu.z), Time.deltaTime, forward, right);


		if (physicsEnabled)
		{
			verticalVelocity = verticalVelocity + 0.5f * GRAVITY * Time.deltaTime;

			// Position update
			{
				Vector3 displacement = new Vector3(velocity.x, verticalVelocity, velocity.y) * Time.deltaTime;

				// Root Motion
				if (isGrounded && currentAction != null && currentAction.rootMotion)
				{
					Vector3 rootMotionDisplacement = currentActionState[2].layers[0].rootMotionDisplacement;
					rootMotionDisplacement = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI + yaw) * rootMotionDisplacement;
					displacement += rootMotionDisplacement;
				}


				ControllerCollisionFlag flags = controller.move(displacement);

				isGrounded = false;
				if (verticalVelocity < 0.5f)
				{
					SweepHit[] hits = new SweepHit[16];
					int numHits = Physics.SweepSphere(PLAYER_RADIUS, position + new Vector3(0.0f, 0.1f, 0.0f), Vector3.Down, 0.2f, hits, 16, QueryFilterFlags.Static | QueryFilterFlags.Dynamic);
					for (int i = 0; i < numHits; i++)
					{
						if (!hits[i].isTrigger)
						{
							isGrounded = true;
							break;
						}
					}
				}
			}

			verticalVelocity = verticalVelocity + 0.5f * GRAVITY * Time.deltaTime;
		}
		else
		{
			Vector3 displacement = Vector3.Zero;

			// Root Motion
			if (isGrounded && currentAction != null && currentAction.rootMotion)
			{
				Vector3 rootMotionDisplacement = currentActionState[2].layers[0].rootMotionDisplacement;
				rootMotionDisplacement = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI + yaw) * rootMotionDisplacement;
				displacement += rootMotionDisplacement;
			}

			Vector3 before = position;
			controller.setPosition(position + displacement);

			isGrounded = true;
		}


		rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw);


		audioMovement.updateTransform(position);
		audioAction.updateTransform(position);

		if (isGrounded)
		{
			stepsWalked += velocity.length * Time.deltaTime * STEP_FREQUENCY;
			if ((int)stepsWalked > lastStep)
			{
				if ((moveMode == MoveMode.Normal || moveMode == MoveMode.Sprint) && !isDucked)
				{
					audioMovement.playSoundOrganic(sfxStep);
					//sfxStep.Play(0.2f, RNG.RandomFloat(-0.2f, 0.2f), 0.0f);
					lastStep = (int)stepsWalked;
				}
			}
		}
	}

	void updatePhysics()
	{
		if (isDucked && controller.height != PLAYER_HEIGHT_DUCKED)
		{
			controller.resize(PLAYER_HEIGHT_DUCKED);
			if (!isGrounded)
			{
				controller.move(new Vector3(0.0f, 1.0f * (PLAYER_HEIGHT_STANDING - PLAYER_HEIGHT_DUCKED), 0.0f));
				//cameraHeight -= 1.0f * (PLAYER_HEIGHT_STANDING - PLAYER_HEIGHT_DUCKED);
			}
		}
		else if (!isDucked && controller.height != PLAYER_HEIGHT_STANDING)
		{
			controller.resize(PLAYER_HEIGHT_STANDING);
			if (!isGrounded)
			{
				controller.move(new Vector3(0.0f, -1.0f * (PLAYER_HEIGHT_STANDING - PLAYER_HEIGHT_DUCKED), 0.0f));
				//cameraHeight += 1.0f * (PLAYER_HEIGHT_STANDING - PLAYER_HEIGHT_DUCKED);
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
							if (handID == 0 && (Input.IsMouseButtonPressed(MouseButton.Left) || Input.IsKeyPressed(KeyCode.KeyU)) ||
								handID == 1 && (Input.IsMouseButtonPressed(MouseButton.Right) || Input.IsKeyPressed(KeyCode.KeyO)))
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
									if (inventory.arrows.Count > 0)
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
							if (handID == 0 && !Input.IsMouseButtonDown(MouseButton.Left) ||
								handID == 1 && !Input.IsMouseButtonDown(MouseButton.Right))
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
							if (handID == 0 && Input.IsMouseButtonPressed(MouseButton.Right) && handItem.weaponType == WeaponType.Bow && currentAction != null && currentAction.type == ActionType.BowDraw)
							{
								cancelAction();
							}
							break;
						case ItemCategory.Shield:
							if (currentAction == null && (
								handID == 0 && (Input.IsMouseButtonDown(MouseButton.Left) || Input.IsKeyDown(KeyCode.KeyU)) ||
								handID == 1 && (Input.IsMouseButtonDown(MouseButton.Right) || Input.IsKeyDown(KeyCode.KeyO))
								))
							{
								queueAction(new ShieldRaiseAction(handItem, handID));
							}
							if (currentAction != null && currentAction.type == ActionType.ShieldRaise)
							{
								ShieldRaiseAction shieldRaiseAction = currentAction as ShieldRaiseAction;
								if (shieldRaiseAction.handID == handID && shieldRaiseAction.handID == 0 && !(Input.IsMouseButtonDown(MouseButton.Left) || Input.IsKeyDown(KeyCode.KeyU)) ||
									shieldRaiseAction.handID == handID && shieldRaiseAction.handID == 1 && !(Input.IsMouseButtonDown(MouseButton.Right) || Input.IsKeyDown(KeyCode.KeyO)) ||
									actionQueue.Count > 1)
								{
									cancelAction();
								}
							}
							break;
						case ItemCategory.Utility:
							//itemUseAction = new UtilityUseAction(handItem);
							break;
						case ItemCategory.Consumable:
							if (handID == 0 && Input.IsMouseButtonPressed(MouseButton.Left) ||
								handID == 1 && Input.IsMouseButtonPressed(MouseButton.Right))
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
			ItemSlot quickSlot = inventory.getQuickSlot(0);
			Item quickSlotItem = inventory.getQuickSlotItem(0);
			if (quickSlot != null)
			{
				switch (quickSlotItem.category)
				{
					case ItemCategory.Consumable:
						if (Input.IsKeyPressed(KeyCode.KeyF))
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


		// Interactions
		if (isCursorLocked)
		{
			interactableInFocus = null;

			if (currentAction == null)
			{
				SweepHit[] hits = new SweepHit[4];
				int numHits = Physics.SweepSphere(0.1f, position + new Vector3(0.0f, 1.0f, 0.0f), rotation.forward, REACH_DISTANCE, hits, 4, QueryFilterFlags.Dynamic | QueryFilterFlags.NoBlock);
				for (int i = 0; i < numHits; i++)
				{
					RigidBody body = RigidBody.GetBodyFromHandle(hits[i].userData);
					if (body != null && body.entity is Interactable)
					{
						Interactable interactable = (Interactable)body.entity;
						if (interactable.canInteract())
						{
							interactableInFocus = interactable;
							break;
						}
					}
				}

				if (interactableInFocus != null)
				{
					if (Input.IsKeyPressed(KeyCode.KeyE))
					{
						interactableInFocus.interact(this);
					}
				}

				// Dropping item
				{
					if (Input.IsKeyPressed(KeyCode.KeyG))
					{
						if (inventory.getSelectedHandSlot(0) != null)
							dropItem(0);
						else if (inventory.getSelectedHandSlot(1) != null)
							dropItem(1);

						//updateMovesetLayer(null, 0);
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
		AnimationState movementState0, movementState1, movementState2;

		if (isGrounded)
		{
			if (inDuckTimer > 0.0f)
			{
				if (velocity.lengthSquared > 0.5f * MAX_GROUND_SPEED * DUCK_SPEED_MULTIPLIER * 0.5f * MAX_GROUND_SPEED * DUCK_SPEED_MULTIPLIER)
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
			{
				float speed = velocity.length;
				if (speed > 0.5f * MAX_GROUND_SPEED)
				{
					movementState0 = runState[0];
					runState[0].animationSpeed = speed / MAX_GROUND_SPEED;

					movementState1 = runState[1];
					runState[1].animationSpeed = speed / MAX_GROUND_SPEED;

					movementState2 = runState[2];
					runState[2].animationSpeed = speed / MAX_GROUND_SPEED;
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
			//if (isDucked)
			//{
			//	animator.setState(fallDuckedState);
			//}
			//else
			{
				if (verticalVelocity > 0.0f)
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

		movementAnimationTimerLooping += Time.deltaTime * movementState2.animationSpeed;

		if (currentAction != null)
		{
			if (currentAction.animationName[0] != null)
			{
				if (currentAction.startTime == Time.currentTime)
					animator0.setState(currentActionState[0]);
				else
					animator0.setStateIfNot(currentActionState[0]);
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

		/*
		{
			Matrix spineNodeTransform = animator.getNodeLocalTransform(spineNode);
			Vector3 spineNodePosition = spineNodeTransform.translation;
			Quaternion spineNodeRotation = spineNodeTransform.rotation;
			spineNodeRotation = Quaternion.FromAxisAngle(Vector3.UnitX, -pitch * 0.5f) * spineNodeRotation;
			spineNodeTransform = Matrix.CreateTranslation(spineNodePosition) * Matrix.CreateRotation(spineNodeRotation);
			animator.setNodeLocalTransform(spineNode, spineNodeTransform);
		}
		{
			Matrix chestNodeTransform = animator.getNodeLocalTransform(chestNode);
			Vector3 chestNodePosition = chestNodeTransform.translation;
			Quaternion chestNodeRotation = chestNodeTransform.rotation;
			chestNodeRotation = Quaternion.FromAxisAngle(Vector3.UnitX, -pitch * 0.5f) * chestNodeRotation;
			chestNodeTransform = Matrix.CreateTranslation(chestNodePosition) * Matrix.CreateRotation(chestNodeRotation);
			animator.setNodeLocalTransform(chestNode, chestNodeTransform);
		}
		*/

		for (int i = 0; i < viewmodel.skeleton.nodes.Length; i++)
		{
			bool isArmBone = viewmodel.skeleton.nodes[i].name.StartsWith("Shoulder") ||
				viewmodel.skeleton.nodes[i].name.StartsWith("ArmUpper") ||
				viewmodel.skeleton.nodes[i].name.StartsWith("ArmLower") ||
				viewmodel.skeleton.nodes[i].name.StartsWith("Hand") ||
				viewmodel.skeleton.nodes[i].name.StartsWith("Fingers") ||
				viewmodel.skeleton.nodes[i].name.StartsWith("Thumb") ||
				viewmodel.skeleton.nodes[i].name.StartsWith("Weapon");

			if (isArmBone && viewmodel.skeleton.nodes[i].name.EndsWith(".R"))
				moveAnimator.nodeLocalTransforms[i] = animator0.nodeLocalTransforms[i];
			if (isArmBone && viewmodel.skeleton.nodes[i].name.EndsWith(".L"))
				moveAnimator.nodeLocalTransforms[i] = animator1.nodeLocalTransforms[i];

			//moveAnimator.nodeLocalTransforms[i] = animator0.nodeLocalTransforms[i];
		}


		/*
		Matrix neckTransform = moveAnimator.getNodeLocalTransform(neckNode);
		{
			Vector3 spineNodePosition = neckTransform.translation;
			Quaternion spineNodeRotation = neckTransform.rotation;
			spineNodeRotation = Quaternion.FromAxisAngle(Vector3.UnitX, -pitch * 0.5f) * spineNodeRotation;
			Matrix newNeckTransform = Matrix.CreateTranslation(spineNodePosition) * Matrix.CreateRotation(spineNodeRotation);
			moveAnimator.setNodeLocalTransform(neckNode, newNeckTransform);
		}
		{
			Matrix spineNodeTransform = moveAnimator.getNodeLocalTransform(clavicleRNode);
			//spineNodeTransform = neckTransform.inverted * spineNodeTransform;
			Vector3 spineNodePosition = spineNodeTransform.translation;
			Quaternion spineNodeRotation = spineNodeTransform.rotation;
			spineNodePosition += viewmodelOffset;
			spineNodeRotation = Quaternion.FromAxisAngle(Vector3.UnitX, -pitch * 0.5f) * spineNodeRotation;
			spineNodeTransform = Matrix.CreateTranslation(spineNodePosition) * Matrix.CreateRotation(spineNodeRotation);
			//spineNodeTransform = neckTransform * spineNodeTransform;
			moveAnimator.setNodeLocalTransform(clavicleRNode, spineNodeTransform);
		}
		{
			Matrix spineNodeTransform = moveAnimator.getNodeLocalTransform(clavicleLNode);
			//spineNodeTransform = neckTransform.inverted * spineNodeTransform;
			Vector3 spineNodePosition = spineNodeTransform.translation;
			Quaternion spineNodeRotation = spineNodeTransform.rotation;
			spineNodePosition += viewmodelOffset;
			spineNodeRotation = Quaternion.FromAxisAngle(Vector3.UnitX, -pitch * 0.5f) * spineNodeRotation;
			spineNodeTransform = Matrix.CreateTranslation(spineNodePosition) * Matrix.CreateRotation(spineNodeRotation);
			//spineNodeTransform = neckTransform * spineNodeTransform;
			moveAnimator.setNodeLocalTransform(clavicleLNode, spineNodeTransform);
		}
		*/
		/*
		{
			Matrix spineNodeTransform = moveAnimator.getNodeLocalTransform(spine03Node);
			Vector3 spineNodePosition = spineNodeTransform.translation;
			Quaternion spineNodeRotation = spineNodeTransform.rotation;
			spineNodeRotation = Quaternion.FromAxisAngle(Vector3.UnitX, -pitch * 0.5f) * spineNodeRotation;
			spineNodeTransform = Matrix.CreateTranslation(spineNodePosition) * Matrix.CreateRotation(spineNodeRotation);
			moveAnimator.setNodeLocalTransform(spine03Node, spineNodeTransform);
		}
		*/
		/*
		{
			Matrix spineNodeTransform = moveAnimator.getNodeLocalTransform(spine02Node);
			Vector3 spineNodePosition = spineNodeTransform.translation;
			Quaternion spineNodeRotation = spineNodeTransform.rotation;
			spineNodeRotation = Quaternion.FromAxisAngle(Vector3.UnitX, -pitch * 0.5f) * spineNodeRotation;
			spineNodeTransform = Matrix.CreateTranslation(spineNodePosition) * Matrix.CreateRotation(spineNodeRotation);
			moveAnimator.setNodeLocalTransform(spine02Node, spineNodeTransform);
		}
		{
			Matrix spineNodeTransform = moveAnimator.getNodeLocalTransform(spine01Node);
			Vector3 spineNodePosition = spineNodeTransform.translation;
			Quaternion spineNodeRotation = spineNodeTransform.rotation;
			spineNodeRotation = Quaternion.FromAxisAngle(Vector3.UnitX, -pitch * 0.166f) * spineNodeRotation;
			spineNodeTransform = Matrix.CreateTranslation(spineNodePosition) * Matrix.CreateRotation(spineNodeRotation);
			//moveAnimator.setNodeLocalTransform(spine01Node, spineNodeTransform);
		}
		{
			Matrix spineNodeTransform = moveAnimator.getNodeLocalTransform(pelvisNode);
			Vector3 spineNodePosition = spineNodeTransform.translation;
			Quaternion spineNodeRotation = spineNodeTransform.rotation;
			spineNodeRotation = Quaternion.FromAxisAngle(Vector3.UnitX, -pitch * 0.166f) * spineNodeRotation;
			spineNodeTransform = Matrix.CreateTranslation(spineNodePosition) * Matrix.CreateRotation(spineNodeRotation);
			moveAnimator.setNodeLocalTransform(pelvisNode, spineNodeTransform);
		}
		*/


		moveAnimator.applyAnimation();


		// Camera height animation
		cameraHeight = moveAnimator.getNodeTransform(headNode, 0).translation.y * 0.5f + 0.5f;
		/*
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
		else
		{
			if (isDucked)
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
		*/
	}

	public override void update()
	{
		base.update();

		Vector3 fsu = updateMovementInputs();
		updateMovement(fsu);
		updatePhysics();
		updateActions();
		updateAnimations();

		handEntities[0].setTransform(getWeaponTransform(0));
		handEntities[1].setTransform(getWeaponTransform(1));

		handEntities[0].update();
		handEntities[1].update();

		stats.update();

		inventoryUI.update();
	}

	public Matrix getWeaponTransform(int handID)
	{
		Node itemNode = handID == 0 ? rightItemNode : leftItemNode;
		return getModelMatrix()
			* Matrix.CreateScale(0.5f)
			* Matrix.CreateRotation(Quaternion.FromAxisAngle(Vector3.Up, MathF.PI))
			* moveAnimator.getNodeTransform(itemNode, 0)
			* Matrix.CreateRotation(Quaternion.FromAxisAngle(Vector3.UnitX, MathF.PI * 0.5f))
			* Matrix.CreateRotation(Quaternion.FromAxisAngle(Vector3.UnitY, MathF.PI))
			//* Matrix.CreateRotation(Quaternion.FromAxisAngle(Vector3.UnitX, (handID == 0 ? 1.0f : -1.0f) * MathF.PI * 0.5f))
			//* Matrix.CreateRotation(Quaternion.FromAxisAngle(Vector3.UnitY, (handID == 0 ? -1.0f : -1.0f) * MathF.PI))
			* Matrix.CreateScale(2.0f);
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

				idleState[handID].layers[0].animationName = "hold";
				runState[handID].layers[0].animationName = "hold";
				duckedState[handID].layers[0].animationName = "hold";
				duckedWalkState[handID].layers[0].animationName = "hold";
				jumpState[handID].layers[0].animationName = "hold";
				fallState[handID].layers[0].animationName = "hold";

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

				idleState[handID].layers[0].animationName = "hold";
				runState[handID].layers[0].animationName = "hold";
				duckedState[handID].layers[0].animationName = "hold";
				duckedWalkState[handID].layers[0].animationName = "hold";
				jumpState[handID].layers[0].animationName = "hold";
				fallState[handID].layers[0].animationName = "hold";

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

				idleState[handID].layers[0].animationName = "hold";
				runState[handID].layers[0].animationName = "hold";
				duckedState[handID].layers[0].animationName = "hold";
				duckedWalkState[handID].layers[0].animationName = "hold";
				jumpState[handID].layers[0].animationName = "hold";
				fallState[handID].layers[0].animationName = "hold";

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

				idleState[handID].layers[0].animationName = "hold";
				runState[handID].layers[0].animationName = "hold";
				duckedState[handID].layers[0].animationName = "hold";
				duckedWalkState[handID].layers[0].animationName = "hold";
				jumpState[handID].layers[0].animationName = "hold";
				fallState[handID].layers[0].animationName = "hold";

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

				idleState[handID].layers[0].animationName = "idle";
				runState[handID].layers[0].animationName = "run";
				duckedState[handID].layers[0].animationName = "ducked";
				duckedWalkState[handID].layers[0].animationName = "ducked_walk";
				jumpState[handID].layers[0].animationName = "jump";
				fallState[handID].layers[0].animationName = "fall";

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
			level.addEntity(new ItemPickup(inventory.getSelectedHandItem(handID)), position + new Vector3(0.0f, 1.0f, 0.0f) + rotation.forward * new Vector3(1.0f, 0.0f, 1.0f), Quaternion.FromAxisAngle(new Vector3(1.0f).normalized, MathHelper.RandomFloat(0.1f, 3.0f)));
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
		if (actionQueue.Count < MAX_ACTION_QUEUE_SIZE)
		{
			bool enoughStamina = action.staminaCost == 0.0f || stats.canDoAction;
			if (enoughStamina)
				actionQueue.Add(action);

			if (actionQueue.Count == 1)
				initializeAction(actionQueue[0]);
		}
	}

	public void insertAction(Action action, int index)
	{
		if (actionQueue.Count < MAX_ACTION_QUEUE_SIZE)
		{
			bool enoughStamina = action.staminaCost == 0.0f || stats.canDoAction;
			if (enoughStamina)
				actionQueue.Insert(index, action);

			if (actionQueue.Count == 1)
				initializeAction(actionQueue[0]);
		}
	}

	public void cancelAction()
	{
		Debug.Assert(actionQueue.Count > 0);
		actionQueue.RemoveAt(0);
	}

	public new void setPosition(Vector3 position)
	{
		controller.move(position - this.position);
	}

	public void setCursorLocked(bool locked)
	{
		isCursorLocked = locked;
		Input.mouseLocked = locked;
	}

	public void setPhysicsEnabled(bool enabled)
	{
		physicsEnabled = enabled;
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


		Matrix transform = getModelMatrix() * Matrix.CreateRotation(Quaternion.FromAxisAngle(Vector3.Up, MathF.PI)) * Matrix.CreateScale(0.5f);
		Renderer.DrawModel(viewmodel, transform, moveAnimator);

		handEntities[0].draw(graphics);
		handEntities[1].draw(graphics);

		for (int i = 0; i < 5; i++)
		{
			if (inventory.armorSlots[i] != null)
			{
				Item armorPiece = inventory.armorSlots[i].item;
				if (armorPiece.armorPiece == ArmorPiece.Head)
				{
					Matrix armorPieceTransform = getModelMatrix() * Matrix.CreateRotation(Quaternion.FromAxisAngle(Vector3.Up, MathF.PI)) * Matrix.CreateScale(0.5f);
					Renderer.DrawModel(armorPiece.model, armorPieceTransform, moveAnimator);
				}
			}
		}

		hud.draw(graphics);
		inventoryUI.draw(graphics);

		Debug.DrawDebugText(0, 0, "grounded=" + isGrounded);
		Debug.DrawDebugText(0, 1, "speed=" + (int)(velocity.length * 100));
		Debug.DrawDebugText(0, 2, "x=" + (int)(position.x * 100));
		Debug.DrawDebugText(0, 3, "y=" + (int)(position.y * 100));
		Debug.DrawDebugText(0, 4, "z=" + (int)(position.z * 100));
	}

	public Action currentAction
	{
		get => actionQueue.Count > 0 && actionQueue[0].startTime != 0 ? actionQueue[0] : null;
	}

	public Vector3 lookOrigin
	{
		get => Vector3.Zero;
	}

	public Vector3 lookDirection
	{
		get => Vector3.One;
	}

	public Quaternion lookRotation
	{
		get => Quaternion.Identity;
	}
}
