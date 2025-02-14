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
	public string chargeAnimation = "charge";
	public string cooldownAnimation = "cooldown";
	public float duration;
	public float chargeTime;
	public float cooldownTime;
	public float walkSpeed;
	public float chargeSpeed;
	public float cooldownSpeed;
	public float rarity = 1;
	public FloatRect[] actionColliders;

	public Func<AIAction, Vector2, float, bool> requirementsMet;
	public Action<AIAction> onStarted;
	public Func<AIAction, float, Vector2, bool> onAction;
	public Action<AIAction> onFinished;
}

public class AdvancedAI : AI
{
	static int idx = 0;


	enum AIState
	{
		Default,
		Action,
		Charge,
		Cooldown,
	}


	int id = idx++;

	AIState state = AIState.Default;
	public int walkDirection = 1;
	public bool patrol = true;
	public string runAnim = "run";

	protected bool useAStar = false;
	List<Vector2i> currentPath = new List<Vector2i>();

	float walkSpeed;

	public List<AIAction> actions = new List<AIAction>();
	AIAction currentAction = null;
	AIAction lastAction = null;
	AIAction triggeredAction = null;
	public int hesitation = 0;
	public float minRunDistance = 0;

	long chargeTime;
	long actionTime;
	long cooldownTime;
	public int actionDirection;

	long targetLastSeen = -1;
	Vector2 targetPosition;


	public AdvancedAI(Mob mob)
		: base(mob)
	{
		aggroRange = 6.0f;
		loseRange = 10.0f;
		loseTime = 3.0f;

		walkSpeed = mob.speed;
	}

	public AIAction addAction(string animation, float charge, float duration, float cooldown, float speed, float maxDistance, float minDistance = 0, float rarity = 1)
	{
		AIAction action = addAction(animation + "1", duration, animation + "0", charge, animation + "2", cooldown, speed, (AIAction action, Vector2 toTarget, float targetDistance) =>
		{
			return targetDistance >= minDistance && targetDistance <= maxDistance && mob.isGrounded && canSeeTarget;
		});
		action.rarity = rarity;
		return action;
	}

	public AIAction addAction(string animation, float duration, string chargeAnimation, float chargeTime, string cooldownAnimation, float cooldownTime, float walkSpeed, Func<AIAction, Vector2, float, bool> requirementsMet, Action<AIAction> onStarted = null, Func<AIAction, float, Vector2, bool> onAction = null, Action<AIAction> onFinished = null)
	{
		AIAction action = new AIAction { ai = this, animation = animation, chargeAnimation = chargeAnimation, cooldownAnimation = cooldownAnimation, duration = duration, chargeTime = chargeTime, cooldownTime = cooldownTime, walkSpeed = walkSpeed, requirementsMet = requirementsMet, onStarted = onStarted, onAction = onAction, onFinished = onFinished };
		actions.Add(action);
		return action;
	}

	public AIAction addAction(string animation, float duration, float chargeTime, float cooldownTime, float walkSpeed, Func<AIAction, Vector2, float, bool> requirementsMet, Action<AIAction> onStarted = null, Func<AIAction, float, Vector2, bool> onAction = null, Action<AIAction> onFinished = null)
	{
		AIAction action = new AIAction { ai = this, animation = animation, duration = duration, chargeTime = chargeTime, cooldownTime = cooldownTime, walkSpeed = walkSpeed, requirementsMet = requirementsMet, onStarted = onStarted, onAction = onAction, onFinished = onFinished };
		actions.Add(action);
		return action;
	}

	public AIAction addJumpAction()
	{
		AIAction jump = addAction("jump", 100, 0, 0, walkSpeed, (AIAction action, Vector2 toTarget, float targetDistance) =>
		{
			TileType forwardTile = GameState.instance.level.getTile(mob.position + new Vector2(walkDirection == 1 ? mob.collider.max.x + 0.1f : walkDirection == -1 ? mob.collider.min.x - 0.1f : 0, 0.5f));
			TileType forwardUpTile = GameState.instance.level.getTile(mob.position + new Vector2(walkDirection == 1 ? mob.collider.max.x + 0.1f : walkDirection == -1 ? mob.collider.min.x - 0.1f : 0, 1.5f));
			return forwardTile != null && forwardUpTile == null;
		});
		jump.onStarted = (AIAction action) =>
		{
			mob.inputJump = true;
		};
		jump.onAction = (AIAction action, float elapsed, Vector2 toTarget) =>
		{
			return !(!mob.inputJump && mob.isGrounded);
		};
		return jump;
	}

