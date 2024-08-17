using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TorchEntity : Entity, Interactable
{
	Sprite sprite;
	uint outline = 0;


	public TorchEntity()
	{
		sprite = new Sprite(TileType.tileset, 1, 3);
	}

	public bool canInteract(Player player)
	{
		return player.handItem == null;
	}

	public void interact(Player player)
	{
		player.giveItem(new Torch());
		remove();
	}

	public void onFocusEnter(Player player)
	{
		outline = 0x7FFFFFFF;
	}

	public void onFocusLeft(Player player)
	{
		outline = 0;
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 1, 1, sprite, false);

		if (outline != 0)
			Renderer.DrawOutline(position.x - 0.5f, position.y - 0.5f, 1, 1, sprite, false, outline);
	}
}
