using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


enum RenderPass
{
	Geometry,
	Lighting,
	Bloom,
	Composite = Bloom + 11,

	Count
}

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
		public int size;
	}

	struct LightDraw
	{
		public Vector2 position;
		public Vector4 color;
		public float radius;
	}


	const int BLOOM_CHAIN_LENGTH = 6;

	static GraphicsDevice graphics;

	static RenderTarget gbuffer;
	static RenderTarget lighting;
	static RenderTarget[] bloomDownsampleChain;
	static RenderTarget[] bloomUpsampleChain;
	static Shader bloomDownsampleShader;
	static Shader bloomUpsampleShader;
	static Shader compositeShader;

	static VertexBuffer quad;
	static Shader lightingShader;
	static List<LightDraw> lightDraws = new List<LightDraw>();

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

	static FontData fontData;
	static Font font;

	static Matrix projection, view;
	static float left, right, bottom, top;


	public static void Init(GraphicsDevice graphics)
	{
		Renderer.graphics = graphics;

		gbuffer = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.RGBA32F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point),
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.D32F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point)
		});
		lighting = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point)
		});

		bloomDownsampleChain = new RenderTarget[BLOOM_CHAIN_LENGTH];
		bloomUpsampleChain = new RenderTarget[BLOOM_CHAIN_LENGTH - 1];
		for (int i = 0; i < BLOOM_CHAIN_LENGTH; i++)
		{
			int exp = (int)Math.Pow(2, i + 1);
			int width = Display.viewportSize.x / exp;
			int height = Display.viewportSize.y / exp;

			bloomDownsampleChain[i] = graphics.createRenderTarget(new RenderTargetAttachment[]
			{
				new RenderTargetAttachment(width, height, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp)
			});

			if (i < BLOOM_CHAIN_LENGTH - 1)
			{
				bloomUpsampleChain[i] = graphics.createRenderTarget(new RenderTargetAttachment[]
				{
					new RenderTargetAttachment(width, height, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp)
				});
			}
		}

		bloomDownsampleShader = Resource.GetShader("res/shaders/bloom/bloom.vs.shader", "res/shaders/bloom/bloom_downsample.fs.shader");
		bloomUpsampleShader = Resource.GetShader("res/shaders/bloom/bloom.vs.shader", "res/shaders/bloom/bloom_upsample.fs.shader");
		compositeShader = Resource.GetShader("res/shaders/composite/composite.vs.shader", "res/shaders/composite/composite.fs.shader");

		quad = graphics.createVertexBuffer(graphics.createVideoMemory(new float[] { -3, -1, 1, -1, 1, 3 }), new VertexElement[]
		{
			new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector2, false)
		});

		lightingShader = Resource.GetShader("res/shaders/lighting/lighting.vs.shader", "res/shaders/lighting/lighting.fs.shader");

		spriteBatch = new SpriteBatch(graphics);
		spriteShader = Resource.GetShader("res/shaders/sprite/sprite.vs.shader", "res/shaders/sprite/sprite.fs.shader");

		uiBatch = new SpriteBatch(graphics);
		uiShader = Resource.GetShader("res/shaders/ui/ui.vs.shader", "res/shaders/ui/ui.fs.shader");

		textBatch = new SpriteBatch(graphics);
		textShader = Resource.GetShader("res/shaders/text/text.vs.shader", "res/shaders/text/text.fs.shader");

		fontData = Resource.GetFontData("res/fonts/dpcomic.ttf");
		font = fontData.createFont(14, false);
	}

	public static void Resize(int width, int height)
	{
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

	public static void DrawVerticalSprite(float x, float y, float z, float width, float height, Sprite sprite, bool flipped, float rotation, uint color = 0xFFFFFFFF)
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
		verticalDraws.Add(new SpriteDraw { position = new Vector3(x, y, z), size = new Vector2(width, height), texture = sprite?.spriteSheet.texture, rect = rect, rotation = rotation, color = MathHelper.ARGBToVector(color) });
	}

	public static void DrawVerticalSprite(float x, float y, float z, float width, float height, Sprite sprite, bool flipped, uint color = 0xFFFFFFFF)
	{
		DrawVerticalSprite(x, y, z, width, height, sprite, flipped, 0.0f, color);
	}

	public static void DrawVerticalSprite(float x, float y, float width, float height, Sprite sprite, bool flipped, uint color = 0xFFFFFFFF)
	{
		DrawVerticalSprite(x, y, 0.0f, width, height, sprite, flipped, color);
	}

	public static void DrawVerticalSprite(float x, float y, float z, float width, float height, Texture texture, int u0, int v0, int w, int h, uint color = 0xFFFFFFFF)
	{
		FloatRect rect = new FloatRect(u0 / (float)texture.width, v0 / (float)texture.height, w / (float)texture.width, h / (float)texture.height);
		verticalDraws.Add(new SpriteDraw { position = new Vector3(x, y, z), size = new Vector2(width, height), texture = texture, rect = rect, color = MathHelper.ARGBToVector(color) });
	}

	public static void DrawLight(Vector2 position, Vector3 color, float radius)
	{
		lightDraws.Add(new LightDraw { position = position, color = new Vector4(color, 1.0f), radius = radius });
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

	public static void DrawUIText(int x, int y, string text, int size, uint color = 0xFFFFFFFF)
	{
		textDraws.Add(new TextDraw { position = new Vector2i(x, y), text = text, size = size, color = color });
	}

	public static int MeasureUIText(string text, int length, int scale)
	{
		return font.measureText(text, length) * scale;
	}

	public static void SetCamera(Matrix projection, Matrix view, float left, float right, float bottom, float top)
	{
		Renderer.projection = projection;
		Renderer.view = view;
		Renderer.left = left;
		Renderer.right = right;
		Renderer.bottom = bottom;
		Renderer.top = top;
	}

	public static void Begin()
	{
	}

	static void GeometryPass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Geometry);
		graphics.setRenderTarget(gbuffer);

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
				draw.rotation,
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

	static void LightingPass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Lighting);

		lightDraws.Sort((LightDraw a, LightDraw b) =>
		{
			Vector2 cameraPos = new Vector2((left + right) * 0.5f, (bottom + top) * 0.5f);
			float d1 = (a.position - cameraPos).length;
			float d2 = (b.position - cameraPos).length;
			return d1 < d2 ? -1 : d1 > d2 ? 1 : 0;
		});

		for (int it = 0; it < (lightDraws.Count + 15) / 16; it++)
		{
			int offset = it * 16;

			graphics.setRenderTarget(lighting);
			graphics.setBlendState(BlendState.Additive);

			graphics.setVertexBuffer(quad);

			graphics.setTexture(lightingShader, "s_frame", 0, gbuffer.getAttachmentTexture(0));

			graphics.setUniform(lightingShader, "u_cameraBounds", new Vector4(left, right, bottom, top));

			Vector4[] lightPositions = new Vector4[16];
			Vector4[] lightColors = new Vector4[16];
			for (int i = 0; i < 16; i++)
			{
				if (offset + i < lightDraws.Count)
				{
					lightPositions[i] = new Vector4(lightDraws[offset + i].position, lightDraws[offset + i].radius, 0.0f);
					lightColors[i] = lightDraws[offset + i].color;
				}
			}
			graphics.setUniform(lightingShader.getUniform("u_lightPositions", UniformType.Vector4, 16), lightPositions);
			graphics.setUniform(lightingShader.getUniform("u_lightColors", UniformType.Vector4, 16), lightColors);

			graphics.draw(lightingShader);
		}
	}

	static void BloomDownsample(int idx, Texture texture, RenderTarget target)
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Bloom + idx);

		graphics.setRenderTarget(target);

		graphics.setTexture(bloomDownsampleShader.getUniform("s_input", UniformType.Sampler), 0, texture);

		graphics.setVertexBuffer(quad);
		graphics.draw(bloomDownsampleShader);
	}

	static void BloomUpsample(int idx, Texture texture0, Texture texture1, RenderTarget target)
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Bloom + 6 + idx);

		graphics.setRenderTarget(target);

		graphics.setTexture(bloomUpsampleShader.getUniform("s_input0", UniformType.Sampler), 0, texture0);
		graphics.setTexture(bloomUpsampleShader.getUniform("s_input1", UniformType.Sampler), 1, texture1);

		graphics.setVertexBuffer(quad);
		graphics.draw(bloomUpsampleShader);
	}

	static void PostProcessingPass()
	{
		BloomDownsample(0, gbuffer.getAttachmentTexture(0), bloomDownsampleChain[0]);
		BloomDownsample(1, bloomDownsampleChain[0].getAttachmentTexture(0), bloomDownsampleChain[1]);
		BloomDownsample(2, bloomDownsampleChain[1].getAttachmentTexture(0), bloomDownsampleChain[2]);
		BloomDownsample(3, bloomDownsampleChain[2].getAttachmentTexture(0), bloomDownsampleChain[3]);
		BloomDownsample(4, bloomDownsampleChain[3].getAttachmentTexture(0), bloomDownsampleChain[4]);
		BloomDownsample(5, bloomDownsampleChain[4].getAttachmentTexture(0), bloomDownsampleChain[5]);

		BloomUpsample(0, bloomDownsampleChain[5].getAttachmentTexture(0), bloomDownsampleChain[4].getAttachmentTexture(0), bloomUpsampleChain[4]);
		BloomUpsample(1, bloomUpsampleChain[4].getAttachmentTexture(0), bloomDownsampleChain[3].getAttachmentTexture(0), bloomUpsampleChain[3]);
		BloomUpsample(2, bloomUpsampleChain[3].getAttachmentTexture(0), bloomDownsampleChain[2].getAttachmentTexture(0), bloomUpsampleChain[2]);
		BloomUpsample(3, bloomUpsampleChain[2].getAttachmentTexture(0), bloomDownsampleChain[1].getAttachmentTexture(0), bloomUpsampleChain[1]);
		BloomUpsample(4, bloomUpsampleChain[1].getAttachmentTexture(0), bloomDownsampleChain[0].getAttachmentTexture(0), bloomUpsampleChain[0]);
	}

	static void CompositePass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Composite);

		graphics.setRenderTarget(null);

		graphics.setDepthTest(DepthTest.None);
		graphics.setCullState(CullState.ClockWise);

		graphics.setTexture(compositeShader.getUniform("s_albedo", UniformType.Sampler), 0, gbuffer.getAttachmentTexture(0));
		graphics.setTexture(compositeShader.getUniform("s_lighting", UniformType.Sampler), 1, lighting.getAttachmentTexture(0));
		graphics.setTexture(compositeShader.getUniform("s_bloom", UniformType.Sampler), 2, bloomUpsampleChain[0].getAttachmentTexture(0));

		Vector3 vignetteColor = new Vector3(0.0f);
		float vignetteFalloff = 0.37f; // default value: 0.37f
		graphics.setUniform(compositeShader, "u_vignetteColor", new Vector4(vignetteColor, vignetteFalloff));

		graphics.setVertexBuffer(quad);
		graphics.draw(compositeShader);
	}

	static void UIPass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Composite);
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
			graphics.drawText(
				draw.position.x, draw.position.y, 0.0f,
				draw.size,
				draw.text, 0, draw.text.Length,
				font, draw.color,
				textBatch);
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
		LightingPass();
		PostProcessingPass();
		CompositePass();
		UIPass();

		draws.Clear();
		verticalDraws.Clear();
		uiDraws.Clear();
		textDraws.Clear();
		lightDraws.Clear();
	}
}
