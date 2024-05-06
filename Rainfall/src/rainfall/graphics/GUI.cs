using Rainfall.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public static class GUI
	{
		struct UITextureDrawCommand
		{
			internal int x, y;
			internal int layer;
			internal int width, height;
			internal Texture texture;
			internal int u0, v0, u1, v1;
			internal Vector4 color;
		}

		unsafe struct TextDrawCommand
		{
			internal int x, y;
			internal int layer;
			internal float scale;
			internal byte* text;
			internal string str;
			internal int offset;
			internal int length;
			internal Font font;
			internal uint color;
		}


		static GraphicsDevice graphics;

		static Shader uiTextureShader;
		static Shader textShader;

		static SpriteBatch uiTextureBatch;
		static SpriteBatch textBatch;

		static int currentUILayer = 0;
		static int uiDepthCounter = 0;

		static List<UITextureDrawCommand> uiTextures = new List<UITextureDrawCommand>();
		static List<TextDrawCommand> texts = new List<TextDrawCommand>();


		public static void Init(GraphicsDevice graphics)
		{
			GUI.graphics = graphics;

			uiTextureShader = Resource.GetShader("res/rainfall/shaders/ui/ui.vsh", "res/rainfall/shaders/ui/ui.fsh");
			textShader = Resource.GetShader("res/rainfall/shaders/text/text.vsh", "res/rainfall/shaders/text/text.fsh");

			uiTextureBatch = new SpriteBatch(graphics);
			textBatch = new SpriteBatch(graphics);
		}

		public static void PushLayer()
		{
			currentUILayer++;
		}

		public static void PopLayer()
		{
			currentUILayer--;
		}

		public static bool IsHovered(int x, int y, int width, int height)
		{
			Vector2i cursorPos = Input.cursorPosition;
			return cursorPos.x >= x && cursorPos.x < x + width && cursorPos.y >= y && cursorPos.y < y + height;
		}

		public static void Texture(int x, int y, int width, int height, Texture texture, int u0, int v0, int u1, int v1, uint color)
		{
			uiTextures.Add(new UITextureDrawCommand { x = x, y = Display.viewportSize.y - y - height, layer = currentUILayer * 1000 + uiDepthCounter++, width = width, height = height, texture = texture, u0 = u0, v0 = v0, u1 = u1, v1 = v1, color = MathHelper.ARGBToVector(color) });
		}

		public static void Texture(int x, int y, int width, int height, Texture texture)
		{
			uiTextures.Add(new UITextureDrawCommand { x = x, y = Display.viewportSize.y - y - height, layer = currentUILayer * 1000 + uiDepthCounter++, width = width, height = height, texture = texture, u0 = 0, v0 = 0, u1 = texture.info.width, v1 = texture.info.height, color = Vector4.One });
		}

		public static void Rect(int x, int y, int width, int height, uint color)
		{
			uiTextures.Add(new UITextureDrawCommand { x = x, y = Display.viewportSize.y - y - height, layer = currentUILayer * 1000 + uiDepthCounter++, width = width, height = height, texture = null, u0 = 0, v0 = 0, u1 = 0, v1 = 0, color = MathHelper.ARGBToVector(color) });
		}

		public static void Text(int x, int y, float scale, Span<byte> text, int length, Font font, uint color)
		{
			unsafe
			{
				fixed (byte* textPtr = text)
					texts.Add(new TextDrawCommand { x = x, y = y, layer = currentUILayer * 1000 + uiDepthCounter++, scale = scale, text = textPtr, length = length, font = font, color = color });
			}
		}

		public static void Text(int x, int y, float scale, Span<byte> text, Font font, uint color)
		{
			Text(x, y, scale, text, StringUtils.StringLength(text), font, color);
		}

		public static void Text(int x, int y, float scale, string text, int length, Font font, uint color)
		{
			texts.Add(new TextDrawCommand { x = x, y = y, layer = currentUILayer * 1000 + uiDepthCounter++, scale = scale, str = text, length = length, font = font, color = color });
		}

		public static void Text(int x, int y, float scale, string text, Font font, uint color)
		{
			Text(x, y, scale, text, text.Length, font, color);
		}

		public static int TextWrapped(int x, int y, float scale, int maxWidth, float lineSpacing, string text, Font font, uint color)
		{
			int getWordLength(string str, int length, int offset)
			{
				for (int i = offset; i < length; i++)
				{
					if (str[i] == ' ' || str[i] == '\t' || str[i] == '\n' || str[i] == '\0')
						return i - offset;
				}
				return length - offset;
			}

			int length = text.Length;
			int xscroll = 0;
			int line = 0;
			int lineStart = 0;
			for (int j = 0; j < length;)
			{
				int nextWordLength = getWordLength(text, length, j);
				int nextWordWidth = font.measureText(text, j, nextWordLength);
				bool wraps = xscroll + nextWordWidth > maxWidth;
				if (wraps)
				{
					// draw line
					int lineLength = j - lineStart;
					texts.Add(new TextDrawCommand { x = x, y = y + (int)(line * lineSpacing * font.size * scale), layer = currentUILayer * 1000 + uiDepthCounter, scale = scale, str = text, offset = lineStart, length = lineLength, font = font, color = color });

					xscroll = 0;
					line++;
					lineStart = j;
				}
				if (j + nextWordLength >= length)
				{
					// draw last line
					int lineLength = j + nextWordLength - lineStart;
					texts.Add(new TextDrawCommand { x = x, y = y + (int)(line * lineSpacing * font.size * scale), layer = currentUILayer * 1000 + uiDepthCounter, scale = scale, str = text, offset = lineStart, length = lineLength, font = font, color = color });
				}

				int nextWordWidthWithSpace = j + nextWordLength < length ? font.measureText(text, j, nextWordLength + 1) : nextWordWidth;
				xscroll += nextWordWidthWithSpace;

				j += nextWordLength + 1;
			}

			uiDepthCounter++;

			return line + 1;
		}

		public static void Draw(RenderTarget target = null)
		{
			graphics.resetState();
			graphics.setPass(108);

			graphics.setRenderTarget(target, false);

			graphics.setViewTransform(Matrix.CreateOrthographic(0, Display.viewportSize.x, 0, Display.viewportSize.y, -1.0f, 1.0f), Matrix.Identity);


			uiTextureBatch.begin(uiTextures.Count);

			for (int i = 0; i < uiTextures.Count; i++)
			{
				float u0 = uiTextures[i].texture != null ? uiTextures[i].u0 / (float)uiTextures[i].texture.info.width : 0.0f;
				float v0 = uiTextures[i].texture != null ? uiTextures[i].v0 / (float)uiTextures[i].texture.info.height : 0.0f;
				float u1 = uiTextures[i].texture != null ? uiTextures[i].u1 / (float)uiTextures[i].texture.info.width : 0.0f;
				float v1 = uiTextures[i].texture != null ? uiTextures[i].v1 / (float)uiTextures[i].texture.info.height : 0.0f;

				uiTextureBatch.draw(
					uiTextures[i].x, uiTextures[i].y, uiTextures[i].layer * 0.0001f,
					uiTextures[i].width, uiTextures[i].height,
					0.0f, Vector2.Zero,
					uiTextures[i].texture, uint.MaxValue,
					u0, v0, u1, v1, false, false,
					uiTextures[i].color,
					Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero);
			}

			uiTextureBatch.end();

			for (int i = 0; i < uiTextureBatch.getNumDrawCalls(); i++)
			{
				//graphics.setDepthTest(DepthTest.None);
				graphics.setBlendState(BlendState.Alpha);

				uiTextureBatch.submitDrawCall(i, uiTextureShader);
			}


			int numCharacters = 0;
			foreach (TextDrawCommand text in texts)
				numCharacters += text.length;

			textBatch.begin(numCharacters);

			for (int i = 0; i < texts.Count; i++)
			{
				unsafe
				{
					int x = texts[i].x;
					int y = texts[i].y;
					float z = texts[i].layer * 0.0001f;
					float scale = texts[i].scale;

					byte* text = texts[i].text;
					string str = texts[i].str;
					int offset = texts[i].offset;
					int length = texts[i].length;

					Font font = texts[i].font;
					uint color = texts[i].color;

					if (text != null)
					{
						graphics.drawText(x, y, z, scale, text, offset, length, font, color, textBatch);
					}
					else if (str != null)
					{
						graphics.drawText(x, y, z, scale, str, offset, length, font, color, textBatch);
					}
				}
			}

			textBatch.end();

			for (int i = 0; i < textBatch.getNumDrawCalls(); i++)
			{
				//graphics.setDepthTest(DepthTest.None);

				graphics.setBlendState(BlendState.Alpha);

				textBatch.submitDrawCall(i, textShader);
			}


			uiTextures.Clear();
			texts.Clear();

			uiDepthCounter = 0;
		}
	}
}
