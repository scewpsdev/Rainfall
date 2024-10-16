using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ScrollOfEarth : Item
{
	public ScrollOfEarth()
		: base("scroll_earth", ItemType.Scroll)
	{
		displayName = "Scroll of Earth";

		value = 8;

		sprite = new Sprite(tileset, 11, 2);
	}

	public override bool use(Player player)
	{
		Vector2 pos = player.position + player.collider.center;
		int x0 = Math.Max((int)pos.x - 1, 0);
		int x1 = Math.Min((int)pos.x + 1, GameState.instance.level.width - 1);
		for (int x = x0; x <= x1; x++)
		{
			HitData hit = GameState.instance.level.raycastTiles(new Vector2(x + 0.5f, (int)pos.y + 0.5f), new Vector2(0, 1), 10);
			if (hit != null && hit.distance > 3)
			{
				SpikeTrap spike = new SpikeTrap();
				spike.trigger();
				GameState.instance.level.addEntity(spike, new Vector2(hit.tile.x + 0.5f, hit.tile.y - 0.5f));
			}
		}
		player.hud.showMessage("The earth rumbles.");
		return true;
	}
}
