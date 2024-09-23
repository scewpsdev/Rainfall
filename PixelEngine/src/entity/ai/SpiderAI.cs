using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SpiderAI : AI
{
	enum AIState
	{
		Charge,
		Jump
	}


	public float jumpChargeTime = 2.0f;
	float jumpSpeed = 4;
	float chargeSpeed = 0.25f;

	AIState state = AIState.Charge;
	int walkDirection;

	long lastAirTime;

	long targetLastSeen = -1;


	public SpiderAI(Mob mob)
		: base(mob)
	{
		aggroRange = 8.0f;
		loseRange = 12.0f;
		loseTime = 6.0f;
	}

	void updateTargetFollow()
	{
		if (canSeeEntity(target, out Vector2 toTarget, out float distance))
			targetLastSeen = Time.currentTime;

		if (!mob.isGrounded)
			lastAirTime = Time.currentTime;

		if (state == AIState.Charge)
		{
			walkDirection = target.position.x > mob.position.x ? 1 : target.position.x < mob.position.x ? -1 : 0;
			if ((Time.currentTime - lastAirTime) / 1e9f > jumpChargeTime)
			{
				state = AIState.Jump;
				mob.speed = jumpSpeed;
			}
		}
		else if (state == AIState.Jump)
		{
			if (mob.isGrounded)
			{
				state = AIState.Charge;
				mob.speed = chargeSpeed;
			}
		}

		if (state == AIState.Charge)
		{
			if (walkDirection == 1)
				mob.inputRight = true;
			else if (walkDirection == -1)
				mob.inputLeft = true;
		}
		else if (state == AIState.Jump)
		{
			if (walkDirection == 1)
				mob.inputRight = true;
			else if (walkDirection == -1)
				mob.inputLeft = true;

			mob.inputJump = true;
		}
	}

	public override void update()
	{
		mob.inputRight = false;
		mob.inputLeft = false;
		mob.inputJump = false;

		if (target == null)
		{
			if (canSeeEntity(GameState.instance.player, out Vector2 toTarget, out float distance))
			{
				if (distance < aggroRange && MathF.Sign(toTarget.x) == mob.direction || distance < (mob.isBoss ? aggroRange : 0.5f * aggroRange))
				{
					target = GameState.instance.player;
				}
			}
		}

		if (target != null)
		{
			if ((target.position - mob.position).lengthSquared > loseRange * loseRange ||
				targetLastSeen != -1 && (Time.currentTime - targetLastSeen) / 1e9f > loseTime)
			{
				target = null;
				targetLastSeen = -1;
			}
		}

		if (target != null)
			updateTargetFollow();
	}
}
