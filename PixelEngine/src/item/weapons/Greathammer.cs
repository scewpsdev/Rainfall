using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Greathammer : Item
{
	public Greathammer()
		: base("greathammer", ItemType.Weapon)
	{
		displayName = "Greathammer";

		baseDamage = 3.5f;
		baseAttackRange = 1.8f;
		baseAttackRate = 0.7f;
		knockback = 12;
		stab = false;
		twoHanded = true;
		attackCooldown = 2.5f;
		attackAngleOffset = 0;
		attackAngle = MathF.PI;
		doubleBladed = false;
		baseWeight = 5;

		value = 25;

		sprite = new Sprite(tileset, 10, 4, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.7f;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}
}
