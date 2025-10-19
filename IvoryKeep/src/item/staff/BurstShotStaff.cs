using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BurstShotStaff : Staff
{
	Spell spell;


	public BurstShotStaff()
		: base("burst_shot_staff")
	{
		displayName = "Burst Shot Staff";

		baseValue = 30;

		sprite = new Sprite(tileset, 13, 11);
		renderOffset.x = -0.2f;
		renderOffset.y = 0.1f;

		spell = new BurstShotSpell();
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
