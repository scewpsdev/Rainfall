using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Spear : Item
{
	public Spear()
		: base("spear", ItemType.Weapon)
	{
		displayName = "Spear";

		attackDamage = 2;
		attackRange = 2.0f;
		attackRate = 1.6f;

		projectileItem = true;
		projectileSticks = true;

		value = 8;

		sprite = new Sprite(tileset, 6, 1, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}

	public override bool useSecondary(Player player)
	{
		player.throwItem(this, player.lookDirection.normalized);
		return true;
	}
}
