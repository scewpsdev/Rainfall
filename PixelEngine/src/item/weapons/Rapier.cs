using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Rapier : Item
{
	public Rapier()
		: base("rapier", ItemType.Weapon)
	{
		displayName = "Rapier";

		attackDamage = 1.5f;
		attackRange = 1.5f;
		attackRate = 2;
		attackCooldown = 2;

		value = 8;

		sprite = new Sprite(tileset, 14, 4);
		renderOffset.x = 0.2f;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}
}
