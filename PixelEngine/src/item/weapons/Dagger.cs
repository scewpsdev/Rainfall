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
		attackRange = 1.0f;
		attackRate = 3;
		criticalChance = 0.1f;

		projectileItem = true;
		projectileSticks = true;
		//projectileAims = true;
		projectileSpins = true;
		isSecondaryItem = true;
		weight = 1;

		value = 4;

		sprite = new Sprite(tileset, 8, 6);
		renderOffset.x = 0.0f;

		useSound = Resource.GetSounds("res/sounds/swing_dagger", 6);
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}

	public override bool useSecondary(Player player)
	{
		ItemEntity entity = player.throwItem(this, player.lookDirection.normalized, 20);
		entity.rotationVelocity = -MathF.PI * 5;
		return true;
	}

	public override void upgrade()
	{
		base.upgrade();
		criticalChance *= 1.2f;
	}
}
