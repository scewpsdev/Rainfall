using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfSwiftness : Item
{
	public RingOfSwiftness()
		: base("ring_of_swiftness")
	{
		displayName = "Ring of Swiftness";
		type = ItemType.Passive;

		sprite = new Sprite(tileset, 9, 0);

		value = 100;
	}

	public override void onEquip(Player player)
	{
		player.speed++;
	}

	public override void onUnequip(Player player)
	{
		player.speed--;
	}
}
