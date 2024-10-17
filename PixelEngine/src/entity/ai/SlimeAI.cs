using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SlimeAI : AdvancedAI
{
	int numBounces = 0;

	public SlimeAI(Mob mob)
		: base(mob)
	{
		aggroRange = 12.0f;
		loseRange = 14.0f;
		loseTime = 6.0f;

		float jumpChargeTime = MathHelper.RandomFloat(0.7f, 0.8f);

		AIAction jump = addAction("idle", 100, jumpChargeTime, 0, 4, (AIAction action, Vector2 toTarget, float targetDistance) => true);
		jump.onStarted = (AIAction action) =>
		{
			mob.inputJump = true;
			numBounces = 0;
		};
		jump.onAction = (AIAction action, float elapsed, Vector2 toTarget) =>
		{
			if (!mob.inputJump && mob.isGrounded)
				return false;
			return true;
		};
	}
}
