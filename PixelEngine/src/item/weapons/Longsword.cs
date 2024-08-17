using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Longsword : Item
{
	public Longsword()
		: base("longsword")
	{
		displayName = "Longsword";

		attackDamage = 4;
		attackRange = 1.2f;
		attackRate = 1.4f;
		stab = false;

		value = 12;

		sprite = new Sprite(tileset, 1, 1);
		//ingameSprite = new Sprite(Resource.GetTexture("res/sprites/sword.png", false));
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this));
		return true;
	}
}
