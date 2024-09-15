using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Greataxe : Item
{
	public Greataxe()
		: base("greataxe", ItemType.Weapon)
	{
		displayName = "Greataxe";

		attackDamage = 5;
		attackRange = 1.8f;
		attackRate = 0.8f;
		knockback = 8;
		stab = false;
		twoHanded = true;
		attackCooldown = 1.0f;

		projectileItem = true;
		projectileSpins = true;
		projectileSticks = true;

		value = 72;

		sprite = new Sprite(tileset, 0, 5, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.5f;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}

	public override bool useSecondary(Player player)
	{
		ItemEntity entity = player.throwItem(this, player.lookDirection.normalized);
		entity.rotationVelocity = -MathF.PI * 5;
		return true;
	}
}
