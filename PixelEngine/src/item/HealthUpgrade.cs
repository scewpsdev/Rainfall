using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HealthUpgrade : Item
{
	public HealthUpgrade()
		: base("health_upgrade")
	{
		displayName = "Health Upgrade";
		type = ItemType.Passive;

		sprite = new Sprite(tileset, 10, 0);
	}

	public override Item createNew()
	{
		return new HealthUpgrade();
	}

	public override void onEquip(Player player)
	{
		player.health++;
		player.maxHealth++;
	}
}
