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


	public float aggroRange = 8.0f;
	public float loseRange = 12.0f;
	public float jumpChargeTime = 2.0f;

	AIState state = AIState.Charge;
	int walkDirection;

	long lastAirTime;

	Player target;


	void updateTargetFollow(Mob mob)
	{
		if (!mob.isGrounded)
			lastAirTime = Time.currentTime;

		if (state == AIState.Charge && (Time.currentTime - lastAirTime) / 1e9f > jumpChargeTime)
		{
			state = AIState.Jump;
			walkDirection = target.position.x > mob.position.x ? 1 : target.position.x < mob.position.x ? -1 : 0;
		}
		else if (state == AIState.Jump && mob.isGrounded)
			state = AIState.Charge;

		if (state == AIState.Charge)
		{
			;
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

	public void update(Mob mob)
	{
		mob.inputRight = false;
		mob.inputLeft = false;
		mob.inputJump = false;

		if (target == null)
		{
			Player player = GameState.instance.player;
			if ((player.position - mob.position).lengthSquared < aggroRange * aggroRange)
			{
				HitData hit = GameState.instance.level.raycastTiles(mob.position + new Vector2(0, 1), (player.position - mob.position).normalized, aggroRange);
				if (hit == null)
					target = player;
			}
		}

		if (target != null)
		{
			if ((target.position - mob.position).lengthSquared > loseRange * loseRange)
			{
				target = null;
			}
		}

		if (target != null)
			updateTargetFollow(mob);
	}
}
