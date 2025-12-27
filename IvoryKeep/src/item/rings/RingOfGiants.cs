using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RingOfGiants : Item
{
	public RingOfGiants()
		: base("ring_of_giants", ItemType.Relic)
	{
		displayName = "Ring of Giants";

		description = "Greatly reduces damage taken while performing an attack";

		baseValue = 36;

		sprite = new Sprite(tileset, 15, 11);

		buff = new ItemBuff(this);
	}

	public override void update(Entity entity)
	{
		base.update(entity);

		if (entity is Player)
		{
			Player player = entity as Player;
			buff.defenseModifier = player.actions.currentAction != null && player.actions.currentAction.type == "attack" ? 2 : 1;
		}
	}
}
