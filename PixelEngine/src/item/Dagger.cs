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

		attackDamage = 2;
		attackRange = 1.0f;
		attackRate = 4;

		sprite = new Sprite(tileset, 2, 1);
		//ingameSprite = new Sprite(Resource.GetTexture("res/sprites/sword.png", false));

		projectileItem = true;
	}

	public override Item createNew()
	{
		return new Dagger();
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this));
		return true;
	}

	public override bool useSecondary(Player player)
	{
		player.throwItem(this);
		return true;
	}
}
