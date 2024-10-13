using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Flamberge : Item
{
	public Flamberge()
		: base("flamberge", ItemType.Weapon)
	{
		displayName = "Flamberge";

		attackDamage = 2;
		attackRange = 1.7f;
		attackRate = 1.5f;
		stab = false;
		twoHanded = true;

		value = 24;

		sprite = new Sprite(tileset, 12, 6, 2, 1);
		icon = new Sprite(tileset.texture, 12 * 16, 6 * 16, 16, 16);
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
