using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Pickaxe : Item
{
	public Pickaxe()
		: base("pickaxe")
	{
		displayName = "Pickaxe";

		attackDamage = 2;
		attackRange = 1.2f;
		attackRate = 3.0f;
		stab = false;

		value = 15;

		sprite = new Sprite(tileset, 0, 1);
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new PickaxeSwingAction(this));
		return true;
	}
}
