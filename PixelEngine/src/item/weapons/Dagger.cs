using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Dagger : Weapon
{
	public Dagger()
		: base("dagger")
	{
		displayName = "Dagger";

		baseDamage = 0.8f;
		baseAttackRange = 1.1f;
		baseAttackRate = 3;

		projectileItem = true;
		projectileSticks = true;
		//projectileAims = true;
		projectileSpins = true;
		//isSecondaryItem = true;
		baseWeight = 1;
		secondaryChargeTime = 0;
		criticalChanceModifier = 2.0f;
		actionMovementSpeed = 0.8f;

		value = 4;

		sprite = new Sprite(tileset, 8, 6);
		renderOffset.x = 0.2f;

		useSound = Resource.GetSounds("sounds/swing_dagger", 6);
	}

	public override bool useSecondary(Player player)
	{
		Vector2 direction = player.lookDirection.normalized; // (player.lookDirection.normalized + new Vector2(MathF.Sign(player.velocity.x), 0)).normalized;
		if (Settings.game.aimMode == AimMode.Simple)
			direction = (direction + Vector2.Up * 0.1f).normalized;
		ItemEntity entity = player.throwItem(this, direction, 20);
		entity.rotationVelocity = -MathF.PI * 5;
		return true;
	}

	public override void upgrade()
	{
		base.upgrade();
		criticalChanceModifier *= 1.2f;
	}
}
