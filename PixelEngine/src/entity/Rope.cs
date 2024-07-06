using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Rope : Entity, Climbable
{
	public int height;

	Sprite sprite;
	Sprite topSprite, bottomSprite;


	public Rope(int height)
	{
		this.height = height;

		sprite = new Sprite(TileType.tileset, 2, 4);
		topSprite = new Sprite(TileType.tileset, 2, 3);
		bottomSprite = new Sprite(TileType.tileset, 2, 5);
	}

	public FloatRect getArea()
	{
		return new FloatRect(-0.5f, -height - 0.5f, 1, height + 1);
	}

	public override void render()
	{
		for (int i = 0; i < height + 1; i++)
		{
			Sprite s = i == 0 ? bottomSprite : i == height ? topSprite : sprite;
			Renderer.DrawSprite(position.x - 0.5f, position.y - height - 0.5f + i, LAYER_BG, 1, 1, 0, s, false, 0xFFFFFFFF);
		}
	}
}
