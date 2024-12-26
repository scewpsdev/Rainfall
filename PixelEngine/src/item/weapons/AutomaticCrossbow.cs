using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class AutomaticCrossbow : Weapon
{
	public AutomaticCrossbow()
		: base("automatic_crossbow", WeaponType.Ranged)
	{
		displayName = "Automatic Crossbow";

		description = "slaps";

		baseDamage = 0.7f;
		baseAttackRate = 7;
		baseAttackRange = 40; // arrow speed
		knockback = 2.0f;
		trigger = false;
		requiredAmmo = "arrow";
		twoHanded = true;
		accuracy = 0.3f;

		value = 42;

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
			Item arrow = player.removeItemSingle(arrows);
			player.actions.queueAction(new CrossbowShootAction(this, arrow, player.handItem == this));
			accuracy = MathF.Max(accuracy - 0.2f, 0.02f);
		}
		return false;
	}

	public override void update(Entity entity)
	{
		base.update(entity);
		accuracy = MathHelper.Lerp(accuracy, 0.7f, 2.0f * Time.deltaTime);
	}
}
