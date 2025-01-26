using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum WeaponType
{
	None = 0,

	Melee,
	Ranged,
	Other,
}

public class Weapon : Item
{
	public WeaponType weaponType;


	public Weapon(string name, WeaponType weaponType = WeaponType.Melee)
		: base(name, ItemType.Weapon)
	{
		this.weaponType = weaponType;
	}

	protected virtual void getAttackAnim(Player player, int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle, out float range)
	{
		anim = this.anim;
		swingDir = anim != AttackAnim.Stab && doubleBladed ? idx % 2 : 0;
		startAngle = attackStartAngle;
		endAngle = attackEndAngle;
		range = attackRange;
	}

	public override bool use(Player player)
	{
		if (weaponType == WeaponType.Melee)
		{
			int attackIdx = 0;
			if (player.actions.currentAction != null && player.actions.currentAction is AttackAction && (player.actions.currentAction as AttackAction).weapon == this)
				attackIdx = (player.actions.currentAction as AttackAction).attackIdx + 1;
			getAttackAnim(player, attackIdx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle, out float range);
			player.actions.queueAction(new AttackAction(this, player.handItem == this, anim, attackRate, getAttackDamage(player), range, startAngle, endAngle) { swingDir = swingDir, attackIdx = attackIdx });
			return false;
		}

		base.use(player);
		return false;
	}
}
