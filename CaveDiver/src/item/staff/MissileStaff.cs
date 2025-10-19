using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MissileStaff : Staff
{
	Spell spell;


	public MissileStaff()
		: base("missile_staff")
	{
		displayName = "Magic Missile Staff";

		value = 35;

		sprite = new Sprite(tileset, 8, 9);
		renderOffset.x = 0.4f;

		spell = new MissileSpell();
		maxStaffCharges = 6;
		staffCharges = 6;
	}

	public override bool use(Player player)
	{
		float manaCost = this.manaCost * spell.manaCost * player.getManaCostModifier();
		player.actions.queueAction(new SpellCastAction(this, player.handItem == this, spell, manaCost));

		if (useSound != null)
			Audio.PlayOrganic(useSound, new Vector3(player.position, 0), 1, 1, 0.0f, 0.15f);

		return staffCharges <= 0;
	}
}
