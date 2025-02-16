using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Player : Entity
{
	const float CAMERA_HEIGHT = 1.5f;
	const float CAMERA_HEIGHT_DUCKED = 0.9f;

	FirstPersonCamera camera;
	float cameraHeight = CAMERA_HEIGHT;

	public FirstPersonController controller;

	AnimationState idleAnim;


	public Player(FirstPersonCamera camera)
	{
		this.camera = camera;

		model = Resource.GetModel("viewmodel.gltf");
		animator = Animator.Create(model, this);

		idleAnim = Animator.CreateAnimation(model, "idle", true);
		animator.setAnimation(idleAnim);
	}

	public override void init()
	{
		controller = new FirstPersonController(this, PhysicsFilter.Default);
	}

	public override void update()
	{
		base.update();

		controller.inputLeft = Input.IsKeyDown(KeyCode.A);
		controller.inputRight = Input.IsKeyDown(KeyCode.D);
		controller.inputUp = Input.IsKeyDown(KeyCode.W);
		controller.inputDown = Input.IsKeyDown(KeyCode.S);
		controller.inputJump = Input.IsKeyPressed(KeyCode.Space);
		controller.inputDuck = Input.IsKeyDown(KeyCode.Ctrl);

		controller.update();

		cameraHeight = controller.isDucked ? CAMERA_HEIGHT_DUCKED :
			controller.inDuckTimer != -1 ? MathHelper.Lerp(CAMERA_HEIGHT, CAMERA_HEIGHT_DUCKED, controller.inDuckTimer / FirstPersonController.DUCK_TRANSITION_DURATION) :
			MathHelper.Linear(cameraHeight, CAMERA_HEIGHT, 5 * Time.deltaTime);

		camera.position = position + new Vector3(0, cameraHeight, 0);
		rotation = Quaternion.FromAxisAngle(Vector3.Up, camera.yaw);

		modelTransform = Matrix.CreateTranslation(0, cameraHeight, 0) * Matrix.CreateRotation(Vector3.Right, camera.pitch) * Matrix.CreateRotation(Vector3.Up, MathF.PI);
	}
}
