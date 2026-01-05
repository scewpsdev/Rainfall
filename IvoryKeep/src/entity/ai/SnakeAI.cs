using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SnakeAI : AdvancedAI
{
	public float dashChargeTime = 0.8f;
	public float dashCooldownTime = 1.0f;
	float dashSpeed = 8;
	float dashTriggerDistance = 2;
	float dashDistance = 4;
	//float dashDuration = 0.15f;

	float mobSpeed;


	public SnakeAI(Mob mob)
		: base(mob)
	{
		aggroRange = 5.0f;
		loseRange = 10.0f;
		loseTime = 4.0f;

		runAnim = "idle";
		hesitation = 0;

		mobSpeed = mob.speed;

		float dashDuration = dashDistance / dashSpeed;
		addAction("attack", dashDuration, "idle", dashChargeTime, "idle", dashCooldownTime, dashSpeed, (AIAction action, Vector2 toTarget, float distance) =>
		{
			return distance < dashTriggerDistance;
		});
	}

	public override void update()
	{
		base.update();
		if (currentAction == null)
		{
			if (target != null)
				mob.speed = mobSpeed * 2;
			else
				mob.speed = mobSpeed;
		}
	}
}