	public AIAction getAction(string name)
	{
		foreach (AIAction action in actions)
		{
			if (action.animation == name)
				return action;
		}
		return null;
	}

	void beginAction()
	{
		state = AIState.Action;
		actionTime = Time.currentTime;
		mob.speed = currentAction.walkSpeed;

		if (currentAction.actionColliders != null)
			mob.actionColliders = currentAction.actionColliders;

		if (currentAction.onStarted != null)
			currentAction.onStarted(currentAction);
	}

	void endAction()
	{
		state = AIState.Cooldown;
		cooldownTime = Time.currentTime;
		mob.speed = walkSpeed;

		if (currentAction.onFinished != null)
			currentAction.onFinished(currentAction);

		if (currentAction.actionColliders != null)
			mob.actionColliders = null;
	}

	bool updatePath(Vector2i currentTile, Vector2i targetTile)
	{
		currentPath.Clear();
		return GameState.instance.level.astar.run(currentTile, targetTile, currentPath, false);
	}

	public void triggerAction(AIAction action)
	{
		if (state == AIState.Action)
			endAction();

		triggeredAction = action;

		lastAction = currentAction;
		currentAction = triggeredAction;
		state = AIState.Charge;
		chargeTime = Time.currentTime;
		actionDirection = walkDirection;

		triggeredAction = null;

		//triggeredAction = action;
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
			if (useAStar)
			{
				Vector2i currentTile = (Vector2i)Vector2.Floor(mob.position);
				Vector2i targetTile = (Vector2i)Vector2.Floor(target.position + target.collider.center);

				if (!updatePath(currentTile, targetTile))
				{
					setTarget(null);
					currentPath.Clear();
					return;
				}

				if (currentPath.Count > 0 && currentAction == null)
				{
					Vector2i nextTile = currentPath.Count > 1 ? currentPath[1] : currentPath[0];
					float xdelta = nextTile.x + 0.5f - mob.position.x;
					float ydelta = nextTile.y + 0.5f - mob.position.y;

					if (xdelta < -0.1f)
						mob.inputLeft = true;
					else if (xdelta > 0.1f)
						mob.inputRight = true;
					if (ydelta < -0.1f)
						mob.inputDown = true;
					else if (ydelta > 0.1f)
						mob.inputUp = true;
				}
			}
			else if (distance >= minRunDistance)
			{
				walkDirection = targetPosition.x < mob.position.x ? -1 : 1;

				if (walkDirection == -1)
					mob.inputLeft = true;
				else if (walkDirection == 1)
					mob.inputRight = true;
			}
			else
			{
				walkDirection = 0;
			}

			if (walkDirection != 0)
				mob.animator.setAnimation(runAnim);
			else
				mob.animator.setAnimation("idle");

			if (!mob.isStunned)
			{
				uint tick = (uint)(Time.currentTime / 200000000);

				List<AIAction> possibleActions = new List<AIAction>();
				foreach (AIAction action in actions)
				{
					uint h = Hash.combine(Hash.hash(id), Hash.combine(Hash.hash(tick), Hash.hash(action.animation)));
					if (action.requirementsMet(action, toTarget, distance) && h % (hesitation + 1) == 0)
						possibleActions.Add(action);
				}

				if (possibleActions.Count > 0)
				{
					AIAction selectedAction = MathHelper.ChooseRandom(possibleActions, (AIAction action) => action.rarity);
					currentAction = selectedAction;
					state = AIState.Charge;
					chargeTime = Time.currentTime;

					walkDirection = targetPosition.x < mob.position.x ? -1 : 1;
					actionDirection = walkDirection;

					if (walkDirection == 1)
						mob.inputRight = true;
					else if (walkDirection == -1)
						mob.inputLeft = true;
				}
				else
				{
					lastAction = null;
				}
			}
		}

