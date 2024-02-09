using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public static class FontManager
	{
		static Dictionary<string, FontData> fontDatas = new Dictionary<string, FontData>();
		static Dictionary<FontData, List<Font>> fonts = new Dictionary<FontData, List<Font>>();


		public static void LoadFont(string name, string path)
		{
			if (!fontDatas.ContainsKey(name))
			{
				FontData fontData = Resource.GetFontData(name);
				fontDatas.Add(name, fontData);
				fonts.Add(fontData, new List<Font>());
			}
		}

		public static Font GetFont(string name, float size, bool antialiased)
		{
			if (fontDatas.ContainsKey(name))
			{
				FontData fontData = fontDatas[name];
				List<Font> availableFonts = fonts[fontData];
				foreach (Font font in availableFonts)
				{
					if (font.size == size && font.antialiased == antialiased)
						return font;
				}

				Font newFont = fontData.createFont(size, antialiased);
				availableFonts.Add(newFont);
				return newFont;
			}
			return null;
		}
	}
}
