using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SleightOfHand : Item
{
	Modifier modifier = new Modifier() { attackSpeedModifier = 1.15f };

	public SleightOfHand()
		: base("sleight_of_hand", ItemType.Relic)
	{
		displayName = "Sleight of Hand";
		description = "Increases attack rate by 15%";
		stackable = true;
		tumbles = false;

		value = 35;

		sprite = new Sprite(tileset, 6, 6);
	}

	public override void onEquip(Player player)
	{
		player.modifiers.Add(modifier);
	}

	public override void onUnequip(Player player)
	{
		player.modifiers.Remove(modifier);
	}
}
