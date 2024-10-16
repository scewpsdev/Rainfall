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

		attackDamage = 4;
		attackRange = 1.8f;
		attackRate = 1.5f;
		stab = false;
		twoHanded = true;
		weight = 3;

		value = 102;

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
		player.actions.queueAction(new AttackAction(this, player.handItem == this, anim, attackRate, attackDamage, attackRange));
		return false;
	}
}
