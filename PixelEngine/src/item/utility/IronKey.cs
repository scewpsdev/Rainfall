using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class IronKey : Item
{
	public IronKey()
		: base("iron_key", ItemType.Utility)
	{
		displayName = "Iron Key";

		value = 8;
		canDrop = false;
		isActiveItem = false;
		upgradable = false;
		stackable = true;

		sprite = new Sprite(tileset, 8, 5);

		hitSound = Resource.GetSounds("res/sounds/hit_rock", 5);
	}
}
