using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Cheese : Item
{
	public Cheese()
		: base("cheese", ItemType.Food)
	{
		displayName = "Mouldy Cheese";
		stackable = true;
		//canDrop = false;

		value = 8;

		sprite = new Sprite(tileset, 13, 0);
	}

	public override bool use(Player player)
	{
		if (player.health < player.maxHealth)
			player.health = MathF.Min(player.health + 0.5f, player.maxHealth);
		return true;
	}
}
