using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TripleShotStaff : Staff
{
	Spell spell;


	public TripleShotStaff()
		: base("triple_shot_staff")
	{
		displayName = "Triple Shot Staff";

		baseValue = 18;

		sprite = new Sprite(tileset, 12, 11);
		renderOffset.x = -0.2f;
		renderOffset.y = 0.1f;

		spell = new TripleShotSpell();
		//maxStaffCharges = 30;
		//staffCharges = 30;
	}

	public override bool use(Player player)
	{
		float manaCost = this.manaCost * spell.manaCost * player.getManaCostModifier();
		player.actions.queueAction(new SpellCastAction(this, player.handItem == this, spell, manaCost));
		staffCharges--;

		if (useSound != null)
			Audio.PlayOrganic(useSound, new Vector3(player.position, 0), 1, 1, 0.0f, 0.15f);

		return false;
	}
}
