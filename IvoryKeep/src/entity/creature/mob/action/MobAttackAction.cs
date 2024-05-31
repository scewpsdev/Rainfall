using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MobAttackAction : MobAction
{
	public MobAttack attack;
	public Item item;

	Matrix lastWeaponTransform;

	List<Entity> hitEntities = new List<Entity>();

	public MobAttackAction(MobAttack attack, Item item)
		: base("mob_attack")
	{
		this.attack = attack;
		this.item = item;

		animationName = attack.animation;
		followUpCancelTime = attack.followUpCancelTime;

		animationSpeed = 0.8f;

		movementSpeedMultiplier = 0.0f;
		rotationSpeedMultiplier = 0.5f;

		addSoundEffect(new ActionSfx(Resource.GetSound("res/item/sfx/swing.ogg"), 1.0f, (attack.damageTimeStart + attack.damageTimeEnd) / 2, true));
	}

	public override void update(Mob mob)
	{
		base.update(mob);

		Span<HitData> hits = stackalloc HitData[256];
		int numHits = 0;
		if (elapsedTime >= attack.damageTimeStart && elapsedTime <= attack.damageTimeEnd)
		{
			foreach (SceneFormat.ColliderData collider in item.entity.Value.colliders)
			{
				Matrix colliderTransform = mob.rightWeaponTransform * Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers));

				Matrix lastColliderTransform = lastWeaponTransform * Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers));
				Vector3 delta = colliderTransform.translation - lastColliderTransform.translation;
				float distance = delta.length;
				Vector3 direction = delta / distance;

				if (collider.type == SceneFormat.ColliderType.Box)
					numHits = Physics.SweepBox(collider.size * 0.5f, lastColliderTransform.translation, colliderTransform.rotation, direction, distance, hits, QueryFilterFlags.Default, PhysicsFiltering.CREATURE_HITBOX | PhysicsFiltering.PLAYER);
				else if (collider.type == SceneFormat.ColliderType.Sphere)
					numHits = Physics.SweepSphere(collider.radius, lastColliderTransform.translation, direction, distance, hits, QueryFilterFlags.Default, PhysicsFiltering.CREATURE_HITBOX | PhysicsFiltering.PLAYER);
				else if (collider.type == SceneFormat.ColliderType.Capsule)
					numHits = Physics.SweepCapsule(collider.radius, collider.height, lastColliderTransform.translation, colliderTransform.rotation, direction, distance, hits, QueryFilterFlags.Default, PhysicsFiltering.CREATURE_HITBOX | PhysicsFiltering.PLAYER);

				for (int i = 0; i < numHits; i++)
				{
					HitData hit = hits[i];
					if (hit.body != null && hit.body.entity != mob)
					{
						// Pseudo friction
						// If the friction is high, the ragdoll gets pushed in the direction of the strike.
						// If the friction is low, the ragdoll slides along the weapon and moves in the direction of the inverse normal.
						float friction = 0.8f;
						Vector3 hitDirection = Vector3.Lerp(-hit.normal, direction, friction);
						onWeaponContact(hit.body, hit.position, hitDirection, mob);
					}
				}
			}
		}

		lastWeaponTransform = mob.rightWeaponTransform;
	}

	public void onWeaponContact(RigidBody body, Vector3 hitPosition, Vector3 hitDirection, Mob mob)
	{
		Entity entity = body.entity as Entity;
		if (entity != null)
		{
			bool firstHit = !hitEntities.Contains(entity);
			if (firstHit)
			{
				if (entity is Hittable)
				{
					if (entity is Mob)
						return;

					int damage = (int)MathF.Ceiling(item.baseDamage * attack.damageMultiplier);
					int poiseDamage = (int)MathF.Ceiling(item.poiseDamage * attack.poiseDamageMultiplier);

					Hittable hittable = entity as Hittable;
					hittable.hit(damage, poiseDamage, mob, item, hitPosition, hitDirection, body);
					hitEntities.Add(entity);
				}
			}
		}
	}
}
