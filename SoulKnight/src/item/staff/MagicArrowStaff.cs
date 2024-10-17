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
		displayName = "Magic Staff";

		attackRate = 1;
		trigger = false;
		isSecondaryItem = false;

		attackDamage = 1;
		manaCost = 1.0f;
		staffCharges = 10000;
		knockback = 1;
		//staffCharges = 28;
		//maxStaffCharges = 28;

		value = 30;

		//sprite = new Sprite(tileset, 8, 1);
		//renderOffset.x = 0.2f;
		sprite = new Sprite(tileset, 2, 6);
		renderOffset.x = 0.4f;

		//useSound = Resource.GetSounds("res/sounds/shoot", 11);
	}

	public override bool use(Player player)
	{
		Spell spell = new MagicArrowSpell();
		float manaCost = this.manaCost * spell.manaCost * player.getManaCostModifier();
		player.actions.queueAction(new SpellCastAction(this, player.handItem == this, spell, manaCost));
		player.consumeMana(manaCost);
		base.use(player);
		staffCharges--;
		return false;
	}
}
