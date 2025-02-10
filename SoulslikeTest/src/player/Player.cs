using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class Player : Creature
{
	const float ROLL_INPUT_RELEASE_WINDOW = 0.2f;
	const float ROLL_BUFFER_WINDOW = 0.2f;
	const float JUMP_BUFFER_WINDOW = 0.2f;


	float speed = 3.6f;
	float sprintSpeed = 5.0f;
	float jumpPower = 8.0f;
	float gravity = -20;

	public PlayerCamera camera;
	CharacterController controller;
	RigidBody kinematicBody;
	Vector3 movementInput;
	public bool strafing = false;
	float directionDst;
	float currentSpeed;
	Vector3 velocity;
	Vector3 rootMotionVelocity;
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

	Vector3 lastRootMotion;
	AnimationState lastRootMotionAnim;

	Node rightWeaponNode;
	Node leftWeaponNode;

	Node backWeaponNode;
	Node waistWeaponNode;

	Animator rightHandAnimator;
	Animator leftHandAnimator;

	AnimationState idleAnim;
	AnimationState runAnim;
	AnimationState fallAnim;
	AnimationState actionAnim1, actionAnim2;
	public AnimationState currentActionAnim;

	Simplex simplex = new Simplex(12345, 3);


	public unsafe Player()
		: base(EntityType.Get("player"))
	{
		stats = new PlayerStats(this);

		hud = new HUD(this);

		inventory = new Inventory(this);
		inventoryUI = new InventoryUI(this);

		actions = new ActionQueue(this);

		model = Resource.GetModel("entity/creature/player/player.gltf");
		rootMotionNode = model.skeleton.getNode("Root");
		rightWeaponNode = model.skeleton.getNode("Weapon.R");
		leftWeaponNode = model.skeleton.getNode("Weapon.L");
		backWeaponNode = model.skeleton.getNode("Weapon.Back");
		waistWeaponNode = model.skeleton.getNode("Weapon.Waist");
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

		capeMesh = Resource.GetModel("entity/creature/player/player_cape.gltf");
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

		controller = new CharacterController(this, 0.3f, Vector3.Zero, 1.8f, 0.1f, PhysicsFiltering.DEFAULT | PhysicsFiltering.CREATURE);
		kinematicBody = new RigidBody(this, RigidBodyType.Kinematic, PhysicsFiltering.PLAYER, PhysicsFiltering.RAGDOLL);
		kinematicBody.addCapsuleCollider(0.35f, 1.8f, new Vector3(0, 0.9f, 0), Quaternion.Identity);

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

	bool isArmNode(string name, string suffix = null)
	{
		bool armNode =
			name.StartsWith("Shoulder") ||
			name.StartsWith("Arm") ||
			name.StartsWith("Hand") ||
			name.StartsWith("Finger") ||
			name.StartsWith("Thumb") ||
			name.StartsWith("Weapon")
			;
		if (suffix != null)
			return armNode && name.EndsWith(suffix);
		else
			return armNode;
	}

	public void setHandItem(int hand, Item item)
	{
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

		//if (item != null)
		//	actions.queueAction(new ItemEquipAction(item, hand));
	}

	public void setDirection(float direction)
	{
		directionDst = direction;
		yaw = direction;
	}

	public float getInputDirection()
	{
		if (movementInput.lengthSquared > 0)
			return directionToAngle(movementInput);
		return yaw;
	}

	float directionToAngle(Vector3 direction)
	{
		return ((direction.xz * new Vector2i(1, -1)).angle - MathF.PI * 0.5f + MathF.PI * 2) % (MathF.PI * 2);
	}

	public override bool hit(float damage, float poiseDamage, Entity from, Item fromItem, Vector3 hitPosition, Vector3 hitDirection, RigidBody hitbox)
	{
		if (stats.isDead)
			return false;

		if (parryingItem != null)
		{
			damage = 0;

			if (from is Mob)
			{
				Mob mob = from as Mob;
				mob.actions.cancelAllActions();

				StaggerType staggerType = parryingItem.weaponType == WeaponType.Melee ? StaggerType.Block : StaggerType.Parry;
				mob.actions.queueAction(new MobStaggerAction(staggerType, mob));
			}

			if (parryingItem.weaponType == WeaponType.Melee)
			{
				actions.queueAction(new BlockHitAction(parryingItem, parryingHand, 0, true));
				actions.cancelAction();
			}
			else
			{
				Audio.Play(parryingItem.parrySound, hitPosition);
			}
		}
		else if (blockingItem != null)
		{
			float damageMultiplier = blockingItem.getAbsorptionDamageModifier();
			damage = damage * damageMultiplier;

			Debug.Assert(blockingHand != -1);
			float staminaCost = damage * blockingItem.getStabilityStaminaModifier();

			if (blockingItem.weaponType == WeaponType.Shield)
			{
				if (from is Mob)
				{
					Mob mob = from as Mob;
					mob.actions.cancelAllActions();
					mob.actions.queueAction(new MobStaggerAction(StaggerType.Block, mob));
				}
			}

			Debug.Assert(actions.currentAction != null && actions.currentAction is BlockStanceAction);
			actions.queueAction(new BlockHitAction(blockingItem, blockingHand, staminaCost));
			actions.cancelAction();
		}
		else if (actions.currentAction != null && actions.currentAction.isInIFrames)
		{
			damage = 0;
		}
		else
		{
			float damageMultiplier = 1.0f;

			Node hitNode = getHitboxNode(hitbox);
			bool criticalHit = hitNode != null && hitNode.name.IndexOf("head", StringComparison.OrdinalIgnoreCase) != -1;
			if (criticalHit)
				damageMultiplier *= (1 + blockingItem.criticalModifier / 100.0f);

			damageMultiplier *= inventory.getArmorProtection();

			damage = (int)MathF.Ceiling(damage * damageMultiplier);

			hud.onHit();

			actions.cancelAllActions();
			actions.queueAction(new StaggerAction());

			if (hitSound != null)
				Audio.PlayOrganic(hitSound, hitPosition);
		}

		stats.damage(damage);

		if (stats.isDead)
			onDeath(from, fromItem, hitPosition, hitDirection);

		return false;
	}

	void onDeath(Entity from, Item fromItem, Vector3 hitPosition, Vector3 hitDirection)
	{
		//killedBy = from;

		inventoryUI.inventoryOpen = false;
		Input.cursorMode = CursorMode.Normal;

		stats.effects.Clear();

		//actions.cancelAllActions();
		//actions.queueAction(new DeathAction());

		// TODO death sound

		base.onDeath();
	}

	public override bool isAlive()
	{
		return true;
	}

	public void onEnemyKill(Creature creature)
	{
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
		movementInput = Quaternion.FromAxisAngle(Vector3.Up, camera.yaw) * movementInput;

		if (isGrounded)
			isSprinting = Input.IsKeyDown(KeyCode.Shift);
		if (Input.IsKeyPressed(KeyCode.Shift))
			lastSprintInput = Time.currentTime;

		if (isGrounded)
		{
			if (movementInput.lengthSquared > 0)
			{
				currentSpeed = (isSprinting ? sprintSpeed : speed) * (actions.currentAction != null ? actions.currentAction.movementSpeedMultiplier : 1);
				velocity.xz = movementInput.xz.normalized * currentSpeed;
				distanceWalked += currentSpeed * Time.deltaTime;
			}
			else
			{
				currentSpeed = 0;
				float friction = 100000;
				velocity.xz *= MathF.Pow(1.0f / friction, Time.deltaTime);
			}
		}

		animator.getRootMotion(out Vector3 rootMotion, out Quaternion _, out bool hasLooped);
		rootMotion = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI) * rootMotion;
		Vector3 rootMotionDelta = hasLooped || animator.currentAnimation != lastRootMotionAnim ? Vector3.Zero : rotation * (rootMotion - lastRootMotion);
		lastRootMotion = rootMotion;
		lastRootMotionAnim = animator.currentAnimation;
		if (isGrounded)
			rootMotionVelocity = rootMotionDelta / Time.deltaTime;

		if (actions.currentAction == null || !actions.currentAction.lockRotation)
		{
			if (movementInput.lengthSquared > 0 && (!strafing || isSprinting))
				directionDst = directionToAngle(movementInput);
			else if (strafing)
				directionDst = camera.yaw;
		}
		yaw = MathHelper.LerpAngle(yaw, directionDst, 10 * Time.deltaTime);
		rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw);

		if (Input.IsKeyPressed(KeyCode.Space))
			lastJumpInput = Time.currentTime;
		if (isGrounded && actions.currentAction == null && (Time.currentTime - lastJumpInput) / 1e9f < JUMP_BUFFER_WINDOW)
		{
			velocity.y = jumpPower;
			lastJumpInput = 0;
		}
		if (!isGrounded && !Input.IsKeyDown(KeyCode.Space))
			velocity.y = MathF.Min(velocity.y, 0);

		isGrounded = velocity.y < 0 && Physics.SweepSphere(0.3f, position + 0.3f + 0.1f, Vector3.Down, 0.2f) != null;
		if (isGrounded)
			velocity.y = 0;

		velocity.y += gravity * Time.deltaTime;

		Vector3 displacement = (velocity + rootMotionVelocity) * Time.deltaTime;

		ControllerCollisionFlag controllerFlags = controller.move(displacement);
		if ((controllerFlags & ControllerCollisionFlag.Down) != 0)
			isGrounded = true;

		kinematicBody.setTransform(position, Quaternion.Identity);
	}

	void updateActions()
	{
		if (Input.IsKeyReleased(KeyCode.Shift) && (Time.currentTime - lastSprintInput) / 1e9f < ROLL_INPUT_RELEASE_WINDOW)
			lastRollInput = Time.currentTime;
		if (isGrounded && (Time.currentTime - lastRollInput) / 1e9f < ROLL_BUFFER_WINDOW)
		{
			actions.queueAction(new RollAction(movementInput.lengthSquared > 0 ? directionToAngle(movementInput) : yaw));
			lastRollInput = 0;
		}

		if (Input.cursorMode == CursorMode.Disabled)
		{
			for (int i = 0; i < 2; i++)
			{
				string input = i == 0 ? "AttackRight" : "AttackLeft";
				string otherInput = i == 0 ? "AttackLeft" : "AttackRight";
				PlayerHand hand = i == 0 ? rightHand : leftHand;
				PlayerHand otherHand = i == 0 ? leftHand : rightHand;

				// Melee Attacks
				if (hand.item != null && hand.item.category == ItemCategory.Weapon && hand.item.weaponType == WeaponType.Melee)
				{
					if (InputManager.IsPressed(input))
					{
						AttackType type = InputManager.IsDown("AttackHeavy") ? AttackType.Heavy : AttackType.Light;
						if (isSprinting && hand.item.getAttack(AttackType.Running) != null)
							type = AttackType.Running;
						Attack? lastAttack = null;
						if (actions.currentAction != null && actions.currentAction is AttackAction && ((AttackAction)actions.currentAction).handID == i)
						{
							AttackAction lastAttackAction = actions.currentAction as AttackAction;
							if (lastAttackAction.attack.type == type || lastAttackAction.attack.type != AttackType.Heavy && type != AttackType.Heavy)
								lastAttack = lastAttackAction.attack;
						}

						Attack? attack = hand.item.getAttack(type, lastAttack);
						if (actions.currentAction != null && actions.currentAction is BlockHitAction && ((BlockHitAction)actions.currentAction).wasParry)
						{
							Attack? riposte = hand.item.getAttack(AttackType.Riposte);
							if (riposte != null)
							{
								attack = riposte;
								actions.cancelAction();
							}
						}
						if (actions.currentAction != null && actions.currentAction is BlockStanceAction)
							actions.cancelAction();
						if (attack != null)
							actions.queueAction(new AttackAction(hand.item, i, attack.Value));
					}
					if (hand.item.twoHanded || otherHand.item == null)
					{
						if (InputManager.IsDown(otherInput))
						{
							if (actions.currentAction == null || actions.actionQueue.Count == 1 && actions.currentAction is BlockHitAction && ((BlockHitAction)actions.currentAction).handID == i)
							{
								bool resumeBlock = actions.currentAction is BlockHitAction;
								if (InputManager.IsPressed(otherInput) && actions.actionQueue.Count == 1)
								{
									actions.cancelAction();
									resumeBlock = false;
								}
								actions.queueAction(new BlockStanceAction(hand.item, i, resumeBlock));
							}
						}
						else
						{
							if (actions.currentAction != null && actions.currentAction is BlockStanceAction && ((BlockStanceAction)actions.currentAction).handID == i)
							{
								actions.cancelAction();
							}
						}
					}
				}

				// Staff
				if (hand.item != null && hand.item.category == ItemCategory.Weapon && hand.item.weaponType == WeaponType.Staff)
				{
					if (InputManager.IsPressed(input))
					{
						// cast
						AttackType type = AttackType.Cast;
						Attack? lastAttack = null;
						if (actions.currentAction != null && actions.currentAction is AttackAction && ((AttackAction)actions.currentAction).handID == i)
						{
							AttackAction lastAttackAction = actions.currentAction as AttackAction;
							if (lastAttackAction.attack.type == type)
								lastAttack = lastAttackAction.attack;
						}

						Item spell = inventory.getCurrentSpellSlot().item;
						if (spell != null)
						{
							Attack? attack = spell.getAttack(type, lastAttack);

							if (actions.currentAction != null && actions.currentAction is BlockStanceAction)
								actions.cancelAction();
							if (attack != null)
								actions.queueAction(new AttackAction(spell, i, attack.Value, hand.item));
						}
					}
					if (hand.item.twoHanded || otherHand.item == null)
					{
						if (InputManager.IsDown(otherInput))
						{
							if (actions.currentAction == null || actions.actionQueue.Count == 1 && actions.currentAction is BlockHitAction && ((BlockHitAction)actions.currentAction).handID == i)
							{
								bool resumeBlock = actions.currentAction is BlockHitAction;
								if (InputManager.IsPressed(otherInput) && actions.actionQueue.Count == 1)
								{
									actions.cancelAction();
									resumeBlock = false;
								}
								actions.queueAction(new BlockStanceAction(hand.item, i, resumeBlock));
							}
						}
						else
						{
							if (actions.currentAction != null && actions.currentAction is BlockStanceAction && ((BlockStanceAction)actions.currentAction).handID == i)
							{
								actions.cancelAction();
							}
						}
					}
				}

				// Shield
				if (hand.item != null && hand.item.category == ItemCategory.Weapon && hand.item.weaponType == WeaponType.Shield)
				{
					if (InputManager.IsDown(input))
					{
						if (InputManager.IsDown("AttackHeavy"))
						{
							if (actions.currentAction == null)
							{
								Attack? lastAttack = null;
								if (actions.currentAction != null && actions.currentAction is AttackAction && ((AttackAction)actions.currentAction).handID == i)
									lastAttack = ((AttackAction)actions.currentAction).attack;
								Attack? attack = hand.item.getAttack(AttackType.Heavy, lastAttack);
								if (attack != null)
									actions.queueAction(new AttackAction(hand.item, i, attack.Value));
							}
						}
						else
						{
							if (actions.currentAction == null || actions.currentAction is BlockHitAction && ((BlockHitAction)actions.currentAction).handID == i)
							{
								bool resumeBlock = actions.currentAction is BlockHitAction;
								actions.queueAction(new BlockStanceAction(hand.item, i, resumeBlock));
							}
						}
					}
					else
					{
						if (actions.currentAction != null && actions.currentAction is BlockStanceAction && ((BlockStanceAction)actions.currentAction).handID == i)
						{
							actions.cancelAction();
						}
					}
				}

				// Bows
				/*
				if (hand.item != null && hand.item.category == ItemCategory.Weapon && hand.item.weaponType == WeaponType.Bow)
				{
					if (InputManager.IsDown(input) && !InputManager.IsDown(otherInput))
					{
						if (actions.currentAction == null)
						{
							if (inventory.arrows.item != null)
								actions.queueAction(new BowDrawAction(hand.item, i));
						}
					}
				}
				*/
			}
		}

		actions.update();
		stats.update(isSprinting, isGrounded, actions.currentAction);
	}

	void updateAnimations()
	{
		AnimationState movementState;
		float movementAnimTimer = animator.timer;

		if (isGrounded)
		{
			if (currentSpeed > 0.25f * speed * speed)
			{
				float animationSpeed = MathHelper.Clamp(currentSpeed / speed, 0, 2) * 0.6f;
				runAnim.animationSpeed = animationSpeed;
				movementAnimTimer = distanceWalked / speed * 0.8f;
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
			rightHandAnimator.timer = movementAnimTimer;
		}
		if (actions.currentAction != null && actions.currentAction.animationName[2] != null)
			leftHandAnimator.setAnimation(currentActionAnim, actions.currentAction.startTime == Time.currentTime);
		else
		{
			leftHandAnimator.setAnimation(movementState);
			leftHandAnimator.timer = movementAnimTimer;
		}
		if (actions.currentAction != null && actions.currentAction.animationName[0] != null)
			animator.setAnimation(currentActionAnim, actions.currentAction.startTime == Time.currentTime);
		else
		{
			animator.setAnimation(movementState);
			animator.timer = movementAnimTimer;
		}

		foreach (Node node in model.skeleton.nodes)
		{
			bool armNode = isArmNode(node.name);
			bool isRight = node.name.EndsWith(".R");
			bool isLeft = node.name.EndsWith(".L");
			if (armNode && isRight)
				animator.setNodeLocalTransform(node, rightHandAnimator.getNodeLocalTransform(node));
			else if (armNode && isLeft)
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
		rightHand.update(transform * modelTransform * animator.getNodeTransform(rightWeaponNode));
		leftHand.update(transform * modelTransform * animator.getNodeTransform(leftWeaponNode));

		audio.updateTransform(position + Vector3.Up);
	}

	public override unsafe void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		Matrix transform = getModelMatrix();

		rightHand.draw();
		leftHand.draw();

		hud.draw();

		//Renderer.DrawCloth(cape, capeMesh.getMaterialData(0), cape.position, rotation);
	}

	public AnimationState getNextActionAnimationState()
	{
		currentActionAnim = currentActionAnim == actionAnim1 ? actionAnim2 : currentActionAnim == actionAnim2 ? actionAnim1 : actionAnim1;
		return currentActionAnim;
	}
}
