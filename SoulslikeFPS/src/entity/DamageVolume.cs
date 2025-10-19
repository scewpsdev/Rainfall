using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DamageVolume : Entity
{
	float radius;
	int damage;
	int stagger;
	Entity by;
	float duration;

	float timer;


	public DamageVolume(float radius, float duration, int damage, int stagger, Entity by)
	{
		this.radius = radius;
		this.duration = duration;
		this.damage = damage;
		this.stagger = stagger;
		this.by = by;
	}

	public override void init()
	{
		base.init();

		body = new RigidBody(this, RigidBodyType.Kinematic, 0, PhysicsFilter.PlayerHitbox);
		body.addSphereTrigger(radius, Vector3.Zero);
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (other != null && other.entity != null && other.entity is Hittable)
		{
			Hittable hittable = other.entity as Hittable;
			HitData hit = new HitData(0);
			hit.damage = damage;
			hit.stagger = stagger;
			hit.hitDirection = (other.entity.getPosition() - position).normalized;
			hit.by = by;
			hit.hitbox = other;
			hittable.hit(hit);
			remove();
		}
	}

	public override void update()
	{
		base.update();

		timer += Time.deltaTime;
		if (timer >= duration)
		{
			remove();
		}
	}
}
