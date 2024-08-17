using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Torch : Item
{
	public Torch()
		: base("torch")
	{
		displayName = "Torch";

		attackDamage = 1;
		attackRange = 1.5f;
		attackRate = 3.0f;
		stab = false;

		value = 2;

		canDrop = false;

		sprite = new Sprite(tileset, 8, 0);
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this));
		return true;
	}
}
