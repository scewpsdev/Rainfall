using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Boomerang : Item
{
	public Boomerang()
		: base("boomerang", ItemType.Weapon)
	{
		displayName = "Boomerang";

		attackDamage = 2;
		attackRange = 4;

		value = 20;

		sprite = new Sprite(tileset, 3, 1);
		renderOffset.x = 0.2f;
	}

	public override bool use(Player player)
	{
		GameState.instance.level.addEntity(new BoomerangProjectile(player.direction, player, this), player.position + new Vector2(renderOffset.x * player.direction, 0.5f + renderOffset.y));
		return true;
	}
}
