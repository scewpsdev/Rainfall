using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class Shortbow : Item
{
	public Shortbow()
		: base("shortbow", ItemType.Weapon)
	{
		displayName = "Shortbow";

		attackDamage = 1;
		attackRate = 2.0f;
		attackRange = 30; // arrow speed
		knockback = 2.0f;
		trigger = false;

		value = 15;

		sprite = new Sprite(tileset, 9, 3);
	}

	public override bool use(Player player)
	{
		//if (player.numArrows > 0)
		{
			player.actions.queueAction(new BowShootAction(this, player.handItem == this));
			player.consumeMana(manaCost);
			//player.numArrows--;
			return false;
		}
	}
}
