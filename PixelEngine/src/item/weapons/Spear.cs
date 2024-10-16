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

		baseDamage = 2;
		baseAttackRange = 2.0f;
		baseAttackRate = 1.6f;
		baseWeight = 1.5f;

		projectileItem = true;
		projectileAims = true;
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
		player.throwItem(this, player.lookDirection.normalized, 25);
		return true;
	}
}
