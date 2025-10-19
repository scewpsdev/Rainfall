using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AmethystRing : Item
{
	public AmethystRing()
		: base("amethyst_ring", ItemType.Relic)
	{
		displayName = "Amethyst Ring";

		description = "Increase max-hp by 1";
		canDrop = false;

		value = 64;

		sprite = new Sprite(tileset, 10, 0);
	}

	public override void onEquip(Player player)
	{
		if (player.health == player.maxHealth)
			player.health++;
		player.maxHealth++;
	}

	public override void onUnequip(Player player)
	{
		player.maxHealth--;
		player.health = MathF.Min(player.health, player.maxHealth);
	}
}
