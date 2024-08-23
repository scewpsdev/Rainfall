using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PotionOfEnergy : Item
{
	public PotionOfEnergy()
		: base("potion_energy", ItemType.Potion)
	{
		displayName = "Potion of Energy";
		stackable = true;

		value = 25;

		sprite = new Sprite(tileset, 6, 2);
	}

	public override bool use(Player player)
	{
		if (player.mana < player.maxMana)
			player.mana = MathF.Min(player.mana + 2, player.maxMana);
		return true;
	}
}
