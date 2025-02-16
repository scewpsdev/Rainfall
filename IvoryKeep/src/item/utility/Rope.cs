using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Rope : Item
{
	public Rope()
		: base("rope", ItemType.Utility)
	{
		displayName = "Rope";
		stackable = true;
		canDrop = false;

		value = 1;

		sprite = new Sprite(tileset, 6, 0);
	}

	int getRopeLength(Vector2i pos)
	{
		for (int y = pos.y - 1; y >= 0; y--)
		{
			TileType tile = GameState.instance.level.getTile(new Vector2i(pos.x, y));
			if (tile != null && tile.isSolid)
				return pos.y - 1 - y;
		}
		return pos.y - 1;
	}

	public override bool use(Player player)
	{
		Vector2i playerTile = new Vector2i((int)MathF.Floor(player.position.x), (int)MathF.Floor(player.position.y + 0.5f));
		TileType tile = GameState.instance.level.getTile(playerTile);
		if (tile == null || !tile.isSolid)
		{
			TileType below = GameState.instance.level.getTile(playerTile + new Vector2i(0, -1));
			if (below != null && below.isSolid)
			{
				TileType forward = GameState.instance.level.getTile(playerTile + new Vector2i(player.direction, 0));
				TileType forwardBelow = GameState.instance.level.getTile(playerTile + new Vector2i(player.direction, -1));
				if ((forward == null || !forward.isSolid) && (forwardBelow == null || !forwardBelow.isSolid))
				{
					Vector2i spawnTile = playerTile + new Vector2i(player.direction, 0);
					GameState.instance.level.addEntity(new RopeEntity(getRopeLength(spawnTile)), spawnTile + new Vector2(0.5f));
					return true;
				}
			}
			else
			{
				GameState.instance.level.addEntity(new RopeEntity(getRopeLength(playerTile)), playerTile + new Vector2(0.5f));
				return true;
			}
		}
		return false;
	}
}
