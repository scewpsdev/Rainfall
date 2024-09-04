using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Stick : Item
{
	public Stick()
		: base("stick", ItemType.Weapon)
	{
		displayName = "Stick";

		attackDamage = 1;
		attackRange = 1.0f;
		attackRate = 2;
		stab = false;
		//attackAngle = MathF.PI * 0.7f;

		value = 1;

		sprite = new Sprite(tileset, 13, 1);
		size = new Vector2(1, 1);
		renderOffset.x = 0.2f;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}
}
