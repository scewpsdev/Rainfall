using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class IronShield : Item
{
	public IronShield()
		: base("iron_shield", ItemType.Shield)
	{
		displayName = "Iron Shield";

		armor = 3;
		value = 8;
		baseWeight = 2;

		isSecondaryItem = true;

		sprite = new Sprite(tileset, 3, 3);
		renderOffset.x = 0.2f;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new BlockAction(this, player.handItem == this));
		return false;
	}
}
