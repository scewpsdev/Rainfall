using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfVitality : Item
{
	public RingOfVitality()
		: base("ring_of_vitality")
	{
		displayName = "Ring of Vitality";
		type = ItemType.Passive;

		sprite = new Sprite(tileset, 10, 0);

		value = 100;
		//rarity = 20;
	}

	public override void onEquip(Player player)
	{
		player.health++;
		player.maxHealth++;
	}
}
