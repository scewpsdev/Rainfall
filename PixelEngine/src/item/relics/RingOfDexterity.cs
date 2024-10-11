using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfDexterity : Item
{
	Modifier modifier = new Modifier() { attackSpeedModifier = 1.2f };

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
		player.modifiers.Add(modifier);
	}

	public override void onUnequip(Player player)
	{
		player.modifiers.Remove(modifier);
	}

	public override void upgrade()
	{
		base.upgrade();
		modifier.attackSpeedModifier = 1.2f + 0.2f * upgradeLevel;
	}
}
