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
		description = "Equip a second weapon in the off-hand. Allows for double attacks with two weapons of the same type.";

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
