using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class FirstPersonCamera : Camera
{
	public float pitch, yaw;
	public float sensitivity = 1;


	public override void init()
	{
		base.init();

		lockCursor();
	}

	public void lockCursor()
	{
		Input.cursorMode = CursorMode.Disabled;
	}

	public void unlockCursor()
	{
		Input.cursorMode = CursorMode.Normal;
	}

	public override void update()
	{
		if (Input.cursorMode == CursorMode.Disabled)
		{
			pitch -= Input.cursorMove.y * 0.001f * sensitivity;
			yaw -= Input.cursorMove.x * 0.001f * sensitivity;

			pitch = MathHelper.Clamp(pitch, MathF.PI * -0.4f, MathF.PI * 0.4f);

			rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw) * Quaternion.FromAxisAngle(Vector3.Right, pitch);
		}
	}
}
