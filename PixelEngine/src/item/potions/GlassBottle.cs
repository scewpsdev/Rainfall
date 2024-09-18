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

		value = 2;

		sprite = new Sprite(tileset, 3, 5);
	}
}
