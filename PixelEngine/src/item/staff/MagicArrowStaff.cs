using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicArrowStaff : Item
{
	public MagicArrowStaff()
		: base("magic_arrow_staff", ItemType.Staff)
	{
		displayName = "Magic Arrow Staff";

		attackRate = 4;
		trigger = false;
		isSecondaryItem = true;

		attackDamage = 1;
		//manaCost = 0.1f;
		staffCharges = 28;
		maxStaffCharges = 28;

		value = 30;

		sprite = new Sprite(tileset, 8, 1);
		renderOffset.x = 0.2f;

		useSound = Resource.GetSounds("res/sounds/cast", 3);
	}

	public override bool use(Player player)
	{
		if (staffCharges > 0 && player.mana >= manaCost)
		{
			player.actions.queueAction(new SpellCastAction(this, player.handItem == this, new MagicArrowSpell(), 0));
			player.consumeMana(manaCost);
			base.use(player);
			staffCharges--;
		}
		return staffCharges == 0;
	}
}
