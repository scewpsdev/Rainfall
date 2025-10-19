using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class FollowCamera : Camera
{
	const float CAMERA_SENSITIVITY = 0.0015f;

	Entity follow;

	float pitch, yaw;

	public FollowCamera(Entity follow)
	{
		this.follow = follow;

		far = 500;
	}

	public override void init()
	{
		base.init();

		Input.cursorMode = CursorMode.Disabled;
	}

	public override void update()
	{
		base.update();

		yaw -= Input.cursorMove.x * CAMERA_SENSITIVITY;
		pitch -= Input.cursorMove.y * CAMERA_SENSITIVITY;
		pitch = MathHelper.Clamp(pitch, -0.5f * MathF.PI, 0.5f * MathF.PI);

		float speed = GameState.instance.player.ragdoll == null ? GameState.instance.cart.lastVelocity.length : 0;
		fov = MathHelper.Lerp(fov, MathHelper.Remap(1 - MathF.Exp(-speed * 0.025f), 0, 1, 70, 120), 1 * Time.deltaTime);

		//if ()
		//	Audio.SetListenerVelocity(GameState.instance.cart.lastVelocity);
		//else
		//	Audio.SetListenerVelocity(GameState.instance.player.ragdoll.getHitboxForNode(GameState.instance.player.ragdoll.rootNode).getVelocity());
	}

	public override void draw(GraphicsDevice graphics)
	{
		float speed = GameState.instance.player.ragdoll == null ? GameState.instance.cart.lastVelocity.length : 0;
		float height = MathHelper.Remap(1 - MathF.Exp(-speed * 0.025f), 0, 1, 3, 2);
		float distance = MathHelper.Remap(1 - MathF.Exp(-speed * 0.025f), 0, 1, 1, 0.7f);
		Matrix transform = Matrix.CreateTranslation(0, height, distance);
		transform = Matrix.CreateRotation(Vector3.Up, yaw) * Matrix.CreateRotation(Vector3.Right, pitch) * transform;
		transform = Matrix.CreateTranslation(follow.position) * transform;

		position = transform.translation;
		rotation = transform.rotation * Quaternion.FromAxisAngle(Vector3.Right, -0.25f * MathF.PI);
		//rotation = Quaternion.LookAt((follow.position - position) * new Vector3(1, 0, 1));

		base.draw(graphics);
	}
}
