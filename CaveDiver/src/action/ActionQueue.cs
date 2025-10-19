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

	public List<EntityAction> actionQueue = new List<EntityAction>();
	public EntityAction currentAction { get => actionQueue.Count > 0 ? actionQueue[0] : null; }
	public int size { get => actionQueue.Count; }


	public ActionQueue(Player player)
	{
		this.player = player;
	}

	void initializeAction(EntityAction currentAction)
	{
		// Initialize action
		currentAction.startTime = Time.currentTime;
		currentAction.onStarted(player);
	}

	void deinitializeAction(EntityAction currentAction)
	{
		currentAction.onFinished(player);
		actionQueue.RemoveAt(0);
	}

	public void update()
	{
		// Actions
		if (actionQueue.Count > 0)
		{
			EntityAction currentAction = actionQueue[0];
			if (currentAction.hasStarted)
			{
				bool actionShouldFinish = currentAction.hasFinished || currentAction.elapsedTime >= currentAction.duration && actionQueue.Count > 1 ||
					(currentAction.elapsedTime >= currentAction.followUpCancelTime && actionQueue.Count > 1 && currentAction.type == actionQueue[1].type);
				if (actionShouldFinish)
				{
					deinitializeAction(currentAction);
					currentAction = actionQueue.Count > 0 ? actionQueue[0] : null;
				}
			}

			if (currentAction != null)
			{
				if (!currentAction.hasStarted /*&& !player.isClimbing*/)
				{
					initializeAction(currentAction);
				}

				if (currentAction.hasStarted)
					currentAction.update(player);
			}
		}
	}

	public void queueAction(EntityAction action)
	{
		//bool enoughStamina = action.staminaCost == 0.0f || player.stats.stamina > 0;
		//bool enoughMana = action.manaCost == 0 || player.stats.mana >= action.manaCost;
		//if (enoughStamina && enoughMana)
		{
			if (actionQueue.Count >= MAX_ACTION_QUEUE_SIZE)
				actionQueue.RemoveRange(1, actionQueue.Count - 1);
			action.onQueued(player);
			actionQueue.Add(action);
			if (actionQueue[0] == action && !player.isClimbing)
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
