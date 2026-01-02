using Rainfall;
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
			//AttackAnim anim = this.anim == AttackAnim.Stab ? AttackAnim.Stab : AttackAnim.SwingOverhead;
			//int swingDir = 0;
			//float startAngle = attackStartAngle;
			//float endAngle = attackEndAngle;
			//float range = attackRange;

			float attackDamage = getAttackDamage(player);
			float attackRate = this.attackRate;

			bool mainHand = player.handItem == this;

			Item powerstancedWeapon = null;
			if (player.canEquipOffhand)
			{
				if (mainHand && player.offhandItem != null && player.offhandItem.name == name)
				{
					powerstancedWeapon = player.offhandItem;
					attackRate *= 1.5f;
				}
				else if (!mainHand && player.handItem != null && player.handItem.name == name)
				{
					powerstancedWeapon = player.handItem;
					attackRate /= 1.5f;
					attackDamage += 0.5f * powerstancedWeapon.getAttackDamage(player);
				}
			}

			player.actions.queueAction(new AttackAction(this, mainHand, anim, attackIdx, attackRate, attackDamage, range, startAngle, endAngle, powerstancedWeapon) { swingDir = swingDir });
			return false;
		}

		base.use(player);
		return false;
	}

	public override bool useSecondary(Player player)
	{
		if (player.canThrowWeapons)
		{
			Vector2 direction = player.lookDirection.normalized; // (player.lookDirection.normalized + new Vector2(MathF.Sign(player.velocity.x), 0)).normalized;
			if (Settings.game.aimMode == AimMode.Simple)
				direction = (direction + Vector2.Up * 0.1f).normalized;
			ItemEntity entity = player.throwItem(this, direction);
			entity.rotationVelocity = -MathF.PI * 5;
			return true;
		}
		else
		{
			if (canParry || canBlock)
				player.actions.queueAction(new BlockAction(this, player.handItem == this));
			return false;
		}
	}
}
