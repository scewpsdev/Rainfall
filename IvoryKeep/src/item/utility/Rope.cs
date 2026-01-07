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
		//canDrop = false;

		baseValue = 1;

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
		Vector2i playerTile = (Vector2i)Vector2.Floor(player.center); // new Vector2i((int)MathF.Floor(player.position.x), (int)MathF.Floor(player.position.y + 0.5f));
		TileType tile = GameState.instance.level.getTile(playerTile);

		int range = 8;

		if (GameState.instance.level.raycastSolid(playerTile + 0.5f, Vector2.Up, range, out HitData hit))
			range = (int)MathF.Ceiling(hit.distance);

		if (range > 3)
		{
			Vector2i attachTile = playerTile + new Vector2i(0, range - 1);

			GameState.instance.level.addEntity(new RopeEntity(range), (Vector2)attachTile);
			return true;
		}

		return false;
	}
}
