using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Lockpick : Item
{
	public Lockpick()
		: base("lockpick", ItemType.Utility)
	{
		displayName = "Lockpick";
		description = "Fragile tool that can open locks cheaply, but may break.";

		baseValue = 7;
		rarity = 0.05f;
		stackable = true;
		//isActiveItem = false;

		sprite = new Sprite(tileset, 9, 5);

	}
}
