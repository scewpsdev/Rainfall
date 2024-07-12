using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WanderAI : AI
{
	int walkDirection = 1;


	public void update(Mob mob)
	{
		mob.inputRight = false;
		mob.inputLeft = false;

		if (walkDirection == 1)
			mob.inputRight = true;
		else if (walkDirection == -1)
			mob.inputLeft = true;

		HitData forwardTile = GameState.instance.level.sampleTiles(mob.position + new Vector2(0.5f * walkDirection, 0.5f));
		if (forwardTile != null)
			walkDirection *= -1;
		else
		{
			HitData forwardDownTile = GameState.instance.level.sampleTiles(mob.position + new Vector2(0.5f * walkDirection, -0.5f));
			HitData forwardDownDownTile = GameState.instance.level.sampleTiles(mob.position + new Vector2(0.5f * walkDirection, -1.5f));
			if (forwardDownTile == null /*&& forwardDownDownTile == null*/)
				walkDirection *= -1;
		}
	}
}
