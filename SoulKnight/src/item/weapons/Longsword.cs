using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Longsword : Item
{
	public Longsword()
		: base("longsword", ItemType.Weapon)
	{
		displayName = "Longsword";

		attackDamage = 2.5f;
		attackRange = 1.2f;
		attackRate = 1.4f;
		stab = false;

		value = 16;

		sprite = new Sprite(tileset, 1, 1);
		renderOffset.x = 0.2f;
		//ingameSprite = new Sprite(Resource.GetTexture("res/sprites/sword.png", false));
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}
}
