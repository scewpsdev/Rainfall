using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Player : Entity
{
	public const float DEFAULT_VIEWMODEL_AIM = 0.2f;
	const float SPRINT_SPEED_MULTIPLIER = 2.0f;
	const float DUCK_SPEED_MULTIPLIER = 0.5f;
	const float REACH_DISTANCE = 3.0f;
	const float CAMERA_HEIGHT = 1.5f;
	const float CAMERA_HEIGHT_DUCKED = 0.9f;
	const float WEAPON_CHARGE_TIME = 0.2f;

	public Camera camera;
	public float pitch, yaw;
	float camerayaw;
	float cameraHeight;

	bool isSprinting;

	float currentViewmodelAim = 0;

	AnimationState idleAnim;
	AnimationState runAnim;
	AnimationState fallAnim;

	Node weaponRNode, weaponLNode;

	Animator rightHandAnimator, leftHandAnimator;
	bool[] rightHandBoneMask, leftHandBoneMask;

	Model rightWeaponModel, leftWeaponModel;
	Model rightWeaponMoveset, leftWeaponMoveset;
	Animator rightWeaponAnimator, leftWeaponAnimator;

	public Matrix rightWeaponTransform;
	public Matrix leftWeaponTransform;

	public FirstPersonController controller;
	RigidBody playerBody;
	public ActionManager actionManager;
	AnimationState actionAnim1, actionAnim2;
	public AnimationState currentActionAnim;

	public Item rightWeapon, leftWeapon;
	public long lastRightWeaponDown = -1, lastLeftWeaponDown = -1;

	public Interactable interactableInFocus = null;


	public Player(Camera camera)
	{
		this.camera = camera;

		camera.fov = 90;

		model = Resource.GetModel("viewmodel.gltf");
		modelTransform = Matrix.CreateTranslation(0, cameraHeight, 0) * Matrix.CreateRotation(Vector3.Up, MathF.PI);
		animator = Animator.Create(model, this);

		animator.setAnimation(Animator.CreateAnimation(model, "default"));
		animator.update();
		animator.applyAnimation();

		weaponRNode = model.skeleton.getNode("weapon_r");
		weaponLNode = model.skeleton.getNode("weapon_l");

		idleAnim = Animator.CreateAnimation(model, [new AnimationLayer(model, "idle", true), null, null], 0.4f);
		runAnim = Animator.CreateAnimation(model, [new AnimationLayer(model, "run", true), null, null], 0.4f);
		fallAnim = Animator.CreateAnimation(model, [new AnimationLayer(model, /*"fall"*/ "idle", true), null, null], 0.1f);
		actionAnim1 = Animator.CreateAnimation(model, [new AnimationLayer(model, "default", false), null, null], 0.1f);
		actionAnim2 = Animator.CreateAnimation(model, [new AnimationLayer(model, "default", false), null, null], 0.1f);

		rightHandAnimator = Animator.Create(model, this);
		leftHandAnimator = Animator.Create(model, this);

		rightHandBoneMask = new bool[model.skeleton.nodes.Length];
		leftHandBoneMask = new bool[model.skeleton.nodes.Length];
		for (int i = 0; i < model.skeleton.nodes.Length; i++)
		{
			Node node = model.skeleton.nodes[i];
			if (node.name.EndsWith("_r"))
				rightHandBoneMask[i] = true;
			else if (node.name.EndsWith("_l"))
				leftHandBoneMask[i] = true;
			else
			{
				rightHandBoneMask[i] = true;
				leftHandBoneMask[i] = true;
			}
		}

		actionManager = new ActionManager(this);

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

		playerBody = new RigidBody(this, RigidBodyType.Kinematic, PhysicsFilter.Player, 0);
		playerBody.addCapsuleCollider(FirstPersonController.COLLIDER_RADIUS, FirstPersonController.COLLIDER_HEIGHT, Vector3.Up * FirstPersonController.COLLIDER_HEIGHT * 0.5f, Quaternion.Identity);

		setRightWeapon(new Longsword());
	}

	public void setRotation(float yaw)
	{
		this.yaw = yaw;
		rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw);
	}

	public void setRightWeapon(Item item)
	{
		if (rightWeaponAnimator != null)
		{
			Animator.Destroy(rightWeaponAnimator);
			rightWeaponAnimator = null;
		}

		rightWeapon = item;

		Model model = item.model;
		Model moveset = item.moveset;

		rightWeaponModel = model;
		rightWeaponMoveset = moveset;
		if (model.isAnimated)
			rightWeaponAnimator = Animator.Create(model);

		idleAnim.layers[1] = new AnimationLayer(moveset, "idle", true, rightHandBoneMask);
		runAnim.layers[1] = new AnimationLayer(moveset, "idle", true, rightHandBoneMask);
		fallAnim.layers[1] = new AnimationLayer(moveset, "idle", true, rightHandBoneMask);

		if (item.twoHanded)
		{
			idleAnim.layers[2] = new AnimationLayer(moveset, "idle", true, leftHandBoneMask);
			runAnim.layers[2] = new AnimationLayer(moveset, "idle", true, leftHandBoneMask);
			fallAnim.layers[2] = new AnimationLayer(moveset, "idle", true, leftHandBoneMask);
		}
		else
		{
			idleAnim.layers[2] = null;
			runAnim.layers[2] = null;
			fallAnim.layers[2] = null;
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


		Matrix weaponSway = actionManager.currentAction == null || !actionManager.currentAction.lockYaw ? calculateWeaponSway() : Matrix.Identity;

		cameraHeight = controller.isDucked ? CAMERA_HEIGHT_DUCKED :
			controller.inDuckTimer != -1 ? MathHelper.Lerp(CAMERA_HEIGHT, CAMERA_HEIGHT_DUCKED, controller.inDuckTimer / FirstPersonController.DUCK_TRANSITION_DURATION) :
			controller.isGrounded ? MathHelper.Linear(cameraHeight, CAMERA_HEIGHT, 5 * Time.deltaTime) :
			CAMERA_HEIGHT;

		camera.setTransform(getModelMatrix() * Matrix.CreateTranslation(0, cameraHeight, 0) * Matrix.CreateRotation(Vector3.Up, camerayaw) * Matrix.CreateRotation(Vector3.Right, pitch));

		modelTransform = Matrix.CreateRotation(Vector3.Up, MathF.PI);
		if (actionManager.currentAction == null || !actionManager.currentAction.ignorePitch)
			modelTransform = Matrix.CreateRotation(Vector3.Right, pitch) * weaponSway * modelTransform;
		modelTransform = Matrix.CreateTranslation(0, cameraHeight, 0) * modelTransform;

		rightWeaponTransform = getModelMatrix() * modelTransform * rightHandAnimator.getNodeTransform(weaponRNode);
		leftWeaponTransform = getModelMatrix() * modelTransform * leftHandAnimator.getNodeTransform(weaponLNode);
	}

	float getCurrentMaxSpeed()
	{
		const float defaultSpeed = 3.6f;
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

		if (rightWeapon != null)
		{
			if (InputManager.IsPressed("Attack1", true) || InputManager.IsDown("Attack1") && !rightWeapon.useTrigger && actionManager.currentAction == null)
			{
				rightWeapon.use(this, 0);
				lastRightWeaponDown = Time.currentTime;
			}
			else if (InputManager.IsDown("Attack1") && rightWeapon.useTrigger && lastRightWeaponDown != -1 && (Time.currentTime - lastRightWeaponDown) / 1e9f > WEAPON_CHARGE_TIME)
			{
				rightWeapon.useCharged(this, 0);
				lastRightWeaponDown = -1;
			}
		}
		if (rightWeapon != null && rightWeapon.twoHanded)
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

	void updateAnimations()
	{
		runAnim.animationSpeed = isSprinting ? SPRINT_SPEED_MULTIPLIER : 1.0f;

		AnimationState moveAnim = !controller.isGrounded ? fallAnim : controller.isMoving ? runAnim : idleAnim;

		if (actionManager.currentAction != null && actionManager.currentAction.fullBodyAnimation)
			animator.setAnimation(currentActionAnim);
		else
			animator.setAnimation(moveAnim);

		if (actionManager.currentAction != null && actionManager.currentAction.animationName[0] != null)
		{
			rightHandAnimator.setAnimation(currentActionAnim);
			currentActionAnim.animationSpeed = actionManager.currentAction.animationSpeed;
			rightHandAnimator.timer = actionManager.currentAction.elapsedTime;
		}
		else
		{
			rightHandAnimator.setAnimation(moveAnim);
		}

		if (actionManager.currentAction != null && actionManager.currentAction.animationName[1] != null)
		{
			leftHandAnimator.setAnimation(currentActionAnim);
			currentActionAnim.animationSpeed = actionManager.currentAction.animationSpeed;
			leftHandAnimator.timer = actionManager.currentAction.elapsedTime;
		}
		else
		{
			leftHandAnimator.setAnimation(moveAnim);
		}

		rightHandAnimator.applyAnimation();
		leftHandAnimator.applyAnimation();

		for (int i = 0; i < model.skeleton.nodes.Length; i++)
		{
			Node node = model.skeleton.nodes[i];
			if (node.name.EndsWith("_r"))
				animator.setNodeLocalTransform(node, rightHandAnimator.getNodeLocalTransform(node));
			else if (node.name.EndsWith("_l"))
				animator.setNodeLocalTransform(node, leftHandAnimator.getNodeLocalTransform(node));
			//else
			//	Console.Error.WriteLine("Invalid arm bone " + node.name);
		}

		float itemViewmodelAim = rightWeapon != null ? rightWeapon.viewmodelAim : leftWeapon != null ? leftWeapon.viewmodelAim : DEFAULT_VIEWMODEL_AIM;
		float viewmodelAim = actionManager.currentAction != null ? actionManager.currentAction.viewmodelAim : itemViewmodelAim;
		currentViewmodelAim = MathHelper.Lerp(currentViewmodelAim, viewmodelAim, 5 * Time.deltaTime);

		bool animateCamera = actionManager.currentAction != null && actionManager.currentAction.animateCameraRotation;

		/*
		Matrix spine2Transform = animator.getNodeLocalTransform(spine2Node);
		spine2Transform = Matrix.CreateRotation(Vector3.Left, 0.5f * currentViewmodelAim * pitch) * spine2Transform;
		animator.setNodeLocalTransform(spine2Node, spine2Transform);

		Matrix spine3Transform = animator.getNodeLocalTransform(spine3Node);
		spine3Transform = Matrix.CreateRotation(Vector3.Left, 0.5f * currentViewmodelAim * pitch) * spine3Transform;
		animator.setNodeLocalTransform(spine3Node, spine3Transform);

		Matrix spine4Transform = animator.getNodeLocalTransform(cameraNode);
		spine4Transform = Matrix.CreateRotation(Vector3.Left, (1 - currentViewmodelAim) * pitch) * spine4Transform;
		animator.setNodeLocalTransform(cameraNode, spine4Transform);

		Matrix cameraAnimation = animator.getNodeTransform(cameraNode) * defaultPoseCameraRotation.inverted;

		if (!animateCamera)
		{
			// This makes it so that the camera is looking straight in default pose, which matches the preview in blender.
			// Otherwise it will look down since the bone is not pointing straight up
			Matrix cameraRotationStable = Matrix.CreateRotation(Vector3.Left, pitch);
			Matrix rotationCorrection = Matrix.CreateTranslation(cameraAnimation.translation) * cameraRotationStable * cameraAnimation.inverted;
			Matrix correctedCameraAnimation = rotationCorrection * cameraAnimation;
			cameraAnimation = correctedCameraAnimation;

			Matrix weaponSway = calculateWeaponSway();

			animator.setNodeTransform(shoulderRNode, weaponSway * rotationCorrection * animator.getNodeTransform(shoulderRNode));
			animator.setNodeTransform(shoulderLNode, weaponSway * rotationCorrection * animator.getNodeTransform(shoulderLNode));
		}
		*/

		animator.applyAnimation();



		if (rightWeaponAnimator != null)
		{
			rightWeaponAnimator.applyAnimation();
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

		// Idle animation
		float swayScale = actionManager.currentAction != null ? actionManager.currentAction.swayAmount : 1;
		float idleProgress = Time.currentTime / 1e9f * MathF.PI * 2 / 6.0f;
		float idleAnimation = (MathF.Cos(idleProgress) * 0.5f - 0.5f) * 0.03f * swayScale;
		sway.y += idleAnimation;
		Vector3 noise = new Vector3(simplex.sample1f(idleProgress * 0.2f), simplex.sample1f(-idleProgress * 0.2f), simplex.sample1f(100 + idleProgress * 0.2f)) * 0.0075f * swayScale;
		sway += noise;

		// Walk animation
		Vector2 viewmodelWalkAnim = Vector2.Zero;
		viewmodelWalkAnim.x = 0.03f * MathF.Sin(controller.distanceWalked * FirstPersonController.STEP_FREQUENCY * MathF.PI);
		viewmodelWalkAnim.y = 0.015f * -MathF.Abs(MathF.Cos(controller.distanceWalked * FirstPersonController.STEP_FREQUENCY * MathF.PI));
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
		float swayYawDst = -1.0f * InputManager.lookVector.x;
		float swayPitchDst = -1.0f * InputManager.lookVector.y;
		float swayRollDst = -1.0f * InputManager.lookVector.x;
		viewmodelLookSwayAnim = Vector3.Lerp(viewmodelLookSwayAnim, new Vector3(swayPitchDst, swayYawDst, swayRollDst), 5.0f * Time.deltaTime);
		pitchSway += viewmodelLookSwayAnim.x;
		yawSway += viewmodelLookSwayAnim.y;
		rollSway += viewmodelLookSwayAnim.z;

		return Matrix.CreateTranslation(sway) * Matrix.CreateRotation(Vector3.Up, yawSway) * Matrix.CreateRotation(Vector3.Right, pitchSway) * Matrix.CreateRotation(Vector3.Back, rollSway);
	}

	public override void update()
	{
		updateMovement();
		updateActions();
		updateAnimations();
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
		base.draw(graphics);

		Model rightWeaponModel = this.rightWeaponModel;
		if (actionManager.currentAction != null && actionManager.currentAction.overrideWeaponModel[0])
			rightWeaponModel = actionManager.currentAction.weaponModel[0];
		if (rightWeaponModel != null)
			Renderer.DrawModel(rightWeaponModel, rightWeaponTransform, rightWeaponAnimator);

		Model leftWeaponModel = this.leftWeaponModel;
		if (actionManager.currentAction != null && actionManager.currentAction.overrideWeaponModel[1])
			leftWeaponModel = actionManager.currentAction.weaponModel[1];
		if (leftWeaponModel != null)
			Renderer.DrawModel(leftWeaponModel, leftWeaponTransform, leftWeaponAnimator);

		if (actionManager.currentAction != null)
			actionManager.currentAction.draw(this);

		//Renderer.DrawLight(camera.position, new Vector3(2));

		HUD.Draw();
	}

	public Vector3 center => position + Vector3.Up * controller.controller.height * 0.5f;
}
