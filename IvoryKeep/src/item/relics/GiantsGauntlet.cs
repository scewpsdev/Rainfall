using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GiantsGauntlet : Item
{
	public GiantsGauntlet()
		: base("giants_gauntlet", ItemType.Relic)
	{
		displayName = "Giant's Gauntlet";
		description = "Forced in ancient times for warriors of unmatched strength, enabling it's bearer to wield even the heaviest of weapons with a single hand.";

		armorSlot = ArmorSlot.Gloves;
		gloveColor = 0xFF4c3435;

		value = 45;

		sprite = new Sprite(tileset, 10, 9);
	}

	public override void onEquip(Player player)
	{
		player.canEquipOnehanded = true;
	}

	public override void onUnequip(Player player)
	{
		player.canEquipOnehanded = false;
	}
}
