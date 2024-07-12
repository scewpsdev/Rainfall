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
	}

	public override Item createNew()
	{
		return new Rock();
	}

	public override void use(Player player)
	{
		player.throwItem(this);
	}
}
