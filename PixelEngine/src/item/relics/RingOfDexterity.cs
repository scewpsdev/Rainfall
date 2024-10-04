using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfDexterity : Item
{
	AttackSpeedModifier modifier;

	public RingOfDexterity()
		: base("ring_of_dexterity", ItemType.Relic)
	{
		displayName = "Ring of Dexterity";

		description = "Increases attack speed by 20%";
		value = 120;

		sprite = new Sprite(tileset, 9, 0);
	}

	public override void onEquip(Player player)
	{
		player.attackSpeedModifier *= 1.2f + 0.2f * upgradeLevel;
		player.addStatusEffect(modifier = new AttackSpeedModifier());
	}

	public override void onUnequip(Player player)
	{
		player.attackSpeedModifier /= 1.2f + 0.2f * upgradeLevel;
		player.removeStatusEffect(modifier);
		modifier = null;
	}
}
