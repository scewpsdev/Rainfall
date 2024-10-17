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
		requiredAmmo = "arrow";
		isSecondaryItem = true;
		secondaryChargeTime = 0;
		weight = 1;

		value = 32;

		sprite = new Sprite(tileset, 9, 3);

		useSound = [Resource.GetSound("res/sounds/bow_shoot.ogg")];
		hitSound = woodHit;
	}

	public override bool use(Player player)
	{
		/*
		Item arrows = player.getItem(requiredAmmo);
		if (player.unlimitedArrows && arrows == null)
		{
			arrows = new Arrow();
			player.giveItem(arrows);
		}
		if (arrows != null)
		*/
		{
			//Item arrow = player.removeItemSingle(arrows);
			base.use(player);
			player.actions.queueAction(new BowShootAction(this, null, player.handItem == this));
		}
		return false;
	}

	public override bool useSecondary(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this, true, 2, 0.5f, 1) { soundPlayed = true });
		return false;
	}
}
