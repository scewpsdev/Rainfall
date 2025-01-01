using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class Crossbow : Weapon
{
	Item loadedArrow = null;

	Sound reloadSound;


	public Crossbow()
		: base("crossbow", WeaponType.Ranged)
	{
		displayName = "Crossbow";

		baseDamage = 4;
		baseAttackRate = 1.0f; // 0.5f;
		baseAttackRange = 60; // arrow speed
		knockback = 12.0f;
		trigger = true;
		twoHanded = true;
		secondaryChargeTime = 1.0f;
		requiredAmmo = "arrow";

		value = 17;

		sprite = new Sprite(tileset, 12, 3);
		renderOffset.x = 0.5f;

		useSound = Resource.GetSounds("sounds/crossbow", 6);
		reloadSound = Resource.GetSound("sounds/crossbow_reload.ogg");
	}

	public override void update(Entity entity)
	{
		base.update(entity);
		sprite.position.x = (loadedArrow != null ? 13 : 12) * sprite.spriteSheet.spriteSize.x;
	}

	public override bool use(Player player)
	{
		if (loadedArrow != null)
		{
			base.use(player);
			player.actions.queueAction(new CrossbowShootAction(this, loadedArrow, player.handItem == this));
			loadedArrow = null;
		}
		return false;
	}

	public override bool useSecondary(Player player)
	{
		if (loadedArrow == null)
		{
			Item arrows = player.getItem(requiredAmmo);
			if (player.unlimitedArrows && arrows == null)
			{
				arrows = new Arrow();
				player.giveItem(arrows);
			}
			if (arrows != null)
			{
				loadedArrow = player.removeItemSingle(arrows);
				Audio.PlayOrganic(reloadSound, new Vector3(player.position, 0), 3);
			}
		}
		return false;
	}
}
