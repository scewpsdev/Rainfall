using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ArcherAI : AdvancedAI
{
	public float attackTriggerDistance = 6.0f;
	public float attackChargeTime = 1.2f;
	public float attackCooldownTime = 1.0f;

	Sound shootSound;


	public ArcherAI(Mob mob)
		: base(mob)
	{
		aggroRange = 12.0f;
		loseRange = 15.0f;
		loseTime = 4.0f;

		shootSound = Resource.GetSound("res/sounds/bow_shoot.ogg");

		AIAction shoot = addAction("idle", 0, attackChargeTime, attackCooldownTime, 0, (AIAction action, Vector2 toTarget, float targetDistance) => targetDistance < attackTriggerDistance);
		shoot.onAction = (AIAction action, float elapsed, Vector2 toTarget) =>
		{
			Vector2 position = mob.position + new Vector2(0, 0.5f);

			Vector2 velocity = toTarget.normalized * 10;

			ArrowProjectile projectile = new ArrowProjectile(velocity.normalized, Vector2.Zero, mob, null, null);
			GameState.instance.level.addEntity(projectile, position);

			Audio.PlayOrganic(shootSound, new Vector3(mob.position, 0));

			return true;
		};
	}
}
