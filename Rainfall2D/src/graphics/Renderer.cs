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
		public Vector2 position;
		public Vector2 size;
		public Sprite sprite;
		public Vector4 color;
	}

	struct UIDraw
	{
		public Vector2i position;
		public Vector2i size;
		public Sprite sprite;
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

	public static void DrawSprite(float x, float y, float width, float height, Sprite sprite, uint color = 0xFFFFFFFF)
	{
		draws.Add(new SpriteDraw { position = new Vector2(x, y), size = new Vector2(width, height), sprite = sprite, color = MathHelper.ARGBToVector(color) });
	}

	public static void DrawUISprite(int x, int y, int width, int height, Sprite sprite, uint color = 0xFFFFFFFF)
	{
		uiDraws.Add(new UIDraw { position = new Vector2i(x, y), size = new Vector2i(width, height), sprite = sprite, color = color });
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

		spriteBatch.begin(draws.Count);
		for (int i = 0; i < draws.Count; i++)
		{
			SpriteDraw draw = draws[i];
			Texture texture = draw.sprite?.spriteSheet.texture;
			float u0 = draw.sprite != null ? draw.sprite.uv0.x : 0.0f;
			float v0 = draw.sprite != null ? draw.sprite.uv0.y : 0.0f;
			float u1 = draw.sprite != null ? draw.sprite.uv1.x : 0.0f;
			float v1 = draw.sprite != null ? draw.sprite.uv1.y : 0.0f;
			spriteBatch.draw(
				draw.position.x, draw.position.y, 0.0f,
				draw.size.x, draw.size.y,
				0.0f,
				texture, uint.MaxValue,
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
			Texture texture = draw.sprite?.spriteSheet.texture;
			float u0 = draw.sprite != null ? draw.sprite.uv0.x : 0.0f;
			float v0 = draw.sprite != null ? draw.sprite.uv0.y : 0.0f;
			float u1 = draw.sprite != null ? draw.sprite.uv1.x : 0.0f;
			float v1 = draw.sprite != null ? draw.sprite.uv1.y : 0.0f;
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
		uiDraws.Clear();
		textDraws.Clear();
	}
}
