using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ShadowStepRing : Item
{
	public ShadowStepRing()
		: base("shadow_step_ring", ItemType.Relic)
	{
		displayName = "Shadow Step Ring";

		description = "Allows it's bearer to perform a quickstep, briefly slipping through danger.";

		baseValue = 45;

		sprite = new Sprite(tileset, 15, 12);
	}

	public override void onEquip(Player player)
	{
		base.onEquip(player);
		player.canDodge = true;
	}

	public override void onUnequip(Player player)
	{
		base.onUnequip(player);
		player.canDodge = false;
	}
}
