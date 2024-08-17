using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PotionOfSupremeHealing : Item
{
	public PotionOfSupremeHealing()
		: base("potion_of_supreme_healing")
	{
		displayName = "Potion of Supreme Healing";
		type = ItemType.Active;
		stackable = true;

		sprite = new Sprite(tileset, 7, 0);

		//rarity = 30;
		value = 160;
	}

	public override bool use(Player player)
	{
		float minimumHealing = 2;
		if (player.health < player.maxHealth - minimumHealing)
			player.health = player.maxHealth;
		else
			player.health += minimumHealing;
		return true;
	}
}
