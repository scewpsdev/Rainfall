using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ActionQueue
{
	const int MAX_ACTION_QUEUE_SIZE = 2;


	Player player;

	public List<Action> actionQueue = new List<Action>();
	public Action currentAction { get => actionQueue.Count > 0 ? actionQueue[0] : null; }
	public int size { get => actionQueue.Count; }


	public ActionQueue(Player player)
	{
		this.player = player;
	}

	void initializeAction(Action currentAction)
	{
		// Initialize action
		currentAction.startTime = Time.currentTime;

		AnimationState anim = player.getNextActionAnimationState();

		for (int i = 0; i < 3; i++)
		{
			AnimationData? animationData = null;
			if (currentAction.animationName[i] != null && currentAction.animationSet[i] != null)
			{
				animationData = currentAction.animationSet[i].getAnimationData(currentAction.animationName[i]);
				anim.layers[i] = new AnimationLayer(currentAction.animationSet[i], currentAction.animationName[i], false);
				//anim.layers[1 + i].animationData = currentAction.animationSet[i];
			}
			if (currentAction.animationName[i] != null && animationData == null)
			{
				animationData = player.model.getAnimationData(currentAction.animationName[i]);
				anim.layers[i] = new AnimationLayer(player.model, currentAction.animationName[i], false);
				//anim.layers[1 + i].animationData = player.model;
			}
			if (animationData != null)
			{
				//anim.layers[1 + i].animationName = currentAction.animationName[i];
				//anim.layers[1 + i].looping = false;
				anim.layers[i].mirrored = currentAction.mirrorAnimation;
				anim.layers[i].rootMotion = currentAction.rootMotion;
				anim.layers[i].rootMotionNode = player.rootMotionNode;
				anim.animationSpeed = currentAction.animationSpeed;
				anim.transitionDuration = currentAction.animationTransitionDuration;

				/*
				if (currentAction.fullBodyAnimation)
				{
					currentActionState[2].layers[0].animationName = currentActionState[i].layers[0].animationName;
					currentActionState[2].layers[0].animationData = currentActionState[i].layers[0].animationData;
					currentActionState[2].layers[0].looping = currentActionState[i].layers[0].looping;
					currentActionState[2].layers[0].mirrored = currentActionState[i].layers[0].mirrored;
					currentActionState[2].layers[0].rootMotion = currentActionState[i].layers[0].rootMotion;
					currentActionState[2].layers[0].rootMotionNode = currentActionState[i].layers[0].rootMotionNode;
					currentActionState[2].animationSpeed = currentActionState[i].animationSpeed;
					currentActionState[2].transitionDuration = currentActionState[i].transitionDuration;
				}
				*/

				if (currentAction.duration == 0.0f)
					currentAction.duration = animationData.Value.duration;
			}
			else
			{
				anim.layers[i] = null;
			}
		}

		currentAction.onStarted(player);
	}

	void deinitializeAction(Action currentAction)
	{
		currentAction.onFinished(player);
		actionQueue.RemoveAt(0);
	}

	public void update()
	{
		// Actions
		if (actionQueue.Count > 0)
		{
			Action currentAction = actionQueue[0];
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

	public void queueAction(Action action)
	{
		bool enoughStamina = action.staminaCost == 0.0f || player.stats.stamina > 0;
		bool enoughMana = action.manaCost == 0 || player.stats.mana >= action.manaCost;
		if (enoughStamina && enoughMana)
		{
			if (actionQueue.Count >= MAX_ACTION_QUEUE_SIZE)
				actionQueue.RemoveRange(1, actionQueue.Count - 1);
			actionQueue.Add(action);
			if (actionQueue[0] == action)
				initializeAction(actionQueue[0]);
		}
	}

	public void cancelAction()
	{
		Debug.Assert(currentAction != null);
		deinitializeAction(currentAction);
	}

	public void cancelAllActions()
	{
		while (actionQueue.Count > 0)
			cancelAction();
	}
}
