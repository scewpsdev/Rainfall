using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EventTrigger : Entity
{
	Vector2 size;
	protected Action<Player> onTriggerEnter, onTriggerLeave;
	bool inTrigger = false;


	public EventTrigger(Vector2 size, Action<Player> onTriggerEnter, Action<Player> onTriggerLeave)
	{
		this.size = size;
		this.onTriggerEnter = onTriggerEnter;
		this.onTriggerLeave = onTriggerLeave;
	}

	public override void update()
	{
		HitData[] hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(position, position + size, hits, FILTER_PLAYER);
		bool playerFound = false;
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity is Player)
			{
				playerFound = true;
				break;
			}
		}

		if (playerFound && !inTrigger)
		{
			inTrigger = true;
			if (onTriggerEnter != null)
				onTriggerEnter(GameState.instance.player);
		}
		else if (!playerFound && inTrigger)
		{
			inTrigger = false;
			if (onTriggerLeave != null)
				onTriggerLeave(GameState.instance.player);
		}
	}
}
