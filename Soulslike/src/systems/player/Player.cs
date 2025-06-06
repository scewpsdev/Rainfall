using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


enum CameraMode
{
	FirstPerson,
	ThirdPerson,
}

public class Player : Entity, Hittable
{
	public const float DEFAULT_VIEWMODEL_AIM = 0.2f;
	const float SPRINT_SPEED_MULTIPLIER = 1.5f;
	const float DUCK_SPEED_MULTIPLIER = 0.5f;
	const float REACH_DISTANCE = 1.5f;
	const float CAMERA_HEIGHT = 1.5f;
	const float CAMERA_HEIGHT_DUCKED = 0.9f;
	const float WEAPON_CHARGE_TIME = 0.2f;

	public Camera camera;
	public float pitch, yaw;
	float camerayaw;
	float cameraHeight;
	CameraMode cameraMode = CameraMode.FirstPerson;

	bool isSprinting;

	float rightViewmodelAim = 0, leftViewmodelAim = 0;

	AnimationState idleAnim, runAnim;
	AnimationState fallAnim;
	AnimationState duckAnim, sneakAnim;

	public Node rightWeaponNode, leftWeaponNode;
	Node rightShoulderNode, leftShoulderNode;
	Node spineNode, chestNode, neckNode;
	Node cameraNode;
	Node[] ringNodes = new Node[4];
	Matrix defaultPoseCameraRotation;
	Matrix defaultPoseNeckTransform;

	Animator rightHandAnimator, leftHandAnimator;
	bool[] rightHandBoneMask, leftHandBoneMask;

	public Matrix rightWeaponTransform;
	public Matrix leftWeaponTransform;


	//Node spine005Node, upperArmR, upperArmL;
	//Animator bodyAnimator;
	//AnimationState bodyIdleAnim;
	//AnimationState bodyRunAnim;
	//AnimationState bodyFallAnim;
	//AnimationState bodyDuckAnim;
	//AnimationState bodySneakAnim;

	public FirstPersonController controller;
	RigidBody playerBody;
	public ActionManager actionManager;
	AnimationState actionAnim1, actionAnim2;
	public AnimationState currentActionAnim;

	public Node rootMotionNode;
	Vector3 lastRootMotion = Vector3.One;
	PlayerAction lastRootMotionAction;

	public Item rightWeapon, leftWeapon;
	public long lastRightWeaponDown = -1, lastLeftWeaponDown = -1;

	Model rightWeaponModel, leftWeaponModel;
	Model rightWeaponMoveset, leftWeaponMoveset;
	Animator rightWeaponAnimator, leftWeaponAnimator;
	PointLight rightWeaponLight;
	ParticleSystem rightWeaponParticles;

	Item helmet, bodyArmor, gloves, boots;

	Animator helmetAnimator, bodyArmorAnimator, glovesAnimator, bootsAnimator;

	Sound[] stepSound;

	Item[] rings = new Item[4];

	public float health = 100;
	public int maxHealth = 100;

	public Interactable interactableInFocus = null;


