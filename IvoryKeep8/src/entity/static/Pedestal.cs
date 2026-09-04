using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Pedestal : Entity
{
	Sprite sprite;


	public Pedestal()
	{
		sprite = new Sprite(tileset, 0, 7, 2, 1);
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 1, position.y, LAYER_BG, 2, 1, 0, sprite);
	}
}
