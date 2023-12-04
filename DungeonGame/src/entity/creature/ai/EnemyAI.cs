using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
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
								currentTarget = entity;
							}
						}
					}
				}
			}
		}
	}

	static float mod(float x, float m) => (x % m + m) % m;

	public override void update()
	{
		base.update();

		if (currentTarget != null)
		{
			bool targetIsDead = currentTarget is Player && ((Player)currentTarget).stats.health <= 0 || currentTarget is Creature && ((Creature)currentTarget).stats.health <= 0;
			if (currentTarget.removed || targetIsDead)
			{
				currentTarget = null;
			}
		}

		if (currentTarget == null)
		{
			seekTarget();
		}

		if (currentTarget != null)
		{
			investigates = false;

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
		else if (investigates)
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
	}
}
