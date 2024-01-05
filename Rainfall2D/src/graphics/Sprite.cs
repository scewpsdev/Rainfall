using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public class Sprite
	{
		public SpriteSheet spriteSheet;
		public Vector2i position;


		public Sprite(SpriteSheet spriteSheet, int x, int y)
		{
			this.spriteSheet = spriteSheet;
			position = new Vector2i(x, y);
		}

		public Sprite(Texture texture)
		{
			spriteSheet = new SpriteSheet(texture, texture.width, texture.height);
			position = new Vector2i(0, 0);
		}

		public int width
		{
			get => spriteSheet.spriteSize.x;
		}

		public int height
		{
			get => spriteSheet.spriteSize.y;
		}

		internal Vector2 uv0
		{
			get => new Vector2(position.x * spriteSheet.spriteSize.x, position.y * spriteSheet.spriteSize.y) / new Vector2(spriteSheet.texture.width, spriteSheet.texture.height);
		}

		internal Vector2 uv1
		{
			get => new Vector2((position.x + 1) * spriteSheet.spriteSize.x, (position.y + 1) * spriteSheet.spriteSize.y) / new Vector2(spriteSheet.texture.width, spriteSheet.texture.height);
		}
	}
}
