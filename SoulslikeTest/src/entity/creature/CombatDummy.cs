using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CombatDummy : Creature
{
	Vector2 knockbackVelocity;
	Vector2 offCenter;


	public CombatDummy(EntityType type)
		: base(type)
	{
	}

	public override bool hit(float damage, float poiseDamage, Entity from, Item item, Vector3 hitPosition, Vector3 hitDirection, RigidBody body)
	{
		Vector2 direction = hitDirection.xz;
		float force = damage / 10.0f;
		knockbackVelocity += direction * force;

		if (hitSound != null)
			Audio.PlayOrganic(hitSound, hitPosition);

		return false;
	}

	public override bool isAlive()
	{
		return true;
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
