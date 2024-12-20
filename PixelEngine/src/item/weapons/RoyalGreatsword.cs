using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RoyalGreatsword : Item
{
	public RoyalGreatsword()
		: base("royal_greatsword", ItemType.Weapon)
	{
		displayName = "Royal Greatsword";

		baseDamage = 4;
		baseAttackRange = 1.8f;
		baseAttackRate = 1.5f;
		stab = false;
		twoHanded = true;
		baseWeight = 3;

		value = 50;

		sprite = new Sprite(tileset, 10, 5, 2, 1);
		icon = new Sprite(tileset, 10.5f, 5);
		size = new Vector2(2, 1);
		renderOffset.x = 0.7f;
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
