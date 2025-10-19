using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GiantsGauntlet : Item
{
	public GiantsGauntlet()
		: base("duelists_gauntlets", ItemType.Relic)
	{
		displayName = "Duelist's Gauntlets";
		description = "Forged in ancient times for warriors of unmatched dexterity, enabling it's bearer to wield two weapons at once.";

		armorSlot = ArmorSlot.Gloves;
		gloveColor = 0xFF4c3435;

		baseValue = 45;

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
