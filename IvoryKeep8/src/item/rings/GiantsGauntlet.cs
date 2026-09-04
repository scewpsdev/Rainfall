using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GiantsGauntlet : Item
{
	public GiantsGauntlet()
		: base("giants_gauntlets", ItemType.Relic)
	{
		displayName = "Giant's Gauntlets";
		description = "Forged in ancient times for warriors of unmatched strength, enabling it's bearer to throw weapons at their enemies.";

		armorSlot = ArmorSlot.Gloves;
		gloveColor = 0xFF4c3435;

		baseValue = 25;

		sprite = new Sprite(tileset, 10, 9);
	}

	public override void onEquip(Player player)
	{
		player.canThrowWeapons = true;
	}

	public override void onUnequip(Player player)
	{
		player.canThrowWeapons = false;
	}
}
