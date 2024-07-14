using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HealthPotion : Item
{
	public HealthPotion()
		: base("health_potion")
	{
		displayName = "Health Potion";
		type = ItemType.Active;
		stackable = true;

		sprite = new Sprite(tileset, 7, 0);
	}

	public override Item createNew()
	{
		return new HealthPotion();
	}

	public override void use(Player player)
	{
		if (player.health < player.maxHealth)
			player.health++;
	}
}
