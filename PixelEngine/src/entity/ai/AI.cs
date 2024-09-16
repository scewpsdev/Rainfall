using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class AI
{
	protected Mob mob;

	public float aggroRange = 4.0f;
	public float loseRange = 5.0f;
	public float loseTime = 3.0f;

	public Entity target = null;


	protected AI(Mob mob)
	{
		this.mob = mob;
	}

	public virtual void update()
	{
	}

	public virtual void onHit(Entity by)
	{
		if (target == null)
		{
			if (by is ItemEntity)
				by = ((ItemEntity)by).thrower;
			if (by is Player || by is Mob)
				target = by;
		}
	}

	protected bool canSeeEntity(Entity entity, out Vector2 toTarget, out float distance)
	{
		if (entity is not Player)
		{
			Console.WriteLine(entity);
			Console.WriteLine(entity.name);
			Debug.Assert(false);
		}
		Vector2 toEntity = entity.position + entity.collider.center - (mob.position + mob.collider.center);
		toTarget = toEntity / toEntity.length;
		distance = toEntity.length;
		HitData hit = GameState.instance.level.raycastTiles(mob.position + mob.collider.center, toTarget, distance + 0.1f);
		return hit == null;
	}
}
