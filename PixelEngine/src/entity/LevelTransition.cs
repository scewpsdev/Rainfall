using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LevelTransition : Door
{
	Vector2 size;
	bool inTrigger = false;


	public LevelTransition(Level destination, Door otherDoor, Vector2 size)
		: base(destination, otherDoor, false, 0)
	{
		this.size = size;
	}

	void onTouch(Player player)
	{
		Vector2 destinationPos = otherDoor is LevelTransition ? (new Vector2(otherDoor.position.x < 0 ? 0.5f : destination.width - 0.5f, otherDoor.position.y)) : otherDoor.position;
		GameState.instance.switchLevel(destination, destinationPos);
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
			onTouch(GameState.instance.player);
		}
		else if (!playerFound && inTrigger)
		{
			inTrigger = false;
		}
	}

	public override void render()
	{
	}
}
