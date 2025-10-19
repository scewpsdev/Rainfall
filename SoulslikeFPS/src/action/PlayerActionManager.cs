using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


public class PlayerActionManager
{
	const int MAX_ACTION_QUEUE_SIZE = 2;


	Player2 player;

	AnimationState actionAnim1, actionAnim2;
	int currentSwapIdx = 0;
	public AnimationState currentActionAnim;

	Node rootMotionNode;

	List<PlayerAction> actionQueue = new List<PlayerAction>();
	public PlayerAction currentAction { get => actionQueue.Count > 0 ? actionQueue[0] : null; }
	public int size { get => actionQueue.Count; }


	public PlayerActionManager(Player2 player, Node rootMotionNode)
	{
		this.player = player;
		this.rootMotionNode = rootMotionNode;

		actionAnim1 = Animator.CreateAnimation(player.model, "default", false, 0.1f);
		actionAnim2 = Animator.CreateAnimation(player.model, "default", false, 0.1f);
	}

	void initializeAction(PlayerAction currentAction)
	{
		// Initialize action
		currentAction.startTime = Time.currentTime;

		AnimationState anim = getNextActionAnimationState();

		AnimationData? animationData = null;
		if (currentAction.animationName != null)
		{
			animationData = player.model.getAnimationData(currentAction.animationName);
			anim.layers[0].animationData = player.model;
		}
		if (animationData != null)
		{
			anim.layers[0].animationName = currentAction.animationName;
			anim.layers[0].looping = false;
			anim.layers[0].mirrored = false;
			anim.layers[0].rootMotion = rootMotionNode != null;
			anim.layers[0].rootMotionNode = rootMotionNode;
			anim.animationSpeed = currentAction.animationSpeed;
			anim.transitionDuration = currentAction.animationTransitionDuration;

			/*
			if (currentMobAction.fullBodyAnimation)
			{
				currentMobActionState[2].layers[0].animationName = currentMobActionState[i].layers[0].animationName;
				currentMobActionState[2].layers[0].animationData = currentMobActionState[i].layers[0].animationData;
				currentMobActionState[2].layers[0].looping = currentMobActionState[i].layers[0].looping;
				currentMobActionState[2].layers[0].mirrored = currentMobActionState[i].layers[0].mirrored;
				currentMobActionState[2].layers[0].rootMotion = currentMobActionState[i].layers[0].rootMotion;
				currentMobActionState[2].layers[0].rootMotionNode = currentMobActionState[i].layers[0].rootMotionNode;
				currentMobActionState[2].animationSpeed = currentMobActionState[i].animationSpeed;
				currentMobActionState[2].transitionDuration = currentMobActionState[i].transitionDuration;
			}
			*/

			if (currentAction.duration == 0.0f)
				currentAction.duration = animationData.Value.duration;
		}

		currentAction.onStarted(player);
	}

	AnimationState getNextActionAnimationState()
	{
		currentActionAnim = currentSwapIdx++ % 2 == 0 ? actionAnim1 : actionAnim2;
		return currentActionAnim;
	}

	void deinitializeAction(PlayerAction currentAction)
	{
		currentAction.onFinished(player);
		actionQueue.RemoveAt(0);
	}

	public void update()
	{
		// MobActions
		if (actionQueue.Count > 0)
		{
			PlayerAction currentAction = actionQueue[0];
			if (currentAction.hasStarted)
			{
				bool actionShouldFinish = currentAction.hasFinished ||
					(currentAction.elapsedTime >= currentAction.followUpCancelTime && actionQueue.Count > 1 && currentAction.type == actionQueue[1].type);
				if (actionShouldFinish)
				{
					deinitializeAction(currentAction);
					currentAction = actionQueue.Count > 0 ? actionQueue[0] : null;
				}
			}

			if (currentAction != null)
			{
				if (!currentAction.hasStarted)
				{
					initializeAction(currentAction);
				}

				currentAction.update(player);
			}
		}
	}

	public void queueAction(PlayerAction action)
	{
		bool enoughStamina = true; // action.staminaCost == 0.0f; // || stats.canDoAction;
		if (enoughStamina)
		{
			actionQueue.Add(action);
			//actionQueue.Sort((MobAction a, MobAction b) => { return a.priority < b.priority ? 1 : a.priority > b.priority ? -1 : 0; });

			if (actionQueue.Count > MAX_ACTION_QUEUE_SIZE)
				actionQueue.RemoveRange(MAX_ACTION_QUEUE_SIZE, actionQueue.Count - MAX_ACTION_QUEUE_SIZE);
			if (actionQueue[0] == action)
				initializeAction(actionQueue[0]);
		}
	}

	public void cancelAction()
	{
		if (currentAction != null)
			deinitializeAction(currentAction);
	}

	public void cancelAllActions()
	{
		while (actionQueue.Count > 0)
			cancelAction();
	}
}
