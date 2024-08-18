using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Cheese : Item
{
	public Cheese()
		: base("cheese")
	{
		displayName = "Mouldy Cheese";
		type = ItemType.Active;
		stackable = true;
		canDrop = false;

		sprite = new Sprite(tileset, 13, 0);

		//rarity = 0;
		value = 8;
	}

	public override bool use(Player player)
	{
		if (player.health < player.maxHealth)
			player.health = MathF.Min(player.health + 0.5f, player.maxHealth);
		return true;
	}
}