		if (target is Player && !(target as Player).isAlive || target is Mob && !(target as Mob).isAlive)
		{
			if (state == AIState.Action)
				endAction();
			state = AIState.Default;
			setTarget(null);
			return;
		}
	}

	void updatePatrol()
	{
		if (patrol)
		{
			mob.animator.setAnimation(runAnim);

			if (walkDirection == 1)
				mob.inputRight = true;
			else if (walkDirection == -1)
				mob.inputLeft = true;

			bool hitsWall = false;
			for (int i = (int)MathF.Floor(mob.collider.min.y + 0.01f); i <= (int)MathF.Floor(mob.collider.max.y - 0.01f); i++)
			{
				TileType forwardTile = GameState.instance.level.getTile(mob.position + new Vector2(walkDirection == 1 ? mob.collider.max.x + 0.1f : walkDirection == -1 ? mob.collider.min.x - 0.1f : 0, 0.5f + i));
				if (forwardTile != null && forwardTile.isSolid)
				{
					hitsWall = true;
					break;
				}
			}
			if (hitsWall)
				walkDirection *= -1;
			else
			{
				TileType forwardDownTile = GameState.instance.level.getTile(mob.position + new Vector2(walkDirection == 1 ? mob.collider.max.x + 0.1f : walkDirection == -1 ? mob.collider.min.x - 0.1f : 0, -0.5f));
				if (mob.isGrounded && forwardDownTile == null)
					walkDirection *= -1;
			}
		}
		else
		{
			mob.animator.setAnimation("idle");
		}
	}

	public override void update()
	{
		mob.inputRight = false;
		mob.inputLeft = false;
		mob.inputJump = false;
		mob.inputDown = false;
		mob.inputUp = false;

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
					Entity newTarget = GameState.instance.player;
					setTarget(newTarget);
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
				setTarget(null);
				targetLastSeen = -1;
			}
		}

		if (target != null)
			updateTargetFollow();
		else
			updatePatrol();

		if (triggeredAction != null)
		{
			if (state == AIState.Action)
				endAction();

			lastAction = currentAction;
			currentAction = triggeredAction;
			state = AIState.Charge;
			chargeTime = Time.currentTime;
			actionDirection = walkDirection;

			triggeredAction = null;
		}

		if (state == AIState.Charge)
		{
			SpriteAnimation anim = mob.animator.getAnimation(currentAction.chargeAnimation);
			mob.animator.setAnimation(currentAction.chargeAnimation);
			if (anim != null)
				anim.duration = currentAction.chargeTime;

			if (mob.isStunned)
				chargeTime = Time.currentTime;

			if (currentAction.chargeSpeed > 0)
			{
				mob.speed = currentAction.chargeSpeed;

				if (walkDirection == -1)
					mob.inputLeft = true;
				else
					mob.inputRight = true;
			}

			if ((Time.currentTime - chargeTime) / 1e9f >= currentAction.chargeTime)
				beginAction();
		}
		if (state == AIState.Action)
		{
			SpriteAnimation anim = mob.animator.getAnimation(currentAction.animation);
			mob.animator.setAnimation(currentAction.animation);
			if (anim != null)
				anim.duration = currentAction.duration;

			Vector2 toTarget = Vector2.Zero;
			float distance = 0;

			if (target != null)
				canSeeEntity(target, out toTarget, out distance);

			float elapsed = (Time.currentTime - actionTime) / 1e9f;
			if (currentAction.onAction != null && !currentAction.onAction(currentAction, elapsed, toTarget * distance) || elapsed >= currentAction.duration || mob.isStunned)
				endAction();

			if (actionDirection == -1)
				mob.inputLeft = true;
			else if (actionDirection == 1)
				mob.inputRight = true;
		}
		if (state == AIState.Cooldown)
		{
			SpriteAnimation anim = mob.animator.getAnimation(currentAction.cooldownAnimation);
			mob.animator.setAnimation(currentAction.cooldownAnimation);
			if (anim != null)
				anim.duration = currentAction.cooldownTime;

			if (currentAction.cooldownSpeed > 0)
			{
				mob.speed = currentAction.cooldownSpeed;

				if (walkDirection == -1)
					mob.inputLeft = true;
				else
					mob.inputRight = true;
			}

			if ((Time.currentTime - cooldownTime) / 1e9f >= currentAction.cooldownTime)
			{
				state = AIState.Default;
				lastAction = currentAction;
				currentAction = null;
			}
		}
	}
}
