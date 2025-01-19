using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfTears : Item
{
	bool active = false;

	float dmgBuff = 0.5f;


	public RingOfTears()
		: base("ring_of_tears", ItemType.Relic)
	{
		displayName = "Ring of Tears";

		description = "Increases attack when health is low";

		value = 110;

		sprite = new Sprite(tileset, 10, 2);

		buff = new ItemBuff(this);
	}

	void activate(Player player)
	{
		player.itemBuffs.Add(buff);
		active = true;
	}

	void deactivate(Player player)
	{
		player.itemBuffs.Remove(buff);
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

		buff.meleeDamageModifier = 1 + dmgBuff;
	}

	public override void upgrade()
	{
		base.upgrade();
		dmgBuff += 0.1f;
	}
}
