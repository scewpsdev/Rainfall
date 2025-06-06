using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GlassBottle : Item
{
	public GlassBottle()
		: base("glass_bottle", ItemType.Utility)
	{
		displayName = "Glass Bottle";
		stackable = true;
		canDrop = false;

		value = 2;

		//isActiveItem = false;

		sprite = new Sprite(tileset, 3, 5);
	}

	public override bool use(Player player)
	{
		player.hud.showMessage("The bottle is empty.");
		return false;
	}
}
