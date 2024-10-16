using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Quarterstaff : Item
{
	public Quarterstaff()
		: base("quarterstaff", ItemType.Weapon)
	{
		displayName = "Quarterstaff";

		baseDamage = 1.0f;
		baseAttackRange = 1.2f;
		baseAttackRate = 2.5f;
		stab = false;
		attackAngle = MathF.PI * 2;
		attackCooldown = 0.5f;
		twoHanded = true;
		secondaryChargeTime = 0.3f;
		baseWeight = 1;
		//stab = false;
		//attackAngle = MathF.PI * 0.7f;

		value = 9;

		sprite = new Sprite(tileset, 4, 1, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;

		hitSound = woodHit;
		blockSound = woodHit;
	}

	public override bool use(Player player)
	{
		bool anim = stab;
		if (player.actions.currentAction != null && player.actions.currentAction is AttackAction)
		{
			AttackAction attack = player.actions.currentAction as AttackAction;
			if (attack.weapon == this)
				anim = !attack.stab;
		}
		player.actions.queueAction(new AttackAction(this, player.handItem == this, anim, baseAttackRate, baseDamage, baseAttackRange));
		return false;
	}

	public override bool useSecondary(Player player)
	{
		player.actions.queueAction(new BlockAction(this, player.handItem == this));
		return false;
	}
}
