using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Rock : Item
{
	public Rock()
		: base("rock", ItemType.Weapon)
	{
		displayName = "Rock";

		projectileItem = true;
		maxPierces = -1;

		attackDamage = 4;

		value = 1;

		sprite = new Sprite(tileset, 4, 0);
	}

	public override bool use(Player player)
	{
		player.throwItem(this, player.lookDirection.normalized);
		return true;
	}
}
