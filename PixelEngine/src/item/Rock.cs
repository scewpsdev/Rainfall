using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Rock : Item
{
	public Rock()
		: base("rock")
	{
		displayName = "Rock";

		projectileItem = true;
		maxPierces = -1;

		attackDamage = 2;

		value = 1;

		sprite = new Sprite(tileset, 4, 0);
	}

	public override bool use(Player player)
	{
		player.throwItem(this);
		return true;
	}
}
