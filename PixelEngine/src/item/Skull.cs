using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Skull : Item
{
	public Skull()
		: base("skull")
	{
		displayName = "Skull";

		sprite = new Sprite(tileset, 0, 0);

		projectileItem = true;
		breakOnHit = true;

		attackDamage = 4;
	}

	public override Item createNew()
	{
		return new Skull();
	}

	public override bool use(Player player)
	{
		player.throwItem(this);
		return true;
	}
}
