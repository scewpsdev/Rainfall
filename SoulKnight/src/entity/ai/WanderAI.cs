using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WanderAI : AI
{
	Vector2i walkDirection;


	public WanderAI(Mob mob)
		: base(mob)
	{
		int dir = Random.Shared.Next() % 4;
		int xdir = (dir + 1) % 2 * (dir / 2 * -2 + 1);
		int ydir = dir % 2 * (dir / 2 * 2 - 1);
		walkDirection = new Vector2i(xdir, ydir);
	}

	public override void update()
	{
		mob.animator.setAnimation("idle");

		mob.inputDirection = walkDirection * 1.0f;

		TileType forwardTile = GameState.instance.level.getTile(mob.position + 0.5f * walkDirection);
		if (forwardTile != null)
			walkDirection *= -1;
	}
}
