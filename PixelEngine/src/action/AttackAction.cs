using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AttackAction : EntityAction
{
	Item weapon;


	public AttackAction(Item weapon)
		: base("attack")
	{
		duration = 0.3f;

		this.weapon = weapon;
	}

	public override void update(Player player)
	{
		base.update(player);

		//HitData hit = GameState.instance.level.raycast(player.position + new Vector2(0.0f, 0.5f), new Vector2(player.direction, 0), weapon.attackRange, Entity.FILTER_MOB);
		HitData hit = GameState.instance.level.overlap(player.position + new Vector2(0.5f * weapon.attackRange * player.direction - 0.5f * weapon.attackRange, 0.25f),
			player.position + new Vector2(0.5f * weapon.attackRange * player.direction + 0.5f * weapon.attackRange, 0.75f), Entity.FILTER_MOB);
		if (hit != null)
		{
			if (hit.entity != null && hit.entity != player && hit.entity is Hittable)
			{
				Hittable hittable = hit.entity as Hittable;
				hittable.hit(weapon.attackDamage, player);
			}
		}
	}
}
