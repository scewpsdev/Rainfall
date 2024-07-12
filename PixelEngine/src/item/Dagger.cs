using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Dagger : Item
{
	public Dagger()
		: base("dagger")
	{
		displayName = "Dagger";

		attackDamage = 1;
		attackRange = 1.0f;
		attackRate = 4;

		sprite = new Sprite(tileset, 2, 1);
		//ingameSprite = new Sprite(Resource.GetTexture("res/sprites/sword.png", false));
	}

	public override Item createNew()
	{
		return new Dagger();
	}

	public override void use(Player player)
	{
		player.actions.queueAction(new AttackAction(this));
	}
}
