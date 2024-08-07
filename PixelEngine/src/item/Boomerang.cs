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

		sprite = new Sprite(tileset, 3, 1);
	}

	public override Item createNew()
	{
		return new Boomerang();
	}

	public override bool use(Player player)
	{
		player.handItem = null;
		GameState.instance.level.addEntity(new BoomerangProjectile(player.direction, player), player.position + player.itemRenderOffset);
		return true;
	}
}
