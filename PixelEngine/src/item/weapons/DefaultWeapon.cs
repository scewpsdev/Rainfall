using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DefaultWeapon : Weapon
{
	public static readonly DefaultWeapon instance = new DefaultWeapon();


	Sprite punchSprite, swingSprite;

	public DefaultWeapon()
		: base("default_weapon")
	{
		baseDamage = 0.8f;
		baseAttackRange = 1.0f;
		baseAttackRate = 3;
		knockback = 5;
		attackAngle = 1.0f * MathF.PI;
		attackAngleOffset = -0.9f * MathF.PI;

		punchSprite = new Sprite(tileset, 0, 2);
		swingSprite = new Sprite(tileset, 1, 8);

		hitSound = [Resource.GetSound("res/sounds/punch_hit.ogg")];
	}

	public override bool use(Player player)
	{
		bool anim = stab;
		if (player.actions.currentAction != null && player.actions.currentAction is AttackAction)
		{
			AttackAction lastAttack = player.actions.currentAction as AttackAction;
			if (lastAttack.weapon == this)
				anim = !lastAttack.stab;
		}
		sprite = anim ? punchSprite : swingSprite;
		AttackAction attack = new AttackAction(this, anim, anim, baseAttackRate, baseDamage, baseAttackRange);
		player.actions.queueAction(attack);
		attack.attackIdx = 0;
		return false;
	}
}
