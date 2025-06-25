using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class Player : Entity
{
	public float yaw;
	Vector3 velocity;
	CharacterController controller;
	bool isGrounded;

	public PlayerActionManager actionManager;
	Vector3 rootMotionDelta;
	Vector3 lastRootMotionDisplacement;
	PlayerAction lastRootMotionAction;
	AnimationState lastRootMotionAnim;

	Node rootNode;
	Node chestNode;
	public Node rightWeaponNode, leftWeaponNode;
	bool[] rightHandBoneMask, leftHandBoneMask;

	public AnimationState defaultAnim;
	AnimationState idleAnim;
	AnimationState runAnim;
	AnimationState fallAnim;

	Item rightWeapon;
	public Matrix rightWeaponTransform;

	Item leftWeapon;
	Matrix leftWeaponTransform;

	Item[] armor = new Item[(int)ArmorSlot.Count];
	ArmorEntity[] armorEntity = new ArmorEntity[(int)ArmorSlot.Count];


	public Player()
	{
		model = Resource.GetModel("entity/creature/generic_dude2.gltf");
		animator = Animator.Create(model);

		defaultAnim = Animator.CreateAnimation(model, "default", true);
		idleAnim = Animator.CreateAnimation(model, [new AnimationLayer(model, "idle", true), null, null], 0.2f);
		//idleAnim.animationSpeed = 0.1f;
		runAnim = Animator.CreateAnimation(model, [new AnimationLayer(model, "run", true), null, null], 0.2f);
		fallAnim = Animator.CreateAnimation(model, [new AnimationLayer(model, "fall", true), null, null], 0.2f);

		animator.setAnimation(idleAnim);
		animator.update();
		animator.applyAnimation();

		rootNode = model.skeleton.getNode("Root");
		chestNode = model.skeleton.getNode("Chest");
		rightWeaponNode = model.skeleton.getNode("Weapon.R");
		leftWeaponNode = model.skeleton.getNode("Weapon.L");

		actionManager = new PlayerActionManager(this, rootNode);
	}

	public override void init()
	{
		base.init();

		controller = new CharacterController(this, 0.3f, Vector3.Zero, 2.0f);

		//setRightWeapon(new KingsSword());
		setRightWeapon(new Longsword());
		//setArmor(ArmorSlot.Hands, new LeatherGauntlets());
		//setArmor(ArmorSlot.Body, new TestCape());
		//setArmor(ArmorSlot.Head, new IronHelmet());
		setArmor(ArmorSlot.Head, new WizardHat());
		setArmor(ArmorSlot.Body, new WhitePants());
	}

	public override void destroy()
	{
		base.destroy();
	}

	public void setRightWeapon(Item item)
	{
		rightWeapon = item;

		if (item != null)
		{
			Model moveset = item.moveset;

			if (rightHandBoneMask == null)
			{
				Debug.Assert(leftHandBoneMask == null);

				rightHandBoneMask = new bool[moveset.skeleton.nodes.Length];
				leftHandBoneMask = new bool[moveset.skeleton.nodes.Length];
				for (int i = 0; i < moveset.skeleton.nodes.Length; i++)
				{
					Node node = moveset.skeleton.nodes[i];
					if (node.hasParent(chestNode))
					{
						if (node.name.EndsWith(".R"))
							rightHandBoneMask[i] = true;
						else if (node.name.EndsWith(".L"))
							leftHandBoneMask[i] = true;
					}
				}
			}

			idleAnim.layers[1] = new AnimationLayer(moveset, "idle", true, rightHandBoneMask);
			runAnim.layers[1] = new AnimationLayer(moveset, moveset.getAnimationData("run") != null ? "run" : "idle", true, rightHandBoneMask);
			fallAnim.layers[1] = new AnimationLayer(moveset, moveset.getAnimationData("fall") != null ? "fall" : "idle", true, rightHandBoneMask);

			if (item.twoHanded)
			{
				idleAnim.layers[2] = new AnimationLayer(moveset, "idle", true, leftHandBoneMask);
				runAnim.layers[2] = new AnimationLayer(moveset, moveset.getAnimationData("run") != null ? "run" : "idle", true, leftHandBoneMask);
				fallAnim.layers[2] = new AnimationLayer(moveset, moveset.getAnimationData("fall") != null ? "fall" : "idle", true, leftHandBoneMask);
			}
			else
			{
				idleAnim.layers[2] = null;
				runAnim.layers[2] = null;
				fallAnim.layers[2] = null;
			}
		}
		else
		{
			idleAnim.layers[1] = null;
			runAnim.layers[1] = null;
			fallAnim.layers[1] = null;

			idleAnim.layers[2] = null;
			runAnim.layers[2] = null;
			fallAnim.layers[2] = null;
		}
	}

	public void setArmor(ArmorSlot slot, Armor item)
	{
		int idx = (int)slot;

		if (armorEntity[idx] != null)
		{
			armorEntity[idx].remove();
			armorEntity[idx] = null;
		}

		armor[idx] = item;

		if (item != null)
		{
			armorEntity[idx] = new ArmorEntity(item, this);
			scene.addEntity(armorEntity[idx], position, rotation);
		}
	}

	public void snapInputPosition()
	{
		Vector3 moveDelta = Vector3.Zero;
		if (Input.IsKeyDown(KeyCode.A)) moveDelta.x--;
		if (Input.IsKeyDown(KeyCode.D)) moveDelta.x++;
		if (Input.IsKeyDown(KeyCode.W)) moveDelta.z--;
		if (Input.IsKeyDown(KeyCode.S)) moveDelta.z++;

		if (moveDelta.lengthSquared != 0)
		{
			Vector3 moveVelocity = Quaternion.FromAxisAngle(Vector3.Up, GameState.instance.camera.yaw) * moveDelta;
			yaw = -moveVelocity.xz.angle - MathF.PI * 0.5f;
			rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw);
		}
	}

	public override void fixedUpdate(float delta)
	{
		Vector3 moveDelta = Vector3.Zero;
		if (Input.IsKeyDown(KeyCode.A)) moveDelta.x--;
		if (Input.IsKeyDown(KeyCode.D)) moveDelta.x++;
		if (Input.IsKeyDown(KeyCode.W)) moveDelta.z--;
		if (Input.IsKeyDown(KeyCode.S)) moveDelta.z++;

		if (moveDelta.lengthSquared != 0)
		{
			float speed = 6;
			Vector3 moveVelocity = Quaternion.FromAxisAngle(Vector3.Up, GameState.instance.camera.yaw) * moveDelta.normalized * speed;
			velocity.xz = moveVelocity.xz;

			if (actionManager.currentAction != null)
				velocity.xz *= actionManager.currentAction.movementSpeedMultiplier;

			if (actionManager.currentAction == null || !actionManager.currentAction.rotationIsLocked)
				yaw = MathHelper.LinearAngle(yaw, -moveVelocity.xz.angle - MathF.PI * 0.5f, 12 * delta);
			rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw);
		}
		else
		{
			velocity.xz = Vector2.Zero;
		}

		if (Input.IsKeyPressed(KeyCode.Space) && isGrounded)
		{
			if (actionManager.currentAction == null || actionManager.currentAction.canJump)
			{
				if (actionManager.currentAction != null)
					actionManager.cancelAction();
				velocity.y = 4;
			}
		}

		Vector3 displacement = Vector3.Zero;

		float gravity = -10;
		velocity.y += 0.5f * gravity * delta;
		displacement.y += velocity.y * delta;
		velocity.y += 0.5f * gravity * delta;

		displacement.xz += velocity.xz * delta;

		displacement.xz += (Quaternion.FromAxisAngle(Vector3.Up, MathF.PI) * rootMotionDelta).xz;
		rootMotionDelta = Vector3.Zero;

		ControllerCollisionFlag collisionFlags = controller.move(displacement);
		if ((collisionFlags & ControllerCollisionFlag.Down) != 0)
			velocity.y = MathHelper.Lerp(velocity.y, 0, 10 * delta);
		//position += displacement;

		HitData? hit = Physics.SweepSphere(controller.radius, position + Vector3.Up * controller.radius + 0.2f, Vector3.Down, 0.2f + controller.radius + 0.2f, QueryFilterFlags.Default);
		isGrounded = hit != null && velocity.y < 0;

		for (int i = 0; i < armor.Length; i++)
		{
			if (armor[i] != null)
			{
				armorEntity[i].position = position;
				armorEntity[i].rotation = rotation * Quaternion.FromAxisAngle(Vector3.Up, MathF.PI);
			}
		}

		if (actionManager.currentAction != null)
			actionManager.currentAction.fixedUpdate(this, delta);
	}

	public override void update()
	{
		if (actionManager.currentAction != null && actionManager.currentActionAnim.layers[0].rootMotion)
		{
			Vector3 rootMotionDisplacement = actionManager.currentActionAnim.layers[0].rootMotionDisplacement.translation;
			if (lastRootMotionAction == actionManager.currentAction && lastRootMotionAnim == actionManager.currentActionAnim && !actionManager.currentActionAnim.layers[0].hasLooped)
			{
				rootMotionDelta += rotation * (rootMotionDisplacement - lastRootMotionDisplacement);
			}
			lastRootMotionDisplacement = rootMotionDisplacement;
			lastRootMotionAction = actionManager.currentAction;
			lastRootMotionAnim = actionManager.currentActionAnim;
			//velocity += displacement.translation / Time.deltaTime;
			//if (MathF.Abs(rootMotionRotationVelocity.angle) > 0.001f)
			//{
			//	rotationVelocity += rootMotionRotationVelocity.angle * MathF.Sign(rootMotionRotationVelocity.axis.z);
			//	Console.WriteLine(rootMotionRotationVelocity.angle * MathF.Sign(rootMotionRotationVelocity.axis.z));
			//}
		}
		else
		{
			rootMotionDelta = Vector3.Zero;
			lastRootMotionDisplacement = Vector3.Zero;
			lastRootMotionAction = null;
			lastRootMotionAnim = null;
		}

		if (Input.IsMouseButtonPressed(MouseButton.Left, true))
		{
			if (rightWeapon != null)
				rightWeapon.use(this, 0);
		}

		if (Input.IsKeyPressed(KeyCode.Shift))
		{
			actionManager.queueAction(new RollAction());
		}

		actionManager.update();

		if (isGrounded)
		{
			if (actionManager.currentAction != null && actionManager.currentAction.animationName != null)
			{
				animator.setAnimation(actionManager.currentActionAnim);
			}
			else
			{
				if (velocity.xz.lengthSquared > 0.1f)
					animator.setAnimation(runAnim);
				else
					animator.setAnimation(idleAnim);
			}
		}
		else
		{
			animator.setAnimation(fallAnim);
		}

		animator.applyAnimation();

		for (int i = 0; i < armor.Length; i++)
		{
			if (armor[i] != null)
			{
				armorEntity[i].animator.setAnimation(animator.currentAnimation);
				armorEntity[i].animator.applyAnimation();
			}
		}

		Matrix transform = getModelMatrix() * Matrix.CreateRotation(Vector3.Up, MathF.PI);
		rightWeaponTransform = transform * animator.getNodeTransform(rightWeaponNode) * Matrix.CreateRotation(Vector3.Right, MathF.PI * 0.5f);
		leftWeaponTransform = transform * animator.getNodeTransform(leftWeaponNode);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix() * Matrix.CreateRotation(Vector3.Up, MathF.PI);

		Renderer.DrawModel(model, transform, animator);

		Model rightWeaponModel = actionManager.currentAction != null && actionManager.currentAction.overrideHandModels[0] ?
			actionManager.currentAction.handItemModels[0].model : rightWeapon != null ? rightWeapon.model : null;
		if (rightWeaponModel != null)
			Renderer.DrawModel(rightWeaponModel, rightWeaponTransform);

		if (actionManager.currentAction != null)
			actionManager.currentAction.draw(this);
	}
}
