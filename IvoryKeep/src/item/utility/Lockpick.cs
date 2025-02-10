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

		value = 4;
		canDrop = false;
		stackable = false;
		isActiveItem = false;

		sprite = new Sprite(tileset, 9, 5);

		rarity = 0.1f;
	}
}
