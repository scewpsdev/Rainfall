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

		baseValue = 25;

		canDrop = false;
		isActiveItem = true;
		isHandItem = false;
		upgradable = false;
		//stackable = true;

		sprite = new Sprite(tileset, 8, 5);

		hitSound = Resource.GetSounds("sounds/hit_rock", 5);
	}

	public override bool use(Player player)
	{
		base.use(player);
		player.throwItem(this);
		return true;
	}
}
