using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Skull : Item
{
	public Skull()
		: base("skull", ItemType.Weapon)
	{
		displayName = "Skull";

		projectileItem = true;
		breakOnHit = true;

		attackDamage = 4;

		value = 0.5f;

		sprite = new Sprite(tileset, 0, 0);
	}

	public override bool use(Player player)
	{
		player.throwItem(this);
		return true;
	}
}
