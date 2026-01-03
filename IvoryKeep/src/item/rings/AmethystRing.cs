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

		description = "Increases maximum health by 5";

		baseValue = 64;
		maxUpgradeLevel = 3;

		sprite = new Sprite(tileset, 10, 0);
	}

	public override void onEquip(Player player)
	{
		if (player.health == player.maxHealth)
			player.health++;
		player.hp += upgradeLevel + 1;
	}

	public override void onUnequip(Player player)
	{
		player.hp -= upgradeLevel + 1;
		player.health = MathF.Min(player.health, player.maxHealth);
	}
}
