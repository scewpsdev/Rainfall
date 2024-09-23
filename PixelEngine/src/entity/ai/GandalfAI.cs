using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GandalfAI : AI
{
	enum AIState
	{
		Default,
		Charge,
		Attack,
		Cooldown,
	}


	public float attackTriggerDistance = 6.0f;
	public float attackChargeTime = 0.5f;
	public float attackDuration = 1.0f;
	public float attackCooldownTime = 0.5f;

	AIState state = AIState.Default;
	int walkDirection = 1;
	int dashDirection;

	long chargeTime;
	long attackTime;
	long cooldownTime;
	int projectilesFired = 0;

	long targetLastSeen = -1;


	public GandalfAI(Mob mob)
		: base(mob)
	{
		aggroRange = 7.0f;
		loseRange = 9.0f;
		loseTime = 4.0f;
	}

	void updateTargetFollow()
	{
		if (canSeeEntity(target, out Vector2 toTarget, out float distance))
			targetLastSeen = Time.currentTime;

		if (state == AIState.Default)
		{
			mob.animator.setAnimation("idle");

			walkDirection = target.position.x < mob.position.x ? -1 : 1;

			if (distance < attackTriggerDistance)
			{
				state = AIState.Charge;
				chargeTime = Time.currentTime;
				dashDirection = walkDirection;
			}
		}
		if (state == AIState.Charge)
		{
			mob.animator.setAnimation("charge");

			if ((Time.currentTime - chargeTime) / 1e9f > attackChargeTime)
			{
				state = AIState.Attack;
				attackTime = Time.currentTime;
				projectilesFired = 0;
			}

			if (mob.isStunned)
			{
				state = AIState.Default;
			}
		}
		if (state == AIState.Attack)
		{
			mob.animator.setAnimation("attack");

			if ((Time.currentTime - attackTime) / 1e9f > attackDuration)
			{
				state = AIState.Cooldown;
				cooldownTime = Time.currentTime;
				projectilesFired = -1;
			}
			else
			{
				int projectilesShouldveFired = (int)MathF.Ceiling((Time.currentTime - attackTime) / 1e9f / attackDuration * 3);
				if (projectilesFired < projectilesShouldveFired)
				{
					projectilesFired++;

					Vector2 position = mob.position + new Vector2(0.0f, 0.3f);
					Vector2 offset = new Vector2(mob.direction * 0.5f, 0.3f);
					Vector2 direction = toTarget;
					GameState.instance.level.addEntity(new FireProjectile(direction, mob.velocity, offset, mob), position);
					GameState.instance.level.addEntity(new FireProjectileCastEffect(mob), position + offset);
				}
			}
		}
		if (state == AIState.Cooldown)
		{
			mob.animator.setAnimation("idle");

			if ((Time.currentTime - cooldownTime) / 1e9f > attackCooldownTime)
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
		else if (state == AIState.Attack)
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

	void updatePatrol()
	{
		mob.animator.setAnimation("idle");

		if (walkDirection == 1)
			mob.inputRight = true;
		else if (walkDirection == -1)
			mob.inputLeft = true;

		TileType forwardTile = GameState.instance.level.getTile(mob.position + new Vector2(0.5f * walkDirection, 0.5f));
		if (forwardTile != null)
			walkDirection *= -1;
		else
		{
			TileType forwardDownTile = GameState.instance.level.getTile(mob.position + new Vector2(0.5f * walkDirection, -0.5f));
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
				if (distance < aggroRange && MathF.Sign(toTarget.x) == mob.direction || distance < 0.2f * aggroRange)
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
		else
			updatePatrol();
	}
}
