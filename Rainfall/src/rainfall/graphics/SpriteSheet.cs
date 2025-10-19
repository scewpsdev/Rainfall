using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public class SpriteSheet
	{
		public Texture texture;
		public Vector2i spriteSize;


		public SpriteSheet(Texture texture, int spriteWidth, int spriteHeight)
		{
			this.texture = texture;
			this.spriteSize = new Vector2i(spriteWidth, spriteHeight);
		}
	}
}
