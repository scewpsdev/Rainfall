using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Arrow : Item
{
	public Arrow()
		: base("arrow")
	{
		displayName = "Arrow";

		sprite = new Sprite(tileset, 2, 0);

		attackDamage = 4;
		projectileItem = true;
		maxPierces = 1;

		value = 1;

		canDrop = false;
	}

	public override bool use(Player player)
	{
		player.throwItem(this, false);
		return true;
	}
}
