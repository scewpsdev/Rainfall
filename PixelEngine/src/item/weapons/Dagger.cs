using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Dagger : Item
{
	public Dagger()
		: base("dagger", ItemType.Weapon)
	{
		displayName = "Dagger";

		attackDamage = 1.5f;
		attackRange = 1.0f;
		attackRate = 4;

		projectileItem = true;

		value = 6;

		sprite = new Sprite(tileset, 2, 1);
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this));
		return false;
	}

	public override bool useSecondary(Player player)
	{
		player.throwItem(this, false, true);
		return true;
	}
}
