using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AttackAction : PlayerAction
{
	public Weapon weapon;
	public AttackData attack;

	float damageStartTime;

	List<Entity> hitEntities = new List<Entity>();

	public AttackAction(Weapon weapon, AttackData attack, int hand)
		: base("attack")
	{
		this.weapon = weapon;
		this.attack = attack;

		animationName[hand] = attack.name;
		animationSet[hand] = weapon.moveset;

		if (weapon.twoHanded)
		{
			animationName[hand ^ 1] = attack.name;
			animationSet[hand ^ 1] = weapon.moveset;
		}

		mirrorAnimation = hand == 1;

		damageStartTime = attack.damageFrame / 24.0f;
		followUpCancelTime = attack.cancelFrame / 24.0f;

		lockYaw = true;

		viewmodelAim = 1;
	}

	bool inDamageWindow => elapsedTime >= damageStartTime && elapsedTime < followUpCancelTime;

	public override void update(Player player)
	{
		base.update(player);

		lockYaw = inDamageWindow;

		return;
		if (elapsedTime > 0.1f)
		{
			Matrix weaponTransform = player.rightWeaponTransform;
			Span<HitData> hits = stackalloc HitData[16];
			SceneFormat.ColliderData hitbox = weapon.colliders[weapon.colliders.Count - 1];
			Debug.Assert(hitbox.trigger && hitbox.type == SceneFormat.ColliderType.Capsule);
			int numHits = Physics.OverlapCapsule(hitbox.radius, hitbox.height, weaponTransform.translation + weaponTransform.rotation * hitbox.offset, weaponTransform.rotation, hits, QueryFilterFlags.Default, PhysicsFilter.CreatureHitbox);
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
