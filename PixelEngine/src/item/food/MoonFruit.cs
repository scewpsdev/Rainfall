using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MoonFruit : Item
{
	public MoonFruit()
		: base("moon_fruit", ItemType.Food)
	{
		displayName = "Moon Fruit";

		value = 72;

		sprite = new Sprite(tileset, 13, 4);
	}

	public override bool use(Player player)
	{
		player.maxMana++;
		player.addStatusEffect(new ManaRechargeEffect(player.maxMana, 3.0f));
		return true;
	}
}
