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
		attackRate = 2.2f;
		attackRange = 30; // arrow speed
		knockback = 2.0f;
		trigger = false;
		requiredAmmo = "arrow";
		isSecondaryItem = true;

		value = 32;

		sprite = new Sprite(tileset, 9, 3);

		useSound = [Resource.GetSound("res/sounds/bow_shoot.ogg")];
	}

	public override bool use(Player player)
	{
		Item arrows = player.getItem(requiredAmmo);
		if (player.unlimitedArrows && arrows == null)
		{
			arrows = new Arrow();
			player.giveItem(arrows);
		}
		if (arrows != null)
		{
			base.use(player);
			player.actions.queueAction(new BowShootAction(this, player.handItem == this));
			player.removeItemSingle(arrows);
		}
		return false;
	}
}
