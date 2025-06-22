using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TargetDummy : Entity, Hittable
{
	Vector2 knockbackVelocity;
	Vector2 offCenter;


	public override void init()
	{
		load("entity/object/target_dummy/target_dummy.rfs", PhysicsFilter.Default | PhysicsFilter.CreatureHitbox);
	}

	public void hit(int damage, bool criticalHit, Vector3 hitDirection, Entity by, Item item, RigidBody hitbox)
	{
		Vector2 direction = hitDirection.xz;
		float force = damage / 5.0f;
		knockbackVelocity += direction * force;
	}

	public override void update()
	{
		base.update();

		knockbackVelocity += -offCenter * Time.deltaTime * 100.0f;
		knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.Zero, 2.0f * Time.deltaTime);
		offCenter += knockbackVelocity * Time.deltaTime;

		rotation = Quaternion.LookAt(new Vector3(offCenter, -1));
		MathHelper.Swap(ref rotation.y, ref rotation.z);
	}
}
