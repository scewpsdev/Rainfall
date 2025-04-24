using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class KickAction : PlayerAction
{
	int damage = 1;

	bool hasKicked;
	List<Entity> hitEntities = new List<Entity>();


	public KickAction()
		: base("kick", 0)
	{
		animationName[2] = "kick";

		fullBodyAnimation = true;
		rootMotion = true;

		movementSpeedMultiplier = 0.0f;
	}

	public override void update(Player player)
	{
		base.update(player);

		lockYaw = hasKicked;
		lockCameraRotation = hasKicked;

		if (elapsedTime > 12 / 24.0f && !hasKicked)
		{
			hasKicked = true;

			float range = 1.2f;
			Vector3 direction = player.rotation.forward;
			Vector3 origin = player.position + Vector3.Up * 1.0f;

			Span<HitData> hits = stackalloc HitData[16];
			int numHits = Physics.OverlapCapsule(0.25f, range, origin + direction * 0.5f * range, player.rotation * Quaternion.FromAxisAngle(Vector3.Right, MathF.PI * 0.5f), hits, QueryFilterFlags.Default, PhysicsFilter.Default | PhysicsFilter.CreatureHitbox);
			for (int i = 0; i < numHits; i++)
			{
				RigidBody body = hits[i].body;
				Entity entity = body.entity as Entity;
				if (!hitEntities.Contains(entity))
				{
					if (body.type == RigidBodyType.Dynamic)
					{
						Vector3 impulse = direction + new Vector3(0, 0.3f, 0);
						body.addImpulse(impulse * 350);
						body.setAngularVelocity(MathHelper.RandomVector3(-1, 1));
					}
					if (entity is Hittable)
					{
						Hittable hittable = entity as Hittable;
						hittable.hit(damage, false, direction, player, null, body);
					}
					hitEntities.Add(entity);
				}
			}
		}
	}
}
