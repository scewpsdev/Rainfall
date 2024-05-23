using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class Player : Entity
{
	const float ROLL_INPUT_RELEASE_WINDOW = 0.2f;
	const float ROLL_BUFFER_WINDOW = 0.2f;
	const float JUMP_BUFFER_WINDOW = 0.2f;


	float speed = 3.6f;
	float sprintSpeed = 5.0f;
	float jumpPower = 8.0f;
	float gravity = -20;

	Vector3 movementInput;
	float direction = 0.0f;
	float currentSpeed;
	Vector3 velocity;
	float distanceWalked;
	bool isGrounded = true;
	bool isSprinting = false;

	long lastSprintInput;
	long lastRollInput;
	long lastJumpInput;

	public ActionQueue actions;

	public PlayerHand rightHand;
	public PlayerHand leftHand;
	public PlayerHand getHand(int id) => id == 0 ? rightHand : leftHand;

	public Inventory inventory;

	public Item blockingItem;
	public int blockingHand = -1;
	public Item parryingItem;
	public int parryingHand = -1;

	public readonly PlayerStats stats;

	public HUD hud;
	public InventoryUI inventoryUI;

	AudioSource audio;

	Model capeMesh;
	Cloth cape;

	public Node rootMotionNode;
	Vector3 lastRootMotion;
	AnimationState lastRootMotionAnim;

	Animator rightHandAnimator;
	Node rightWeaponNode;

	Animator leftHandAnimator;
	Node leftWeaponNode;

	AnimationState idleAnim;
	AnimationState runAnim;
	AnimationState fallAnim;
	AnimationState actionAnim1, actionAnim2;
	public AnimationState currentActionAnim;

	Simplex simplex = new Simplex(12345, 3);


	public unsafe Player()
	{
		stats = new PlayerStats(this);

		hud = new HUD(this);

		inventory = new Inventory(this);
		inventoryUI = new InventoryUI(this);

		actions = new ActionQueue(this);

		model = Resource.GetModel("res/entity/player/player.gltf");
		rootMotionNode = model.skeleton.getNode("Root");
		rightWeaponNode = model.skeleton.getNode("Weapon.R");
		leftWeaponNode = model.skeleton.getNode("Weapon.L");
		modelTransform = Matrix.CreateRotation(Vector3.Up, MathF.PI);
		animator = Animator.Create(model, this);

		idleAnim = Animator.CreateAnimation(model, [new AnimationLayer(model, "idle", true), null, null], 0.2f);
		runAnim = Animator.CreateAnimation(model, [new AnimationLayer(model, "run", true), null, null], 0.2f);
		fallAnim = Animator.CreateAnimation(model, [new AnimationLayer(model, "fall", true), null, null], 0.2f);

		actionAnim1 = Animator.CreateAnimation(model, [new AnimationLayer(model, "default", false), null, null], 0.1f);
		actionAnim2 = Animator.CreateAnimation(model, [new AnimationLayer(model, "default", false), null, null], 0.1f);

		rightHand = new PlayerHand(0, this);
		leftHand = new PlayerHand(1, this);

		rightHandAnimator = Animator.Create(model);
		leftHandAnimator = Animator.Create(model);

		capeMesh = Resource.GetModel("res/entity/player/player_cape.gltf");
		float[] clothInvMasses = new float[capeMesh.getMeshData(0)->vertexCount];
		for (int i = 0; i < clothInvMasses.Length; i++)
		{
			uint color = capeMesh.getMeshData(0)->getVertexColor(i);
			float invMass = ((color & 0x0000FF00) >> 8) / 255.0f;
			clothInvMasses[i] = invMass;
		}
		cape = new Cloth(capeMesh, clothInvMasses, new Vector3(0, 2, 0), Quaternion.Identity);
	}

	public override void init()
	{
		base.init();

		audio = new AudioSource(position + Vector3.Up);

		setHandItem(0, Item.Get("broadsword"));
		setHandItem(1, Item.Get("wooden_round_shield"));
	}

	public void playSound(Sound sound, float gain = 1)
	{
		audio.playSound(sound, gain);
	}

	public void playSoundOrganic(Sound sound, float gain = 1)
	{
		audio.playSoundOrganic(sound, gain);
	}

	public void setHandItem(int hand, Item item)
	{
		bool isArmNode(string name, string suffix)
		{
			bool isArmNode =
				name.StartsWith("Shoulder") ||
				name.StartsWith("Arm") ||
				name.StartsWith("Hand") ||
				name.StartsWith("Finger") ||
				name.StartsWith("Thumb") ||
				name.StartsWith("Weapon")
				;
			return isArmNode && name.EndsWith(suffix);
		}

		getHand(hand).setItem(item);

		if (item != null)
		{
			bool[] movesetMask = new bool[item.moveset.skeleton.nodes.Length];
			for (int i = 0; i < item.moveset.skeleton.nodes.Length; i++)
				movesetMask[i] = isArmNode(item.moveset.skeleton.nodes[i].name, item.twoHanded ? "" : hand == 0 ? ".R" : ".L");

			bool[] skeletonMask = new bool[model.skeleton.nodes.Length];
			for (int i = 0; i < model.skeleton.nodes.Length; i++)
				skeletonMask[i] = isArmNode(model.skeleton.nodes[i].name, item.twoHanded ? "" : hand == 0 ? ".R" : ".L");

			idleAnim.layers[1 + hand] = new AnimationLayer(item.moveset.getAnimationData(idleAnim.layers[0].animationName) != null ? item.moveset : model, idleAnim.layers[0].animationName, idleAnim.layers[0].looping, item.moveset.getAnimationData(idleAnim.layers[0].animationName) != null ? movesetMask : skeletonMask);
			idleAnim.layers[1 + hand].mirrored = hand == 1;

			runAnim.layers[1 + hand] = new AnimationLayer(item.moveset.getAnimationData(runAnim.layers[0].animationName) != null ? item.moveset : model, runAnim.layers[0].animationName, runAnim.layers[0].looping, item.moveset.getAnimationData(runAnim.layers[0].animationName) != null ? movesetMask : skeletonMask);
			runAnim.layers[1 + hand].mirrored = hand == 1;
			runAnim.layers[1 + hand].timerOffset = hand == 1 ? runAnim.layers[1 + hand].animationData.getAnimationData(runAnim.layers[0].animationName).Value.duration * 0.5f : 0.0f;

			fallAnim.layers[1 + hand] = new AnimationLayer(item.moveset.getAnimationData(fallAnim.layers[0].animationName) != null ? item.moveset : model, fallAnim.layers[0].animationName, fallAnim.layers[0].looping, item.moveset.getAnimationData(fallAnim.layers[0].animationName) != null ? movesetMask : skeletonMask);
			fallAnim.layers[1 + hand].mirrored = hand == 1;
		}
		else
		{
			idleAnim.layers[1 + hand] = null;
			runAnim.layers[1 + hand] = null;
			fallAnim.layers[1 + hand] = null;
		}

		if (item != null)
			actions.queueAction(new ItemEquipAction(item, hand));
	}

	public void setDirection(float direction)
	{
		this.direction = direction;
	}

	float directionToAngle(Vector3 direction)
	{
		return ((direction.xz * new Vector2i(1, -1)).angle - MathF.PI * 0.5f + MathF.PI * 2) % (MathF.PI * 2);
	}

	void updateMovement()
	{
		movementInput = Vector3.Zero;
		if (Input.IsKeyDown(KeyCode.A))
			movementInput.x--;
		if (Input.IsKeyDown(KeyCode.D))
			movementInput.x++;
		if (Input.IsKeyDown(KeyCode.W))
			movementInput.z--;
		if (Input.IsKeyDown(KeyCode.S))
			movementInput.z++;

		isSprinting = isGrounded && Input.IsKeyDown(KeyCode.Shift);
		if (Input.IsKeyPressed(KeyCode.Shift))
			lastSprintInput = Time.currentTime;

		if (isGrounded)
		{
			animator.getRootMotion(out Vector3 rootMotion, out Quaternion _, out bool hasLooped);
			rootMotion = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI) * rootMotion;
			Vector3 rootMotionDelta = hasLooped || animator.currentAnimation != lastRootMotionAnim ? Vector3.Zero : rotation * (rootMotion - lastRootMotion);
			lastRootMotion = rootMotion;
			lastRootMotionAnim = animator.currentAnimation;

			if (movementInput.lengthSquared > 0)
			{
				currentSpeed = (isSprinting ? sprintSpeed : speed) * (actions.currentAction != null ? actions.currentAction.movementSpeedMultiplier : 1);
				velocity.xz = movementInput.xz.normalized * currentSpeed + rootMotionDelta.xz / Time.deltaTime;
				distanceWalked += currentSpeed * Time.deltaTime + rootMotionDelta.length;
			}
			else
			{
				currentSpeed = 0;
				velocity.xz = rootMotionDelta.xz / Time.deltaTime;
			}
		}

		if (movementInput.lengthSquared > 0)
		{
			float directionDst = directionToAngle(movementInput);
			direction = MathHelper.LerpAngle(direction, directionDst, 6 * Time.deltaTime * (actions.currentAction != null ? actions.currentAction.rotationSpeedMultiplier : 1));
			rotation = Quaternion.FromAxisAngle(Vector3.Up, direction);
		}

		if (Input.IsKeyPressed(KeyCode.Space))
			lastJumpInput = Time.currentTime;
		if (isGrounded && (Time.currentTime - lastJumpInput) / 1e9f < JUMP_BUFFER_WINDOW)
		{
			velocity.y = jumpPower;
			lastJumpInput = 0;
		}
		if (!isGrounded && !Input.IsKeyDown(KeyCode.Space))
			velocity.y = MathF.Min(velocity.y, 0);

		isGrounded = velocity.y < -0.001f && position.y <= 0;
		if (isGrounded)
			position.y = 0;

		if (isGrounded)
			velocity.y = 0;

		velocity.y += gravity * Time.deltaTime;

		Vector3 displacement = velocity * Time.deltaTime;
		position += displacement;
	}

	void updateActions()
	{
		if (Input.IsKeyReleased(KeyCode.Shift) && (Time.currentTime - lastSprintInput) / 1e9f < ROLL_INPUT_RELEASE_WINDOW)
			lastRollInput = Time.currentTime;
		if (isGrounded && (Time.currentTime - lastRollInput) / 1e9f < ROLL_BUFFER_WINDOW)
		{
			actions.queueAction(new RollAction(movementInput.lengthSquared > 0 ? directionToAngle(movementInput) : direction));
			lastRollInput = 0;
		}

		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			Attack? currentAttack = null;
			if (actions.currentAction != null && actions.currentAction is AttackAction)
				currentAttack = ((AttackAction)actions.currentAction).attack;
			Attack? attack = rightHand.item.getAttack(AttackType.Light, currentAttack);
			if (attack.HasValue)
				actions.queueAction(new AttackAction(rightHand.item, 0, attack.Value));
		}

		actions.update();
	}

	void updateAnimations()
	{
		AnimationState movementState;
		//float movementAnimTimer = animator.timer;

		if (isGrounded)
		{
			if (currentSpeed > 0.25f * speed * speed)
			{
				float animationSpeed = MathHelper.Clamp(currentSpeed / speed, 0, 2) * 0.8f;
				runAnim.animationSpeed = animationSpeed;
				//movementAnimTimer = distanceWalked / speed * 0.8f;
				movementState = runAnim;
			}
			else
			{
				movementState = idleAnim;
			}
		}
		else
		{
			movementState = fallAnim;
		}

		if (actions.currentAction != null && actions.currentAction.animationName[1] != null)
			rightHandAnimator.setAnimation(currentActionAnim, actions.currentAction.startTime == Time.currentTime);
		else
		{
			rightHandAnimator.setAnimation(movementState);
			//rightHandAnimator.timer = movementAnimTimer;
		}
		if (actions.currentAction != null && actions.currentAction.animationName[2] != null)
			leftHandAnimator.setAnimation(currentActionAnim, actions.currentAction.startTime == Time.currentTime);
		else
		{
			leftHandAnimator.setAnimation(movementState);
			//leftHandAnimator.timer = movementAnimTimer;
		}
		if (actions.currentAction != null && actions.currentAction.animationName[0] != null)
			animator.setAnimation(currentActionAnim, actions.currentAction.startTime == Time.currentTime);
		else
		{
			animator.setAnimation(movementState);
			//animator.timer = movementAnimTimer;
		}

		foreach (Node node in model.skeleton.nodes)
		{
			bool isArmNode = node.name.Contains("Shoulder")
				|| node.name.Contains("Arm")
				|| node.name.Contains("Hand")
				|| node.name.Contains("Finger")
				|| node.name.Contains("Thumb")
				|| node.name.Contains("Weapon");
			bool isRight = node.name.EndsWith(".R");
			bool isLeft = node.name.EndsWith(".L");
			if (isArmNode && isRight)
				animator.setNodeLocalTransform(node, rightHandAnimator.getNodeLocalTransform(node));
			else if (isArmNode && isLeft)
				animator.setNodeLocalTransform(node, leftHandAnimator.getNodeLocalTransform(node));
		}

		animator.applyAnimation();


		cape.setTransform(position, Quaternion.Identity);

		Span<Vector4> spheres = [
			new Vector4(rotation * new Vector3(0, 0.2f, -0.1f), 0.2f),
			new Vector4(rotation * new Vector3(0, 1.0f, -0.1f), 0.2f)
		];
		cape.setSpheres(spheres, 0, cape.numSpheres);

		Span<Vector2i> capsules = [new Vector2i(0, 1)];
		cape.setCapsules(capsules, 0, cape.numCapsules);

		float time = Time.currentTime / 1e9f;
		Cloth.SetWind(Vector3.Zero);
		//Cloth.SetWind(new Vector3(1, 0, 1) * (simplex.sample1f(time) * 1 + 0.5f));
	}

	public override void update()
	{
		//base.update();

		updateMovement();
		updateActions();
		updateAnimations();

		Matrix transform = getModelMatrix();
		rightHand.update(transform * animator.getNodeTransform(rightWeaponNode) * Matrix.CreateRotation(Vector3.Up, MathF.PI));
		leftHand.update(transform * animator.getNodeTransform(leftWeaponNode) * Matrix.CreateRotation(Vector3.Up, MathF.PI));

		audio.updateTransform(position + Vector3.Up);
	}

	public override unsafe void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		rightHand.draw();
		leftHand.draw();

		//Renderer.DrawCloth(cape, capeMesh.getMaterialData(0), cape.position, rotation);
	}

	public AnimationState getNextActionAnimationState()
	{
		currentActionAnim = currentActionAnim == actionAnim1 ? actionAnim2 : currentActionAnim == actionAnim2 ? actionAnim1 : actionAnim1;
		return currentActionAnim;
	}
}
