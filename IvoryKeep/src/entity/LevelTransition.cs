using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LevelTransition : Door
{
	public Vector2i size;
	public Vector2i direction;
	bool inTrigger = false;

	public Action onTrigger;


	public LevelTransition(Level destination, Door otherDoor, Vector2i size, Vector2i direction)
		: base(destination, otherDoor, false, 0)
	{
		this.size = size;
		this.direction = direction;

		openSound = null;
	}

	public override Vector2 getSpawnPoint()
	{
		if (direction != Vector2i.Up)
			return position + 0.5f - direction;
		else
			return position + 0.5f - 2 * direction;
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
			if (onTrigger != null)
				onTrigger();
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
