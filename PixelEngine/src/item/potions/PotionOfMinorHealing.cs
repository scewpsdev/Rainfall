using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PotionOfMinorHealing : Item
{
	public PotionOfMinorHealing()
		: base("potion_of_minor_healing")
	{
		displayName = "Potion of Minor Healing";
		type = ItemType.Active;
		stackable = true;

		sprite = new Sprite(tileset, 7, 0);

		//rarity = 0;
		value = 10;
	}

	public override bool use(Player player)
	{
		if (player.health < player.maxHealth)
			player.health = MathF.Min(player.health + 0.5f, player.maxHealth);
		return true;
	}
}
