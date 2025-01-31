using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SpiderAI : AdvancedAI
{
	float jumpChargeTime = 2.0f;
	float jumpSpeed = 4;
	float chargeSpeed = 0.25f;


	public SpiderAI(Mob mob, float jumpChargeTime = 2.0f)
		: base(mob)
	{
		aggroRange = 8.0f;
		loseRange = 12.0f;
		loseTime = 6.0f;

		this.jumpChargeTime = jumpChargeTime;

		patrol = false;

		AIAction jump = addAction("", 10, 0, jumpChargeTime, jumpSpeed, (AIAction action, Vector2 toTarget, float distance) =>
		{
			return true;
		}, (AIAction action) =>
		{
			mob.inputJump = true;
		}
		, (AIAction action, float elapsed, Vector2 toTarget) =>
		{
			if (elapsed > 0.1f && mob.isGrounded)
				return false;
			return true;
		});
		jump.cooldownSpeed = chargeSpeed;
	}
}
