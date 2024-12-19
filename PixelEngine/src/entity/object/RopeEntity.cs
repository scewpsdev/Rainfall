using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RopeEntity : Entity, Climbable
{
	public int height;

	Sprite sprite;
	Sprite topSprite, bottomSprite;


	public RopeEntity(int height)
	{
		this.height = height;

		sprite = new Sprite(TileType.tileset, 2, 4);
		topSprite = new Sprite(TileType.tileset, 2, 3);
		bottomSprite = new Sprite(TileType.tileset, 2, 5);
	}

	public FloatRect getArea()
	{
		return new FloatRect(0, 0, 1, height);
	}

	public override void render()
	{
		for (int i = 0; i < height; i++)
		{
			Sprite s = i == 0 ? bottomSprite : i == height - 1 ? topSprite : sprite;
			Renderer.DrawSprite(position.x, position.y + i, LAYER_BG, 1, 1, 0, s, false, 0xFFFFFFFF);
		}
	}
}
