using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RelicOffer : Entity
{
	List<ItemEntity> items = new List<ItemEntity>(3);

	public override void init(Level level)
	{
		// TODO modify count?
		for (int i = 0; i < 3; i++)
		{
			Item item = Item.CreateRandom(ItemType.Relic, Random.Shared, level.lootValue);
			Vector2 velocity = new Vector2(i - 1, 3) * 0.5f;
			ItemEntity entity = new ItemEntity(item, null, velocity);
			GameState.instance.level.addEntity(entity, position + new Vector2((i - 1) * 0.2f, 0.5f));
			items.Add(entity);
			entity.removeCallbacks.Add(() => { items.Remove(entity); });
		}
	}

	public override void update()
	{
		if (items.Count < 3)
		{
			for (int i = 0; i < items.Count; i++)
				items[i].remove();
			remove();
		}
	}
}
