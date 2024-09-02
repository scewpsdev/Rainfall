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
	}

	public override bool use(Player player)
	{
		GameState.instance.level.addEntity(new BoomerangProjectile(player.direction, player, this), player.position + renderOffset * new Vector2(player.direction, 1));
		return true;
	}
}
