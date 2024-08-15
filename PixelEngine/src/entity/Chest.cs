using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Chest : Entity, Interactable, Destructible
{
	Sprite sprite;
	Sprite openSprite;
	uint outline = 0;
	bool flipped;

	bool open = false;
	Item[] items;


	public Chest(Item[] items, bool flipped = false)
	{
		this.items = items;

		sprite = new Sprite(TileType.tileset, 0, 0);
		openSprite = new Sprite(TileType.tileset, 1, 0);

		collider = new FloatRect(-0.25f, 0.0f, 0.5f, 0.5f);

		this.flipped = flipped;
	}

	public Chest(params Item[] items)
		: this(items, false)
	{
	}

	public void onDestroyed(Entity entity, Item item)
	{
		if (!open && items != null)
			dropItems();
	}

	public bool canInteract(Player player)
	{
		return !open;
	}

	public void onFocusEnter(Player player)
	{
		outline = 0x9FFFFFFF;
	}

	public void onFocusLeft(Player player)
	{
		outline = 0;
	}

	void dropItems()
	{
		for (int i = 0; i < items.Length; i++)
		{
			Vector2 itemVelocity = new Vector2(MathHelper.RandomFloat(-0.2f, 0.2f), 1) * 8;
			Vector2 throwOrigin = position + new Vector2(0, 0.5f);
			ItemEntity obj = new ItemEntity(items[i], null, itemVelocity);
			GameState.instance.level.addEntity(obj, throwOrigin);
		}
		items = null;
	}

	public void interact(Player player)
	{
		open = true;
		GameState.instance.run.chestsOpened++;

		if (items != null)
			dropItems();
		else
		{
			// Scam chest
			Vector2 itemVelocity = new Vector2(MathHelper.RandomFloat(-0.2f, 0.2f), 1) * 8;
			Vector2 throwOrigin = position + new Vector2(0, 0.5f);
			Bomb bomb = new Bomb();
			bomb.ignite();
			ItemEntity obj = new ItemEntity(bomb, null, itemVelocity);
			GameState.instance.level.addEntity(obj, throwOrigin);
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, 1, 1, open ? openSprite : sprite, flipped);

		if (outline != 0)
			Renderer.DrawOutline(position.x - 0.5f, position.y, 0, 1, 1, 0, open ? openSprite : sprite, flipped, outline);
	}
}
