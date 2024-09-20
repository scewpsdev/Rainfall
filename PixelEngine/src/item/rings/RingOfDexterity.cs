using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfDexterity : Item
{
	public RingOfDexterity()
		: base("ring_of_dexterity", ItemType.Ring)
	{
		displayName = "Ring of Dexterity";

		description = "Increases attack speed by 20%";
		value = 120;

		sprite = new Sprite(tileset, 9, 0);
	}

	public override void onEquip(Player player)
	{
		player.attackSpeedModifier *= 1.2f + 0.2f * upgradeLevel;
		player.addStatusEffect(new AttackSpeedModifier());
	}

	public override void onUnequip(Player player)
	{
		player.attackSpeedModifier /= 1.2f + 0.2f * upgradeLevel;
		player.removeStatusEffect("attack_speed_modifier");
	}
}
