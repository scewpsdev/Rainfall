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
		sprite = new Sprite(tileset, 2, 0);

		attackDamage = 2;
		rarity = 0;

		projectileItem = true;

		maxPierces = 1;
	}

	public override Item createNew()
	{
		return new Arrow();
	}

	public override void use(Player player)
	{
		player.throwItem(this, false);
	}
}