	public unsafe Player(Camera camera)
	{
		this.camera = camera;

		camera.fov = 100;
		camera.far = 1000;

		model = Resource.GetModel("player_test.gltf");
		modelTransform = /*Matrix.CreateTranslation(0, cameraHeight, 0) **/ Matrix.CreateRotation(Vector3.Up, MathF.PI);
		animator = Animator.Create(model);

		//animator.setAnimation(Animator.CreateAnimation(model, "default"));
		//animator.update();
		//animator.applyAnimation();

		rootMotionNode = model.skeleton.getNode("Root");
		rightWeaponNode = model.skeleton.getNode("Weapon.R");
		leftWeaponNode = model.skeleton.getNode("Weapon.L");
		rightShoulderNode = model.skeleton.getNode("Shoulder.R");
		leftShoulderNode = model.skeleton.getNode("Shoulder.L");
		cameraNode = model.skeleton.getNode("Camera");
		ringNodes[0] = model.skeleton.getNode("Finger1A.R");
		ringNodes[1] = model.skeleton.getNode("Finger3A.R");
		ringNodes[2] = model.skeleton.getNode("Finger1A.L");
		ringNodes[3] = model.skeleton.getNode("Finger3A.L");

		idleAnim = Animator.CreateAnimation(model, [new AnimationLayer(model, "idle", true), null, null], 0.4f);
		idleAnim.animationSpeed = 0.001f;
		runAnim = Animator.CreateAnimation(model, [new AnimationLayer(model, "run", true), null, null], 0.4f);
		fallAnim = Animator.CreateAnimation(model, [new AnimationLayer(model, "fall", true), null, null], 0.4f);
		duckAnim = Animator.CreateAnimation(model, [new AnimationLayer(model, "duck", true), null, null], 0.2f);
		duckAnim.transitionFromDuration = 0.2f;
		duckAnim.animationSpeed = 0.001f;
		sneakAnim = Animator.CreateAnimation(model, [new AnimationLayer(model, "sneak", true), null, null], 0.4f);
		actionAnim1 = Animator.CreateAnimation(model, [new AnimationLayer(model, "default", false), null, null], 0.1f);
		actionAnim2 = Animator.CreateAnimation(model, [new AnimationLayer(model, "default", false), null, null], 0.1f);

		rightHandAnimator = Animator.Create(model);
		leftHandAnimator = Animator.Create(model);

		//bodyModel = Resource.GetModel("player_test.gltf");
		//cameraAnchorNode = bodyModel.skeleton.getNode("Camera");
		spineNode = model.skeleton.getNode("Spine");
		chestNode = model.skeleton.getNode("Chest");
		neckNode = model.skeleton.getNode("Neck");
		//spine005Node = bodyModel.skeleton.getNode("spine.005");
		//upperArmR = bodyModel.skeleton.getNode("upper_arm.R");
		//upperArmL = bodyModel.skeleton.getNode("upper_arm.L");
		//bodyAnimator = Animator.Create(bodyModel);
		//bodyIdleAnim = Animator.CreateAnimation(model, "idle", true, 0.4f);
		//bodyIdleAnim.animationSpeed = 0.001f;
		//bodyRunAnim = Animator.CreateAnimation(model, "run", true, 0.4f);
		//bodyFallAnim = Animator.CreateAnimation(model, "fall", true, 0.1f);
		//bodyDuckAnim = Animator.CreateAnimation(model, "duck", true, 0.2f);
		//bodyDuckAnim.transitionFromDuration = 0.2f;
		//bodyDuckAnim.animationSpeed = 0.001f;
		//bodySneakAnim = Animator.CreateAnimation(model, "sneak", true, 0.4f);

		animator.setAnimation(idleAnim);
		animator.update();
		animator.applyAnimation();
		defaultPoseCameraRotation = Matrix.CreateRotation(animator.getNodeTransform(cameraNode).rotation);
		defaultPoseNeckTransform = animator.getNodeLocalTransform(neckNode);

		actionManager = new ActionManager(this);

		stepSound = Resource.GetSounds("sound/step/step_bare", 3);

		Input.cursorMode = CursorMode.Disabled;
	}

	public override void init()
	{
		base.init();

		yaw = rotation.eulers.y;

		controller = new FirstPersonController(this, PhysicsFilter.Default | PhysicsFilter.Creature);
		controller.canJump = () =>
		{
			//return stats.stamina > 0;
			return true;
		};
		controller.onJump = () =>
		{
			//player.stats.consumeStamina(JUMP_STAMINA_COST);
			/*
			if (player.jumpSound != null)
			{
				Audio.PlayOrganic(player.jumpSound, player.position);
				AIManager.NotifySound(player.position, 3.0f);
			}
			*/
		};
		controller.onStep = () =>
		{
			Sound[] stepSound = this.stepSound;
			if (boots != null && boots.stepSound != null)
				stepSound = boots.stepSound;
			Audio.PlayOrganic(stepSound, position + rotation * Vector3.Forward);

			/*
			if (!isDucked && player.stepSound != null)
			{
				Audio.PlayOrganic(player.stepSound, player.position, rolloff: 1);
				AIManager.NotifySound(player.position, 3.0f);
			}
			*/
		};
		controller.onLadderStep = () =>
		{
			//Audio.PlayOrganic(currentLadder.sfxStep, player.camera.position + player.camera.rotation.forward * 0.5f);
		};
		controller.onLand = (float speed) =>
		{
			/*
			if (player.landSound != null)
			{
				float gain = (1.0f - MathF.Exp(-velocityChange * 0.1f)) * 0.5f;
				Audio.PlayOrganic(player.landSound, position, gain);
				AIManager.NotifySound(player.position, 5.0f);
			}

			if (speed > FALL_DMG_THRESHHOLD)
			{
				float fallDamage = MathF.Pow(speed - FALL_DMG_THRESHHOLD, 2) * 0.1f;
				player.hit(fallDamage);
			}
			*/
		};

		playerBody = new RigidBody(this, RigidBodyType.Kinematic, PhysicsFilter.PlayerHitbox, 0);
		playerBody.addCapsuleCollider(FirstPersonController.COLLIDER_RADIUS + 0.1f, FirstPersonController.COLLIDER_HEIGHT, Vector3.Up * FirstPersonController.COLLIDER_HEIGHT * 0.5f, Quaternion.Identity);

		//setRightWeapon(new KingsSword());
		//setGloves(new LeatherGauntlets());
		//setRing(0, new SapphireRing());
		//setRightWeapon(new Torch());
	}

	void playSound(Sound[] sound)
	{
		Audio.Play(sound, camera.position + camera.rotation.forward);
	}

