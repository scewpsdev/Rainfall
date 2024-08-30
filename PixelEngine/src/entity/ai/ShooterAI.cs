using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ShooterAI : AI
{
	enum AIState
	{
		Default,
		Charge,
		Attack,
		Cooldown,
	}


	public float aggroRange = 7.0f;
	public float loseRange = 9.0f;
	public float loseTime = 4.0f;
	public float attackTriggerDistance = 6.0f;
	public float attackChargeTime = 0.5f;
	public float attackDuration = 1.0f;
	public float attackCooldownTime = 0.5f;

	AIState state = AIState.Charge;
	int walkDirection = 1;
	int dashDirection;

	long chargeTime;
	long attackTime;
	long cooldownTime;
	int projectilesFired = 0;

	Entity target;
	long targetLastSeen = -1;


	public void onHit(Entity by)
	{
		if (target == null)
			target = by;
	}

	void updateTargetFollow(Mob mob)
	{
		walkDirection = target.position.x < mob.position.x ? -1 : 1;

		Vector2 toPlayer = target.position + target.collider.center - mob.position;
		float distance = toPlayer.length;

		HitData hit = GameState.instance.level.raycastTiles(mob.position, toPlayer.normalized, distance + 0.1f);
		if (hit == null)
			targetLastSeen = Time.currentTime;

		if (state == AIState.Default)
		{
			mob.animator.setAnimation("idle");

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
					((Gandalf)mob).shootProjectile();
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

	void updatePatrol(Mob mob)
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

	public void update(Mob mob)
	{
		mob.inputRight = false;
		mob.inputLeft = false;
		mob.inputJump = false;

		if (target == null)
		{
			Player player = GameState.instance.player;
			Vector2 toPlayer = player.position + player.collider.center - mob.position - new Vector2(0, 0.5f);
			float distance = toPlayer.length;
			if (distance < aggroRange)
			{
				HitData hit = GameState.instance.level.raycastTiles(mob.position + new Vector2(0, 0.5f), toPlayer.normalized, distance + 0.1f);
				if (hit == null)
					target = player;
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
			updateTargetFollow(mob);
		else
			updatePatrol(mob);
	}
}
