using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class Gem : Item
{
	protected Gem(string name)
		: base(name, ItemType.Gem)
	{
		stackable = false;
		baseValue = 50;
		isSecondaryItem = true;
	}

	public override bool use(Player player)
	{
		base.use(player);
		player.throwItem(this);
		return true;
	}
}
