using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TorchItem : Item
{
	public TorchItem()
		: base("torch")
	{
		displayName = "Torch";

		attackDamage = 1;
		attackRange = 1.5f;
		attackRate = 3.0f;
		stab = false;
		rarity = 0;

		sprite = new Sprite(tileset, 8, 0);
	}

	public override Item createNew()
	{
		return new TorchItem();
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this));
		return true;
	}
}
