using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GoldenApple : Item
{
	public GoldenApple()
		: base("golden_apple", ItemType.Food)
	{
		displayName = "Golden Apple";
		stackable = true;

		value = 50;

		sprite = new Sprite(tileset, 4, 2);
	}

	public override bool use(Player player)
	{
		player.addStatusEffect(new HealStatusEffect(player.maxHealth, 12));
		player.addStatusEffect(new ManaRechargeEffect(player.maxMana, 5));
		return true;
	}
}
