using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Boomerang : Item
{
	public Boomerang()
		: base("boomerang")
	{
		displayName = "Boomerang";

		value = 20;

		sprite = new Sprite(tileset, 3, 1);
	}

	public override bool use(Player player)
	{
		player.handItem = null;
		GameState.instance.level.addEntity(new BoomerangProjectile(player.direction, player), player.position + renderOffset * new Vector2(player.direction, 1));
		return true;
	}
}
