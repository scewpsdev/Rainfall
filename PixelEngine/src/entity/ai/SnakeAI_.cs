using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SnakeAI_ : AI
{
	enum AIState
	{
		Default,
		Charge,
		Dash,
		Cooldown,
	}


	public float dashChargeTime = 0.5f;
	public float dashCooldownTime = 1.0f;
	float dashDuration = 0.3f;
	float dashSpeedMultiplier = 3;
	float dashTriggerDistance = 2;

	AIState state = AIState.Default;
	int walkDirection = 1;
	int dashDirection;

	long chargeTime;
	long dashTime;
	long cooldownTime;

	long targetLastSeen = -1;
	long lastTurn = -1;


	public SnakeAI_(Mob mob)
		: base(mob)
	{
		aggroRange = 4.0f;
		loseRange = 5.0f;
		loseTime = 3.0f;
	}

	void beginDash()
	{
		state = AIState.Dash;
		dashTime = Time.currentTime;
		mob.speed *= dashSpeedMultiplier;
	}

	void endDash()
	{
		state = AIState.Cooldown;
		cooldownTime = Time.currentTime;
		mob.speed /= dashSpeedMultiplier;
	}

	void updateTargetFollow()
	{
		if (canSeeEntity(target, out Vector2 toTarget, out float distance))
			targetLastSeen = Time.currentTime;

		if (state == AIState.Default)
		{
			mob.animator.setAnimation("idle");

			walkDirection = target.position.x < mob.position.x ? -1 : 1;

			if (distance < dashTriggerDistance)
			{
				state = AIState.Charge;
				chargeTime = Time.currentTime;
				dashDirection = walkDirection;
			}
		}
		if (state == AIState.Charge)
		{
			mob.animator.setAnimation("idle");

			if ((Time.currentTime - chargeTime) / 1e9f > dashChargeTime)
				beginDash();
		}
		if (state == AIState.Dash)
		{
			mob.animator.setAnimation("attack");

			if ((Time.currentTime - dashTime) / 1e9f > dashDuration)
				endDash();
		}
		if (state == AIState.Cooldown)
		{
			mob.animator.setAnimation("idle");

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
	}

	void updatePatrol()
	{
		mob.animator.setAnimation("idle");

		if (walkDirection == 1)
			mob.inputRight = true;
		else if (walkDirection == -1)
			mob.inputLeft = true;

		TileType forwardTile = GameState.instance.level.getTile(mob.position + new Vector2(0.5f * walkDirection, 0.5f));
		if (forwardTile != null && forwardTile.isSolid)
		{
			if ((Time.currentTime - lastTurn) / 1e9f > 0.1f)
			{
				walkDirection *= -1;
				lastTurn = Time.currentTime;
			}
		}
		else
		{
			TileType forwardDownTile = GameState.instance.level.getTile(mob.position + new Vector2(0.5f * walkDirection, -0.5f));
			if (forwardDownTile == null && (Time.currentTime - lastTurn) / 1e9f > 0.1f)
			{
				walkDirection *= -1;
				lastTurn = Time.currentTime;
			}
		}
	}

	public override void update()
	{
		mob.inputRight = false;
		mob.inputLeft = false;
		mob.inputJump = false;

		if (target == null)
		{
			if (state == AIState.Dash)
				endDash();
			if (canSeeEntity(GameState.instance.player, out Vector2 toTarget, out float distance))
			{
				if (distance < aggroRange && MathF.Sign(toTarget.x) == mob.direction || distance < (mob.isBoss ? aggroRange : 0.25f * aggroRange))
				{
					target = GameState.instance.player;
				}
			}
		}

		if (target != null)
		{
			float effectiveLoseRange = loseRange * GameState.instance.player.visibility;
			if ((target.position - mob.position).lengthSquared > effectiveLoseRange * effectiveLoseRange ||
				targetLastSeen != -1 && (Time.currentTime - targetLastSeen) / 1e9f > loseTime)
			{
				if (state == AIState.Dash)
					endDash();
				target = null;
				targetLastSeen = -1;
			}
		}

		if (target != null)
			updateTargetFollow();
		else
			updatePatrol();
	}
}
