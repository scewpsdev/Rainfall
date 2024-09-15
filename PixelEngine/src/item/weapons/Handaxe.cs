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

		attackDamage = 1.5f;
		attackRange = 1.2f;
		attackRate = 1.5f;
		stab = false;

		projectileItem = true;
		projectileSpins = true;
		projectileSticks = true;

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
		ItemEntity entity = player.throwItem(this, player.lookDirection.normalized);
		entity.rotationVelocity = -MathF.PI * 5;
		return true;
	}
}
