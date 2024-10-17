using Rainfall;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;


public class AIAction
{
	public AdvancedAI ai;
	public string animation;
	public float duration;
	public float chargeTime;
	public float cooldownTime;
	public float walkSpeed;
	public FloatRect actionCollider;

	public Func<AIAction, Vector2, float, bool> requirementsMet;
	public Action<AIAction> onStarted;
	public Func<AIAction, float, Vector2, bool> onAction;
	public Action<AIAction> onFinished;
}

public class AdvancedAI : AI
{
	enum AIState
	{
		Default,
		Action,
		Charge,
		Cooldown,
	}


	AIState state = AIState.Default;
	public Vector2 walkDirection = Vector2.Zero;
	public bool patrol = true;
	public int hesitation = 0;

	float walkSpeed;
	FloatRect collider;

	List<AIAction> actions = new List<AIAction>();
	public AIAction currentAction = null;

	long chargeTime;
	long actionTime;
	long cooldownTime;
	Vector2 actionDirection;

	long targetLastSeen = -1;
	Vector2 targetPosition;


	public AdvancedAI(Mob mob)
		: base(mob)
	{
		aggroRange = 6.0f;
		loseRange = 10.0f;
		loseTime = 3.0f;

		walkSpeed = mob.speed;
		collider = mob.collider;
	}

	public override void onDeath()
	{
		if (GameState.instance.currentBoss == mob)
			GameState.instance.currentBoss = null;
	}

	public AIAction addAction(string animation, float duration, float chargeTime, float cooldownTime, float walkSpeed, Func<AIAction, Vector2, float, bool> requirementsMet, Action<AIAction> onStarted = null, Func<AIAction, float, Vector2, bool> onAction = null, Action<AIAction> onFinished = null)
	{
		AIAction action = new AIAction { ai = this, animation = animation, duration = duration, chargeTime = chargeTime, cooldownTime = cooldownTime, walkSpeed = walkSpeed, requirementsMet = requirementsMet, onStarted = onStarted, onAction = onAction, onFinished = onFinished };
		actions.Add(action);
		return action;
	}

	void beginAction()
	{
		state = AIState.Action;
		actionTime = Time.currentTime;
		mob.speed = currentAction.walkSpeed;
		if (currentAction.onStarted != null)
			currentAction.onStarted(currentAction);
	}

	void endAction()
	{
		if (currentAction.onFinished != null)
			currentAction.onFinished(currentAction);
		state = AIState.Cooldown;
		cooldownTime = Time.currentTime;
		mob.speed = walkSpeed;
	}

	void updateTargetFollow()
	{
		canSeeTarget = false;
		if (canSeeEntity(target, out Vector2 toTarget, out float distance))
		{
			targetLastSeen = Time.currentTime;
			targetPosition = mob.position + mob.collider.center + toTarget * distance;
			canSeeTarget = true;
		}

		if (state == AIState.Default)
		{
			mob.animator.setAnimation("run");

			walkDirection = toTarget.normalized;

			List<AIAction> possibleActions = new List<AIAction>();
			foreach (AIAction action in actions)
			{
				if (action.requirementsMet(action, toTarget, distance) && (Time.currentTime / 1000000000 + Hash.hash(action.animation) + Hash.hash(action.duration)) % (hesitation + 1) == 0)
					possibleActions.Add(action);
			}

			if (possibleActions.Count > 0)
			{
				currentAction = possibleActions[Random.Shared.Next() % possibleActions.Count];
				state = AIState.Charge;
				chargeTime = Time.currentTime;
				actionDirection = walkDirection;
				if (currentAction.actionCollider != null)
					mob.collider = currentAction.actionCollider;
			}
		}
		if (state == AIState.Charge)
		{
			mob.animator.setAnimation("charge");

			if ((Time.currentTime - chargeTime) / 1e9f >= currentAction.chargeTime)
				beginAction();
		}
		if (state == AIState.Action)
		{
			mob.animator.setAnimation(currentAction.animation);

			float elapsed = (Time.currentTime - actionTime) / 1e9f;
			if (currentAction.onAction != null && !currentAction.onAction(currentAction, elapsed, toTarget * distance) || elapsed >= currentAction.duration || mob.isStunned)
				endAction();
		}
		if (state == AIState.Cooldown)
		{
			mob.animator.setAnimation("cooldown");

			if ((Time.currentTime - cooldownTime) / 1e9f >= currentAction.cooldownTime)
			{
				if (currentAction.actionCollider != null)
					mob.collider = collider;
				state = AIState.Default;
				currentAction = null;
			}
		}

		if (state == AIState.Default)
		{
			mob.inputDirection = walkDirection;
		}
		else if (state == AIState.Action)
		{
			mob.inputDirection = actionDirection;
		}
	}

	void updatePatrol()
	{
		if (patrol)
		{
			mob.animator.setAnimation("run");

			mob.inputDirection = walkDirection;

			float radius = MathF.Max(MathF.Max(MathF.Abs(mob.collider.min.x), MathF.Abs(mob.collider.min.y)), MathF.Max(MathF.Abs(mob.collider.max.x), MathF.Abs(mob.collider.max.y)));
			TileType forwardTile = GameState.instance.level.getTile(mob.position + walkDirection * (radius * 1.5f));
			if (forwardTile != null && forwardTile.isSolid)
				walkDirection *= -1;
		}
		else
		{
			mob.animator.setAnimation("idle");
		}
	}

	public override void update()
	{
		mob.inputDirection = Vector2.Zero;
		mob.inputJump = false;

		if (target == null)
		{
			if (state == AIState.Action)
				endAction();

			if (canSeeEntity(GameState.instance.player, out Vector2 toTarget, out float distance))
			{
				float effectiveAggroRange = aggroRange * GameState.instance.player.visibility;
				float effectiveAwareness = mob.awareness * (GameState.instance.player.isDucked ? 0.5f : 1.0f);
				if (distance < effectiveAggroRange && MathF.Sign(toTarget.x) == mob.direction || distance < effectiveAwareness * effectiveAggroRange)
				{
					target = GameState.instance.player;
					if (mob.isBoss)
					{
						GameState.instance.currentBoss = mob;
						GameState.instance.currentBossMaxHealth = mob.health;
					}
				}
			}
		}

		if (target != null)
		{
			float effectiveLoseRange = loseRange * GameState.instance.player.visibility;
			bool targetLost = (target.position - mob.position).lengthSquared > effectiveLoseRange * effectiveLoseRange ||
				targetLastSeen != -1 && (Time.currentTime - targetLastSeen) / 1e9f > loseTime;
			if (targetLost && state == AIState.Default && !mob.isBoss)
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
