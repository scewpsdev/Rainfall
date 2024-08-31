using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class AI
{
	protected Mob mob;


	protected AI(Mob mob)
	{
		this.mob = mob;
	}

	public virtual void update()
	{
	}

	public virtual void onHit(Entity by)
	{
	}

	protected bool canSeeEntity(Entity entity, out Vector2 toTarget, out float distance)
	{
		Vector2 toEntity = entity.position + entity.collider.center - (mob.position + mob.collider.center);
		toTarget = toEntity / toEntity.length;
		distance = toEntity.length;
		HitData hit = GameState.instance.level.raycastTiles(mob.position + mob.collider.center, toTarget, distance + 0.1f);
		return hit == null;
	}
}
