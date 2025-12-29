using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class Crossbow : Weapon
{
	public Item loadedArrow = null;

	public Sound reloadSound;


	public Crossbow()
		: base("crossbow", WeaponType.Ranged)
	{
		displayName = "Crossbow";

		baseDamage = 3.2f;
		baseAttackRate = 0.7f;
		baseAttackRange = 7; // arrow speed
		knockback = 12.0f;
		trigger = true;
		twoHanded = true;
		secondaryChargeTime = 0;
		//requiredAmmo = "arrow";

		baseValue = 17;

		sprite = new Sprite(tileset, 12, 3);
		renderOffset.x = 0.5f;
		backRotation = 0.5f * MathF.PI;

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
		if (loadedArrow != null && player.actions.currentAction == null)
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
			//Item arrows = player.getItem(requiredAmmo);
			//if (player.unlimitedArrows && arrows == null)
			//{
			//	arrows = new Arrow();
			//	player.giveItem(arrows);
			//}
			//if (arrows != null)
			{
				loadedArrow = new Arrow(); // player.removeItemSingle(arrows);
				Audio.PlayOrganic(reloadSound, new Vector3(player.position, 0), 3);
			}
		}
		return false;
	}
}
