using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Chest : Entity, Interactable
{
	Sprite sprite;
	Sprite openSprite;

	bool flipped;
	bool open = false;
	Item item;


	public Chest(Item item, bool flipped = false)
	{
		this.item = item;

		sprite = new Sprite(TileType.tileset, 0, 0);
		openSprite = new Sprite(TileType.tileset, 1, 0);

		this.flipped = flipped;
	}

	public bool canInteract(Player player)
	{
		return !open;
	}

	public KeyCode getInput()
	{
		return KeyCode.X;
	}

	public float getRange()
	{
		return 1;
	}

	public void interact(Player player)
	{
		open = true;

		Vector2 itemVelocity = new Vector2(0, 1) * 8;
		Vector2 throwOrigin = position + new Vector2(0, 0.5f);
		ItemEntity obj = new ItemEntity(item, this, itemVelocity);
		GameState.instance.level.addEntity(obj, throwOrigin);
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, 1, 1, open ? openSprite : sprite, flipped);
	}
}
