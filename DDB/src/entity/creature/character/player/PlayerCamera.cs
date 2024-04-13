using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class PlayerCamera : Camera
{
	const float MOUSE_SENSITIVITY = 0.001f;


	Player player;

	public float pitch, yaw;
	public float cameraDistance;


	public PlayerCamera(Player player)
	{
		this.player = player;
		player.camera = this;

		near = 0.1f;
		fov = 70.0f;

		pitch = MathHelper.ToRadians(-30.0f);
		yaw = 0.0f;
		cameraDistance = 3.0f;
	}

	public override void update()
	{
		base.update();

		float dx = Input.cursorMove.x * MOUSE_SENSITIVITY
			+ (Input.IsKeyDown(KeyCode.KeyL) ? 4.0f : 0.0f) * Time.deltaTime
			- (Input.IsKeyDown(KeyCode.KeyJ) ? 4.0f : 0.0f) * Time.deltaTime;
		float dy = Input.cursorMove.y * MOUSE_SENSITIVITY
			+ (Input.IsKeyDown(KeyCode.KeyK) ? 4.0f : 0.0f) * Time.deltaTime
			- (Input.IsKeyDown(KeyCode.KeyI) ? 4.0f : 0.0f) * Time.deltaTime;
		pitch = Math.Clamp(pitch - dy, -MathF.PI * 0.4f, MathF.PI * 0.4f);
		yaw -= dx;

		Vector3 anchorPoint = new Vector3(0.0f, player.cameraHeight, 0.0f);

		float clampedCameraDistance = cameraDistance;
		RaycastHit[] hits = new RaycastHit[8];
		int numHits = Physics.Raycast(player.position + anchorPoint, rotation.back, cameraDistance, hits, 8, QueryFilterFlags.Static | QueryFilterFlags.NoBlock);
		for (int i = 0; i < numHits; i++)
			clampedCameraDistance = MathF.Min(clampedCameraDistance, hits[i].distance - 2 * near);

		Vector3 cameraOffset = Quaternion.FromAxisAngle(Vector3.Up, yaw) * Quaternion.FromAxisAngle(Vector3.Right, pitch) * new Vector3(0.0f, 0.0f, clampedCameraDistance);

		position = player.position + anchorPoint + cameraOffset;
		rotation = Quaternion.LookAt(cameraOffset, Vector3.Zero);
	}
}
