using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class StaffOfIllumination : Staff
{
	Spell spell;


	public StaffOfIllumination()
		: base("staff_of_illumination")
	{
		displayName = "Staff of Illumination";

		isSecondaryItem = true;

		staffCharges = 10;
		maxStaffCharges = 10;

		value = 9;
		canDrop = false;

		sprite = new Sprite(tileset, 5, 4);
		renderOffset.x = 0.2f;

		spell = new IlluminationSpell();
	}

	public override bool use(Player player)
	{
		float manaCost = this.manaCost * spell.manaCost * player.getManaCostModifier();
		player.actions.queueAction(new SpellCastAction(this, player.handItem == this, spell, manaCost));
		staffCharges--;
		base.use(player);
		return staffCharges <= 0;
	}

	public override void render(Entity entity)
	{
		Renderer.DrawLight(entity.position + entity.collider.center, MathHelper.ARGBToVector(0xFFffecb5).xyz * 2, 7);
	}
}
