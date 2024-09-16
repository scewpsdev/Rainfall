using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicStaff : Item
{
	public MagicStaff()
		: base("magic_staff", ItemType.Staff)
	{
		displayName = "Magic Staff";

		attackRate = 4;
		trigger = false;
		isSecondaryItem = true;

		attackDamage = 1;
		manaCost = 0.1f;
		staffCharges = 28;
		maxStaffCharges = 28;

		value = 30;

		sprite = new Sprite(tileset, 8, 1);
		renderOffset.x = 0.2f;
	}

	public override bool use(Player player)
	{
		if (staffCharges > 0 && player.mana >= manaCost)
		{
			player.actions.queueAction(new SpellCastAction(this, player.handItem == this, new MagicProjectileSpell()));
			player.consumeMana(manaCost);
			staffCharges--;
		}
		return staffCharges == 0;
	}
}
