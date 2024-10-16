using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Formation : Item
{
	bool active = false;

	public Formation()
		: base("formation", ItemType.Relic)
	{
		displayName = "Formation";
		description = "Ducking increases armor";
		stackable = false;
		tumbles = false;

		value = 36;

		sprite = new Sprite(tileset, 13, 7);

		modifier = new Modifier() { defenseModifier = 2, movementSpeedModifier = 0.5f };
	}

	void activate(Player player)
	{
		active = true;
		player.modifiers.Add(modifier);
	}

	void deactivate(Player player)
	{
		active = false;
		player.modifiers.Remove(modifier);
	}

	public override void update(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;
			if (player.isDucked && !active)
				activate(player);
			else if (!player.isDucked && active)
				deactivate(player);
		}
	}
}
