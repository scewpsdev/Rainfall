using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DebugItem : Item
{
	public DebugItem()
		: base("debug_item")
	{
	}

	public override void use(Player player)
	{
		player.throwItem(this);
		player.handItem = null;
	}
}
