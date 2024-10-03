using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Magnet : Item
{
	public Magnet()
		: base("magnet", ItemType.Relic)
	{
		displayName = "Magnet";

		description = "Attracts nearby coins";
		value = 28;

		sprite = new Sprite(tileset, 12, 5);
	}

	public override void onEquip(Player player)
	{
		player.coinCollectDistance *= 10;
	}

	public override void onUnequip(Player player)
	{
		player.coinCollectDistance /= 10;
	}
}
