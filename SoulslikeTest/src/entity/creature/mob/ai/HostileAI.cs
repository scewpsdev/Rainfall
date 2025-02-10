using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


public class HostileAI : AI
{
	public static HostileAI Create(Mob mob)
	{
		HostileAI ai = new HostileAI(mob);
		ai.lastUpdate = (long)(Random.Shared.NextSingle() / TICK_RATE * 1e9f);
		aiList.Add(ai);
		return ai;
	}

	public static void Destroy(HostileAI ai)
	{
		aiList.Remove(ai);
	}


	enum AggressionLevel
	{
		Retreat,
		Circle,
		Approach,
	}


	float detectionAngle = 90;

	Creature currentTarget;
	int currentStrafeDirection = 1;

	public Vector3 currentPatrolTarget = Vector3.Zero;

	float stamina = 100;
	//AggressionLevel aggressionLevel = AggressionLevel.Approach;


	HostileAI(Mob mob)
		: base(mob)
	{
	}

	public override void onHit(Entity from)
	{
		if (from is Creature)
		{
			if (currentTarget == null)
				currentTarget = from as Creature;
		}
	}

	public override void onSound(Vector3 position)
	{
		if (currentTarget == null)
		{
			Vector3 mobCenter = mob.position + Vector3.Up;
			Vector3 toSound = position - mobCenter;
			float distanceToSound = toSound.length;
			HitData? hit = Physics.Raycast(mobCenter, toSound / distanceToSound, distanceToSound - 0.1f);
			bool soundOriginVisible = hit == null;
			if (soundOriginVisible)
				currentPatrolTarget = position;
		}
	}

	bool attackIsSuitable(MobAttack attack, float distanceSq, float angle)
	{
		if (angle > attack.triggerAngleMin + 360)
			angle -= 360;
		if (angle < attack.triggerAngleMax - 360)
			angle += 360;
		return distanceSq >= attack.triggerDistanceMin * attack.triggerDistanceMin &&
				distanceSq <= attack.triggerDistanceMax * attack.triggerDistanceMax &&
				angle >= attack.triggerAngleMin &&
				angle <= attack.triggerAngleMax;
	}

	void getSuitableAttacks(float distanceSq, float angle, List<MobAttack> attacks)
	{
		for (int i = 0; i < mob.type.attacks.Count; i++)
		{
			if (attackIsSuitable(mob.type.attacks[i], distanceSq, angle))
				attacks.Add(mob.type.attacks[i]);
		}
	}

	void runToTarget(Vector3 toTarget, float delta)
	{
		mob.fsu = Vector3.Zero;
		mob.rotationTarget = Vector3.Zero;
		mob.running = true;

		Vector3 toTargetLocal = mob.rotation.conjugated * toTarget;
		float distanceSq = toTarget.lengthSquared;
		float angle = MathHelper.ToDegrees(Quaternion.LookAt(toTargetLocal).angle * MathF.Sign(Quaternion.LookAt(toTargetLocal).axis.y));

		stamina = MathHelper.Clamp(stamina + 5 * delta, 0, 100);

		// Attacking

		const float stoppingDistance = 2;

		AggressionLevel aggressionLevel = stamina > 20 ? AggressionLevel.Approach : AggressionLevel.Circle;

		float attackChance = aggressionLevel == AggressionLevel.Approach ? 1.0f
			: aggressionLevel == AggressionLevel.Circle ? 0.2f
			: 0.0f;
		attackChance *= 1.0f / MathF.Max(distanceSq - stoppingDistance * stoppingDistance, 1); // * (stamina / 100.0f);

		if (mob.actions.currentAction == null)
		{
			if (Random.Shared.NextSingle() < attackChance)
			{
				List<MobAttack> suitableAttacks = new List<MobAttack>();
				getSuitableAttacks(distanceSq, angle, suitableAttacks);
				if (suitableAttacks.Count > 0)
				{
					MobAttack attack = suitableAttacks[Random.Shared.Next() % suitableAttacks.Count];
					mob.actions.queueAction(new MobAttackAction(attack, mob.rightHandItem));
					//return;
					stamina -= 20;
				}
			}
		}
		else if (mob.actions.currentAction is MobAttackAction && mob.actions.currentAction.elapsedTime > mob.actions.currentAction.duration * 0.8f)
		{
			MobAttackAction currentAttackAction = mob.actions.currentAction as MobAttackAction;
			MobAttack? attack = mob.type.getNextAttack(currentAttackAction.attack);
			if (attack != null && attackIsSuitable(attack.Value, distanceSq, angle))
				mob.actions.queueAction(new MobAttackAction(attack.Value, currentAttackAction.item));
			stamina -= 20;
		}

		// Moving

		if (aggressionLevel == AggressionLevel.Approach)
		{
			mob.rotationTarget = toTarget;

			if (distanceSq > 2 * 2)
				mob.fsu.z = 1;
		}
		else if (aggressionLevel == AggressionLevel.Circle)
		{
			mob.rotationTarget = toTarget;

			if (Random.Shared.NextSingle() < 0.03f)
				currentStrafeDirection *= -1;

			if (distanceSq > 2 * 2)
				mob.fsu.x = currentStrafeDirection * 0.3f;
		}
	}

