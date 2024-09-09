using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class AutomaticCrossbow : Item
{
	public AutomaticCrossbow()
		: base("automatic_crossbow", ItemType.Weapon)
	{
		displayName = "Automatic Crossbow";

		attackDamage = 0.8f;
		attackRate = 7;
		attackRange = 30; // arrow speed
		knockback = 2.0f;
		trigger = false;

		value = 132;

		sprite = new Sprite(tileset, 14, 3);
		renderOffset.x = 0.5f;
	}

	public override bool use(Player player)
	{
		Item arrows = player.getItem("arrow");
		if (arrows != null)
		{
			player.actions.queueAction(new GunShootAction(this, player.handItem == this));
			player.removeItemSingle(arrows);
		}
		return false;
	}
}
