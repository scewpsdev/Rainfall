using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Greatsword : Item
{
	public Greatsword()
		: base("greatsword", ItemType.Weapon)
	{
		displayName = "Greatsword";

		attackDamage = 6;
		attackRange = 1.8f;
		attackRate = 1.0f;
		stab = false;
		twoHanded = true;

		value = 48;

		sprite = new Sprite(tileset, 7, 3, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;
		//ingameSprite = new Sprite(Resource.GetTexture("res/sprites/sword.png", false));
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}
}
