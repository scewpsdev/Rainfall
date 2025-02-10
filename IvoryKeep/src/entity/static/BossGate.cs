using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BossGate : Entity
{
	Sprite sprite;
	int height = 3;

	public bool isOpen;


	public BossGate(bool isOpen)
	{
		this.isOpen = isOpen;

		sprite = new Sprite(tileset, 2, 7);
	}

	public override void init(Level level)
	{
		HitData hit = level.raycastTiles(position + 0.5f, Vector2.Down, 10);
		if (hit != null)
			height = (int)MathF.Ceiling(hit.distance);

		if (!isOpen)
			close();
	}

	public void open()
	{
		Vector2i tile = (Vector2i)Vector2.Floor(position + 0.5f);
		for (int i = 0; i < height; i++)
		{
			level.setTile(tile.x, tile.y - i, null);
		}
		isOpen = true;
	}

	public void close()
	{
		Vector2i tile = (Vector2i)Vector2.Floor(position + 0.5f);
		for (int i = 0; i < height; i++)
		{
			level.setTile(tile.x, tile.y - i, TileType.dummy);
		}
		isOpen = false;
	}

	public override void render()
	{
		if (!isOpen)
		{
			for (int i = 0; i < height; i++)
			{
				Renderer.DrawSprite(position.x, position.y - i, 1, 1, sprite);
			}
		}
	}
}
