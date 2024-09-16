using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GolemAI : AI
{
	enum AIState
	{
		Default,
		Charge,
		Dash,
		Cooldown,
		Jump,
	}


	public float dashChargeTime = 0.5f;
	public float dashCooldownTime = 1.0f;
	float walkSpeed = 0.7f;
	float dashSpeed = 8;
	float jumpSpeed = 1.5f;
	float dashDistance = 3;
	float dashTriggerDistance = 3;

	AIState state = AIState.Default;
	int walkDirection = 1;
	int dashDirection;

	long chargeTime;
	long dashTime;
	long cooldownTime;

	long targetLastSeen = -1;


	public GolemAI(Mob mob)
		: base(mob)
	{
		aggroRange = 6.0f;
		loseRange = 10.0f;
		loseTime = 3.0f;
	}

	void beginDash()
	{
		state = AIState.Dash;
		dashTime = Time.currentTime;
		mob.speed = dashSpeed;
	}

	void endDash()
	{
		state = AIState.Cooldown;
		cooldownTime = Time.currentTime;
		mob.speed = walkSpeed;
	}

	void updateTargetFollow()
	{
		if (canSeeEntity(target, out Vector2 toTarget, out float distance))
			targetLastSeen = Time.currentTime;

		if (state == AIState.Default)
		{
			mob.animator.setAnimation("run");

			walkDirection = target.position.x < mob.position.x ? -1 : 1;

			TileType forwardTile = GameState.instance.level.getTile(mob.position + new Vector2(1.0f * walkDirection, 0.5f));
			TileType forwardUpTile = GameState.instance.level.getTile(mob.position + new Vector2(1.0f * walkDirection, 1.5f));
			if (forwardTile != null && forwardUpTile == null)
			{
				state = AIState.Jump;
				dashDirection = walkDirection;
				mob.inputJump = true;
				mob.speed = jumpSpeed;
			}
			else if (distance < dashTriggerDistance)
			{
				state = AIState.Charge;
				chargeTime = Time.currentTime;
				dashDirection = walkDirection;
			}
		}
		if (state == AIState.Charge)
		{
			mob.animator.setAnimation("charge");

			if ((Time.currentTime - chargeTime) / 1e9f > dashChargeTime)
				beginDash();
		}
		if (state == AIState.Dash)
		{
			mob.animator.setAnimation("attack");

			float dashDuration = dashDistance / dashSpeed;
			mob.animator.getAnimation("attack").duration = dashDuration;
			if ((Time.currentTime - dashTime) / 1e9f > dashDuration)
			{
				TileType tile = GameState.instance.level.getTile(mob.position - new Vector2(0, 0.5f));
				if (tile != null)
					GameState.instance.level.addEntity(Effects.CreateImpactEffect(Vector2.Up, 6, 40, MathHelper.ARGBToVector(tile.particleColor).xyz), mob.position + mob.direction * Vector2.Right);
				GameState.instance.camera.addScreenShake(mob.position + mob.direction * Vector2.Right, 1, 3);
				endDash();
			}
		}
		if (state == AIState.Cooldown)
		{
			mob.animator.setAnimation("cooldown");

			if ((Time.currentTime - cooldownTime) / 1e9f > dashCooldownTime)
			{
				state = AIState.Default;
			}
		}
		if (state == AIState.Jump)
		{
			mob.animator.setAnimation("jump");

			if (!mob.inputJump && mob.isGrounded)
			{
				state = AIState.Default;
				mob.speed = walkSpeed;
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
		else if (state == AIState.Jump)
		{
			if (dashDirection == -1)
				mob.inputLeft = true;
			else
				mob.inputRight = true;
		}
	}

	void updatePatrol()
	{
		mob.animator.setAnimation("run");

		if (walkDirection == 1)
			mob.inputRight = true;
		else if (walkDirection == -1)
			mob.inputLeft = true;

		TileType forwardTile = GameState.instance.level.getTile(mob.position + new Vector2(1.0f * walkDirection, 0.5f));
		TileType forwardUpTile = GameState.instance.level.getTile(mob.position + new Vector2(1.0f * walkDirection, 1.5f));
		if (forwardTile != null || forwardUpTile != null)
			walkDirection *= -1;
		else
		{
			TileType forwardDownTile = GameState.instance.level.getTile(mob.position + new Vector2(1.0f * walkDirection, -0.5f));
			if (forwardDownTile == null)
				walkDirection *= -1;
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
				if (distance < aggroRange && MathF.Sign(toTarget.x) == mob.direction || distance < 0.5f * aggroRange)
				{
					target = GameState.instance.player;
				}
			}
		}

		if (target != null)
		{
			bool targetLost = (target.position - mob.position).lengthSquared > loseRange * loseRange ||
				targetLastSeen != -1 && (Time.currentTime - targetLastSeen) / 1e9f > loseTime;
			if (targetLost && state == AIState.Default)
			{
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
