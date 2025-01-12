using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LevelTransition : Door
{
	Vector2i size;
	public Vector2i direction;
	bool inTrigger = false;


	public LevelTransition(Level destination, Door otherDoor, Vector2i size, Vector2i direction)
		: base(destination, otherDoor, false, 0)
	{
		this.size = size;
		this.direction = direction;
	}

	public override Vector2 getSpawnPoint()
	{
		return new Vector2(position.x + 0.5f * size.x - direction.x, position.y);
	}

	public override void update()
	{
		HitData[] hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(position + new Vector2(0.25f, 0), position + size + new Vector2(-0.25f, 0), hits, FILTER_PLAYER);
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
			base.interact(GameState.instance.player);
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
