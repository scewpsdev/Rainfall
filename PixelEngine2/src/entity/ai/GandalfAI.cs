using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GandalfAI : AdvancedAI
{
	public float attackTriggerDistance = 6.0f;
	public float attackChargeTime = 0.5f;
	public float attackDuration = 1.0f;
	public float attackCooldownTime = 0.5f;

	int projectilesFired = 0;


	public GandalfAI(Mob mob)
		: base(mob)
	{
		aggroRange = 7.0f;
		loseRange = 9.0f;
		loseTime = 4.0f;

		AIAction shoot = addAction("attack", attackDuration, attackChargeTime, attackCooldownTime, mob.speed, (AIAction action, Vector2 toTarget, float targetDistance) => targetDistance < attackTriggerDistance && mob.ai.canSeeTarget);
		shoot.onStarted = (AIAction action) =>
		{
			projectilesFired = 0;
		};
		shoot.onFinished = (AIAction action) =>
		{
			projectilesFired = -1;
		};
		shoot.onAction = (AIAction action, float elapsed, Vector2 toTarget) =>
		{
			int projectilesShouldveFired = (int)MathF.Ceiling(elapsed / attackDuration * 3);
			if (projectilesFired < projectilesShouldveFired)
			{
				projectilesFired++;

				Vector2 position = mob.position + new Vector2(0.0f, 0.3f);
				Vector2 offset = new Vector2(mob.direction * 0.5f, 0.3f);
				Vector2 direction = toTarget.normalized;
				GameState.instance.level.addEntity(new FireProjectile(direction, mob.velocity, offset, mob), position);
				GameState.instance.level.addEntity(new FireProjectileCastEffect(mob), position + offset);
			}
			return true;
		};
	}
}
