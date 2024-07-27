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


	public Door(Level destination, Door otherDoor = null)
	{
		this.destination = destination;
		this.otherDoor = otherDoor;

		sprite = new Sprite(TileType.tileset, 2, 2);
	}

	public bool canInteract(Player player)
	{
		return true;
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
		GameState.instance.switchLevel(destination, otherDoor);
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, LAYER_BG, 1, 1, 0, sprite, false, 0xFFFFFFFF);
	}
}
