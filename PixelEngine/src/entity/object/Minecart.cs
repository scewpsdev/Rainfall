using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Minecart : Entity, Interactable
{
	Sprite sprite;
	bool looted = false;

	uint outline = 0;


	public Minecart()
	{
		sprite = new Sprite(TileType.tileset, 0, 8, 2, 1);
	}

	public bool canInteract(Player player)
	{
		return !looted;
	}

	public float getRange()
	{
		return 2;
	}

	public void onFocusEnter(Player player)
	{
		outline = OUTLINE_COLOR;
	}

	public void onFocusLeft(Player player)
	{
		outline = 0;
	}

	public void interact(Player player)
	{
		GameState.instance.level.addEntity(new ItemEntity(new Diamond(), null, Vector2.Up * 4), position + new Vector2(0, 1));
		looted = true;
	}

	public override void render()
	{
		if (outline != 0)
			Renderer.DrawOutline(position.x - 1, position.y, 2, 1, sprite, false, outline);

		Renderer.DrawSprite(position.x - 1, position.y, 2, 1, sprite);
	}
}
