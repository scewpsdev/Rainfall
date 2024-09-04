using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicStaff : Item
{
	int charges = 17000;


	public MagicStaff()
		: base("staff_magic", ItemType.Staff)
	{
		displayName = "Magic Staff";

		attackRate = 2.0f;
		trigger = false;

		attackDamage = 2;
		manaCost = 0.2f;

		value = 30;

		sprite = new Sprite(tileset, 8, 1);
	}

	public override bool use(Player player)
	{
		if (charges > 0 && player.mana >= manaCost)
		{
			player.actions.queueAction(new SpellCastAction(this, player.handItem == this));
			player.consumeMana(manaCost);
			charges--;
		}
		return charges == 0;
	}
}
