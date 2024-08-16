using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Quarterstaff : Item
{
	public Quarterstaff()
		: base("quarterstaff")
	{
		displayName = "Quarterstaff";

		attackDamage = 1;
		attackRange = 1.2f;
		attackRate = 2;
		//stab = false;
		//attackAngle = MathF.PI * 0.7f;

		value = 2;

		sprite = new Sprite(tileset, 4, 1, 2, 1);
		size = new Vector2(2, 1);
		//ingameSprite = new Sprite(Resource.GetTexture("res/sprites/sword.png", false));
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this));
		return true;
	}
}
