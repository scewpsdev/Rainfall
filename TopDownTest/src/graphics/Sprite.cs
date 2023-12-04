using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Sprite
{
	public Tileset tileset;
	public Vector2i pos, size;


	public Sprite(Tileset tileset, int x, int y, int width = 1, int height = 1)
	{
		this.tileset = tileset;
		pos = new Vector2i(x, y);
		size = new Vector2i(width, height);
	}

	public void getUVs(out float u0, out float v0, out float u1, out float v1)
	{
		u0 = pos.x * tileset.spriteSize.x / (float)tileset.texture.info.width;
		v0 = pos.y * tileset.spriteSize.y / (float)tileset.texture.info.height;
		u1 = (pos.x + size.x) * tileset.spriteSize.x / (float)tileset.texture.info.width;
		v1 = (pos.y + size.y) * tileset.spriteSize.y / (float)tileset.texture.info.height;
	}
}
