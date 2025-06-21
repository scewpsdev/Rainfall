using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Player2 : Entity
{
	public float yaw;
	Vector3 velocity;
	CharacterController controller;
	bool isGrounded;

	PlayerActionManager actionManager;
	Vector3 rootMotionDelta;
	Vector3 lastRootMotionDisplacement;
	PlayerAction lastRootMotionAction;
	AnimationState lastRootMotionAnim;

	Node rootNode;
	public Node rightWeaponNode;

	AnimationState defaultAnim;
	AnimationState idleAnim;
	AnimationState runAnim;
	AnimationState fallAnim;

	ClothEntity cloth;


	public Player2()
	{
		model = Resource.GetModel("entity/creature/generic_dude2.gltf");
		animator = Animator.Create(model);

		defaultAnim = Animator.CreateAnimation(model, "default", true);
		idleAnim = Animator.CreateAnimation(model, "idle", true, 0.2f);
		//idleAnim.animationSpeed = 0.1f;
		runAnim = Animator.CreateAnimation(model, "run", true, 0.2f);
		fallAnim = Animator.CreateAnimation(model, "fall", true, 0.2f);

		animator.setAnimation(idleAnim);
		animator.update();
		animator.applyAnimation();

		rootNode = model.skeleton.getNode("Root");
		rightWeaponNode = model.skeleton.getNode("Weapon.R");

		actionManager = new PlayerActionManager(this, rootNode);
	}

	public override void init()
	{
		base.init();

		controller = new CharacterController(this, 0.3f, Vector3.Zero, 2.0f);

		ClothParams clothParams = new ClothParams(0);
		clothParams.inertia = 0.1f;
		clothParams.gravity = new Vector3(0, -3, 0);
		scene.addEntity(cloth = new ClothEntity(Resource.GetModel("entity/creature/generic_dude2_cloth.gltf"), 0, animator, clothParams), position, rotation * Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));
		cloth.setSpheres(
			[new Vector4(0, 1.2f, 0, 0.2f), new Vector4(0, 0.1f, 0, 0.2f), new Vector4(0, 1.6f, 0, 0.1f)],
			[model.skeleton.getNode("Chest"), model.skeleton.getNode("Hips"), model.skeleton.getNode("Head")]
		);
		cloth.setCapsules([new Vector2i(0, 1)]);
		cloth.animator = Animator.Create(cloth.model);
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
			velocity.y = 4;
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

		cloth.position = position;
		cloth.rotation = rotation * Quaternion.FromAxisAngle(Vector3.Up, MathF.PI);
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
			actionManager.queueAction(new AttackAction2());
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

		cloth.animator.setAnimation(animator.currentAnimation);

		animator.applyAnimation();
		cloth.animator.applyAnimation();
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix() * Matrix.CreateRotation(Vector3.Up, MathF.PI), animator);
	}
}
