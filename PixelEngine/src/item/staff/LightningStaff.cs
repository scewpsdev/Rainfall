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

		baseAttackRate = 1;
		trigger = false;
		isSecondaryItem = false;

		baseDamage = 1;
		manaCost = 1.0f;
		staffCharges = 10000;
		//maxStaffCharges = 8;

		value = 30;

		sprite = new Sprite(tileset, 8, 2);
		renderOffset.x = 0.4f;

		//useSound = Resource.GetSounds("res/sounds/lightning", 4);
	}

	public override bool use(Player player)
	{
		Spell spell = new LightningSpell();
		float manaCost = this.manaCost * spell.manaCost * player.getManaCostModifier();
		player.actions.queueAction(new SpellCastAction(this, player.handItem == this, spell, manaCost));
		player.consumeMana(manaCost);
		base.use(player);
		return false;
	}
}
