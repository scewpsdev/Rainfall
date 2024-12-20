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

		baseDamage = 2;
		baseAttackRange = 4;
		baseWeight = 1;

		value = 12;

		sprite = new Sprite(tileset, 3, 1);
		renderOffset.x = 0.2f;
	}

	public override bool use(Player player)
	{
		GameState.instance.level.addEntity(new BoomerangProjectile(player.lookDirection.normalized, player.velocity, player, this), player.position + new Vector2(renderOffset.x * player.direction, 0.5f + renderOffset.y));
		return true;
	}
}
