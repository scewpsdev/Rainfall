using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall2D
{
	struct CharData
	{
		public char character;
		public int x, y;
		public int width, height;
	}

	public class PixelFont
	{
		public Texture texture;

		List<CharData> characters = new List<CharData>();
		Dictionary<char, int> charMap = new Dictionary<char, int>();


		public unsafe PixelFont(string path)
		{
			texture = Resource.GetTexture(path, false, true);
			texture.getImageData(out ImageData image);
			texture.freeCPUData();

			uint* pixels = image.data;

			char currentCharacter = ' ';
			int currentWidth = 0;
			int currentStart = 0;
			for (int i = 0; i < texture.width; i++)
			{
				if (pixels[i] == 0xFFFF00FF)
				{
					if (currentWidth > 0)
					{
						CharData data = new CharData();
						data.character = currentCharacter;
						data.x = currentStart;
						data.y = 0;
						data.width = currentWidth;
						data.height = texture.height;
						characters.Add(data);
						charMap.Add(currentCharacter, characters.Count - 1);

						currentCharacter++;
						currentWidth = 0;
					}

					currentStart = i + 1;
				}
				else
				{
					currentWidth++;
				}
			}

			image.free();
		}

		public bool getCharacterRect(char character, out IntRect rect)
		{
			if (charMap.ContainsKey(character))
			{
				CharData data = characters[charMap[character]];
				rect = new IntRect(data.x, data.y, data.width, data.height);
				return true;
			}
			rect = new IntRect();
			return false;
		}

		public int measureText(string text, int length)
		{
			int cursor = 0;
			for (int i = 0; i < length; i++)
			{
				char c = text[i];
				if (c != '\\')
				{
					if (!getCharacterRect(text[i], out IntRect rect))
						getCharacterRect('?', out rect);

					cursor += rect.size.x;
				}
				else
				{
					i++;
				}
			}
			return cursor;
		}

		public int size
		{
			get => texture.height;
		}
	}
}
