using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SnakeAI : AI
{
	enum AIState
	{
		Default,
		Charge,
		Dash,
		Cooldown,
	}


	public float aggroRange = 4.0f;
	public float loseRange = 5.0f;
	public float dashChargeTime = 0.5f;
	public float dashCooldownTime = 1.0f;
	float dashDuration = 0.3f;

	AIState state = AIState.Charge;
	int walkDirection;
	int dashDirection;

	long chargeTime;
	long dashTime;
	long cooldownTime;

	Player target;


	public void onHit(Entity by)
	{
		if (target == null && by is Player)
			target = by as Player;
	}

	void updateTargetFollow(Mob mob)
	{
		walkDirection = target.position.x < mob.position.x ? -1 : 1;

		float distance = MathF.Abs(mob.position.x - target.position.x);

		if (state == AIState.Default)
		{
			if (distance < 2.0f)
			{
				state = AIState.Charge;
				chargeTime = Time.currentTime;
				dashDirection = walkDirection;
			}
		}
		if (state == AIState.Charge)
		{
			if ((Time.currentTime - chargeTime) / 1e9f > dashChargeTime)
			{
				state = AIState.Dash;
				dashTime = Time.currentTime;
				mob.speed *= 3;
			}
		}
		if (state == AIState.Dash)
		{
			if ((Time.currentTime - dashTime) / 1e9f > dashDuration)
			{
				state = AIState.Cooldown;
				cooldownTime = Time.currentTime;
				mob.speed /= 3;
			}
		}
		if (state == AIState.Cooldown)
		{
			if ((Time.currentTime - cooldownTime) / 1e9f > dashCooldownTime)
			{
				state = AIState.Default;
			}
		}

		if (state == AIState.Default)
		{
			if (walkDirection == -1)
				mob.inputLeft = true;
			else
				mob.inputRight = true;
		}
		else if (state == AIState.Dash)
		{
			if (dashDirection == -1)
				mob.inputLeft = true;
			else
				mob.inputRight = true;
		}

		HitData forwardTile = GameState.instance.level.sampleTiles(mob.position + new Vector2(0.4f * walkDirection, 0.5f));
		if (forwardTile != null)
		{
			walkDirection *= -1;
		}
		else
		{
			HitData forwardDownTile = GameState.instance.level.sampleTiles(mob.position + new Vector2(0.4f * walkDirection, -0.5f));
			HitData forwardDownDownTile = GameState.instance.level.sampleTiles(mob.position + new Vector2(0.4f * walkDirection, -1.5f));
			if (forwardDownTile == null /*&& forwardDownDownTile == null*/)
			{
				walkDirection *= -1;
			}
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
