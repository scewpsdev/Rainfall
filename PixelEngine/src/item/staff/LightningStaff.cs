using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LightningStaff : Item
{
	public LightningStaff()
		: base("lightning_staff", ItemType.Staff)
	{
		displayName = "Lightning Staff";

		attackRate = 2;
		trigger = false;

		attackDamage = 2;
		manaCost = 0.35f;
		staffCharges = 8;
		maxStaffCharges = 8;

		value = 30;

		sprite = new Sprite(tileset, 8, 2);
		renderOffset.x = 0.2f;
	}

	public override bool use(Player player)
	{
		if (staffCharges > 0 && player.mana >= manaCost)
		{
			player.actions.queueAction(new SpellCastAction(this, player.handItem == this, new LightningSpell()));
			player.consumeMana(manaCost);
			staffCharges--;
		}
		return staffCharges == 0;
	}
}
