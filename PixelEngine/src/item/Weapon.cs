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

	protected virtual void getAttackAnim(int idx, out bool stab, out int swingDir, out float startAngle, out float endAngle)
	{
		stab = this.stab;
		swingDir = !stab && doubleBladed ? idx % 2 : 0;
		startAngle = attackAngleOffset + attackAngle;
		endAngle = attackAngleOffset;
	}

	public override bool use(Player player)
	{
		base.use(player);
		if (weaponType == WeaponType.Melee)
		{
			int attackIdx = 0;
			if (player.actions.currentAction != null && player.actions.currentAction is AttackAction && (player.actions.currentAction as AttackAction).weapon == this)
				attackIdx = (player.actions.currentAction as AttackAction).attackIdx + 1;
			getAttackAnim(attackIdx, out bool stab, out int swingDir, out float startAngle, out float endAngle);
			player.actions.queueAction(new AttackAction(this, player.handItem == this, stab, baseAttackRate, baseDamage, baseAttackRange, startAngle, endAngle) { swingDir = swingDir, attackIdx = attackIdx });
		}
		return false;
	}
}
