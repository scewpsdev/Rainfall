using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SpiderAI : AdvancedAI
{
	public SpiderAI(Mob mob, float jumpChargeTime = 0.75f, float jumpSpeed = 4)
		: base(mob)
	{
		aggroRange = 8.0f;
		loseRange = 12.0f;
		loseTime = 6.0f;

		AIAction jump = addAction("idle", 100, jumpChargeTime, 0, jumpSpeed, (AIAction action, Vector2 toTarget, float targetDistance) => true);
		jump.onStarted = (AIAction action) =>
		{
			mob.inputJump = true;
		};
		jump.onAction = (AIAction action, float elapsed, Vector2 toTarget) =>
		{
			if (!mob.inputJump && mob.isGrounded)
				return false;
			return true;
		};
	}
}
