using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class PlayerCamera : Camera
{
	Entity player;

	Vector3 target;
	float distance = 4;
	float pitch = 0.0f, yaw = 0.0f;


	public PlayerCamera(Entity player)
	{
		this.player = player;

		fov = 60;
		near = 0.5f;
		far = 200;

		target = player.position + Vector3.Up * 2;
		pitch = MathHelper.ToRadians(-45);
	}

	public override void update()
	{
		base.update();

		Vector3 targetDst = player.position + Vector3.Up * 2;
		target = Vector3.Lerp(target, targetDst, 2 * Time.deltaTime);
		Matrix transform = Matrix.CreateTranslation(target) * Matrix.CreateRotation(Vector3.Up, yaw) * Matrix.CreateRotation(Vector3.Right, pitch) * Matrix.CreateTranslation(0.0f, 0.0f, distance);
		transform.decompose(out position, out rotation, out _);
	}
}
