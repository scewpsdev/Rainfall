using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BlademastersRing : Item
{
	public BlademastersRing()
		: base("blademasters_ring", ItemType.Relic)
	{
		displayName = "Blademaster's Ring";
		description = "Wield a second weapon in the off-hand. Allows for double attacks with two weapons of the same type.";

		value = 35;

		sprite = new Sprite(tileset, 2, 10);
	}

	public override void onEquip(Player player)
	{
		player.canEquipOffhand = true;
	}

	public override void onUnequip(Player player)
	{
		player.canEquipOffhand = false;
	}
}
