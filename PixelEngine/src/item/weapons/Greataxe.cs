using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Greataxe : Item
{
	public Greataxe()
		: base("greataxe", ItemType.Weapon)
	{
		displayName = "Greataxe";

		baseDamage = 5;
		baseAttackRange = 1.8f;
		baseAttackRate = 0.8f;
		knockback = 8;
		stab = false;
		twoHanded = true;
		attackCooldown = 1.0f;
		baseWeight = 5;

		value = 72;

		sprite = new Sprite(tileset, 0, 5, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.5f;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}
}
