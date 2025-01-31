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

			float attackDamage = getAttackDamage(player);
			float attackRate = this.attackRate;

			bool mainHand = player.handItem == this;

			Item powerstancedWeapon = null;
			if (mainHand && player.offhandItem != null && player.offhandItem.name == name)
			{
				powerstancedWeapon = player.offhandItem;
				attackRate *= 1.25f;
			}
			else if (!mainHand && player.handItem != null && player.handItem.name == name)
			{
				powerstancedWeapon = player.handItem;
				attackRate /= 1.5f;
				attackDamage += 0.5f * powerstancedWeapon.getAttackDamage(player);
			}

			player.actions.queueAction(new AttackAction(this, mainHand, anim, attackIdx, attackRate, attackDamage, range, startAngle, endAngle, powerstancedWeapon) { swingDir = swingDir });
			return false;
		}

		base.use(player);
		return false;
	}

	public override bool useSecondary(Player player)
	{
		if (canParry || canBlock)
			player.actions.queueAction(new BlockAction(this, player.handItem == this));
		return false;
	}
}
