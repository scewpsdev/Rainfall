using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicArrowStaff : Staff
{
	Spell spell;


	public MagicArrowStaff()
		: base("magic_arrow_staff")
	{
		displayName = "Magic Arrow Staff";

		value = 30;

		sprite = new Sprite(tileset, 8, 1);
		renderOffset.x = 0.4f;

		spell = new MagicArrowSpell();
		maxStaffCharges = 30;
		staffCharges = 30;
	}

	public override bool use(Player player)
	{
		float manaCost = this.manaCost * spell.manaCost * player.getManaCostModifier();
		player.actions.queueAction(new SpellCastAction(this, player.handItem == this, spell, manaCost));
		staffCharges--;

		if (useSound != null)
			Audio.PlayOrganic(useSound, new Vector3(player.position, 0), 1, 1, 0.0f, 0.15f);

		return staffCharges <= 0;
	}
}
