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
		sprite = new Sprite(tileset, 4, 0);

		projectileItem = true;
		maxPierces = -1;

		attackDamage = 4;
	}

	public override Item createNew()
	{
		return new Rock();
	}

	public override bool use(Player player)
	{
		player.throwItem(this);
		return true;
	}
}
