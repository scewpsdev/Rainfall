using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AssassinsDagger : Item
{
	public AssassinsDagger()
		: base("assassins_dagger", ItemType.Weapon)
	{
		displayName = "Assassins Dagger";

		attackDamage = 1.5f;
		attackRange = 1.0f;
		attackRate = 2;
		attackCooldown = 2.0f;
		canDrop = false;
		//stab = false;
		weight = 1;

		isSecondaryItem = true;
		attackRotationOffset = MathF.PI * 0.25f;

		value = 17;

		sprite = new Sprite(tileset, 15, 5);
		renderOffset.x = 0.0f;
		renderOffset.y = -0.25f;

		useSound = Resource.GetSounds("res/sounds/swing_dagger", 6);
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}
}
