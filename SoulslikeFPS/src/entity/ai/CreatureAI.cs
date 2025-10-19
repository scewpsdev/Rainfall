using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


enum CreatureAIState
{
	Idle,
	Alert,
	Pursuit,
}

enum AggressionLevel
{
	Retreat,
	Circle,
	Approach,
}

public class CreatureAI : AI
{
	public CreatureAI(Creature creature)
		: base(creature)
	{
	}

	float detectionAngle = 90;

	CreatureAIState aiState = CreatureAIState.Idle;
	Entity currentTarget;
	int currentStrafeDirection = 1;

	public Vector3 currentPatrolTarget = Vector3.Zero;

	float stamina = 100;
	AggressionLevel aggressionLevel = AggressionLevel.Approach;


	public override void onHit(Entity from)
	{
		if (currentTarget == null)
		{
			if (from is Creature || from is Player)
				currentTarget = from;
		}
	}

	public override void onSound(Vector3 position)
	{
		if (currentTarget == null)
		{
			Vector3 mobCenter = creature.position + Vector3.Up;
			Vector3 toSound = position - mobCenter;
			float distanceToSound = toSound.length;
			PhysicsHit? hit = Physics.Raycast(mobCenter, toSound / distanceToSound, distanceToSound - 0.1f);
			bool soundOriginVisible = hit == null;
			if (soundOriginVisible)
				currentPatrolTarget = position;
		}
	}

	bool attackIsSuitable(CreatureAttack attack, float distanceSq, float angle)
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

	void getSuitableAttacks(float distanceSq, float angle, List<CreatureAttack> attacks)
	{
		for (int i = 0; i < creature.attacks.Count; i++)
		{
			if (attackIsSuitable(creature.attacks[i], distanceSq, angle))
				attacks.Add(creature.attacks[i]);
		}
	}

	void runToTarget(Vector3 toTarget, float delta)
	{
		Vector3 toTargetLocal = creature.rotation.conjugated * toTarget;
		float distanceSq = toTarget.lengthSquared;
		float angle = Mathf.ToDegrees(Quaternion.LookAt(toTargetLocal).angle * MathF.Sign(Quaternion.LookAt(toTargetLocal).axis.y));

		float staminaRegenSpeed = 10;
		stamina = Mathf.Clamp(stamina + staminaRegenSpeed * delta, 0, 100);

		// Attacking

		const float stoppingDistance = 2;

		AggressionLevel aggressionLevel = stamina > 20 ? AggressionLevel.Approach : AggressionLevel.Circle;

		float attackChance = aggressionLevel == AggressionLevel.Approach ? 1.0f
			: aggressionLevel == AggressionLevel.Circle ? 0.2f
			: 0.0f;
		attackChance *= 1.0f / MathF.Max(distanceSq - stoppingDistance * stoppingDistance, 1); // * (stamina / 100.0f);

		CreatureAction action = creature.actionManager.currentAction;

		if (action == null)
		{
			if (Random.Shared.NextSingle() < attackChance)
			{
				List<CreatureAttack> suitableAttacks = new List<CreatureAttack>();
				getSuitableAttacks(distanceSq, angle, suitableAttacks);
				if (suitableAttacks.Count > 0)
				{
					CreatureAttack attack = Mathf.ChooseRandom(suitableAttacks, (CreatureAttack element) => element.rarity, Random.Shared);

					//CreatureAttack attack = suitableAttacks[Random.Shared.Next() % suitableAttacks.Count];
					creature.actionManager.queueAction(new CreatureAttackAction(attack));
					stamina -= 20;
				}
			}
		}
		else if (action is CreatureAttackAction && action.elapsedTime > action.duration * 0.8f)
		{
			CreatureAttackAction currentAttackAction = action as CreatureAttackAction;
			if (creature.getAttack(currentAttackAction.attack.nextAttack, out int nextAttackIdx))
			{
				CreatureAttack attack = creature.attacks[nextAttackIdx];
				if (attackIsSuitable(attack, distanceSq, angle))
				{
					creature.actionManager.queueAction(new CreatureAttackAction(attack));
					stamina -= 20;
				}
			}
		}

		// Moving

		if (aggressionLevel == AggressionLevel.Approach)
		{
			creature.rotationTarget = toTarget;

			if (distanceSq > 2 * 2)
				creature.fsu.z = 1;
		}
		else if (aggressionLevel == AggressionLevel.Circle)
		{
			creature.rotationTarget = toTarget;

			if (Random.Shared.NextSingle() < 0.03f)
				currentStrafeDirection *= -1;

			if (distanceSq > 2 * 2)
				creature.fsu.x = currentStrafeDirection * 0.3f;
		}
	}

