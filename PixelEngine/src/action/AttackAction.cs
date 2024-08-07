using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AttackAction : EntityAction
{
	Item weapon;

	public int direction;

	List<Entity> hitEntities = new List<Entity>();


	public AttackAction(Item weapon)
		: base("attack")
	{
		duration = 1.0f / weapon.attackRate;

		this.weapon = weapon;
	}

	public override void onStarted(Player player)
	{
		direction = player.direction;
	}

	public override void update(Player player)
	{
		base.update(player);

		//HitData hit = GameState.instance.level.raycast(player.position + new Vector2(0.0f, 0.5f), new Vector2(player.direction, 0), weapon.attackRange, Entity.FILTER_MOB);
		Span<HitData> hits = new HitData[16];
		//int numHits = GameState.instance.level.overlap(player.position + new Vector2(0.5f * currentRange * direction - 0.5f * currentRange, 0.25f),
		//	player.position + new Vector2(0.5f * currentRange * direction + 0.5f * currentRange, 0.75f), hits, Entity.FILTER_MOB);
		int numHits = GameState.instance.level.raycastNoBlock(player.position + new Vector2(0, 0.5f), new Vector2(MathF.Cos(currentDirection) * direction, MathF.Sin(currentDirection)), currentRange, hits, Entity.FILTER_MOB);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity != null && hits[i].entity != player && hits[i].entity is Hittable && !hitEntities.Contains(hits[i].entity))
			{
				Hittable hittable = hits[i].entity as Hittable;
				hittable.hit(weapon.attackDamage, player);
				hitEntities.Add(hits[i].entity);

				if (hittable is Mob && ((Mob)hittable).health == 0)
					GameState.instance.run.kills++;
			}
		}
	}

	public float currentProgress
	{
		get => MathF.Min(elapsedTime / duration * 2, 1);
	}

	public float currentRange
	{
		get => weapon.stab ? currentProgress * weapon.attackRange : weapon.attackRange;
	}

	public float currentDirection
	{
		get => (1 - currentProgress) * weapon.attackAngle - 0.25f * MathF.PI;
	}
}
