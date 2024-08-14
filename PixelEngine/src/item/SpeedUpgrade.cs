using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SpeedUpgrade : Item
{
	public SpeedUpgrade()
		: base("speed_upgrade")
	{
		displayName = "Speed Upgrade";
		type = ItemType.Passive;

		sprite = new Sprite(tileset, 9, 0);
	}

	public override Item createNew()
	{
		return new SpeedUpgrade();
	}

	public override void onEquip(Player player)
	{
		player.speed++;
	}
}
