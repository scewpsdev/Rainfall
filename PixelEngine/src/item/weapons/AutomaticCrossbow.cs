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

		description = "slaps";

		attackDamage = 1;
		attackRate = 7;
		attackRange = 40; // arrow speed
		knockback = 2.0f;
		trigger = false;
		requiredAmmo = "arrow";

		value = 82;

		sprite = new Sprite(tileset, 14, 3);
		renderOffset.x = 0.5f;

		//useSound = [Resource.GetSound("res/sounds/bow_shoot.ogg")];
		useSound = Resource.GetSounds("res/sounds/crossbow", 6);
	}

	public override bool use(Player player)
	{
		Item arrows = player.getItem("arrow");
		if (player.unlimitedArrows && arrows == null)
		{
			arrows = new Arrow();
			player.giveItem(arrows);
		}
		if (arrows != null)
		{
			base.use(player);
			player.actions.queueAction(new CrossbowShootAction(this, player.handItem == this));
			player.removeItemSingle(arrows);
		}
		return false;
	}
}
