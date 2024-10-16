using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RelicOffer : Entity
{
	List<ItemEntity> items = new List<ItemEntity>(3);

	Sprite pedestal;


	public RelicOffer()
	{
		pedestal = new Sprite(TileType.tileset, 0, 6);
	}

	public override void init(Level level)
	{
		// TODO modify count?
		for (int i = 0; i < 3; i++)
		{
			Item item = Item.CreateRandom(ItemType.Relic, Random.Shared, level.lootValue);
			while (hasItem(item.name))
				item = Item.CreateRandom(ItemType.Relic, Random.Shared, level.lootValue);

			ItemEntity entity = new ItemEntity(item);
			entity.gravity = 0;
			entity.stuck = true;
			entity.removeCallbacks.Add(() => { items.Remove(entity); });
			GameState.instance.level.addEntity(entity, getPedestalPosition(i) + new Vector2(0, 1.0f));
			items.Add(entity);
		}
	}

	Vector2 getPedestalPosition(int i)
	{
		Vector2 pedestalPosition = position + new Vector2((i - 1) * 1.5f, 0);
		for (int j = 0; j < 3; j++)
		{
			TileType tile = GameState.instance.level.getTile(pedestalPosition + new Vector2(0, 0.1f));
			if (tile != null && tile.isSolid)
				pedestalPosition += Vector2.Up;
		}
		for (int j = 0; j < 3; j++)
		{
			TileType below = GameState.instance.level.getTile(pedestalPosition + new Vector2(0, -0.9f));
			if (below == null || !below.isSolid)
				pedestalPosition -= Vector2.Up;
		}
		return pedestalPosition;
	}

	bool hasItem(string name)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].name == name)
				return true;
		}
		return false;
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

	public override void render()
	{
		for (int i = 0; i < 3; i++)
		{
			Vector2 pedestalPosition = getPedestalPosition(i);
			Renderer.DrawSprite(pedestalPosition.x - 0.5f, pedestalPosition.y, 1, 1, pedestal);
		}
	}
}
