using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Weapon : Item
{
	public Weapon(string name, string displayName)
		: base(ItemType.Weapon, name, displayName)
	{
	}

	public override void use(Player player)
	{
		player.actionManager.queueAction(new AttackAction(this));
	}
}
