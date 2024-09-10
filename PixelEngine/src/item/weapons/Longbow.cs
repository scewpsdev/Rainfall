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
		requiredAmmo = "arrow";

		value = 48;

		sprite = new Sprite(tileset, 10, 3, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;
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
