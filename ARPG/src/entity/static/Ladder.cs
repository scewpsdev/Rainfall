using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Ladder : Entity, Climbable
{
	public int height;

	Sprite sprite;


	public Ladder(int height)
	{
		this.height = height;

		sprite = new Sprite(tileset, 0, 2);
	}

	public FloatRect getArea()
	{
		return new FloatRect(0, 0, 1, height);
	}

	public override void render()
	{
		for (int i = 0; i < height; i++)
		{
			Renderer.DrawSprite(position.x, position.y + i, LAYER_BG, 1, 1, 0, sprite, false, 0xFFFFFFFF);
		}
	}
}
