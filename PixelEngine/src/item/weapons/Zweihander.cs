using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Zweihander : Item
{
	public Zweihander()
		: base("zweihander", ItemType.Weapon)
	{
		displayName = "Zweihander";

		attackDamage = 5;
		attackRange = 1.8f;
		attackRate = 1.0f;
		stab = false;
		twoHanded = true;

		value = 48;

		sprite = new Sprite(tileset, 7, 3, 2, 1);
		icon = new Sprite(tileset.texture, 7 * 16 + 8, 3 * 16, 16, 16);
		size = new Vector2(2, 1);
		renderOffset.x = 0.7f;
		//ingameSprite = new Sprite(Resource.GetTexture("res/sprites/sword.png", false));
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}
}
