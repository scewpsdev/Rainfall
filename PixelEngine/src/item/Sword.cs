using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Sword : Item
{
	public Sword()
		: base("sword")
	{
		displayName = "Sword";

		attackDamage = 4;
		attackRange = 1.5f;
		attackRate = 1.4f;
		stab = false;

		sprite = new Sprite(tileset, 1, 1);
		//ingameSprite = new Sprite(Resource.GetTexture("res/sprites/sword.png", false));
	}

	public override Item createNew()
	{
		return new Sword();
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this));
		return true;
	}
}
