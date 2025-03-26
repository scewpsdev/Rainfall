using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CreatureActionManager
{
	const int MAX_ACTION_QUEUE_SIZE = 2;


	Creature mob;

	List<CreatureAction> actionQueue = new List<CreatureAction>();
	public CreatureAction currentAction { get => actionQueue.Count > 0 ? actionQueue[0] : null; }
	public int size { get => actionQueue.Count; }


	public CreatureActionManager(Creature mob)
	{
		this.mob = mob;
	}

	void initializeAction(CreatureAction currentAction)
	{
		// Initialize action
		currentAction.startTime = Time.currentTime;

		AnimationState anim = mob.getNextActionAnimationState();

		AnimationData? animationData = null;
		if (currentAction.animationName != null)
		{
			animationData = mob.model.getAnimationData(currentAction.animationName);
			anim.layers[0].animationData = mob.model;
		}
		if (animationData != null)
		{
			anim.layers[0].animationName = currentAction.animationName;
			anim.layers[0].looping = false;
			anim.layers[0].mirrored = false;
			anim.layers[0].rootMotion = mob.rootMotionNode != null;
			anim.layers[0].rootMotionNode = mob.rootMotionNode;
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

		currentAction.onStarted(mob);
	}

	void deinitializeAction(CreatureAction currentAction)
	{
		currentAction.onFinished(mob);
		actionQueue.RemoveAt(0);
	}

	public void update()
	{
		// MobActions
		if (actionQueue.Count > 0)
		{
			CreatureAction currentAction = actionQueue[0];
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

				currentAction.update(mob);
			}
		}
	}

	public void queueAction(CreatureAction action)
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
