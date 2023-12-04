using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Tileset
{
	public Texture texture;
	public Vector2i spriteSize;


	public Tileset(Texture texture, int spriteWidth, int spriteHeight)
	{
		this.texture = texture;
		spriteSize = new Vector2i(spriteWidth, spriteHeight);
	}
}
