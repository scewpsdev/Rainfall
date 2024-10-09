using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Waraxe : Item
{
	public Waraxe()
		: base("waraxe", ItemType.Weapon)
	{
		displayName = "Waraxe";

		attackDamage = 1.8f;
		attackRange = 1.7f;
		attackRate = 1.3f;
		stab = false;
		weight = 2.5f;

		value = 23;

		sprite = new Sprite(tileset, 15, 4, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.4f;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}
}
