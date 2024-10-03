using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SleightOfHand : Item
{
	public SleightOfHand()
		: base("sleight_of_hand", ItemType.Relic)
	{
		displayName = "Sleight of Hand";
		description = "Increases attack rate by 15%";
		stackable = true;

		value = 35;

		sprite = new Sprite(tileset, 6, 6);
	}

	public override void onEquip(Player player)
	{
		player.attackSpeedModifier *= 1 + 0.15f;
	}

	public override void onUnequip(Player player)
	{
		player.attackSpeedModifier /= 1 + 0.15f;
	}
}
