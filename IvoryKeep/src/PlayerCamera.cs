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
	float distance = 50;
	float distanceDst = 10;
	float pitch = 0.0f, yaw = 0.0f;


	public PlayerCamera(Entity player)
	{
		this.player = player;

		fov = 40;
		near = 0.5f;
		far = 200;

		target = player.position + Vector3.Up;
		pitch = MathHelper.ToRadians(-45);
	}

	public override void update()
	{
		base.update();

		Vector3 targetDst = player.position + Vector3.Up;
		target = Vector3.Lerp(target, targetDst, 5 * Time.deltaTime);
		Matrix transform = Matrix.CreateTranslation(target) * Matrix.CreateRotation(Vector3.Up, yaw) * Matrix.CreateRotation(Vector3.Right, pitch) * Matrix.CreateTranslation(0.0f, 0.0f, distance);
		transform.decompose(out position, out rotation, out _);

		distanceDst *= MathF.Pow(1.5f, -Input.scrollMove * 0.2f);
		distance = MathHelper.Lerp(distance, distanceDst, 6 * Time.deltaTime);
	}
}