	public void setRotation(float yaw)
	{
		this.yaw = yaw;
		rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw);
	}

	public void giveItem(Item item)
	{
		if (item.type == ItemType.Weapon)
		{
			if (rightWeapon != null)
				dropItem(rightWeapon);
			setRightWeapon(item);
		}
		else if (item.type == ItemType.Ring)
		{
			setRing(0, item);
		}
	}

	public ItemEntity dropItem(Item item)
	{
		ItemEntity entity = new ItemEntity(item);
		GameState.instance.scene.addEntity(entity, rightWeaponTransform);
		entity.body.setVelocity(camera.rotation.forward * 2);
		return entity;
	}

	public void throwWeapon(int hand)
	{
		actionManager.queueAction(new ThrowItemAction(hand));
	}

	bool isArmBone(Node node, string suffix)
	{
		bool arm = node.name.StartsWith("Shoulder") || node.name.StartsWith("Arm") || node.name.StartsWith("Hand") || node.name.StartsWith("Thumb") || node.name.StartsWith("Finger") || node.name.StartsWith("Weapon");
		if (arm)
		{
			if (suffix != null)
				return node.name.EndsWith(suffix);
			return true;
		}
		return false;
	}

	public void setRightWeapon(Item item)
	{
		if (rightWeaponAnimator != null)
		{
			Animator.Destroy(rightWeaponAnimator);
			rightWeaponAnimator = null;
		}
		if (rightWeaponLight != null)
		{
			rightWeaponLight.destroy(Renderer.graphics);
			rightWeaponLight = null;
		}
		if (rightWeaponParticles != null)
		{
			ParticleSystem.Destroy(rightWeaponParticles);
			rightWeaponParticles = null;
		}
		rightWeapon = item;
		if (item != null)
		{
			Model model = item.model;
			Model moveset = item.moveset;
			actionManager.queueAction(new EquipAction(item, 0));

			rightWeaponModel = model;
			rightWeaponMoveset = moveset;
			if (model.isAnimated)
				rightWeaponAnimator = Animator.Create(model);
			if (item.entityData.lights.Count > 0)
				rightWeaponLight = new PointLight(item.entityData.lights[0].offset, item.entityData.lights[0].color * item.entityData.lights[0].intensity);
			if (item.entityData.particles.Length > 0)
			{
				rightWeaponParticles = ParticleSystem.Create(rightWeaponTransform);
				rightWeaponParticles.setData(item.entityData.particles[0]);
			}

			if (rightHandBoneMask == null)
			{
				Debug.Assert(leftHandBoneMask == null);

				rightHandBoneMask = new bool[moveset.skeleton.nodes.Length];
				leftHandBoneMask = new bool[moveset.skeleton.nodes.Length];
				for (int i = 0; i < moveset.skeleton.nodes.Length; i++)
				{
					Node node = moveset.skeleton.nodes[i];
					if (isArmBone(node, ".R"))
						rightHandBoneMask[i] = true;
					else if (isArmBone(node, ".L"))
						leftHandBoneMask[i] = true;
					else
					{
						rightHandBoneMask[i] = false;
						leftHandBoneMask[i] = false;
					}
				}
			}

			idleAnim.layers[1] = new AnimationLayer(moveset, "idle", true, rightHandBoneMask);
			runAnim.layers[1] = new AnimationLayer(moveset, "idle", true, rightHandBoneMask);
			fallAnim.layers[1] = new AnimationLayer(moveset, "idle", true, rightHandBoneMask);
			duckAnim.layers[1] = new AnimationLayer(moveset, "idle", true, rightHandBoneMask);
			sneakAnim.layers[1] = new AnimationLayer(moveset, "idle", true, rightHandBoneMask);

			if (item.twoHanded)
			{
				idleAnim.layers[2] = new AnimationLayer(moveset, "idle", true, leftHandBoneMask);
				runAnim.layers[2] = new AnimationLayer(moveset, "idle", true, leftHandBoneMask);
				fallAnim.layers[2] = new AnimationLayer(moveset, "idle", true, leftHandBoneMask);
				duckAnim.layers[2] = new AnimationLayer(moveset, "idle", true, leftHandBoneMask);
				sneakAnim.layers[2] = new AnimationLayer(moveset, "idle", true, leftHandBoneMask);
			}
			else
			{
				idleAnim.layers[2] = null;
				runAnim.layers[2] = null;
				fallAnim.layers[2] = null;
				duckAnim.layers[2] = null;
				sneakAnim.layers[2] = null;
			}
		}
		else
		{
			rightWeaponModel = null;
			rightWeaponMoveset = null;
			rightWeaponLight = null;
			rightWeaponParticles = null;

			idleAnim.layers[1] = null;
			runAnim.layers[1] = null;
			fallAnim.layers[1] = null;
			duckAnim.layers[1] = null;
			sneakAnim.layers[1] = null;

			idleAnim.layers[2] = null;
			runAnim.layers[2] = null;
			fallAnim.layers[2] = null;
			duckAnim.layers[2] = null;
			sneakAnim.layers[2] = null;
		}
	}

	public void setWeapon(int hand, Item weapon)
	{
		if (hand == 0)
			setRightWeapon(weapon);
		else
			//	setLeftWeapon(weapon);
			Debug.Assert(false);
	}

	public Item getWeapon(int hand)
	{
		return hand == 0 ? rightWeapon : leftWeapon;
	}

	public void setGloves(Item item)
	{
		if (glovesAnimator != null)
		{
			Animator.Destroy(glovesAnimator);
			glovesAnimator = null;
		}

		gloves = item;

		if (item != null)
		{
			if (item.model.isAnimated)
				glovesAnimator = Animator.Create(item.model);
			if (item.equipSound != null)
				playSound(item.equipSound);
		}
	}

	public void setRing(int idx, Item item)
	{
		rings[idx] = item;

		if (item != null && item.equipSound != null)
			playSound(item.equipSound);
	}

	public void hit(int damage, bool criticalHit, Vector3 hitDirection, Entity by, Item item, RigidBody hitbox)
	{
		if (actionManager.currentAction != null && actionManager.currentAction is ParryAction)
		{
			ParryAction parryAction = actionManager.currentAction as ParryAction;
			if (parryAction.inParryWindow)
			{
				damage = 0;

				actionManager.cancelAllActions();
				actionManager.queueAction(new ParryHitAction(parryAction.weapon, parryAction.hand));
			}
		}

		if (damage > 0)
		{
			health -= damage;
			//if (health <= 0)
			//	death();
			//else
			{

			}

			HUD.OnPlayerHit();
		}
	}

	void updateMovement()
	{
		controller.inputLeft = false;
		controller.inputRight = false;
		controller.inputUp = false;
		controller.inputDown = false;
		controller.inputJump = false;
		controller.inputDuck = false;

		if (actionManager.currentAction == null || !actionManager.currentAction.lockMovement)
		{
			controller.inputLeft = Input.IsKeyDown(KeyCode.A);
			controller.inputRight = Input.IsKeyDown(KeyCode.D);
			controller.inputUp = Input.IsKeyDown(KeyCode.W);
			controller.inputDown = Input.IsKeyDown(KeyCode.S);
		}

		if (actionManager.currentAction != null)
		{
			controller.inputLeft |= actionManager.currentAction.inputLeft;
			controller.inputRight |= actionManager.currentAction.inputRight;
			controller.inputUp |= actionManager.currentAction.inputForward;
			controller.inputDown |= actionManager.currentAction.inputBack;
		}

		controller.inputJump = Input.IsKeyPressed(KeyCode.Space);
		controller.inputDuck = InputManager.IsDown("Duck");

		isSprinting = InputManager.IsDown("Sprint");
		controller.maxSpeed = getCurrentMaxSpeed();

		controller.rootMotionDisplacement = Vector3.Zero;
		if (actionManager.currentAction != null && actionManager.currentAction.rootMotion)
		{
			animator.getRootMotion(out Vector3 rootMotion, out _, out bool hasLooped);
			if (!hasLooped && lastRootMotion != Vector3.One && actionManager.currentAction == lastRootMotionAction)
			{
				controller.rootMotionDisplacement = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI + yaw) * (rootMotion - lastRootMotion);
			}
			lastRootMotion = rootMotion;
			lastRootMotionAction = actionManager.currentAction;
		}

		controller.update();

		pitch = MathHelper.Clamp(pitch - Input.cursorMove.y * 0.001f, -MathF.PI * 0.5f, MathF.PI * 0.5f);
		if ((actionManager.currentAction == null || !actionManager.currentAction.lockYaw) && controller.currentLadder == null)
		{
			yaw -= Input.cursorMove.x * 0.001f;
			if (camerayaw != 0)
			{
				float newCameraYaw = MathHelper.Lerp(camerayaw, 0, 10 * Time.deltaTime);
				if (MathF.Abs(newCameraYaw) < 0.0001f)
					newCameraYaw = 0;
				yaw += camerayaw - newCameraYaw;
				camerayaw = newCameraYaw;
			}
		}
		else if (actionManager.currentAction == null || !actionManager.currentAction.lockCameraRotation || controller.currentLadder != null)
		{
			camerayaw = MathHelper.Clamp(camerayaw - Input.cursorMove.x * 0.001f, -MathF.PI * 0.75f, MathF.PI * 0.75f);
		}

		rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw);

		playerBody.setTransform(position, rotation);


		Matrix weaponSway = calculateWeaponSway(); // actionManager.currentAction == null || !actionManager.currentAction.lockYaw ?  : Matrix.Identity;

		cameraHeight = controller.isDucked ? CAMERA_HEIGHT_DUCKED :
			controller.inDuckTimer != -1 ? MathHelper.Lerp(CAMERA_HEIGHT, CAMERA_HEIGHT_DUCKED, controller.inDuckTimer / FirstPersonController.DUCK_TRANSITION_DURATION) :
			controller.isGrounded ? MathHelper.Linear(cameraHeight, CAMERA_HEIGHT, 2 * Time.deltaTime) :
			CAMERA_HEIGHT;

		//modelTransform = Matrix.CreateRotation(Vector3.Up, MathF.PI);
		//modelTransform = weaponSway * modelTransform;
		//if (actionManager.currentAction == null || !actionManager.currentAction.ignorePitch)
		//	modelTransform = Matrix.CreateRotation(Vector3.Right, pitch) * modelTransform;
		//modelTransform = Matrix.CreateTranslation(0, cameraHeight, 0) * modelTransform;
	}

	float getCurrentMaxSpeed()
	{
		const float defaultSpeed = 3.2f; // 3.6f;
		float speed = defaultSpeed;
		if (actionManager.currentAction != null)
			speed *= actionManager.currentAction.movementSpeedMultiplier;
		else if (controller.isDucked)
			speed *= DUCK_SPEED_MULTIPLIER;
		else if (isSprinting)
			speed *= SPRINT_SPEED_MULTIPLIER;
		return speed;
	}

	void updateActions()
	{
		if (Input.IsKeyPressed(KeyCode.O))
			actionManager.queueAction(new SitAction());
		if (Input.IsKeyPressed(KeyCode.P))
			actionManager.queueAction(new TestSwordHold(0));
		if (Input.IsKeyPressed(KeyCode.F))
			actionManager.queueAction(new HealAction(1));
		if (Input.IsKeyPressed(KeyCode.V))
			actionManager.queueAction(new KickAction());
		if (Input.IsKeyPressed(KeyCode.G))
			throwWeapon(0);

		if (Input.IsKeyPressed(KeyCode.F5))
			cameraMode = (cameraMode == CameraMode.FirstPerson ? CameraMode.ThirdPerson : CameraMode.FirstPerson);

		if (rightWeapon != null)
		{
			if (InputManager.IsPressed("Attack1", true) || InputManager.IsDown("Attack1") && !rightWeapon.useTrigger && actionManager.currentAction == null)
			{
				//if (InputManager.IsDown("Charge"))
				//{
				//	rightWeapon.useCharged(this, 0);
				//	lastRightWeaponDown = -1;
				//}
				//else
				{
					rightWeapon.use(this, 0);
					lastRightWeaponDown = Time.currentTime;
				}
			}
			else if (InputManager.IsDown("Attack1") && rightWeapon.useTrigger && lastRightWeaponDown != -1 && (Time.currentTime - lastRightWeaponDown) / 1e9f > WEAPON_CHARGE_TIME)
			{
				rightWeapon.useCharged(this, 0);
				lastRightWeaponDown = -1;
			}
		}
		if (rightWeapon != null && (rightWeapon.twoHanded || leftWeapon == null))
		{
			if (InputManager.IsPressed("Attack2", true) || InputManager.IsDown("Attack2") && !rightWeapon.secondaryUseTrigger && actionManager.currentAction == null)
			{
				rightWeapon.useSecondary(this, 0);
			}
		}

		if (rightWeapon != null)
			rightWeapon.update(this, rightWeaponAnimator);

		actionManager.update();

		interactableInFocus = null;
		Span<HitData> hits = stackalloc HitData[16];
		int numHits = Physics.Raycast(camera.position, camera.rotation.forward, REACH_DISTANCE, hits, QueryFilterFlags.Default, PhysicsFilter.Interactable);
		for (int i = 0; i < numHits; i++)
		{
			Interactable interactable = hits[i].body.entity as Interactable;
			if (interactable.canInteract(this))
			{
				interactableInFocus = interactable;
				break;
			}
		}

		if (interactableInFocus != null && InputManager.IsPressed("Interact", true))
		{
			interactableInFocus.interact(this);
		}
	}

	Simplex simplex = new Simplex();
	float viewmodelVerticalSpeedAnim;
	Vector3 viewmodelLookSwayAnim;
	Matrix calculateWeaponSway()
	{
		Vector3 sway = Vector3.Zero;
		float yawSway = 0;
		float pitchSway = 0;
		float rollSway = 0;

		float swayScale = actionManager.currentAction != null ? actionManager.currentAction.swayAmount : 1;

		// Idle animation
		float idleProgress = Time.currentTime / 1e9f * MathF.PI * 2 / 6.0f;
		float idleAnimation = (MathF.Cos(idleProgress) * 0.5f - 0.5f) * 0.03f;
		sway.y += idleAnimation;
		Vector3 noise = new Vector3(simplex.sample1f(idleProgress * 0.2f), simplex.sample1f(-idleProgress * 0.2f), simplex.sample1f(100 + idleProgress * 0.2f)) * 0.0075f * swayScale;
		sway += noise;

		// Walk animation
		Vector2 viewmodelWalkAnim = Vector2.Zero;
		viewmodelWalkAnim.x = 0.03f * MathF.Sin(controller.distanceWalked * FirstPersonController.STEP_FREQUENCY * MathF.PI);
		viewmodelWalkAnim.y = 0.015f * (0.5f - MathF.Abs(MathF.Cos(controller.distanceWalked * FirstPersonController.STEP_FREQUENCY * MathF.PI)));
		//viewmodelWalkAnim *= 1 - MathHelper.Smoothstep(1.0f, 1.5f, movementSpeed);
		viewmodelWalkAnim *= 1 - MathF.Exp(-controller.velocity.xz.length);
		//viewmodelWalkAnim *= (sprinting && runAnim.layers[1 + 0] != null && runAnim.layers[1 + 0].animationName == "run" || movement.isMoving && walkAnim.layers[1 + 0] != null && walkAnim.layers[1 + 0].animationName == "walk") ? 0 : 1;
		yawSway += viewmodelWalkAnim.x;
		sway.y += viewmodelWalkAnim.y;

		// Vertical speed animation
		float verticalSpeedAnimDst = controller.velocity.y;
		verticalSpeedAnimDst = Math.Clamp(verticalSpeedAnimDst, -5.0f, 5.0f);
		viewmodelVerticalSpeedAnim = MathHelper.Lerp(viewmodelVerticalSpeedAnim, verticalSpeedAnimDst * 0.0075f, 5 * Time.deltaTime);
		pitchSway += viewmodelVerticalSpeedAnim;

		// Land bob animation
		float timeSinceLanding = (Time.currentTime - controller.lastLandedTime) / 1e9f;
		float landBob = (1.0f - MathF.Pow(0.5f, timeSinceLanding * 4.0f)) * MathF.Pow(0.1f, timeSinceLanding * 4.0f) * 0.5f;
		sway.y -= landBob;

		// Look sway
		float swayYawDst = -0.3f * InputManager.lookVector.x;
		float swayPitchDst = -0.7f * InputManager.lookVector.y;
		float swayRollDst = -0.3f * InputManager.lookVector.x;
		viewmodelLookSwayAnim = Vector3.Lerp(viewmodelLookSwayAnim, new Vector3(swayPitchDst, swayYawDst, swayRollDst), 5.0f * Time.deltaTime);
		pitchSway += viewmodelLookSwayAnim.x;
		yawSway += viewmodelLookSwayAnim.y;
		rollSway += viewmodelLookSwayAnim.z;

		sway *= swayScale;
		pitchSway *= swayScale;
		yawSway *= swayScale;
		rollSway *= swayScale;

		return Matrix.CreateTranslation(sway) * Matrix.CreateRotation(Vector3.Up, yawSway) * Matrix.CreateRotation(Vector3.Right, pitchSway) * Matrix.CreateRotation(Vector3.Back, rollSway);
	}

	void updateAnimations()
	{
		runAnim.animationSpeed = (isSprinting ? SPRINT_SPEED_MULTIPLIER : 1) * (FirstPersonController.STEP_FREQUENCY / 0.6f / 1.5f);
		//bodyRunAnim.animationSpeed = (isSprinting ? SPRINT_SPEED_MULTIPLIER : 1) * (FirstPersonController.STEP_FREQUENCY / 0.6f / 1.5f);

		if (actionManager.currentAction != null)
			currentActionAnim.animationSpeed = actionManager.currentAction.animationSpeed;

		//AnimationState moveAnim = controller.isMoving && controller.isGrounded ? runAnim : idleAnim;
		AnimationState moveAnim = !controller.isGrounded ? fallAnim : controller.inDuckTimer != -1 ? (controller.isMoving ? sneakAnim : duckAnim) : controller.isMoving ? runAnim : idleAnim;

		if (actionManager.currentAction != null && actionManager.currentAction.animationName[2] != null)
		{
			animator.setAnimation(currentActionAnim);
			animator.timer = actionManager.currentAction.elapsedTime;
		}
		else
		{
			animator.setAnimation(moveAnim);
			animator.timer = controller.distanceWalked / controller.maxSpeed;
		}

		if (actionManager.currentAction != null && actionManager.currentAction.animationName[0] != null)
		{
			rightHandAnimator.setAnimation(currentActionAnim);
			rightHandAnimator.timer = actionManager.currentAction.elapsedTime;
		}
		else
		{
			rightHandAnimator.setAnimation(moveAnim);
			rightHandAnimator.timer = animator.timer;
		}

		if (actionManager.currentAction != null && actionManager.currentAction.animationName[1] != null)
		{
			leftHandAnimator.setAnimation(currentActionAnim);
			leftHandAnimator.timer = actionManager.currentAction.elapsedTime;
		}
		else
		{
			leftHandAnimator.setAnimation(moveAnim);
			leftHandAnimator.timer = animator.timer;
		}

		rightHandAnimator.applyAnimation();
		leftHandAnimator.applyAnimation();

		for (int i = 0; i < model.skeleton.nodes.Length; i++)
		{
			Node node = model.skeleton.nodes[i];
			if (isArmBone(node, ".R"))
				animator.setNodeLocalTransform(node, rightHandAnimator.getNodeLocalTransform(node));
			if (isArmBone(node, ".L"))
				animator.setNodeLocalTransform(node, leftHandAnimator.getNodeLocalTransform(node));
			//else
			//	Console.Error.WriteLine("Invalid arm bone " + node.name);
		}

		{
			float itemViewmodelAim = rightWeapon != null ? rightWeapon.viewmodelAim : DEFAULT_VIEWMODEL_AIM;
			float viewmodelAim = actionManager.currentAction != null ? actionManager.currentAction.viewmodelAim[0] : itemViewmodelAim;
			rightViewmodelAim = MathHelper.Lerp(rightViewmodelAim, viewmodelAim, (viewmodelAim > rightViewmodelAim ? 10 : 2) * Time.deltaTime);
		}
		{
			float itemViewmodelAim = leftWeapon != null ? leftWeapon.viewmodelAim : DEFAULT_VIEWMODEL_AIM;
			float viewmodelAim = actionManager.currentAction != null ? actionManager.currentAction.viewmodelAim[1] : itemViewmodelAim;
			leftViewmodelAim = MathHelper.Lerp(leftViewmodelAim, viewmodelAim, (viewmodelAim > leftViewmodelAim ? 10 : 2) * Time.deltaTime);
		}

		if (rightWeaponAnimator != null)
		{
			rightWeaponAnimator.applyAnimation();
		}


		Matrix spine2Transform = animator.getNodeLocalTransform(spineNode);
		spine2Transform = Matrix.CreateRotation(Vector3.Left, 0.1f * pitch) * spine2Transform;
		animator.setNodeLocalTransform(spineNode, spine2Transform);

		Matrix spine3Transform = animator.getNodeLocalTransform(chestNode);
		spine3Transform = Matrix.CreateRotation(Vector3.Left, 0.1f * pitch) * spine3Transform;
		animator.setNodeLocalTransform(chestNode, spine3Transform);

		// ignore neck animation, so the viewmodel doesn't move around after applying correction factor
		animator.setNodeLocalTransform(neckNode, defaultPoseNeckTransform);
		Matrix cameraBeforeNeckTransform = animator.getNodeTransform(cameraNode);

		Matrix spine4Transform = animator.getNodeLocalTransform(neckNode);
		spine4Transform = Matrix.CreateRotation(Vector3.Left, 0.8f * pitch) * spine4Transform;
		animator.setNodeLocalTransform(neckNode, spine4Transform);

		bool animateCamera = actionManager.currentAction != null && actionManager.currentAction.animateCameraRotation;
		if (!animateCamera)
		{
			// This makes it so that the camera is looking straight in default pose, which matches the preview in blender.
			// Otherwise it will look down since the bone is not pointing straight up
			Matrix cameraAfterNeckTransform = animator.getNodeTransform(cameraNode);
			Matrix rotationCorrection = Matrix.CreateTranslation(cameraAfterNeckTransform.translation) * Matrix.CreateRotation(Vector3.Left, pitch) * cameraAfterNeckTransform.inverted;
			Matrix correctedCameraAnimation = rotationCorrection * cameraAfterNeckTransform * defaultPoseCameraRotation;
			animator.setNodeTransform(cameraNode, correctedCameraAnimation);

			Matrix cameraDelta = Matrix.CreateTranslation(cameraAfterNeckTransform.translation) * Matrix.CreateRotation(Vector3.Left, pitch) * defaultPoseCameraRotation * cameraBeforeNeckTransform.inverted; // cameraAnimation * defaultPoseCameraRotation * cameraBeforeNeckTransform.inverted;

			Matrix weaponSway = calculateWeaponSway();

			Matrix chestTransform = animator.getNodeTransform(chestNode);

			Matrix rightViewmodelAimTransform = (rightWeapon != null ? chestTransform * weaponSway * chestTransform.inverted : Matrix.Identity) * Matrix.LerpTransform(Matrix.Identity, cameraDelta, rightViewmodelAim);
			Matrix leftViewmodelAimTransform = (leftWeapon != null ? chestTransform * weaponSway * chestTransform.inverted : Matrix.Identity) * Matrix.LerpTransform(Matrix.Identity, cameraDelta, leftViewmodelAim);

			animator.setNodeTransform(rightShoulderNode, rightViewmodelAimTransform * animator.getNodeTransform(rightShoulderNode));
			animator.setNodeTransform(leftShoulderNode, leftViewmodelAimTransform * animator.getNodeTransform(leftShoulderNode));

			animator.setNodeTransform(rightWeaponNode, rightViewmodelAimTransform * animator.getNodeTransform(rightWeaponNode));
			animator.setNodeTransform(leftWeaponNode, leftViewmodelAimTransform * animator.getNodeTransform(leftWeaponNode));
		}
		/*
		*/

		rightWeaponTransform = getModelMatrix() * modelTransform * animator.getNodeTransform(rightWeaponNode) * Matrix.CreateRotation(Vector3.Right, MathF.PI * 0.5f);
		leftWeaponTransform = getModelMatrix() * modelTransform * animator.getNodeTransform(leftWeaponNode);

		//camera.setTransform(getModelMatrix() * Matrix.CreateTranslation(0, cameraHeight, 0) * Matrix.CreateRotation(Vector3.Up, camerayaw) * Matrix.CreateRotation(Vector3.Right, pitch) * animator.getNodeLocalTransform(cameraNode) * leftHandAnimator.getNodeLocalTransform(cameraNode) * rightHandAnimator.getNodeLocalTransform(cameraNode));
		Vector3 cameraOffset = cameraMode == CameraMode.FirstPerson ? Vector3.Zero : new Vector3(0, 0.5f, 2);
		camera.setTransform(getModelMatrix() * Matrix.CreateRotation(Vector3.Up, MathF.PI) * animator.getNodeTransform(cameraNode) * defaultPoseCameraRotation.inverted * Matrix.CreateTranslation(cameraOffset.x, cameraOffset.y, -cameraOffset.z) * Matrix.CreateRotation(Vector3.Up, -MathF.PI));

		animator.applyAnimation();
	}

	public override void update()
	{
		updateMovement();
		updateActions();
		updateAnimations();

		if (rightWeaponParticles != null)
			rightWeaponParticles.setTransform(rightWeaponTransform);
	}

	public override void fixedUpdate(float delta)
	{
		if (actionManager.currentAction != null)
			actionManager.currentAction.fixedUpdate(this, delta);
	}

	public AnimationState getNextActionAnimationState()
	{
		currentActionAnim = currentActionAnim == actionAnim1 ? actionAnim2 : currentActionAnim == actionAnim2 ? actionAnim1 : actionAnim1;
		return currentActionAnim;
	}

	public override void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix() * modelTransform;

		//if ((gloves == null || !gloves.hidesArms) && (bodyArmor == null || !bodyArmor.hidesArms))
		//	Renderer.DrawMesh(model, 0, transform, animator); // arms
		//if (gloves == null || !gloves.hidesHands)
		//	Renderer.DrawMesh(model, 1, transform, animator); // hands

		Model rightWeaponModel = this.rightWeaponModel;
		if (actionManager.currentAction != null && actionManager.currentAction.overrideWeaponModel[0])
			rightWeaponModel = actionManager.currentAction.weaponModel[0];
		if (rightWeaponModel != null)
			Renderer.DrawModel(rightWeaponModel, rightWeaponTransform, rightWeaponAnimator);
		if (rightWeaponLight != null)
			Renderer.DrawPointLight(rightWeaponLight, rightWeaponTransform);
		if (rightWeaponParticles != null)
			Renderer.DrawParticleSystem(rightWeaponParticles);

		Model leftWeaponModel = this.leftWeaponModel;
		if (actionManager.currentAction != null && actionManager.currentAction.overrideWeaponModel[1])
			leftWeaponModel = actionManager.currentAction.weaponModel[1];
		if (leftWeaponModel != null)
			Renderer.DrawModel(leftWeaponModel, leftWeaponTransform, leftWeaponAnimator);

		if (gloves != null)
			Renderer.DrawModel(gloves.model, transform, animator);

		Matrix ringRenderTransform = Matrix.CreateRotation(Vector3.Right, MathF.PI * -0.5f) * Matrix.CreateTranslation(0, -0.007f, 0.01f) * Matrix.CreateScale(0.24f);
		for (int i = 0; i < rings.Length; i++)
		{
			if (rings[i] != null)
			{
				Renderer.DrawModel(rings[i].model, transform * animator.getNodeTransform(ringNodes[i]) * ringRenderTransform);
			}
		}

		Matrix bodyTransform = getModelMatrix()
			//* Matrix.CreateTranslation(bodyAnimator.getNodeTransform(spine005Node).translation * new Vector3(1, 0, 1) + new Vector3(0, cameraHeight - bodyAnimator.getNodeTransform(spine005Node).translation.y, 0.2f))
			//* Matrix.CreateTranslation(-bodyAnimator.getNodeTransform(cameraAnchorNode).translation * new Vector3(1, 0, 1))
			//* Matrix.CreateRotation(Vector3.Up, MathF.PI);
			* modelTransform;
		Renderer.DrawModel(model, bodyTransform, animator);

		if (rightWeapon != null)
			rightWeapon.draw(this, rightWeaponAnimator);
		if (leftWeapon != null)
			leftWeapon.draw(this, leftWeaponAnimator);
		if (helmet != null)
			helmet.draw(this, helmetAnimator);
		if (bodyArmor != null)
			bodyArmor.draw(this, bodyArmorAnimator);
		if (gloves != null)
			gloves.draw(this, glovesAnimator);
		if (boots != null)
			boots.draw(this, bootsAnimator);

		if (actionManager.currentAction != null)
			actionManager.currentAction.draw(this);

		//Renderer.DrawLight(camera.position, new Vector3(2));

		HUD.Draw();
	}

	public Vector3 center => position + Vector3.Up * controller.controller.height * 0.5f;
}
