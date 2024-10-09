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
		attackRange = 1.2f;
		attackRate = 1.5f;
		stab = false;
		weight = 1;
		//attackAngle = MathF.PI * 0.7f;

		value = 1;
		upgradable = false;

		sprite = new Sprite(tileset, 13, 1);
		renderOffset.x = 0.2f;

		hitSound = woodHit;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}
}
