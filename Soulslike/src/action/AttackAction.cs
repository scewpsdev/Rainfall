using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AttackAction : PlayerAction
{
	Weapon weapon;

	List<Entity> hitEntities = new List<Entity>();

	public AttackAction(Weapon weapon)
		: base("attack")
	{
		this.weapon = weapon;

		animationName[0] = "attack1";
		animationSet[0] = weapon.moveset;

		viewmodelAim = 1;
	}

	public override void update(Player player)
	{
		base.update(player);

		if (elapsedTime > 0.1f)
		{
			Matrix weaponTransform = player.rightWeaponTransform;
			Span<HitData> hits = stackalloc HitData[16];
			SceneFormat.ColliderData hitbox = weapon.colliders[weapon.colliders.Count - 1];
			Debug.Assert(hitbox.trigger && hitbox.type == SceneFormat.ColliderType.Capsule);
			int numHits = Physics.OverlapCapsule(hitbox.radius, hitbox.height, weaponTransform.translation + weaponTransform.rotation * hitbox.offset, weaponTransform.rotation, hits, QueryFilterFlags.Default, PhysicsFiltering.CREATURE_HITBOX);
			for (int i = 0; i < numHits; i++)
			{
				Entity entity = hits[i].body.entity as Entity;
				if (!hitEntities.Contains(entity))
				{
					hitEntities.Add(entity);
					if (entity is Hittable)
					{
						Hittable hittable = entity as Hittable;
						hittable.hit(player, weapon);
					}
				}
			}
		}
	}
}
