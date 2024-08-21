using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PotionOfHealing : Item
{
	public PotionOfHealing()
		: base("potion_of_healing", ItemType.Potion)
	{
		displayName = "Potion of Healing";
		stackable = true;

		value = 25;

		sprite = new Sprite(tileset, 7, 0);
	}

	public override bool use(Player player)
	{
		if (player.health < player.maxHealth)
			player.health = MathF.Min(player.health + 1, player.maxHealth);
		return true;
	}
}
