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
		public Vector2i size;


		public Sprite(SpriteSheet spriteSheet, int x, int y, int w = 1, int h = 1)
		{
			this.spriteSheet = spriteSheet;
			position = new Vector2i(x, y) * spriteSheet.spriteSize;
			size = new Vector2i(w, h) * spriteSheet.spriteSize;
		}

		public Sprite(SpriteSheet spriteSheet, float x, float y, float w = 1, float h = 1)
		{
			this.spriteSheet = spriteSheet;
			position = (Vector2i)Vector2.Round(new Vector2(x, y) * spriteSheet.spriteSize);
			size = (Vector2i)Vector2.Round(new Vector2(w, h) * spriteSheet.spriteSize);
		}

		public Sprite(Texture texture, int x, int y, int width, int height)
		{
			spriteSheet = new SpriteSheet(texture, 1, 1);
			position = new Vector2i(x, y);
			size = new Vector2i(width, height);
		}

		public Sprite(string path, int x, int y, int width, int height)
		{
			spriteSheet = new SpriteSheet(Resource.GetTexture(path, false), 1, 1);
			position = new Vector2i(x, y);
			size = new Vector2i(width, height);
		}

		public Sprite(Texture texture)
		{
			spriteSheet = new SpriteSheet(texture, texture.width, texture.height);
			position = new Vector2i(0, 0);
			size = new Vector2i(texture.width, texture.height);
		}

		public int width
		{
			get => size.x;
		}

		public int height
		{
			get => size.y;
		}

		public Vector2 uv0
		{
			get => new Vector2(position.x, position.y) / new Vector2(spriteSheet.texture.width, spriteSheet.texture.height);
		}

		public Vector2 uv1
		{
			get => new Vector2(position.x + size.x, position.y + size.y) / new Vector2(spriteSheet.texture.width, spriteSheet.texture.height);
		}
	}
}
