using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LifegemRing : Item
{
	float rate = 0.01f;

	bool enabled = false;


	public LifegemRing()
		: base("lifegem_ring", ItemType.Relic)
	{
		displayName = "Lifegem Ring";
		description = "Passively restores health over time";
		stackable = false;
		canDrop = false;

		value = 67;

		sprite = new Sprite(tileset, 14, 6);
	}

	public override void onEquip(Player player)
	{
		enabled = true;
	}

	public override void onUnequip(Player player)
	{
		enabled = false;
	}

	public override void update(Entity entity)
	{
		if (enabled && entity is Player)
		{
			Player player = entity as Player;
			player.heal(rate * Time.deltaTime);
		}
	}
}
