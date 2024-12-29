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


	public BatAI(Mob mob)
		: base(mob)
	{
		aggroRange = 8.0f;
		loseRange = 12.0f;
		loseTime = 3.0f;

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
			mob.speed *= 1.5f;
		}
		else if (newTarget == null && target != null)
		{
			mob.speed *= 1.0f / 1.5f;
		}
	}
}
