using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PlayerCamera : Camera
{
	Entity target;
	Vector3 offset;
	Vector3 anchor;

	float distance;


	public PlayerCamera(Entity target)
	{
		this.target = target;

		offset = new Vector3(0, 0, 0);

		pitch = -30;
		fov = 43;

		distance = 16;
	}

	public override void init()
	{
		base.init();

		Input.cursorMode = CursorMode.Disabled;
	}

	public override void update()
	{
		//yaw -= 0.001f * Input.cursorMove.x;
		//pitch -= 0.001f * Input.cursorMove.y;
		if (Input.IsKeyDown(KeyCode.Right))
			yaw -= 90 * Time.deltaTime;
		if (Input.IsKeyDown(KeyCode.Left))
			yaw += 90 * Time.deltaTime;
		if (Input.IsKeyDown(KeyCode.Up))
			pitch += 90 * Time.deltaTime;
		if (Input.IsKeyDown(KeyCode.Down))
			pitch -= 90 * Time.deltaTime;

		rotation = Quaternion.FromAxisAngle(Vector3.Up, MathHelper.ToRadians(yaw)) * Quaternion.FromAxisAngle(Vector3.Right, MathHelper.ToRadians(pitch));
		position = anchor + rotation.back * distance;
	}

	public override void fixedUpdate(float delta)
	{
		Vector3 targetAnchor = target.position + offset;
		float anchorDistance = Vector3.Distance(targetAnchor, anchor);
		anchor = Vector3.Lerp(anchor, targetAnchor, anchorDistance * 20 * delta);
	}
}
