using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SnakeAI : AdvancedAI
{
	const float dashChargeTime = 0.5f;
	const float dashCooldownTime = 0.5f;
	const float dashDuration = 0.5f;
	const float dashSpeedMultiplier = 4;
	const float dashTriggerDistance = 2;


	public SnakeAI(Mob mob)
		: base(mob)
	{
		aggroRange = 12.0f;
		loseRange = 15.0f;
		loseTime = 3.0f;

		AIAction dash = addAction("attack", dashDuration, dashChargeTime, dashCooldownTime, mob.speed * dashSpeedMultiplier, (AIAction action, Vector2 toTarget, float targetDistance) => targetDistance < dashTriggerDistance && mob.ai.canSeeTarget);
	}
}
