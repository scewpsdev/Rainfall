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

		attackDamage = 1;
		attackRange = 1.0f;
		attackRate = 2;
		stab = false;
		//attackAngle = MathF.PI * 0.7f;

		value = 1;

		sprite = new Sprite(tileset, 13, 1);
		size = new Vector2(1, 1);
		//ingameSprite = new Sprite(Resource.GetTexture("res/sprites/sword.png", false));
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this));
		return true;
	}
}
