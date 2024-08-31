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
		displayName = "Cheese";
		stackable = true;
		//canDrop = false;

		value = 12;

		sprite = new Sprite(tileset, 13, 0);
	}

	public override bool use(Player player)
	{
		player.addStatusEffect(new HealEffect(1.5f, 5));
		//if (player.health < player.maxHealth)
		//	player.health = MathF.Min(player.health + 0.5f, player.maxHealth);
		return true;
	}
}
