using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class Bow : Weapon
{
	public Bow(string name)
		: base(name, WeaponType.Ranged)
	{
		baseDamage = 1;
		baseAttackRate = 2.5f;
		baseAttackRange = 30; // arrow speed
		knockback = 2.0f;
		trigger = false;
		requiredAmmo = "arrow";
		secondaryChargeTime = 0;
		baseWeight = 1;

		useSound = [Resource.GetSound("sounds/bow_shoot.ogg")];
		hitSound = woodHit;
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

	public override bool useSecondary(Player player)
	{
		int attackIdx = 0;
		if (player.actions.currentAction != null && player.actions.currentAction is AttackAction && (player.actions.currentAction as AttackAction).weapon == this)
			attackIdx = (player.actions.currentAction as AttackAction).attackIdx + 1;
		getAttackAnim(attackIdx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle);
		player.actions.queueAction(new AttackAction(this, player.handItem == this, anim, 2, 0.5f, 1, startAngle, endAngle) { swingDir = swingDir, attackIdx = attackIdx, useSoundPlayed = true });
		return false;
	}
}
