using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Torch : Entity, Interactable
{
	Sprite sprite;


	public Torch()
	{
		sprite = new Sprite(TileType.tileset, 1, 3);
	}

	public KeyCode getInput()
	{
		return KeyCode.X;
	}

	public float getRange()
	{
		return 1;
	}

	public bool canInteract(Player player)
	{
		return player.handItem == null;
	}

	public void interact(Player player)
	{
		player.pickupObject(new ItemEntity(new TorchItem()));
		remove();
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 1, 1, sprite, false);
	}
}
