using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Scimitar : Item
{
	public Scimitar()
		: base("scimitar", ItemType.Weapon)
	{
		displayName = "Scimitar";

		attackDamage = 2;
		attackRange = 1.2f;
		attackRate = 2.0f;
		stab = false;
		attackCooldown = 0.3f;

		value = 18;

		sprite = new Sprite(tileset, 5, 3);
		renderOffset.x = 0.2f;
		//ingameSprite = new Sprite(Resource.GetTexture("res/sprites/sword.png", false));
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}
}
