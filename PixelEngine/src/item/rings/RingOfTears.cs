using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfTears : Item
{
	bool active = false;

	float buff = 0.5f;


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
		player.attack *= 1 + buff;
		active = true;
	}

	void deactivate(Player player)
	{
		player.attack /= 1 + buff;
		active = false;
	}

	public override void onUnequip(Player player)
	{
		if (active)
			deactivate(player);
	}

	public override void update(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;

			if (player.health <= 1.1f && !active)
				activate(player);
			else if (player.health > 1.1f && active)
				deactivate(player);
		}
	}
}
