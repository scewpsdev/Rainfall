using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Quarterstaff : Item
{
	public Quarterstaff()
		: base("quarterstaff", ItemType.Weapon)
	{
		displayName = "Quarterstaff";

		attackDamage = 1;
		attackRange = 1.2f;
		attackRate = 2;
		stab = false;
		attackAngle = MathF.PI * 2;
		attackCooldown = 0.5f;
		//stab = false;
		//attackAngle = MathF.PI * 0.7f;

		value = 4;

		sprite = new Sprite(tileset, 4, 1, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}
}
