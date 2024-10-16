using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LeprechaunAI : AdvancedAI
{
	const float teleportTriggerDistance = 2.0f;
	const float teleportChargeTime = 0.0f;
	const float teleportCooldownTime = 0.5f;


	public LeprechaunAI(Mob mob)
		: base(mob)
	{
		aggroRange = 12.0f;
		loseRange = 20.0f;
		loseTime = 5.0f;

		AIAction teleport = addAction("teleport", 0, teleportChargeTime, teleportCooldownTime, 0, (AIAction action, Vector2 toTarget, float targetDistance) =>
		{
			return targetDistance > teleportTriggerDistance && Hash.hash(Time.currentTime / 1000000000) % 4 == 0;
		});
		teleport.onStarted = (AIAction action) =>
		{
			GameState.instance.level.addEntity(Effects.CreateTeleportEffect(mob, 0xFF4b692f), mob.position);
			SpellEffects.TeleportEntity(mob, true, target.position, aggroRange);
		};
	}

	public override void onAttacked(Entity e)
	{
		if (e is Player)
		{
			Player player = e as Player;
			if (player.money > 0)
			{
				int stealAmount = MathHelper.RandomInt(1, (int)MathF.Ceiling(player.money * MathF.Exp(-player.money * 0.004f)));
				player.money -= stealAmount;
				for (int i = 0; i < stealAmount; i++)
				{
					Coin coin = new Coin();
					coin.velocity = new Vector2(MathHelper.RandomFloat(-1, 1), 2) * 5;
					GameState.instance.level.addEntity(coin, e.position);
					coin.target = mob;
				}
			}
		}
	}

	/*
	void updateTargetFollow()
	{
		if (canSeeEntity(target, out Vector2 toTarget, out float distance))
			targetLastSeen = Time.currentTime;

		TileType forwardTile = GameState.instance.level.getTile(mob.position + new Vector2(0.4f * walkDirection, 0.5f));

		if (state == AIState.Default)
		{
			mob.animator.setAnimation("run");

			walkDirection = target.position.x < mob.position.x ? -1 : 1;

			if (canJump && forwardTile != null)
			{
				state = AIState.Jump;
				dashDirection = walkDirection;
				mob.inputJump = true;
				//mob.speed = jumpSpeed;
			}
			else if (distance > teleportTriggerDistance && Hash.hash(Time.currentTime / 1000000000) % 4 == 0)
			{
				state = AIState.Charge;
				chargeTime = Time.currentTime;
				dashDirection = walkDirection;
			}
		}
		if (state == AIState.Charge)
		{
			mob.animator.setAnimation("idle");

			if ((Time.currentTime - chargeTime) / 1e9f > teleportChargeTime)
			{
				state = AIState.Attack;
				attackTime = Time.currentTime;
			}

			if (mob.isStunned)
			{
				state = AIState.Default;
			}
		}
		if (state == AIState.Attack)
		{
			mob.animator.setAnimation("idle");

			SpellEffects.TeleportEntity(mob, true, target.position, aggroRange);

			state = AIState.Cooldown;
			cooldownTime = Time.currentTime;
		}
		if (state == AIState.Cooldown)
		{
			mob.animator.setAnimation("idle");

			if ((Time.currentTime - cooldownTime) / 1e9f > teleportCooldownTime)
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
				//mob.speed = walkSpeed;
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
		else if (state == AIState.Jump)
		{
			if (dashDirection == -1)
				mob.inputLeft = true;
			else
				mob.inputRight = true;
		}

		if (forwardTile != null)
		{
			walkDirection *= -1;
		}
		else
		{
			HitData forwardDownTile = GameState.instance.level.sampleTiles(mob.position + new Vector2(0.4f * walkDirection, -0.5f));
			HitData forwardDownDownTile = GameState.instance.level.sampleTiles(mob.position + new Vector2(0.4f * walkDirection, -1.5f));
			if (forwardDownTile == null /*&& forwardDownDownTile == null)
			{
				walkDirection *= -1;
			}
		}
	}

	void updatePatrol()
	{
		mob.animator.setAnimation("run");

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
				if (distance < aggroRange && MathF.Sign(toTarget.x) == mob.direction || distance < 0.5f * aggroRange)
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
	*/
}
