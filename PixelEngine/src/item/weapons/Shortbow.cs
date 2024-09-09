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
		attackRange = 20; // arrow speed
		knockback = 2.0f;
		trigger = false;
		requiredAmmo = "arrow";

		value = 32;

		sprite = new Sprite(tileset, 9, 3);
	}

	public override bool use(Player player)
	{
		Item arrows = player.getItem(requiredAmmo);
		if (arrows != null)
		{
			player.actions.queueAction(new BowShootAction(this, player.handItem == this));
			player.removeItemSingle(arrows);
		}
		return false;
	}
}
