using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SnakeAI : AdvancedAI
{
	public float dashChargeTime = 0.5f;
	public float dashCooldownTime = 1.0f;
	float dashDuration = 0.3f;
	float dashSpeedMultiplier = 3;
	float dashTriggerDistance = 2;


	public SnakeAI(Mob mob)
		: base(mob)
	{
		aggroRange = 4.0f;
		loseRange = 5.0f;
		loseTime = 3.0f;

		runAnim = "idle";
		hesitation = 0;

		addAction("attack", dashDuration, "idle", dashChargeTime, "idle", dashCooldownTime, mob.speed * dashSpeedMultiplier, (AIAction action, Vector2 toTarget, float distance) =>
		{
			return distance < dashTriggerDistance;
		});
	}
}
