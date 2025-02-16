using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Formation : Item
{
	public Formation()
		: base("formation", ItemType.Relic)
	{
		displayName = "Formation";
		description = "Ducking increases defense";
		stackable = false;
		tumbles = false;

		value = 36;

		sprite = new Sprite(tileset, 13, 7);

		buff = new ItemBuff(this) { defenseModifier = 5, movementSpeedModifier = 0.2f };
	}

	public override void update(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;
			buff.active = player.isDucked;
		}
	}
}
