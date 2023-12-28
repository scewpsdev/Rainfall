using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class FreeCamera : Camera
{
	const float SPEED = 3.0f;
	const float SENSITIVITY = 0.0015f;


	float pitch, yaw;

	public override void update()
	{
		base.update();

		float yawDelta = -Input.cursorMove.x * SENSITIVITY;
		float pitchDelta = -Input.cursorMove.y * SENSITIVITY;

		pitch += pitchDelta;
		yaw += yawDelta;

		pitch = MathHelper.Clamp(pitch, MathF.PI * -0.5f, MathF.PI * 0.5f);

		rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw) * Quaternion.FromAxisAngle(Vector3.Right, pitch);


		Vector3 velocity = Vector3.Zero;

		if (Input.IsKeyDown(KeyCode.KeyA))
			velocity.x -= 1;
		if (Input.IsKeyDown(KeyCode.KeyD))
			velocity.x += 1;
		if (Input.IsKeyDown(KeyCode.KeyW))
			velocity.z -= 1;
		if (Input.IsKeyDown(KeyCode.KeyS))
			velocity.z += 1;
		if (Input.IsKeyDown(KeyCode.LeftCtrl))
			velocity.y -= 1;
		if (Input.IsKeyDown(KeyCode.Space))
			velocity.y += 1;

		if (velocity.lengthSquared > 0)
		{
			velocity = Quaternion.FromAxisAngle(Vector3.Up, yaw) * velocity.normalized * SPEED;
			Vector3 displacement = velocity * Time.deltaTime;
			position += displacement;
		}
	}
}
