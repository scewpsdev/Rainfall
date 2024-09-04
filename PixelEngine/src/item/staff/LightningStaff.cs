using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LightningStaff : Item
{
	int charges = 8000;


	public LightningStaff()
		: base("staff_lightning", ItemType.Staff)
	{
		displayName = "Lightning Staff";

		attackRate = 2;
		trigger = false;

		attackDamage = 2;
		manaCost = 0.35f;

		value = 30;

		sprite = new Sprite(tileset, 8, 2);
	}

	public override bool use(Player player)
	{
		if (charges > 0 && player.mana >= manaCost)
		{
			player.actions.queueAction(new LightningCastAction(this, player.handItem == this));
			player.consumeMana(manaCost);
			charges--;
		}
		return charges == 0;
	}
}
