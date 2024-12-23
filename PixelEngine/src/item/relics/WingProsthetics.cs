using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WingProsthetics : Item
{
	public WingProsthetics()
		: base("wing_prosthetics", ItemType.Armor)
	{
		armorSlot = ArmorSlot.Back;
		displayName = "Wing Prosthetics";

		description = "Allows the wearer to fling themselves upwards mid air";

		value = 25;

		sprite = new Sprite(tileset, 6, 8);
	}

	public override void onEquip(Player player)
	{
		player.airJumps++;
	}

	public override void onUnequip(Player player)
	{
		player.airJumps--;
	}
}
