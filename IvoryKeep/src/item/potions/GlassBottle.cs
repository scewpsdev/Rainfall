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
		stackable = false;
		canDrop = false;
		rarity = 0.1f;

		baseValue = 2;

		isActiveItem = false;
		isHandItem = true;

		sprite = new Sprite(tileset, 3, 5);
		renderOffset.x = 0;
	}
}
