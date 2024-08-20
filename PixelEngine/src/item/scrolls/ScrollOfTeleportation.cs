using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ScrollOfTeleportation : Item
{
	public ScrollOfTeleportation()
		: base("scroll_teleport")
	{
		type = ItemType.Active;
		displayName = "Scroll of Teleportation";

		value = 7;

		sprite = new Sprite(tileset, 13, 2);
	}

	public override bool use(Player player)
	{
		for (int i = 0; i < 1000; i++)
		{
			int x = MathHelper.RandomInt(3, GameState.instance.level.width - 4);
			int y = MathHelper.RandomInt(3, GameState.instance.level.height - 4);
			TileType tile = GameState.instance.level.getTile(x, y);
			if (tile == null || !tile.isSolid)
			{
				player.position = new Vector2(x + 0.5f, y + 0.5f) - player.collider.center;
				return true;
			}
		}
		Debug.Assert(false);
		return false;
	}
}
