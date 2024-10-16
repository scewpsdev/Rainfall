using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WoodenMallet : Item
{
	public WoodenMallet()
		: base("wooden_mallet", ItemType.Weapon)
	{
		displayName = "Wooden Mallet";

		baseDamage = 3;
		baseAttackRange = 1.2f;
		baseAttackRate = 1.3f;
		stab = false;
		twoHanded = true;
		attackCooldown = 2.5f;
		attackAngleOffset = 0;
		baseWeight = 2;

		value = 28;

		sprite = new Sprite(tileset, 1, 4, 2, 1);
		icon = new Sprite(tileset.texture, 1 * 16 + 12, 4 * 16, 16, 16);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;

		hitSound = woodHit;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}
}
