using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BlacksteelBracer : Item
{
	public BlacksteelBracer()
		: base("blacksteel_bracer", ItemType.Relic)
	{
		displayName = "Reinforced Glove";
		description = "Allows the bearer to wield big weapons with a single hand.";

		isPassiveItem = true;
		armorSlot = ArmorSlot.Gloves;

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
