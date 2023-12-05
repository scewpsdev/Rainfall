using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


internal class EnemyAI : AI
{
	Entity currentTarget = null;

	bool investigates = false;
	Vector3 investigateTarget = Vector3.Zero;

	public float detectionRange;
	public float detectionAngle;
	public float stoppingDistance;
	public float minimumAttackDistance;


	public EnemyAI(Creature creature)
		: base(creature)
	{
		this.detectionRange = 8.0f;
		this.detectionAngle = MathHelper.ToRadians(100.0f);
		this.stoppingDistance = 2.0f;
		minimumAttackDistance = 1.5f;
	}

	public override void onSoundHeard(Vector3 from)
	{
		if (!investigates)
		{
			investigates = true;
			investigateTarget = from;
		}
	}

	public override void onHit(int damage, Entity from)
	{
		if (from == creature)
			Debug.Assert(false);
		currentTarget = from;
	}

	bool canSeeEntity(Entity entity)
	{
		Vector3 targetCenter = entity.position + Vector3.Up;
		Vector3 creatureCenter = creature.position + Vector3.Up;
		Vector3 delta = targetCenter - creatureCenter;
		float distance = delta.length;
		//HitData? hit = Physics.Raycast(creatureCenter, delta / distance, distance, (uint)PhysicsFilterGroup.PlayerControllerKinematicBody | (uint)PhysicsFilterGroup.PlayerController, QueryFilterFlags.Default);
		//if (hit != null)
		//	Console.WriteLine(hit.Value.body.entity);

		bool visible = true;
		Span<HitData> hits = stackalloc HitData[16];
		int numHits = Physics.Raycast(creatureCenter, delta / distance, distance, hits, QueryFilterFlags.Default);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].body != null && hits[i].body.entity != creature && hits[i].body.entity != entity)
			{
				if (hits[i].body.entity == null || (hits[i].body.filterGroup & (uint)PhysicsFilterGroup.Default) != 0)
				{
					visible = false;
					break;
				}
			}
		}

		return visible;
	}

	void seekTarget()
	{
		// TODO line of sight and last seen position

		Span<HitData> hits = stackalloc HitData[16];
		int numHits = Physics.OverlapSphere(detectionRange, creature.position, hits, QueryFilterFlags.Dynamic);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].body != null && hits[i].body.entity is Player ||
				hits[i].controller != null && hits[i].controller.entity is Player)
			{
				Entity entity = hits[i].body != null ? (Entity)hits[i].body.entity : (Entity)hits[i].controller.entity;
				if (entity != null && entity != creature && entity is Player)
				{
					Player player = entity as Player;
					if (player.isAlive)
					{
						Vector3 toEntity = entity.position - creature.position;
						float distance = toEntity.length;
						//if (distance < detectionRange)
						{
							float angle = MathF.Acos(Vector3.Dot(toEntity / distance, creature.rotation.forward));
							if (angle < detectionAngle * 0.5f)
							{
								if (canSeeEntity(entity))
									currentTarget = entity;
							}
						}
					}
				}
			}
		}
	}

	static float mod(float x, float m) => (x % m + m) % m;

	void updateTargetFollow()
	{
		investigates = false;


		if (currentTarget != null)
		{
			bool targetIsDead = currentTarget is Player && ((Player)currentTarget).stats.health <= 0 || currentTarget is Creature && ((Creature)currentTarget).stats.health <= 0;
			if (currentTarget.removed || targetIsDead)
			{
				currentTarget = null;
				return;
			}

			if (!canSeeEntity(currentTarget))
			{
				investigateTarget = currentTarget.position;
				currentTarget = null;
				investigates = true;
				return;
			}
		}


		Vector3 targetPosition = currentTarget.position;

		//if (creature.currentAction == null)
		{
			Vector3 toTarget = (targetPosition - creature.position) * new Vector3(1, 0, 1);
			float distance = toTarget.length;
			Vector3 targetDir = toTarget / distance;
			float dstYaw = MathF.Atan2(targetDir.x, targetDir.z) + MathF.PI;
			float currentYaw = creature.yaw;

			currentYaw = (currentYaw + MathF.PI) % (MathF.PI * 2.0f) - MathF.PI;
			dstYaw = (dstYaw + MathF.PI) % (MathF.PI * 2.0f) - MathF.PI;
			if (currentYaw - dstYaw > MathF.PI)
				currentYaw -= MathF.PI * 2.0f;
			else if (dstYaw - currentYaw > MathF.PI)
				dstYaw -= MathF.PI * 2.0f;

			if (creature.currentAction == null)
			{
				if (dstYaw > currentYaw + 0.1f)
					creature.rotationDirection = 1.0f;
				else if (dstYaw < currentYaw - 0.1f)
					creature.rotationDirection = -1.0f;
				else
					creature.rotationDirection = 0.0f;
			}
			else
			{
				if (dstYaw > currentYaw + 0.1f)
					creature.rotationDirection = creature.currentAction.rotationSpeedMultiplier;
				else if (dstYaw < currentYaw - 0.1f)
					creature.rotationDirection = -creature.currentAction.rotationSpeedMultiplier;
				else
					creature.rotationDirection = 0.0f;
			}


			creature.fsu = Vector2.Zero;
			if (distance > stoppingDistance)
			{
				if (MathF.Abs(currentYaw - dstYaw) < MathHelper.ToRadians(30.0f))
				{
					if (creature.currentAction == null)
					{
						creature.fsu = new Vector2(0.0f, 1.0f);
					}
				}
			}
			else if (distance <= stoppingDistance)
			{
				bool isAttacking = creature.currentAction != null && creature.currentAction.type == MobActionType.Attack;
				if (distance >= minimumAttackDistance || isAttacking)
				{
					if (MathF.Abs(currentYaw - dstYaw) < MathHelper.ToRadians(30.0f))
					{
						if (creature.currentAction != null && creature.currentAction.type == MobActionType.Attack)
						{
							MobAttackAction attackAction = creature.currentAction as MobAttackAction;
							if (attackAction.index == 0 && attackAction.elapsedTime >= attackAction.followUpCancelTime)
							{
								creature.queueAction(new MobAttackAction(0, 1));
							}
						}
						else if (creature.currentAction == null)
						{
							creature.queueAction(new MobAttackAction(0, 0));
						}
					}
				}
				else if (distance < minimumAttackDistance)
				{
					if (MathF.Abs(currentYaw - dstYaw) < MathHelper.ToRadians(30.0f))
					{
						if (creature.currentAction == null)
						{
							creature.queueAction(new MobDodgeAction());
						}
					}
				}
				else
				{
					Debug.Assert(false);
				}
			}
			else
			{
				Debug.Assert(false);
			}
		}
		//else
		{
			//creature.fsu = Vector2.Zero;
			//creature.rotationDirection = 0.0f;
		}
	}

	void updateInvestigation()
	{
		Vector3 targetPosition = investigateTarget;
		Vector3 toTarget = (targetPosition - creature.position) * new Vector3(1, 0, 1);
		float distance = toTarget.length;
		Vector3 targetDir = toTarget / distance;
		float dstYaw = MathF.Atan2(targetDir.x, targetDir.z) + MathF.PI;
		float currentYaw = creature.yaw;

		currentYaw = mod(currentYaw + MathF.PI, MathF.PI * 2.0f) - MathF.PI;
		dstYaw = mod(dstYaw + MathF.PI, MathF.PI * 2.0f) - MathF.PI;
		if (currentYaw - dstYaw > MathF.PI)
			currentYaw -= MathF.PI * 2.0f;
		else if (dstYaw - currentYaw > MathF.PI)
			dstYaw -= MathF.PI * 2.0f;

		if (creature.currentAction == null)
		{
			if (dstYaw > currentYaw + 0.1f)
				creature.rotationDirection = 0.5f;
			else if (dstYaw < currentYaw - 0.1f)
				creature.rotationDirection = -0.5f;
			else
				creature.rotationDirection = 0.0f;
		}
		else
		{
			if (dstYaw > currentYaw + 0.1f)
				creature.rotationDirection = 0.5f * creature.currentAction.rotationSpeedMultiplier;
			else if (dstYaw < currentYaw - 0.1f)
				creature.rotationDirection = -0.5f * creature.currentAction.rotationSpeedMultiplier;
			else
				creature.rotationDirection = 0.0f;
		}

		if (Vector3.Dot(targetDir, creature.rotation.forward) > 0.8f)
		{
			investigates = false;
			creature.rotationDirection = 0.0f;
			creature.fsu = Vector2.Zero;
		}


		creature.fsu = Vector2.Zero;
		if (distance > 0.5f)
		{
			if (MathF.Abs(currentYaw - dstYaw) < MathHelper.ToRadians(30.0f))
			{
				if (creature.currentAction == null)
				{
					//creature.fsu = new Vector2(0.0f, 1.0f);
				}
			}
		}
		else
		{
			//investigates = false;
		}
	}

	public override void update()
	{
		base.update();

		if (currentTarget == null)
		{
			seekTarget();
		}

		if (currentTarget != null)
		{
			updateTargetFollow();
		}
		else if (investigates)
		{
			updateInvestigation();
		}
	}
}
