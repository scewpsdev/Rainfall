using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfTears : Item
{
	bool active = false;


	public RingOfTears()
		: base("ring_of_tears", ItemType.Ring)
	{
		displayName = "Ring of Tears";

		description = "Increases attack when health is low";

		value = 110;

		sprite = new Sprite(tileset, 10, 2);
	}

	void activate(Player player)
	{
		player.attack *= 1.5f;
	}

	void deactivate(Player player)
	{
		player.attack /= 1.5f;
	}

	public override void update(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;

			if (player.health <= 1.0f && !active)
				activate(player);
			else if (player.health > 1.0f && active)
				deactivate(player);
		}
	}
}
