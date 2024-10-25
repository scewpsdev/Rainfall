using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Handaxe : Item
{
	public Handaxe()
		: base("handaxe", ItemType.Weapon)
	{
		displayName = "Handaxe";

		baseDamage = 1.5f;
		baseAttackRange = 1.2f;
		baseAttackRate = 1.5f;
		stab = false;

		projectileItem = true;
		projectileSpins = true;
		projectileSticks = true;
		doubleBladed = false;

		value = 5;

		sprite = new Sprite(tileset, 15, 4);
		renderOffset.x = 0.2f;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}

	public override bool useSecondary(Player player)
	{
		Vector2 direction = player.lookDirection.normalized;
		if (Settings.game.aimMode == AimMode.Simple)
			direction = (direction + Vector2.Up * 0.1f).normalized;
		ItemEntity entity = player.throwItem(this, direction);
		entity.rotationVelocity = -MathF.PI * 5;
		return true;
	}
}
