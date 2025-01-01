using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LightningStaff : Staff
{
	Spell spell;


	public LightningStaff()
		: base("lightning_staff")
	{
		displayName = "Lightning Staff";

		value = 30;

		sprite = new Sprite(tileset, 8, 2);
		renderOffset.x = 0.4f;

		spell = new LightningSpell();
		staffCharges = 12;
	}

	public override bool use(Player player)
	{
		float manaCost = this.manaCost * spell.manaCost * player.getManaCostModifier();
		player.actions.queueAction(new SpellCastAction(this, player.handItem == this, spell, manaCost));
		staffCharges--;
		base.use(player);
		return staffCharges <= 0;
	}
}
