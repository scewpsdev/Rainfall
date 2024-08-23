using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Apple : Item
{
	public Apple()
		: base("apple", ItemType.Food)
	{
		displayName = "Apple";
		stackable = true;

		value = 4;

		sprite = new Sprite(tileset, 5, 2);
	}

	public override bool use(Player player)
	{
		player.addStatusEffect(new HealEffect(0.25f, 5));
		return true;
	}
}
