using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class Longbow : Item
{
	public Longbow()
		: base("longbow", ItemType.Weapon)
	{
		displayName = "Longbow";

		attackDamage = 3;
		attackRate = 0.7f;
		attackRange = 50; // arrow speed
		knockback = 8.0f;
		trigger = false;
		twoHanded = true;

		value = 56;

		sprite = new Sprite(tileset, 10, 3, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;
	}

	public override bool use(Player player)
	{
		//if (player.numArrows > 0)
		{
			player.actions.queueAction(new BowShootAction(this, player.handItem == this));
			//player.numArrows--;
			return false;
		}
	}
}
