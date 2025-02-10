using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BatAI : AdvancedAI
{
	float shootCooldown = 1.0f;

	public bool preferVerticalMovement = false;
	public bool canShoot = false;

	float mobSpeed;


	public BatAI(Mob mob)
		: base(mob)
	{
		aggroRange = 8.0f;
		loseRange = 12.0f;
		loseTime = 3.0f;

		mobSpeed = mob.speed;
		runAnim = "idle";

		useAStar = true;

		addAction("", 0, 0, shootCooldown, 0, (AIAction action, Vector2 toTarget, float distance) =>
		{
			return canShoot && distance < 3.0f && target.position.y < mob.position.y - 0.1f;
		}, null, (AIAction action, float elapsed, Vector2 toTarget) =>
		{
			Vector2 offset = Vector2.Zero;
			Vector2 direction = (toTarget.normalized + new Vector2(mob.direction, 0) * 0.1f).normalized;
			FireProjectile projectile = new FireProjectile(direction, mob.velocity, offset, mob);
			GameState.instance.level.addEntity(projectile, mob.position);
			return true;
		});
	}

	protected override void onTargetSwitched(Entity newTarget)
	{
		if (newTarget != null && target == null)
		{
			mob.speed = mobSpeed * 1.5f;
		}
		else if (newTarget == null && target != null)
		{
			mob.speed = mobSpeed;
		}
	}

	public override void update()
	{
		base.update();

		patrol = true;
		mob.speed = mobSpeed;
		mob.animator.setAnimation(runAnim);

		if (target == null)
		{

			TileType tile = mob.level.getTile(mob.position);
			TileType up = mob.level.getTile(mob.position + Vector2.Up * 0.6f);
			if ((tile == null || !tile.isSolid) && up != null && up.isSolid)
			{
				mob.position.y = MathF.Ceiling(mob.position.y) - mob.collider.max.y - 0.1f;
				mob.speed = 0;
				mob.animator.setAnimation("hanging");
				patrol = false;
			}
		}
	}
}
