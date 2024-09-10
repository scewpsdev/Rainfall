using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class Crossbow : Item
{
	bool loaded = false;


	public Crossbow()
		: base("crossbow", ItemType.Weapon)
	{
		displayName = "Crossbow";

		attackDamage = 4;
		attackRate = 1.0f; // 0.5f;
		attackRange = 60; // arrow speed
		knockback = 12.0f;
		trigger = true;
		secondaryChargeTime = 1.0f;
		requiredAmmo = "arrow";

		value = 56;

		sprite = new Sprite(tileset, 12, 3);
		renderOffset.x = 0.5f;
	}

	public override void update(Entity entity)
	{
		sprite.position.x = (loaded ? 13 : 12) * sprite.spriteSheet.spriteSize.x;
	}

	public override bool use(Player player)
	{
		if (loaded)
		{
			player.actions.queueAction(new GunShootAction(this, player.handItem == this));
			loaded = false;
		}
		return false;
	}

	public override bool useSecondary(Player player)
	{
		if (!loaded)
		{
			Item arrows = player.getItem(requiredAmmo);
			if (arrows != null)
			{
				player.removeItemSingle(arrows);
				loaded = true;
			}
		}
		return false;
	}
}
