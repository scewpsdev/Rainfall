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
		attackRange = 1.5f;
		attackRate = 3.0f;
		stab = false;

		sprite = new Sprite(tileset, 0, 1);
	}

	public override Item createNew()
	{
		return new Pickaxe();
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new PickaxeSwingAction(this));
		return true;
	}
}
