using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Player : Entity
{
	const float radius = 0.2f;

	float yaw;
	Vector3 velocity;
	bool isGrounded;

	CharacterController controller;

	Node rootNode;

	AnimationState idleAnim;
	AnimationState runAnim;


	public Player()
	{
		model = Resource.GetModel("player.gltf");
		modelTransform = Matrix.CreateRotation(Vector3.Up, MathF.PI);

		rootNode = model.skeleton.getNode("Root");

		animator = Animator.Create(model);
		idleAnim = Animator.CreateAnimation(model, "idle", true);
		runAnim = Animator.CreateAnimation(model, "run", true);
		runAnim.animationSpeed = 2;
	}

	public override void init()
	{
		base.init();

		controller = new CharacterController(this, radius, Vector3.Zero, 1);
	}

	public override void update()
	{
		base.update();

		Vector3 delta = Vector3.Zero;

		if (Input.IsKeyDown(KeyCode.A))
			delta.x--;
		if (Input.IsKeyDown(KeyCode.D))
			delta.x++;
		if (Input.IsKeyDown(KeyCode.W))
			delta.z--;
		if (Input.IsKeyDown(KeyCode.S))
			delta.z++;

		if (Input.IsKeyDown(KeyCode.Space) && isGrounded)
		{
			float jumpPower = 4;
			velocity.y = jumpPower;
		}

		velocity.y += -10 * Time.deltaTime;

		if (delta.lengthSquared > 0)
		{
			float speed = 6;
			velocity.xz = (delta.normalized * speed).xz;
		}
		else
		{
			velocity.xz = Vector2.Zero;
		}

		Vector3 displacement = velocity * Time.deltaTime;
		ControllerCollisionFlag collision = controller.move(displacement);
		if ((collision & ControllerCollisionFlag.Down) != 0)
			velocity.y = 0;

		isGrounded = false;
		PhysicsHit? hit = Physics.SweepSphere(radius, position + Vector3.Up * (radius + 0.2f), Vector3.Down, 0.2f + 0.2f);
		if (hit != null)
			isGrounded = true;

		yaw = MathHelper.LinearAngle(yaw, -displacement.xz.angle - MathF.PI * 0.5f, 15 * Time.deltaTime);
		rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw);


		if (velocity.xz.lengthSquared > 0)
		{
			animator.setAnimation(runAnim);
		}
		else
		{
			animator.setAnimation(idleAnim);
		}

		animator.applyAnimation();
	}
}
