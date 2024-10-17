using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Halberd : Item
{
	public Halberd()
		: base("halberd", ItemType.Weapon)
	{
		displayName = "Halberd";

		attackDamage = 2;
		attackRange = 1.8f;
		attackRate = 1.6f;
		twoHanded = true;
		weight = 2.5f;

		value = 16;

		sprite = new Sprite(tileset, 7, 4, 2, 1);
		icon = new Sprite(tileset.texture, 7 * 16 + 12, 4 * 16, 16, 16);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;
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
