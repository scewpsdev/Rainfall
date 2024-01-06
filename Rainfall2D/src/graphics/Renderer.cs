using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Renderer
{
	struct SpriteDraw
	{
		public Vector3 position;
		public Vector2 size;
		public float rotation;
		public Texture texture;
		public FloatRect rect;
		public Vector4 color;
	}

	struct UIDraw
	{
		public Vector2i position;
		public Vector2i size;
		public Texture texture;
		public FloatRect rect;
		public uint color;
	}

	struct TextDraw
	{
		public Vector2i position;
		public string text;
		public uint color;
	}


	static GraphicsDevice graphics;

	static SpriteBatch spriteBatch;
	static Shader spriteShader;
	static List<SpriteDraw> draws = new List<SpriteDraw>();
	static List<SpriteDraw> verticalDraws = new List<SpriteDraw>();

	static SpriteBatch uiBatch;
	static Shader uiShader;
	static List<UIDraw> uiDraws = new List<UIDraw>();

	static SpriteBatch textBatch;
	static Shader textShader;
	static List<TextDraw> textDraws = new List<TextDraw>();

	static Texture font;

	static Matrix projection, view;


	public static void Init(GraphicsDevice graphics)
	{
		Renderer.graphics = graphics;

		spriteBatch = new SpriteBatch(graphics);
		spriteShader = Resource.GetShader("res/shaders/sprite/sprite.vs.shader", "res/shaders/sprite/sprite.fs.shader");

		uiBatch = new SpriteBatch(graphics);
		uiShader = Resource.GetShader("res/shaders/ui/ui.vs.shader", "res/shaders/ui/ui.fs.shader");

		textBatch = new SpriteBatch(graphics);
		textShader = Resource.GetShader("res/shaders/text/text.vs.shader", "res/shaders/text/text.fs.shader");

		font = Resource.GetTexture("res/sprites/font.png", false);
	}

	public static void DrawSprite(float x, float y, float z, float width, float height, float rotation, Sprite sprite, bool flipped, uint color = 0xFFFFFFFF)
	{
		float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
		if (sprite != null)
		{
			u0 = sprite.uv0.x;
			v0 = sprite.uv0.y;
			u1 = sprite.uv1.x;
			v1 = sprite.uv1.y;
			if (flipped)
			{
				float tmp = u0;
				u0 = u1;
				u1 = tmp;
			}
		}
		FloatRect rect = new FloatRect(u0, v0, u1 - u0, v1 - v0);
		draws.Add(new SpriteDraw { position = new Vector3(x, y, z), size = new Vector2(width, height), rotation = rotation, texture = sprite?.spriteSheet.texture, rect = rect, color = MathHelper.ARGBToVector(color) });
	}

	public static void DrawSprite(float x, float y, float width, float height, Sprite sprite, bool flipped, uint color = 0xFFFFFFFF)
	{
		DrawSprite(x, y, 0.0f, width, height, 0.0f, sprite, flipped, color);
	}

	public static void DrawSprite(float x, float y, float z, float width, float height, Texture texture, int u0, int v0, int w, int h, uint color = 0xFFFFFFFF)
	{
		FloatRect rect = new FloatRect(u0 / (float)texture.width, v0 / (float)texture.height, w / (float)texture.width, h / (float)texture.height);
		draws.Add(new SpriteDraw { position = new Vector3(x, y, z), size = new Vector2(width, height), texture = texture, rect = rect, color = MathHelper.ARGBToVector(color) });
	}

	public static void DrawVerticalSprite(float x, float y, float width, float height, Sprite sprite, bool flipped, uint color = 0xFFFFFFFF)
	{
		float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
		if (sprite != null)
		{
			u0 = sprite.uv0.x;
			v0 = sprite.uv0.y;
			u1 = sprite.uv1.x;
			v1 = sprite.uv1.y;
			if (flipped)
			{
				float tmp = u0;
				u0 = u1;
				u1 = tmp;
			}
		}
		FloatRect rect = new FloatRect(u0, v0, u1 - u0, v1 - v0);
		verticalDraws.Add(new SpriteDraw { position = new Vector3(x, y, 0), size = new Vector2(width, height), texture = sprite?.spriteSheet.texture, rect = rect, color = MathHelper.ARGBToVector(color) });
	}

	public static void DrawVerticalSprite(float x, float y, float z, float width, float height, Texture texture, int u0, int v0, int w, int h, uint color = 0xFFFFFFFF)
	{
		FloatRect rect = new FloatRect(u0 / (float)texture.width, v0 / (float)texture.height, w / (float)texture.width, h / (float)texture.height);
		verticalDraws.Add(new SpriteDraw { position = new Vector3(x, y, z), size = new Vector2(width, height), texture = texture, rect = rect, color = MathHelper.ARGBToVector(color) });
	}

	public static void DrawUISprite(int x, int y, int width, int height, Sprite sprite, bool flipped, uint color = 0xFFFFFFFF)
	{
		float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
		if (sprite != null)
		{
			u0 = sprite.uv0.x;
			v0 = sprite.uv0.y;
			u1 = sprite.uv1.x;
			v1 = sprite.uv1.y;
			if (flipped)
			{
				float tmp = u0;
				u0 = u1;
				u1 = tmp;
			}
		}
		FloatRect rect = new FloatRect(u0, v0, u1 - u0, v1 - v0);
		uiDraws.Add(new UIDraw { position = new Vector2i(x, y), size = new Vector2i(width, height), texture = sprite?.spriteSheet.texture, rect = rect, color = color });
	}

	public static void DrawUISprite(int x, int y, int width, int height, Texture texture, int u0, int v0, int w, int h, uint color = 0xFFFFFFFF)
	{
		FloatRect rect = new FloatRect(u0 / (float)texture.width, v0 / (float)texture.height, w / (float)texture.width, h / (float)texture.height);
		uiDraws.Add(new UIDraw { position = new Vector2i(x, y), size = new Vector2i(width, height), texture = texture, rect = rect, color = color });
	}

	public static void DrawUIText(int x, int y, string text, uint color = 0xFFFFFFFF)
	{
		textDraws.Add(new TextDraw { position = new Vector2i(x, y), text = text, color = color });
	}

	public static void SetCamera(Matrix projection, Matrix view)
	{
		Renderer.projection = projection;
		Renderer.view = view;
	}

	public static void Begin()
	{
	}

	static void GeometryPass()
	{
		graphics.resetState();
		graphics.setPass(0);
		graphics.setRenderTarget(null);

		graphics.setViewTransform(projection, view);

		spriteBatch.begin(draws.Count + verticalDraws.Count);
		for (int i = 0; i < draws.Count; i++)
		{
			SpriteDraw draw = draws[i];
			float u0 = draw.rect.min.x, v0 = draw.rect.min.y, u1 = draw.rect.max.x, v1 = draw.rect.max.y;
			spriteBatch.draw(
				draw.position.x, draw.position.y, draw.position.z,
				draw.size.x, draw.size.y,
				draw.rotation,
				draw.texture, uint.MaxValue,
				u0, v0, u1, v1,
				draw.color);
		}
		for (int i = 0; i < verticalDraws.Count; i++)
		{
			SpriteDraw draw = verticalDraws[i];
			float u0 = draw.rect.min.x, v0 = draw.rect.min.y, u1 = draw.rect.max.x, v1 = draw.rect.max.y;
			spriteBatch.drawVertical(
				draw.position.x, draw.position.y, draw.position.z,
				draw.size.x, draw.size.y,
				0.0f,
				draw.texture, uint.MaxValue,
				u0, v0, u1, v1,
				draw.color);
		}
		spriteBatch.end();

		for (int i = 0; i < spriteBatch.getNumDrawCalls(); i++)
		{
			graphics.setCullState(CullState.ClockWise);
			graphics.setBlendState(BlendState.Alpha);

			spriteBatch.submitDrawCall(i, spriteShader);
		}
	}

	static void UIPass()
	{
		graphics.resetState();
		graphics.setPass(1);
		graphics.setRenderTarget(null);

		graphics.setViewTransform(Matrix.CreateOrthographic(0, Display.width, 0, Display.height, 1.0f, -1.0f), Matrix.Identity);

		uiBatch.begin(uiDraws.Count);
		for (int i = 0; i < uiDraws.Count; i++)
		{
			UIDraw draw = uiDraws[i];
			Texture texture = draw.texture;
			float u0 = draw.rect.min.x;
			float v0 = draw.rect.min.y;
			float u1 = draw.rect.max.x;
			float v1 = draw.rect.max.y;
			uiBatch.draw(
				draw.position.x, Display.height - draw.position.y - draw.size.y, 0.0f,
				draw.size.x, draw.size.y,
				0.0f,
				texture, uint.MaxValue,
				u0, v0, u1, v1,
				MathHelper.ARGBToVector(draw.color));
		}
		uiBatch.end();

		for (int i = 0; i < uiBatch.getNumDrawCalls(); i++)
		{
			graphics.setCullState(CullState.None);
			graphics.setBlendState(BlendState.Alpha);
			graphics.setDepthTest(DepthTest.None);

			uiBatch.submitDrawCall(i, uiShader);
		}

		textBatch.begin(countChars(textDraws));
		for (int i = 0; i < textDraws.Count; i++)
		{
			TextDraw draw = textDraws[i];
			Texture texture = font;
			for (int j = 0; j < draw.text.Length; j++)
			{
				char c = draw.text[j];
				int idx = c - '!';
				float u0 = idx * 8 / (float)font.width;
				float v0 = 0.0f;
				float u1 = (idx + 1) * 8 / (float)font.width;
				float v1 = 1.0f;
				textBatch.draw(
					draw.position.x + j * 8 * 4, Display.height - draw.position.y - 8 * 4, 0.0f,
					8 * 4, 8 * 4,
					0.0f,
					texture, uint.MaxValue,
					u0, v0, u1, v1,
					MathHelper.ARGBToVector(draw.color));
			}
		}
		textBatch.end();

		for (int i = 0; i < textBatch.getNumDrawCalls(); i++)
		{
			graphics.setCullState(CullState.None);
			graphics.setBlendState(BlendState.Alpha);
			graphics.setDepthTest(DepthTest.None);

			textBatch.submitDrawCall(i, textShader);
		}
	}

	static int countChars(List<TextDraw> texts)
	{
		int result = 0;
		foreach (TextDraw draw in texts)
			result += draw.text.Length;
		return result;
	}

	public static void End()
	{
		GeometryPass();
		UIPass();

		draws.Clear();
		verticalDraws.Clear();
		uiDraws.Clear();
		textDraws.Clear();
	}
}