	void updateTargetFollow(float delta)
	{
		if (!currentTarget.isAlive())
		{
			currentTarget = null;
			return;
		}

		Vector3 toTarget = currentTarget.position - mob.position;
		float distanceToTarget = toTarget.length;
		HitData? hit = Physics.Raycast(mob.position + Vector3.Up, toTarget / distanceToTarget, distanceToTarget);
		bool targetVisible = hit == null;
		if (targetVisible)
			runToTarget(toTarget, delta);
		else
		{
			currentPatrolTarget = currentTarget.position;
			currentTarget = null;
		}
	}

	bool lookForTarget()
	{
		float detectionRange = 20;
		if (TestState.instance != null && TestState.instance.player != null)
		{
			Player player = TestState.instance.player;
			Vector3 toPlayer = player.position - mob.position;
			float distanceSq = toPlayer.lengthSquared;
			if (distanceSq < detectionRange * detectionRange)
			{
				float distanceToPlayer = toPlayer.length;
				float d = Vector3.Dot(toPlayer / distanceToPlayer, mob.rotation.forward);
				if (d > MathF.Cos(0.5f * detectionAngle))
				{
					bool targetVisible = Physics.Raycast(mob.position + Vector3.Up, toPlayer / distanceToPlayer, distanceToPlayer) == null;

					if (targetVisible)
					{
						currentTarget = player;
					}
				}
			}
		}
		return false;
	}

	void updatePatrol(float delta)
	{
		if (!lookForTarget())
		{
			mob.rotationTarget = Vector3.Zero;
			mob.fsu = Vector3.Zero;
			mob.running = false;

			if (currentPatrolTarget == Vector3.Zero)
			{
				float patrolTargetSeekChance = 0.03f;
				if (Random.Shared.NextSingle() < patrolTargetSeekChance)
				{
					int rotation = Random.Shared.Next() % 4;
					Vector3 direction = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f * rotation) * Vector3.Forward;
					Span<HitData> hits = stackalloc HitData[16];
					int numHits = Physics.Raycast(mob.position + Vector3.Up, direction, 20, hits);
					float distance = float.MaxValue;
					for (int i = 0; i < numHits; i++)
					{
						if (hits[i].distance < distance)
							distance = hits[i].distance;
					}
					if (distance != float.MaxValue && distance > 2)
					{
						float distanceMultiplier = Random.Shared.NextSingle();
						distanceMultiplier = 1 - distanceMultiplier * distanceMultiplier;
						currentPatrolTarget = mob.position + direction * (distanceMultiplier * distance - 1);
					}
				}
			}
			else
			{
				Vector3 toPatrolTarget = (currentPatrolTarget - mob.position) * new Vector3(1, 0, 1);
				float distanceToPatrolTarget = toPatrolTarget.length;
				HitData? hit = Physics.Raycast(mob.position + Vector3.Up, toPatrolTarget / distanceToPatrolTarget, distanceToPatrolTarget);
				bool patrolTargetStillVisible = hit == null || hit.Value.distance > distanceToPatrolTarget * 0.9f;
				patrolTargetStillVisible = true;

				if (patrolTargetStillVisible)
				{
					float patrolTargetReachedDistance = 0.6f;
					if (toPatrolTarget.lengthSquared < patrolTargetReachedDistance * patrolTargetReachedDistance)
					{
						currentPatrolTarget = Vector3.Zero;
						mob.rotationTarget = Vector3.Zero;
						mob.fsu = Vector3.Zero;
					}
					else
					{
						mob.rotationTarget = toPatrolTarget.normalized;
						mob.fsu = new Vector3(0, 0, 1);
					}
				}
				else
				{
					currentPatrolTarget = Vector3.Zero;
					mob.rotationTarget = Vector3.Zero;
					mob.fsu = Vector3.Zero;
				}
			}
		}
	}

	public override void update(float delta)
	{
		if (currentTarget != null)
			updateTargetFollow(delta);
		else
			updatePatrol(delta);
	}
}