	void updateTargetFollow(float delta)
	{
		/*
		if (currentTarget.health <= 0)
		{
			currentTarget = null;
			return;
		}
		*/

		Vector3 toTarget = currentTarget.position - creature.position;
		float distanceToTarget = toTarget.length;
		PhysicsHit? hit = Physics.Raycast(creature.position + Vector3.Up, toTarget / distanceToTarget, distanceToTarget);
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
		float detectionRange = 6;
		if (GameState.instance != null && GameState.instance.player != null)
		{
			Player player = GameState.instance.player;
			Vector3 toPlayer = player.position - creature.position;
			float distanceSq = toPlayer.lengthSquared;
			if (distanceSq < detectionRange * detectionRange)
			{
				float distanceToPlayer = toPlayer.length;
				float d = Vector3.Dot(toPlayer / distanceToPlayer, creature.rotation.forward);
				if (d > MathF.Cos(0.5f * detectionAngle))
				{
					bool targetVisible = Physics.Raycast(creature.position + Vector3.Up, toPlayer / distanceToPlayer, distanceToPlayer) == null;

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
			if (currentPatrolTarget == Vector3.Zero)
			{
				float patrolTargetSeekChance = 0.03f;
				if (Random.Shared.NextSingle() < patrolTargetSeekChance)
				{
					int rotation = Random.Shared.Next() % 4;
					Vector3 direction = Quaternion.AxisAngle(Vector3.Up, MathF.PI * 0.5f * rotation) * Vector3.Forward;
					Span<PhysicsHit> hits = stackalloc PhysicsHit[16];
					int numHits = Physics.Raycast(creature.position + Vector3.Up, direction, 20, hits);
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
						currentPatrolTarget = creature.position + direction * (distanceMultiplier * distance - 1);
					}
				}
			}
			else
			{
				Vector3 toPatrolTarget = (currentPatrolTarget - creature.position) * new Vector3(1, 0, 1);
				float distanceToPatrolTarget = toPatrolTarget.length;
				PhysicsHit? hit = Physics.Raycast(creature.position + Vector3.Up, toPatrolTarget / distanceToPatrolTarget, distanceToPatrolTarget);
				bool patrolTargetStillVisible = hit == null || hit.Value.distance > distanceToPatrolTarget * 0.9f;

				if (patrolTargetStillVisible)
				{
					float patrolTargetReachedDistance = 0.6f;
					if (toPatrolTarget.lengthSquared < patrolTargetReachedDistance * patrolTargetReachedDistance)
					{
						currentPatrolTarget = Vector3.Zero;
						creature.rotationTarget = Vector3.Zero;
						creature.fsu = Vector3.Zero;
					}
					else
					{
						creature.rotationTarget = toPatrolTarget.normalized;
						creature.fsu = new Vector3(0, 0, 1);
					}
				}
				else
				{
					currentPatrolTarget = Vector3.Zero;
					creature.rotationTarget = Vector3.Zero;
					creature.fsu = Vector3.Zero;
				}
			}
		}
	}

	public override void tick10()
	{
		creature.fsu = Vector3.Zero;
		creature.rotationTarget = Vector3.Zero;

		float delta = 1.0f / 10;

		if (currentTarget != null)
			updateTargetFollow(delta);
		else
			updatePatrol(delta);
	}
}
