using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Torch : Entity
{
	Sprite sprite;


	public Torch()
	{
		sprite = new Sprite(TileType.tileset, 1, 3);
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 1, 1, sprite, false);
	}
}
