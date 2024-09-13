using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class StaffOfIllumination : Item
{
	int charges = 17000;


	public StaffOfIllumination()
		: base("staff_light", ItemType.Staff)
	{
		displayName = "Staff of Illumination";

		attackRate = 1;
		trigger = false;
		isSecondaryItem = true;

		attackDamage = 0;
		manaCost = 0.5f;

		value = 26;

		sprite = new Sprite(tileset, 5, 4);
		renderOffset.x = 0.2f;
	}

	public override bool use(Player player)
	{
		if (charges > 0 && player.mana >= manaCost)
		{
			player.actions.queueAction(new SpellCastAction(this, player.handItem == this, new LightOrbSpell()));
			player.consumeMana(manaCost);
			charges--;
		}
		return charges == 0;
	}

	public override void render(Entity entity)
	{
		Renderer.DrawLight(entity.position + entity.collider.center, MathHelper.ARGBToVector(0xFFffecb5).xyz * 2, 7);
	}
}
