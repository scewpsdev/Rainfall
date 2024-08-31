using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Bread : Item
{
	public Bread()
		: base("Bread", ItemType.Food)
	{
		displayName = "Bread";
		stackable = true;

		value = 8;

		sprite = new Sprite(tileset, 7, 2);
	}

	public override bool use(Player player)
	{
		player.addStatusEffect(new HealEffect(1, 5));
		return true;
	}
}
