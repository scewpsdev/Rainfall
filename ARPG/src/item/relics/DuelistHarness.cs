using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DuelistHarness : Item
{
	public DuelistHarness()
		: base("duelist_harness", ItemType.Relic)
	{
		displayName = "Duelist Harness";
		description = "Finely crafted mechanism designed to anchor weapons in the offhand securely.";

		isPassiveItem = true;
		armorSlot = ArmorSlot.Gloves;

		value = 35;

		sprite = new Sprite(tileset, 7, 6);
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
