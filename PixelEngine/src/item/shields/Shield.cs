using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Shield : Item
{
	public Shield(string name)
		: base(name, ItemType.Shield)
	{
		isSecondaryItem = true;
		rarity = 0.2f;
		renderOffset.x = 0;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new BlockAction(this, player.handItem == this));
		return false;
	}
}
