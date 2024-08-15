using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Door : Entity, Interactable
{
	public Level destination;
	public Door otherDoor;

	Sprite sprite;
	uint outline = 0;


	public Door(Level destination, Door otherDoor = null)
	{
		this.destination = destination;
		this.otherDoor = otherDoor;

		sprite = new Sprite(TileType.tileset, 2, 2);
	}

	public void interact(Player player)
	{
		GameState.instance.switchLevel(destination, otherDoor);
	}

	public void onFocusEnter(Player player)
	{
		outline = 0x9FFFFFFF;
	}

	public void onFocusLeft(Player player)
	{
		outline = 0;
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, LAYER_BG, 1, 1, 0, sprite, false, 0xFFFFFFFF);

		if (outline != 0)
			Renderer.DrawOutline(position.x - 0.5f, position.y, LAYER_BG, 1, 1, 0, sprite, false, outline);
	}
}
