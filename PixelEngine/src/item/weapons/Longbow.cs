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

		baseDamage = 2.5f;
		baseAttackRate = 0.7f;
		baseAttackRange = 50; // arrow speed
		knockback = 8.0f;
		trigger = false;
		twoHanded = true;
		requiredAmmo = "arrow";
		accuracy = 3.0f;

		value = 25;

		sprite = new Sprite(tileset, 10, 3, 2, 1);
		icon = new Sprite(tileset.texture, 10 * 16 + 8, 3 * 16, 16, 16);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;

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
			Item arrow = player.removeItemSingle(arrows);
			player.actions.queueAction(new BowShootAction(this, arrow, player.handItem == this));
		}
		return false;
	}
}
