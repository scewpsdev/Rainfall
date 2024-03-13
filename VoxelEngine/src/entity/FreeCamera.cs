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


	public override void update()
	{
		base.update();

		float yawDelta = -Input.cursorMove.x * SENSITIVITY;
		float pitchDelta = -Input.cursorMove.y * SENSITIVITY;

		pitch += pitchDelta;
		yaw += yawDelta;

		pitch = MathHelper.Clamp(pitch, MathF.PI * -0.5f, MathF.PI * 0.5f);


		Vector3 velocity = Vector3.Zero;

		if (Input.IsKeyDown(KeyCode.A))
			velocity.x -= 1;
		if (Input.IsKeyDown(KeyCode.D))
			velocity.x += 1;
		if (Input.IsKeyDown(KeyCode.W))
			velocity.z -= 1;
		if (Input.IsKeyDown(KeyCode.S))
			velocity.z += 1;
		if (Input.IsKeyDown(KeyCode.Ctrl))
			velocity.y -= 1;
		if (Input.IsKeyDown(KeyCode.Space))
			velocity.y += 1;

		if (velocity.lengthSquared > 0)
		{
			float speedMultiplier = Input.IsKeyDown(KeyCode.Shift) ? 3 : 1;

			velocity = Quaternion.FromAxisAngle(Vector3.Up, yaw) * velocity.normalized * SPEED * speedMultiplier;
			Vector3 displacement = velocity * Time.deltaTime;
			position += displacement;
		}
	}
}
