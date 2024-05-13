using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class FreeCamera : Camera
{
	const float CAMERA_SENSITIVITY = 0.001f;


	float pitch, yaw;


	public override void init()
	{
		base.init();

		Input.mouseLocked = true;

		pitch = rotation.eulers.x;
		yaw = rotation.eulers.y;
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
		if (Input.IsKeyDown(KeyCode.Space))
			delta.y++;
		if (Input.IsKeyDown(KeyCode.Ctrl))
			delta.y--;

		if (delta.lengthSquared > 0)
		{
			delta = delta.normalized;
			float speed = Input.IsKeyDown(KeyCode.Shift) ? 12 : Input.IsKeyDown(KeyCode.Alt) ? 4 : 8;
			Vector3 velocity = rotation * delta * speed;
			position += velocity * Time.deltaTime;
		}

		yaw -= Input.cursorMove.x * CAMERA_SENSITIVITY;
		pitch -= Input.cursorMove.y * CAMERA_SENSITIVITY;
		pitch = MathHelper.Clamp(pitch, -0.5f * MathF.PI, 0.5f * MathF.PI);
		rotation = Quaternion.FromAxisAngle(Vector3.UnitY, yaw) * Quaternion.FromAxisAngle(Vector3.UnitX, pitch);
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		Renderer.SetCamera(position, rotation, getProjectionMatrix(), near, far);
	}
}
