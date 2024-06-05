using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PlayerCamera : Camera
{
	Vector3 targetOffset = new Vector3(0, 1.8f, 0);

	Player player;

	Creature lockedOnTarget = null;

	Vector3 target;
	float distance = 50;
	float distanceDst = 2.2f;
	public float pitch = 0.0f, yaw = 0.0f;


	public PlayerCamera(Player player)
	{
		this.player = player;

		fov = 60;
		near = 0.5f;
		far = 200;

		target = player.position + targetOffset;
		pitch = MathHelper.ToRadians(-45);
		yaw = MathHelper.ToRadians(45);
	}

	public override void init()
	{
		base.init();

		Input.mouseLocked = true;
	}

	Creature getLockOnTarget()
	{
		float range = 10;
		Span<HitData> hits = stackalloc HitData[32];
		int numHits = Physics.OverlapSphere(range, player.position, hits, QueryFilterFlags.Dynamic, PhysicsFiltering.CREATURE);
		if (numHits > 0)
		{
			List<Creature> candidates = new List<Creature>();
			for (int i = 0; i < numHits; i++)
			{
				float d = Vector3.Dot(hits[i].position - position, rotation.forward);
				if (d > 0)
				{
					candidates.Add(hits[i].body.entity as Creature);
				}
			}

			if (candidates.Count > 0)
			{
				Debug.Assert(candidates.Count == 1);

				candidates.Sort((Creature a, Creature b) =>
				{
					float da = (a.position - player.position).length;
					float db = (b.position - player.position).length;
					return da < db ? 1 : da > db ? -1 : 0;
				});
				return candidates[0];
			}
		}

		return null;
	}

	public override void update()
	{
		base.update();

		Vector3 targetDst = player.position + targetOffset;
		target = Vector3.Lerp(target, targetDst, 8 * Time.deltaTime);
		Matrix transform = Matrix.CreateTranslation(target) * Matrix.CreateRotation(Vector3.Up, yaw) * Matrix.CreateRotation(Vector3.Right, pitch) * Matrix.CreateTranslation(0.0f, 0.0f, distance);
		transform.decompose(out position, out rotation, out _);

		distanceDst *= MathF.Pow(1.5f, -Input.scrollMove * 0.2f);
		distance = MathHelper.Lerp(distance, distanceDst, 6 * Time.deltaTime);

		if (lockedOnTarget != null)
		{
			float dz = lockedOnTarget.position.z - player.position.z;
			float dx = lockedOnTarget.position.x - player.position.x;
			float yawDst = MathF.Atan2(-dz, dx) - MathF.PI * 0.5f;
			yaw = MathHelper.LerpAngle(yaw, yawDst, 3 * Time.deltaTime);

			float dy = lockedOnTarget.position.y - (player.position.y + 1);
			float dd = (lockedOnTarget.position.xz - player.position.xz).length;
			float pitchDst = MathF.Atan2(dy, dd);
			pitch = MathHelper.LerpAngle(pitch, pitchDst, 1 * Time.deltaTime);
		}
		else
		{
			pitch -= Input.cursorMove.y * 0.001f;
			yaw -= Input.cursorMove.x * 0.001f;
		}

		if (Input.IsKeyPressed(KeyCode.R))
		{
			if (lockedOnTarget == null)
			{
				lockedOnTarget = getLockOnTarget();
				if (lockedOnTarget != null)
					player.strafing = true;
			}
			else
			{
				lockedOnTarget = null;
				player.strafing = false;
			}
		}
		if (lockedOnTarget != null && !lockedOnTarget.isAlive())
		{
			lockedOnTarget = null;
			player.strafing = false;
		}
	}
}
