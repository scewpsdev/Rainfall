using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class NimbleGloves : Item
{
	public NimbleGloves()
		: base("nimbleGloves", ItemType.Armor)
	{
		displayName = "Nimble Gloves";
		description = "Increases attack rate by 15%";
		armorSlot = ArmorSlot.Gloves;

		value = 35;

		sprite = new Sprite(tileset, 9, 9);

		buff = new ItemBuff(this) { attackSpeedModifier = 1.15f };
	}

	public override void onEquip(Player player)
	{
		player.itemBuffs.Add(buff);
	}

	public override void onUnequip(Player player)
	{
		player.itemBuffs.Remove(buff);
	}
}
