using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfAgility : Item
{
	public RingOfAgility()
		: base("ring_of_agility", ItemType.Relic)
	{
		displayName = "Ring of Agility";
		description = "Allows it's bearer to stay light on their feet while swinging their weapon";

		baseValue = 36;

		sprite = new Sprite(tileset, 13, 7);
	}

	public override void onEquip(Player player)
	{
		base.onEquip(player);
		player.attackSlowdown = false;
	}

	public override void onUnequip(Player player)
	{
		base.onUnequip(player);
		player.attackSlowdown = true;
	}
}
