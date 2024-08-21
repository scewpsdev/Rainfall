using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PotionOfGreaterHealing : Item
{
	public PotionOfGreaterHealing()
		: base("potion_of_greater_healing", ItemType.Potion)
	{
		displayName = "Potion of Greater Healing";
		stackable = true;

		value = 80;

		sprite = new Sprite(tileset, 7, 0);
	}

	public override bool use(Player player)
	{
		if (player.health < player.maxHealth - 0.1f)
			player.health = MathF.Min(player.health + 2, player.maxHealth);
		else
			player.health = player.maxHealth = player.maxHealth + 1;
		return true;
	}
}
