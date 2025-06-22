using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ThirdPersonCamera : Camera
{
	Entity target;
	Vector3 offset;
	Vector3 anchor;

	float distance = 1.2f;


	public ThirdPersonCamera(Entity target, Vector3 offset)
	{
		this.target = target;
		this.offset = offset;
	}

	public override void init()
	{
		base.init();

		Input.cursorMode = CursorMode.Disabled;
	}

	public override void update()
	{
		yaw -= 0.001f * Input.cursorMove.x;
		pitch -= 0.001f * Input.cursorMove.y;

		rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw) * Quaternion.FromAxisAngle(Vector3.Right, pitch);
		position = anchor + rotation.back * distance;
	}

	public override void fixedUpdate(float delta)
	{
		Vector3 targetAnchor = target.position + offset;
		anchor = Vector3.Lerp(anchor, targetAnchor, 12 * delta);
	}
}
