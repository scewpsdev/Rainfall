using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Boomerang : Weapon
{
	public Boomerang()
		: base("boomerang")
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
		Vector2 origin = player.position + new Vector2(0, 0.5f);
		Vector2 offset = new Vector2(player.direction * 0.5f, 0);
		GameState.instance.level.addEntity(new BoomerangProjectile(player.lookDirection.normalized, offset, player.velocity, player, this), origin);
		return true;
	}
}
