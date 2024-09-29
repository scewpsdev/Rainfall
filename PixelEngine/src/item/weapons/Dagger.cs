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

		attackDamage = 1;
		attackRange = 0.6f;
		attackRate = 4;

		projectileItem = true;
		projectileSticks = true;

		value = 4;

		sprite = new Sprite(tileset, 2, 1);
		renderOffset.x = 0.2f;

		useSound = Resource.GetSounds("res/sounds/swing_dagger", 6);
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
