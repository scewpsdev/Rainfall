using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MoonbladeAxe : Item
{
	public MoonbladeAxe()
		: base("moonblade_axe", ItemType.Weapon)
	{
		displayName = "Moonblade Axe";

		attackDamage = 2.0f;
		attackRange = 1.9f;
		attackRate = 1.2f;
		stab = false;

		value = 57;

		sprite = new Sprite(tileset, 15, 4, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.4f;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}
}
