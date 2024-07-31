using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Stick : Item
{
	public Stick()
		: base("stick")
	{
		displayName = "Stick";

		attackDamage = 2;
		attackRange = 1.0f;
		attackRate = 2;
		//stab = false;

		sprite = new Sprite(tileset, 5, 1);
		//ingameSprite = new Sprite(Resource.GetTexture("res/sprites/sword.png", false));
	}

	public override Item createNew()
	{
		return new Stick();
	}

	public override void use(Player player)
	{
		player.actions.queueAction(new AttackAction(this));
	}
}
