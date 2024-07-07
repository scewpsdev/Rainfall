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

		attackDamage = 3;
		attackRange = 1.5f;

		sprite = new Sprite(tileset, 1, 0);
		ingameSprite = new Sprite(Resource.GetTexture("res/sprites/sword.png", false));
	}

	public override void use(Player player)
	{
		player.actions.queueAction(new AttackAction(this));
	}
}
