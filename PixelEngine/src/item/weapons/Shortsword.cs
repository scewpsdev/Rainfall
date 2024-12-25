using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Shortsword : Item
{
	public Shortsword()
		: base("shortsword", ItemType.Weapon)
	{
		displayName = "Shortsword";

		baseDamage = 1.25f;
		baseAttackRange = 0.8f;
		baseAttackRate = 2.2f;
		stab = false;
		baseWeight = 1;

		value = 9;

		sprite = new Sprite(tileset, 2, 1);
		renderOffset.x = 0.2f;
		//ingameSprite = new Sprite(Resource.GetTexture("res/sprites/sword.png", false));
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
}
