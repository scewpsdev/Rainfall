using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Spear : Item
{
	public Spear()
		: base("spear")
	{
		displayName = "Spear";

		attackDamage = 2;
		attackRange = 2.0f;
		attackRate = 2;

		sprite = new Sprite(tileset, 6, 1, 2, 1);
		size = new Vector2(2, 1);
	}

	public override Item createNew()
	{
		return new Spear();
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this));
		return true;
	}
}
