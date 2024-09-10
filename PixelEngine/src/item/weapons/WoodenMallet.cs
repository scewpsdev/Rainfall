using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WoodenMallet : Item
{
	public WoodenMallet()
		: base("wooden_mallet", ItemType.Weapon)
	{
		displayName = "Wooden Mallet";

		attackDamage = 3;
		attackRange = 1.8f;
		attackRate = 1.0f;
		knockback = 10;
		stab = false;
		twoHanded = true;
		attackCooldown = 2.5f;
		attackAngleOffset = 0;

		value = 28;

		sprite = new Sprite(tileset, 1, 4, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.7f;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}
}
