using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PotionOfGreaterHealing : Item
{
	public PotionOfGreaterHealing()
		: base("potion_of_greater_healing")
	{
		displayName = "Potion of Greater Healing";
		type = ItemType.Active;
		stackable = true;

		sprite = new Sprite(tileset, 7, 0);

		//rarity = 20;
		value = 70;
	}

	public override bool use(Player player)
	{
		if (player.health < player.maxHealth)
			player.health = MathF.Min(player.health + 2, player.maxHealth);
		return true;
	}
}
