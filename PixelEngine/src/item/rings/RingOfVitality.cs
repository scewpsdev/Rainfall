using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfVitality : Item
{
	public RingOfVitality()
		: base("ring_of_vitality", ItemType.Ring)
	{
		displayName = "Ring of Vitality";

		description = "Increases maximum health by 1";

		value = 100;

		sprite = new Sprite(tileset, 10, 0);
	}

	public override void onEquip(Player player)
	{
		if (player.health == player.maxHealth)
			player.health += 1 + upgradeLevel;
		player.maxHealth += 1 + upgradeLevel;
	}

	public override void onUnequip(Player player)
	{
		player.maxHealth -= 1 + upgradeLevel;
		player.health = MathF.Min(player.health, player.maxHealth);
	}
}
