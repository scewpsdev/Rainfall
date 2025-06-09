using Rainfall;
using Rainfall2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;


enum RenderPass
{
	Geometry,
	Lighting,
	Parallax,
	Composite,
	Bloom,
	UI = Bloom + 11,

	Count
}

public static class Renderer
{
	struct SpriteDraw
	{
		public Vector3 position;
		public float rotation;
		public Vector2 size;
		public bool useTransform;
		public Matrix transform;
		public Texture texture;
		public FloatRect rect;
		public Vector4 color;
		public bool solid;
	}

	struct SpriteExDraw
	{
		public Vector3 vertex0;
		public Vector3 vertex1;
		public Vector3 vertex2;
		public Vector3 vertex3;
		public Texture texture;
		public FloatRect rect;
		public Vector4 color;
		public bool solid;
	}

	struct UIDraw
	{
		public Vector2 position;
		public Vector2 size;
		public float rotation;
		public Texture texture;
		public FloatRect rect;
		public uint color;
		public bool solid;
	}

	struct TextDraw
	{
		public Vector2i position;
		public string text;
		public uint color;
		public int size;
	}

	struct LineDraw
	{
		public Vector3 vertex0;
		public Vector3 vertex1;
		public Vector4 color;
	}

	struct LightDraw
	{
		public Vector2 position;
		public Vector4 color;
		public float radius;
	}


	const int BLOOM_CHAIN_LENGTH = 6;

	public static GraphicsDevice graphics;

	static RenderTarget gbuffer;
	static RenderTarget parallax;
	static RenderTarget lighting;
	static RenderTarget composite;
	static RenderTarget[] bloomDownsampleChain;
	static RenderTarget[] bloomUpsampleChain;
	static Shader bloomDownsampleShader;
	static Shader bloomUpsampleShader;
	static Shader compositeShader;
	static Shader blitShader;

	static VertexBuffer quad;
	static Shader lightingShader;
	static Shader lightMaskShader;
	static List<LightDraw> lightDraws = new List<LightDraw>();

	static SpriteBatch spriteBatch;
	static SpriteBatch additiveBatch;
	static SpriteBatch parallaxBatch;
	static Shader spriteShader;
	static List<SpriteDraw> draws = new List<SpriteDraw>();
	static List<SpriteExDraw> exDraws = new List<SpriteExDraw>();
	static List<SpriteDraw> additiveDraws = new List<SpriteDraw>();
	static List<SpriteExDraw> additiveExDraws = new List<SpriteExDraw>();
	static List<SpriteDraw> verticalDraws = new List<SpriteDraw>();
	static List<SpriteDraw> parallaxDraws = new List<SpriteDraw>();

	static SpriteBatch uiBatch;
	static Shader uiShader;
	static List<UIDraw> uiDraws = new List<UIDraw>();

	static SpriteBatch textBatch;
	static Shader textShader;
	static List<TextDraw> textDraws = new List<TextDraw>();

	static LineRenderer lineRenderer;
	static Shader lineShader;
	static List<LineDraw> lineDraws = new List<LineDraw>();

	static FontData fontData;
	public static Font font;
	public static PixelFont smallFont;

	static Matrix ortho, orthoView, perspective, perspectiveView;
	static float cameraX, cameraY;
	static float left, right, bottom, top;
	static float cameraFractX, cameraFractY;

	public static Vector3 ambientLight = Vector3.Zero;
	public static Texture lightMask = null;
	public static FloatRect lightMaskRect;

	public static Vector3 vignetteColor = new Vector3(0.0f);
	public static float vignetteFalloff = 0.37f; // default value: 0.37f
	public static float bloomStrength = 0.1f;
	public static float bloomFalloff = 4.0f;

	public static bool bloomEnabled = true;
	public static bool vignetteEnabled = true;

	public static int UIHeight;
	public static int UIWidth;


	public static void Init(GraphicsDevice graphics, int width, int height)
	{
		Renderer.graphics = graphics;

		UIWidth = width;
		UIHeight = height;

		gbuffer = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(UIWidth, UIHeight, TextureFormat.RGBA32F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point | (uint)SamplerFlags.Clamp),
			new RenderTargetAttachment(UIWidth, UIHeight, TextureFormat.RGBA8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point | (uint)SamplerFlags.Clamp),
			new RenderTargetAttachment(UIWidth, UIHeight, TextureFormat.D32F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point)
		});
		parallax = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(UIWidth, UIHeight, TextureFormat.RGBA32F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point | (uint)SamplerFlags.Clamp),
			new RenderTargetAttachment(UIWidth, UIHeight, TextureFormat.RGBA8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point | (uint)SamplerFlags.Clamp),
			new RenderTargetAttachment(UIWidth, UIHeight, TextureFormat.D32F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point)
		});
		lighting = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(UIWidth, UIHeight, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point | (uint)SamplerFlags.Clamp),
		});
		composite = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(Display.width, Display.height, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point | (uint)SamplerFlags.Clamp),
		});

		bloomDownsampleChain = new RenderTarget[BLOOM_CHAIN_LENGTH];
		bloomUpsampleChain = new RenderTarget[BLOOM_CHAIN_LENGTH - 1];
		for (int i = 0; i < BLOOM_CHAIN_LENGTH; i++)
		{
			int exp = (int)Math.Pow(2, i + 1);
			int w = Display.width / exp;
			int h = Display.height / exp;

			bloomDownsampleChain[i] = graphics.createRenderTarget(new RenderTargetAttachment[]
			{
				new RenderTargetAttachment(w, h, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp)
			});

			if (i < BLOOM_CHAIN_LENGTH - 1)
			{
				bloomUpsampleChain[i] = graphics.createRenderTarget(new RenderTargetAttachment[]
				{
					new RenderTargetAttachment(w, h, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp)
				});
			}
		}

		bloomDownsampleShader = Resource.GetShader("shaders/bloom/bloom.vsh", "shaders/bloom/bloom_downsample.fsh");
		bloomUpsampleShader = Resource.GetShader("shaders/bloom/bloom.vsh", "shaders/bloom/bloom_upsample.fsh");
		compositeShader = Resource.GetShader("shaders/composite/composite.vsh", "shaders/composite/composite.fsh");
		blitShader = Resource.GetShader("shaders/final/final.vsh", "shaders/final/final.fsh");

		quad = graphics.createVertexBuffer(graphics.createVideoMemory(new float[] { -3, -1, 1, -1, 1, 3 }), new VertexElement[]
		{
			new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector2, false)
		});

		lightingShader = Resource.GetShader("shaders/lighting/lighting.vsh", "shaders/lighting/lighting.fsh");
		lightMaskShader = Resource.GetShader("shaders/lighting/light_mask.vsh", "shaders/lighting/light_mask.fsh");

		spriteBatch = new SpriteBatch(graphics);
		additiveBatch = new SpriteBatch(graphics);
		parallaxBatch = new SpriteBatch(graphics);
		spriteShader = Resource.GetShader("shaders/sprite/sprite.vsh", "shaders/sprite/sprite.fsh");

		uiBatch = new SpriteBatch(graphics);
		uiShader = Resource.GetShader("shaders/ui/ui.vsh", "shaders/ui/ui.fsh");

		textBatch = new SpriteBatch(graphics);
		textShader = Resource.GetShader("shaders/text/text.vsh", "shaders/text/text.fsh");

		lineRenderer = new LineRenderer();
		lineShader = Resource.GetShader("shaders/line/line.vsh", "shaders/line/line.fsh");

		fontData = Resource.GetFontData("fonts/dpcomic.ttf");
		font = fontData.createFont(14, false);

		smallFont = new PixelFont("fonts/font2.png");
	}

	public static void Resize(int width, int height)
	{
		UIWidth = width;
		UIHeight = height;

		if (gbuffer != null)
			graphics.destroyRenderTarget(gbuffer);
		if (parallax != null)
			graphics.destroyRenderTarget(parallax);
		if (lighting != null)
			graphics.destroyRenderTarget(lighting);
		if (composite != null)
			graphics.destroyRenderTarget(composite);
		for (int i = 0; i < bloomDownsampleChain.Length; i++)
		{
			if (bloomDownsampleChain[i] != null)
				graphics.destroyRenderTarget(bloomDownsampleChain[i]);
		}
		Array.Fill(bloomDownsampleChain, null);
		for (int i = 0; i < bloomUpsampleChain.Length; i++)
		{
			if (bloomUpsampleChain[i] != null)
				graphics.destroyRenderTarget(bloomUpsampleChain[i]);
		}
		Array.Fill(bloomUpsampleChain, null);


		gbuffer = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(UIWidth, UIHeight, TextureFormat.RGBA32F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point | (uint)SamplerFlags.Clamp),
			new RenderTargetAttachment(UIWidth, UIHeight, TextureFormat.RGBA8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point | (uint)SamplerFlags.Clamp),
			new RenderTargetAttachment(UIWidth, UIHeight, TextureFormat.D32F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point)
		});
		parallax = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(UIWidth, UIHeight, TextureFormat.RGBA32F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point | (uint)SamplerFlags.Clamp),
			new RenderTargetAttachment(UIWidth, UIHeight, TextureFormat.RGBA8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point | (uint)SamplerFlags.Clamp),
			new RenderTargetAttachment(UIWidth, UIHeight, TextureFormat.D32F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point)
		});
		lighting = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(UIWidth, UIHeight, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point | (uint)SamplerFlags.Clamp),
		});
		composite = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(Display.width, Display.height, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Point | (uint)SamplerFlags.Clamp),
		});


		for (int i = 0; i < BLOOM_CHAIN_LENGTH; i++)
		{
			int exp = (int)Math.Pow(2, i + 1);
			int w = Display.width / exp;
			int h = Display.height / exp;

			bloomDownsampleChain[i] = graphics.createRenderTarget(new RenderTargetAttachment[]
			{
				new RenderTargetAttachment(w, h, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Clamp)
			});

			if (i < BLOOM_CHAIN_LENGTH - 1)
			{
				bloomUpsampleChain[i] = graphics.createRenderTarget(new RenderTargetAttachment[]
				{
					new RenderTargetAttachment(w, h, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.Clamp)
				});
			}
		}
	}

	public static void DrawSprite(float x, float y, float z, float width, float height, float rotation, Sprite sprite, bool flipX, bool flipY, Vector4 color, bool additive)
	{
		float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
		if (sprite != null)
		{
			u0 = sprite.uv0.x;
			v0 = sprite.uv0.y;
			u1 = sprite.uv1.x;
			v1 = sprite.uv1.y;

			if (flipX)
				MathHelper.Swap(ref u0, ref u1);
			if (flipY)
				MathHelper.Swap(ref v0, ref v1);
		}
		FloatRect rect = new FloatRect(u0, v0, u1 - u0, v1 - v0);
		(additive ? additiveDraws : draws).Add(new SpriteDraw { position = new Vector3(x, y, z), size = new Vector2(width, height), rotation = rotation, texture = sprite?.spriteSheet.texture, rect = rect, color = color });
	}

	public static void DrawSprite(float x, float y, float z, float width, float height, float rotation, Sprite sprite, bool flippedX, Vector4 color, bool additive = false)
	{
		float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
		if (sprite != null)
		{
			u0 = sprite.uv0.x;
			v0 = sprite.uv0.y;
			u1 = sprite.uv1.x;
			v1 = sprite.uv1.y;

			if (flippedX)
				MathHelper.Swap(ref u0, ref u1);
		}
		FloatRect rect = new FloatRect(u0, v0, u1 - u0, v1 - v0);
		(additive ? additiveDraws : draws).Add(new SpriteDraw { position = new Vector3(x, y, z), size = new Vector2(width, height), rotation = rotation, texture = sprite?.spriteSheet.texture, rect = rect, color = color });
	}

	public static void DrawSprite(float x, float y, float z, float width, float height, float rotation, Sprite sprite, bool flipped)
	{
		DrawSprite(x, y, z, width, height, rotation, sprite, flipped, Vector4.One);
	}

	public static void DrawSprite(float x, float y, float z, float width, float height, float rotation, Sprite sprite)
	{
		DrawSprite(x, y, z, width, height, rotation, sprite, false, Vector4.One, false);
	}

	public static void DrawSprite(float x, float y, float width, float height, Sprite sprite, bool flipped, Vector4 color)
	{
		DrawSprite(x, y, 0.0f, width, height, 0.0f, sprite, flipped, color);
	}

	public static void DrawSprite(float x, float y, float width, float height, Sprite sprite, bool flipped)
	{
		DrawSprite(x, y, 0.0f, width, height, 0.0f, sprite, flipped, Vector4.One);
	}

	public static void DrawSprite(float x, float y, float width, float height, Sprite sprite)
	{
		DrawSprite(x, y, width, height, sprite, false, Vector4.One);
	}

	public static void DrawSprite(float x, float y, float z, float width, float height, float rotation, Texture texture, int u0, int v0, int w, int h, Vector4 color, bool additive)
	{
		FloatRect rect = texture != null ? new FloatRect(u0 / (float)texture.width, v0 / (float)texture.height, w / (float)texture.width, h / (float)texture.height) : new FloatRect(0, 0, 0, 0);
		(additive ? additiveDraws : draws).Add(new SpriteDraw { position = new Vector3(x, y, z), size = new Vector2(width, height), rotation = rotation, texture = texture, rect = rect, color = color });
	}

	public static void DrawSprite(float x, float y, float z, float width, float height, Texture texture, int u0, int v0, int w, int h, Vector4 color)
	{
		FloatRect rect = texture != null ? new FloatRect(u0 / (float)texture.width, v0 / (float)texture.height, w / (float)texture.width, h / (float)texture.height) : new FloatRect(0, 0, 0, 0);
		draws.Add(new SpriteDraw { position = new Vector3(x, y, z), size = new Vector2(width, height), texture = texture, rect = rect, color = color });
	}

	public static void DrawSprite(float width, float height, Matrix transform, Texture texture, int u0, int v0, int w, int h, Vector4 color, bool additive = false)
	{
		FloatRect rect = texture != null ? new FloatRect(u0 / (float)texture.width, v0 / (float)texture.height, w / (float)texture.width, h / (float)texture.height) : new FloatRect(0, 0, 0, 0);
		(additive ? additiveDraws : draws).Add(new SpriteDraw { useTransform = true, transform = transform, size = new Vector2(width, height), texture = texture, rect = rect, color = color });
	}

	public static void DrawSprite(float width, float height, Matrix transform, Sprite sprite, uint color = 0xFFFFFFFF)
	{
		DrawSprite(width, height, transform, sprite.spriteSheet.texture, sprite.position.x, sprite.position.y, sprite.size.x, sprite.size.y, color);
	}

	public static void DrawSpriteSolid(float x, float y, float z, float width, float height, float rotation, Sprite sprite, bool flipped, Vector4 color, bool additive = false)
	{
		float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
		if (sprite != null)
		{
			u0 = sprite.uv0.x;
			v0 = sprite.uv0.y;
			u1 = sprite.uv1.x;
			v1 = sprite.uv1.y;

			if (flipped)
				MathHelper.Swap(ref u0, ref u1);
		}
		FloatRect rect = new FloatRect(u0, v0, u1 - u0, v1 - v0);
		(additive ? additiveDraws : draws).Add(new SpriteDraw { position = new Vector3(x, y, z), size = new Vector2(width, height), rotation = rotation, texture = sprite?.spriteSheet.texture, rect = rect, color = color, solid = true });
	}

	public static void DrawSpriteSolid(float width, float height, Matrix transform, Sprite sprite, bool flipped, Vector4 color, bool additive = false)
	{
		float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
		if (sprite != null)
		{
			u0 = sprite.uv0.x;
			v0 = sprite.uv0.y;
			u1 = sprite.uv1.x;
			v1 = sprite.uv1.y;

			if (flipped)
				MathHelper.Swap(ref u0, ref u1);
		}
		FloatRect rect = new FloatRect(u0, v0, u1 - u0, v1 - v0);
		(additive ? additiveDraws : draws).Add(new SpriteDraw { useTransform = true, transform = transform, size = new Vector2(width, height), texture = sprite?.spriteSheet.texture, rect = rect, color = color, solid = true });
	}

	public static void DrawOutline(float x, float y, float z, float width, float height, float rotation, Sprite sprite, bool flipped, uint color)
	{
		DrawSpriteSolid(x - 1.0f / 16.0f, y, z + 0.00001f, width, height, rotation, sprite, flipped, color);
		DrawSpriteSolid(x + 1.0f / 16.0f, y, z + 0.00001f, width, height, rotation, sprite, flipped, color);
		DrawSpriteSolid(x, y - 1.0f / 16.0f, z + 0.00001f, width, height, rotation, sprite, flipped, color);
		DrawSpriteSolid(x, y + 1.0f / 16.0f, z + 0.00001f, width, height, rotation, sprite, flipped, color);
	}

	public static void DrawOutline(float x, float y, float width, float height, Sprite sprite, bool flipped = false, uint color = 0xFFFFFFFF)
	{
		DrawOutline(x, y, 0.00001f, width, height, 0, sprite, flipped, color);
	}

	public static void DrawOutline(float width, float height, Matrix transform, Sprite sprite, bool flipped, uint color)
	{
		DrawSpriteSolid(width, height, Matrix.CreateTranslation(-1.0f / 16, 0, 0.00001f) * transform, sprite, flipped, color);
		DrawSpriteSolid(width, height, Matrix.CreateTranslation(1.0f / 16, 0, 0.00001f) * transform, sprite, flipped, color);
		DrawSpriteSolid(width, height, Matrix.CreateTranslation(0, -1.0f / 16, 0.00001f) * transform, sprite, flipped, color);
		DrawSpriteSolid(width, height, Matrix.CreateTranslation(0, 1.0f / 16, 0.00001f) * transform, sprite, flipped, color);
	}

	public static void DrawSpriteEx(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Sprite sprite, bool flipped, Vector4 color, bool additive = false, bool solid = false)
	{
		float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
		if (sprite != null)
		{
			u0 = sprite.uv0.x;
			v0 = sprite.uv0.y;
			u1 = sprite.uv1.x;
			v1 = sprite.uv1.y;

			if (flipped)
				MathHelper.Swap(ref u0, ref u1);
		}
		FloatRect rect = new FloatRect(u0, v0, u1 - u0, v1 - v0);
		(additive ? additiveExDraws : exDraws).Add(new SpriteExDraw
		{
			vertex0 = vertex0,
			vertex1 = vertex1,
			vertex2 = vertex2,
			vertex3 = vertex3,
			texture = sprite != null ? sprite.spriteSheet.texture : null,
			rect = rect,
			color = color,
			solid = solid
		});
	}

	public static void DrawSpriteEx(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Texture texture, int u0, int v0, int w, int h, Vector4 color, bool additive = false, bool solid = false)
	{
		FloatRect rect = texture != null ? new FloatRect(u0 / (float)texture.width, v0 / (float)texture.height, w / (float)texture.width, h / (float)texture.height) : new FloatRect(0, 0, 0, 0);
		(additive ? additiveExDraws : exDraws).Add(new SpriteExDraw
		{
			vertex0 = vertex0,
			vertex1 = vertex1,
			vertex2 = vertex2,
			vertex3 = vertex3,
			texture = texture,
			rect = rect,
			color = color,
			solid = solid
		});
	}

	public static void DrawVerticalSprite(float x, float y, float z, float width, float height, Sprite sprite, bool flipped, Vector4 color, bool additive = false)
	{
		DrawSpriteEx(new Vector3(x, y, z), new Vector3(x + width, y, z), new Vector3(x + width, y + height, z + height), new Vector3(x, y + height, z + height), sprite, flipped, color, additive);
	}

	public static void DrawVerticalSpriteSolid(float x, float y, float z, float width, float height, Sprite sprite, bool flipped, Vector4 color)
	{
		DrawSpriteEx(new Vector3(x, y, z), new Vector3(x + width, y, z), new Vector3(x + width, y + height, z + height), new Vector3(x, y + height, z + height), sprite, flipped, color, false, true);
	}

	public static void DrawVerticalSpriteSolid(float x, float y, float z, float width, float height, float rotation, Sprite sprite, bool flipped, Vector4 color)
	{
		Vector3 vertex0 = new Vector3(x + 0.5f * width, y, z + 0.5f * height);
		Vector3 vertex1 = new Vector3(x + 0.5f * width, y, z + 0.5f * height);
		Vector3 vertex2 = new Vector3(x + 0.5f * width, y, z + 0.5f * height);
		Vector3 vertex3 = new Vector3(x + 0.5f * width, y, z + 0.5f * height);

		vertex0.xz += Vector2.Rotate(new Vector2(-0.5f * width, -0.5f * height), rotation);
		vertex1.xz += Vector2.Rotate(new Vector2(0.5f * width, -0.5f * height), rotation);
		vertex2.xz += Vector2.Rotate(new Vector2(0.5f * width, 0.5f * height), rotation);
		vertex3.xz += Vector2.Rotate(new Vector2(-0.5f * width, 0.5f * height), rotation);

		vertex0.y += vertex0.z;
		vertex1.y += vertex1.z;
		vertex2.y += vertex2.z;
		vertex3.y += vertex3.z;

		DrawSpriteEx(vertex0, vertex1, vertex2, vertex3, sprite, flipped, color, false, true);
	}

	public static void DrawVerticalSprite(float x, float y, float z, float width, float height, Sprite sprite, bool flipped, float rotation, Vector4 color)
	{
		Vector3 vertex0 = new Vector3(x + 0.5f * width, y, z + 0.5f * height);
		Vector3 vertex1 = new Vector3(x + 0.5f * width, y, z + 0.5f * height);
		Vector3 vertex2 = new Vector3(x + 0.5f * width, y, z + 0.5f * height);
		Vector3 vertex3 = new Vector3(x + 0.5f * width, y, z + 0.5f * height);

		vertex0.xz += Vector2.Rotate(new Vector2(-0.5f * width, -0.5f * height), rotation);
		vertex1.xz += Vector2.Rotate(new Vector2(0.5f * width, -0.5f * height), rotation);
		vertex2.xz += Vector2.Rotate(new Vector2(0.5f * width, 0.5f * height), rotation);
		vertex3.xz += Vector2.Rotate(new Vector2(-0.5f * width, 0.5f * height), rotation);

		vertex0.y += vertex0.z;
		vertex1.y += vertex1.z;
		vertex2.y += vertex2.z;
		vertex3.y += vertex3.z;

		DrawSpriteEx(vertex0, vertex1, vertex2, vertex3, sprite, flipped, color);
	}

	/*
	public static void DrawVerticalSprite(float x, float y, float z, float width, float height, Sprite sprite, bool flipped, uint color = 0xFFFFFFFF)
	{
		DrawVerticalSprite(x, y, z, width, height, sprite, flipped, 0.0f, color);
	}
	*/

	public static void DrawVerticalSprite(float x, float y, float width, float height, Sprite sprite, bool flipped = false, uint color = 0xFFFFFFFF)
	{
		DrawVerticalSprite(x, y, 0.0f, width, height, sprite, flipped, MathHelper.ARGBToVector(color));
	}

	/*
	public static void DrawVerticalSprite(float x, float y, float z, float width, float height, Texture texture, int u0, int v0, int w, int h, uint color = 0xFFFFFFFF)
	{
		FloatRect rect = new FloatRect(u0 / (float)texture.width, v0 / (float)texture.height, w / (float)texture.width, h / (float)texture.height);
		verticalDraws.Add(new SpriteDraw { position = new Vector3(x, y, z), size = new Vector2(width, height), texture = texture, rect = rect, color = MathHelper.ARGBToVector(color) });
	}
	*/

	public static void DrawVerticalSprite(float x, float y, float z, float width, float height, float rotation, Texture texture, int u0, int v0, int w, int h, Vector4 color, bool additive)
	{
		Vector3 vertex0 = new Vector3(x + 0.5f * width, y, z + 0.5f * height);
		Vector3 vertex1 = new Vector3(x + 0.5f * width, y, z + 0.5f * height);
		Vector3 vertex2 = new Vector3(x + 0.5f * width, y, z + 0.5f * height);
		Vector3 vertex3 = new Vector3(x + 0.5f * width, y, z + 0.5f * height);

		vertex0.xz += Vector2.Rotate(new Vector2(-0.5f * width, -0.5f * height), rotation);
		vertex1.xz += Vector2.Rotate(new Vector2(0.5f * width, -0.5f * height), rotation);
		vertex2.xz += Vector2.Rotate(new Vector2(0.5f * width, 0.5f * height), rotation);
		vertex3.xz += Vector2.Rotate(new Vector2(-0.5f * width, 0.5f * height), rotation);

		vertex0.y += vertex0.z;
		vertex1.y += vertex1.z;
		vertex2.y += vertex2.z;
		vertex3.y += vertex3.z;

		DrawSpriteEx(vertex0, vertex1, vertex2, vertex3, texture, u0, v0, w, h, color, additive);
	}

	public static void DrawVerticalSprite(float x, float y, float z, float width, float height, float rotation, Texture texture, int u0, int v0, int w, int h, uint color = 0xFFFFFFFF)
	{
		FloatRect rect = new FloatRect(u0 / (float)texture.width, v0 / (float)texture.height, w / (float)texture.width, h / (float)texture.height);
		verticalDraws.Add(new SpriteDraw { position = new Vector3(x, y, z), size = new Vector2(width, height), texture = texture, rect = rect, rotation = rotation, color = MathHelper.ARGBToVector(color) });
	}

	public static void DrawVerticalOutline(float x, float y, float z, float width, float height, float rotation, Sprite sprite, bool flipped, uint color)
	{
		DrawVerticalSpriteSolid(x - 1.0f / 16.0f, y + 0.001f, z, width, height, rotation, sprite, flipped, color);
		DrawVerticalSpriteSolid(x + 1.0f / 16.0f, y + 0.001f, z, width, height, rotation, sprite, flipped, color);
		DrawVerticalSpriteSolid(x, y + 0.001f, z - 1.0f / 16.0f, width, height, rotation, sprite, flipped, color);
		DrawVerticalSpriteSolid(x, y + 0.001f, z + 1.0f / 16.0f, width, height, rotation, sprite, flipped, color);
	}

	public static void DrawLine(Vector3 vertex0, Vector3 vertex1, Vector4 color)
	{
		lineDraws.Add(new LineDraw { vertex0 = vertex0, vertex1 = vertex1, color = color });
	}

	public static void DrawLight(Vector2 position, Vector3 color, float radius)
	{
		lightDraws.Add(new LightDraw { position = position, color = new Vector4(color, 1.0f), radius = radius });
	}

	public static void DrawParallaxSprite(float x, float y, float z, float width, float height, float rotation, Sprite sprite, Vector4 color)
	{
		float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
		if (sprite != null)
		{
			u0 = sprite.uv0.x;
			v0 = sprite.uv0.y;
			u1 = sprite.uv1.x;
			v1 = sprite.uv1.y;
		}
		FloatRect rect = new FloatRect(u0, v0, u1 - u0, v1 - v0);
		parallaxDraws.Add(new SpriteDraw { position = new Vector3(x, y, z), size = new Vector2(width, height), rotation = rotation, texture = sprite?.spriteSheet.texture, rect = rect, color = color });
	}

	public static void DrawParallaxSprite(float x, float y, float z, float width, float height, float rotation, Texture texture, int u0, int v0, int w, int h, Vector4 color)
	{
		FloatRect rect = texture != null ? new FloatRect(u0 / (float)texture.width, v0 / (float)texture.height, w / (float)texture.width, h / (float)texture.height) : new FloatRect(0, 0, 0, 0);
		parallaxDraws.Add(new SpriteDraw { position = new Vector3(x, y, z), size = new Vector2(width, height), rotation = rotation, texture = texture, rect = rect, color = color });
	}

	public static void DrawParallaxSolid(float x, float y, float z, float width, float height, float rotation, Sprite sprite, Vector4 color)
	{
		float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
		if (sprite != null)
		{
			u0 = sprite.uv0.x;
			v0 = sprite.uv0.y;
			u1 = sprite.uv1.x;
			v1 = sprite.uv1.y;
		}
		FloatRect rect = new FloatRect(u0, v0, u1 - u0, v1 - v0);
		parallaxDraws.Add(new SpriteDraw { position = new Vector3(x, y, z), size = new Vector2(width, height), rotation = rotation, texture = sprite?.spriteSheet.texture, rect = rect, color = color, solid = true });
	}

	public static void DrawParallaxOutline(float x, float y, float z, float width, float height, float rotation, Sprite sprite, Vector4 color)
	{
		float pixelSize = (10 - z) * 0.1f / 16;

		DrawParallaxSolid(x - pixelSize, y, z, width, height, rotation, sprite, color);
		DrawParallaxSolid(x + pixelSize, y, z, width, height, rotation, sprite, color);
		DrawParallaxSolid(x, y - pixelSize, z, width, height, rotation, sprite, color);
		DrawParallaxSolid(x, y + pixelSize, z, width, height, rotation, sprite, color);
	}

	public static void DrawUISprite(float x, float y, int width, int height, Sprite sprite, bool flipped = false, uint color = 0xFFFFFFFF)
	{
		float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
		if (sprite != null)
		{
			u0 = sprite.uv0.x;
			v0 = sprite.uv0.y;
			u1 = sprite.uv1.x;
			v1 = sprite.uv1.y;

			if (flipped)
				MathHelper.Swap(ref u0, ref u1);
		}
		FloatRect rect = new FloatRect(u0, v0, u1 - u0, v1 - v0);
		uiDraws.Add(new UIDraw { position = new Vector2(x, y), size = new Vector2(width, height), texture = sprite?.spriteSheet.texture, rect = rect, color = color });
	}

	public static void DrawUISprite(float x, float y, int width, int height, float rotation, Sprite sprite, uint color = 0xFFFFFFFF)
	{
		float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
		if (sprite != null)
		{
			u0 = sprite.uv0.x;
			v0 = sprite.uv0.y;
			u1 = sprite.uv1.x;
			v1 = sprite.uv1.y;
		}
		FloatRect rect = new FloatRect(u0, v0, u1 - u0, v1 - v0);
		uiDraws.Add(new UIDraw { position = new Vector2(x, y), size = new Vector2(width, height), rotation = rotation, texture = sprite?.spriteSheet.texture, rect = rect, color = color });
	}

	public static void DrawUISprite(float x, float y, int width, int height, Texture texture, int u0, int v0, int w, int h, uint color = 0xFFFFFFFF)
	{
		FloatRect rect = texture != null ? new FloatRect(u0 / (float)texture.width, v0 / (float)texture.height, w / (float)texture.width, h / (float)texture.height) : new FloatRect(0, 0, 0, 0);
		uiDraws.Add(new UIDraw { position = new Vector2(x, y), size = new Vector2(width, height), texture = texture, rect = rect, color = color });
	}

	public static void DrawUISpriteCutout(int x, int y, Sprite sprite, int width, int height, int uoffset = 0, int voffset = 0, uint color = 0xFFFFFFFF)
	{
		float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
		if (sprite != null)
		{
			uoffset = Math.Min(uoffset, (sprite.width - width) / 2);
			voffset = Math.Min(voffset, (sprite.height - height) / 2);

			u0 = sprite.uv0.x + ((sprite.width - width) / 2 + uoffset) / (float)sprite.spriteSheet.texture.width;
			v0 = sprite.uv0.y + ((sprite.height - height) / 2 + voffset) / (float)sprite.spriteSheet.texture.height;
			u1 = sprite.uv1.x - ((sprite.width - width) / 2 - uoffset) / (float)sprite.spriteSheet.texture.width;
			v1 = sprite.uv1.y - ((sprite.height - height) / 2 - voffset) / (float)sprite.spriteSheet.texture.height;
		}
		FloatRect rect = new FloatRect(u0, v0, u1 - u0, v1 - v0);
		uiDraws.Add(new UIDraw { position = new Vector2(x, y), size = new Vector2(width, height), texture = sprite?.spriteSheet.texture, rect = rect, color = color });
	}

	public static void DrawUISpriteSolid(float x, float y, int width, int height, Sprite sprite, bool flipped = false, uint color = 0xFFFFFFFF)
	{
		float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
		if (sprite != null)
		{
			u0 = sprite.uv0.x;
			v0 = sprite.uv0.y;
			u1 = sprite.uv1.x;
			v1 = sprite.uv1.y;

			if (flipped)
				MathHelper.Swap(ref u0, ref u1);
		}
		FloatRect rect = new FloatRect(u0, v0, u1 - u0, v1 - v0);
		uiDraws.Add(new UIDraw { position = new Vector2(x, y), size = new Vector2(width, height), texture = sprite?.spriteSheet.texture, rect = rect, color = color, solid = true });
	}

	public static void DrawUISpriteSolid(int x, int y, int width, int height, Texture texture, int u0, int v0, int w, int h, uint color = 0xFFFFFFFF)
	{
		FloatRect rect = texture != null ? new FloatRect(u0 / (float)texture.width, v0 / (float)texture.height, w / (float)texture.width, h / (float)texture.height) : new FloatRect(0, 0, 0, 0);
		uiDraws.Add(new UIDraw { position = new Vector2(x, y), size = new Vector2(width, height), texture = texture, rect = rect, color = color, solid = true });
	}

	public static void DrawUIOutline(float x, float y, int width, int height, Sprite sprite, bool flipped, uint color)
	{
		DrawUISpriteSolid(x - 1, y, width, height, sprite, flipped, color);
		DrawUISpriteSolid(x + 1, y, width, height, sprite, flipped, color);
		DrawUISpriteSolid(x, y - 1, width, height, sprite, flipped, color);
		DrawUISpriteSolid(x, y + 1, width, height, sprite, flipped, color);
	}

	public static void DrawUIText(int x, int y, string text, int size = 1, uint color = 0xFFFFFFFF)
	{
		textDraws.Add(new TextDraw { position = new Vector2i(x, y), text = text, size = size, color = color });
	}

	public static Vector2i MeasureUIText(string text, int length = -1, int scale = 1)
	{
		return new Vector2i(font.measureText(text, length != -1 ? length : text.Length) * scale, (int)(font.size * scale));
	}

	public static int DrawUITextBMP(float x, float y, string text, int size = 1, uint color = 0xFFFFFFFF)
	{
		int cursor = 0;
		for (int i = 0; i < text.Length; i++)
		{
			IntRect rect = smallFont.getCharacterRect(text[i]);
			if (rect == null)
				rect = smallFont.getCharacterRect('?');

			uiDraws.Add(new UIDraw { position = new Vector2(x + cursor * size, y), size = new Vector2(rect.size.x * size, rect.size.y * size), texture = smallFont.texture, rect = new FloatRect(rect.position / (Vector2)smallFont.texture.size.xy, rect.size / (Vector2)smallFont.texture.size.xy), color = color });

			cursor += rect.size.x;
		}
		return cursor;
	}

	public static int DrawUITextBMPFormatted(float x, float y, string text, int size = 1, uint defaultColor = 0xFFFFFFFF)
	{
		uint color = defaultColor;

		int cursor = 0;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (i < text.Length - 2 && c == '\\' && text[i + 1] == 'x')
			{
				i += 2;
				int end = text.IndexOf('\\', i);
				string content = text.Substring(i, end - i);
				if (content.Length == 10 && content.StartsWith("0x"))
					color = Convert.ToUInt32(content, 16);
				else if (content.Length == 1 && content == "0")
					color = defaultColor;
				i = end;
				continue;
			}

			IntRect rect = smallFont.getCharacterRect(text[i]);
			if (rect == null)
				rect = smallFont.getCharacterRect('?');

			uiDraws.Add(new UIDraw { position = new Vector2(x + cursor * size, y), size = new Vector2(rect.size.x * size, rect.size.y * size), texture = smallFont.texture, rect = new FloatRect(rect.position / (Vector2)smallFont.texture.size.xy, rect.size / (Vector2)smallFont.texture.size.xy), color = color });

			cursor += rect.size.x;
		}
		return cursor;
	}

	public static int DrawUITextBMP(float x, float y, char c, bool flippedX = false, bool flippedY = false, int size = 1, uint color = 0xFFFFFFFF)
	{
		IntRect rect = smallFont.getCharacterRect(c);
		if (rect == null)
			rect = smallFont.getCharacterRect('?');

		int w = rect.size.x;
		int h = rect.size.y;

		if (flippedX)
		{
			rect.position.x += rect.size.x;
			rect.size.x *= -1;
		}
		if (flippedY)
		{
			rect.position.y += rect.size.y;
			rect.size.y *= -1;
		}

		uiDraws.Add(new UIDraw { position = new Vector2(x, y), size = new Vector2(w * size, h * size), texture = smallFont.texture, rect = new FloatRect(rect.position / (Vector2)smallFont.texture.size.xy, rect.size / (Vector2)smallFont.texture.size.xy), color = color });

		return rect.size.x;
	}

	static string RemoveFormatCharacters(string str)
	{
		StringBuilder result = new StringBuilder();
		for (int i = 0; i < str.Length; i++)
		{
			char c = str[i];
			if (i < str.Length - 2 && c == '\\' && str[i + 1] == 'x')
			{
				i += 2;
				int end = str.IndexOf('\\', i);
				if (end == -1)
					return null;
				i = end;
				continue;
			}
			else
			{
				result.Append(c);
			}
		}
		return result.ToString();
	}

	public static Vector2i MeasureUITextBMP(string text, int length = -1, int scale = 1)
	{
		text = RemoveFormatCharacters(text);
		return new Vector2i(smallFont.measureText(text, length != -1 ? length : text.Length) * scale, smallFont.size * scale);
	}

	public static Vector2i MeasureUITextBMP(char c)
	{
		return smallFont.getCharacterRect(c).size;
	}

	public static Vector2i size
	{
		get => new Vector2i(UIWidth, UIHeight);
	}

	public static Vector2i cursorPosition
	{
		get => Input.cursorPosition * new Vector2i(UIWidth, UIHeight) / Display.viewportSize;
	}

	public static Vector2i cursorMove
	{
		get
		{
			Vector2i lastCursorPos = new Vector2i(Input.mouseLast.x, Input.mouseLast.y) * new Vector2i(UIWidth, UIHeight) / Display.viewportSize;
			return cursorPosition - lastCursorPos;
		}
	}

	public static bool IsHovered(float x, float y, int width, int height)
	{
		return cursorPosition.x >= x && cursorPosition.x < x + width && cursorPosition.y >= y && cursorPosition.y < y + height;
	}

	public static void DrawWorldTextBMP(float x, float y, float z, string text, float scale, uint color = 0xFFFFFFFF)
	{
		float cursor = 0;
		for (int i = 0; i < text.Length; i++)
		{
			IntRect rect = smallFont.getCharacterRect(text[i]);
			if (rect == null)
				rect = smallFont.getCharacterRect('?');

			FloatRect frect = new FloatRect(rect.position / (Vector2)smallFont.texture.size.xy, rect.size / (Vector2)smallFont.texture.size.xy);
			draws.Add(new SpriteDraw { position = new Vector3(x + cursor + 0.001f, y, z), size = rect.size * scale, texture = smallFont.texture, rect = frect, color = MathHelper.ARGBToVector(color) });

			cursor += rect.size.x * scale;
		}
	}

	public static void DrawWorldTextBMPVertical(float x, float y, float z, string text, float scale, uint color = 0xFFFFFFFF)
	{
		float cursor = 0;
		for (int i = 0; i < text.Length; i++)
		{
			IntRect rect = smallFont.getCharacterRect(text[i]);
			if (rect == null)
				rect = smallFont.getCharacterRect('?');

			DrawVerticalSprite(x + cursor + 0.001f, y, z, rect.size.x * scale, rect.size.y * scale, 0, smallFont.texture, rect.position.x, rect.position.y, rect.size.x, rect.size.y, MathHelper.ARGBToVector(color), false);

			cursor += rect.size.x * scale;
		}
	}

	public static Vector2 MeasureWorldTextBMP(string text, int length = -1, float scale = 1)
	{
		return new Vector2(smallFont.measureText(text, length != -1 ? length : text.Length) * scale, smallFont.size * scale);
	}

	public static string[] SplitMultilineText(string txt, int maxWidth)
	{
		int spaceWidth = MeasureUITextBMP(" ").x;

		string[] words = txt.Split(' ');

		List<string> lines = new List<string>();
		int currentLineWidth = 0;
		StringBuilder currentLine = new StringBuilder();

		for (int i = 0; i < words.Length; i++)
		{
			string word = words[i];
			int wordWidth = MeasureUITextBMP(RemoveFormatCharacters(word)).x;
			if (currentLineWidth + spaceWidth + wordWidth > maxWidth)
			{
				lines.Add(currentLine.ToString());
				currentLineWidth = 0;
				currentLine.Clear();
			}
			if (i == words.Length - 1)
			{
				if (currentLineWidth > 0)
					currentLine.Append(' ');
				currentLine.Append(word);
				lines.Add(currentLine.ToString());
			}
			else
			{
				if (currentLineWidth > 0)
				{
					currentLine.Append(' ');
					currentLineWidth += spaceWidth;
				}
				currentLine.Append(word);
				currentLineWidth += wordWidth;
			}
		}

		return lines.ToArray();
	}

	const float perspectiveDistance = 10;
	public static void SetCamera(Vector2 position, float width, float height)
	{
		float fov = MathF.Atan2(0.5f * height, perspectiveDistance) * 2;
		Matrix perspective = Matrix.CreatePerspective(fov, Display.aspectRatio, 0.1f, 500);
		Matrix ortho = Matrix.CreateOrthographic(width, height, 1, -1);
		Matrix transform = Matrix.CreateTranslation(new Vector3(position, 0));

		float pixelx = transform.m30 * 16;
		float snappedx = MathF.Floor(pixelx);
		float cameraFractX = pixelx - snappedx;
		transform.m30 = snappedx / 16.0f;

		float pixely = transform.m31 * 16;
		float snappedy = MathF.Floor(pixely);
		float cameraFractY = pixely - snappedy;
		transform.m31 = snappedy / 16.0f;

		// offset by a quarter pixel so that sprites rendered exactly at integer coordinates dont glitch out
		transform.m30 -= 0.25f / 16;
		transform.m31 -= 0.25f / 16;

		Matrix orthoView = transform.inverted;

		transform.m32 = perspectiveDistance;
		Matrix perspectiveView = transform.inverted;

		Renderer.ortho = ortho;
		Renderer.perspective = perspective;
		Renderer.orthoView = orthoView;
		Renderer.perspectiveView = perspectiveView;
		Renderer.cameraX = position.x;
		Renderer.cameraY = position.y;
		Renderer.left = position.x - 0.5f * width;
		Renderer.right = position.x + 0.5f * width;
		Renderer.bottom = position.y - 0.5f * height;
		Renderer.top = position.y + 0.5f * height;
		Renderer.cameraFractX = cameraFractX;
		Renderer.cameraFractY = cameraFractY;
	}

	public static void Begin()
	{
	}

	static void GeometryPass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Geometry);
		graphics.setViewMode(ViewMode.Sequential);
		graphics.setRenderTarget(gbuffer);

		graphics.setViewTransform(ortho, orthoView);

		spriteBatch.begin(draws.Count + exDraws.Count + verticalDraws.Count);
		for (int i = 0; i < draws.Count; i++)
		{
			SpriteDraw draw = draws[i];
			float u0 = draw.rect.min.x, v0 = draw.rect.min.y, u1 = draw.rect.max.x, v1 = draw.rect.max.y;

			u0 += 0.00001f;
			v0 += 0.00001f;
			u1 -= 0.00001f;
			v1 -= 0.00001f;

			if (draw.useTransform)
			{
				// pixel perfect correction
				draw.transform.m30 = MathF.Round(draw.transform.m30 * 16) / 16;
				draw.transform.m31 = MathF.Round(draw.transform.m31 * 16) / 16;

				spriteBatch.draw(
					draw.size.x, draw.size.y, 0.0f - i * 0.0000001f,
					draw.transform,
					draw.texture, uint.MaxValue,
					u0, v0, u1, v1,
					draw.color, draw.solid ? 0.0f : 1.0f);
			}
			else
			{
				// pixel perfect correction
				draw.position.x = MathF.Round(draw.position.x * 16) / 16;
				draw.position.y = MathF.Round(draw.position.y * 16) / 16;

				spriteBatch.draw(
					draw.position.x, draw.position.y, draw.position.z - i * 0.0000001f,
					draw.size.x, draw.size.y,
					draw.rotation,
					draw.texture, uint.MaxValue,
					u0, v0, u1, v1,
					draw.color, draw.solid ? 0.0f : 1.0f);
			}
		}
		for (int i = 0; i < exDraws.Count; i++)
		{
			SpriteExDraw draw = exDraws[i];
			float u0 = draw.rect.min.x, v0 = draw.rect.min.y, u1 = draw.rect.max.x, v1 = draw.rect.max.y;
			spriteBatch.draw(draw.vertex0, draw.vertex1, draw.vertex2, draw.vertex3, draw.texture, uint.MaxValue, u0, v0, u1, v1, draw.color, draw.solid ? 0.0f : 1.0f);
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
			graphics.setCullState(CullState.None);
			graphics.setBlendState(BlendState.Alpha);

			spriteBatch.submitDrawCall(i, spriteShader);
		}

		additiveBatch.begin(additiveDraws.Count + additiveExDraws.Count);
		for (int i = 0; i < additiveDraws.Count; i++)
		{
			SpriteDraw draw = additiveDraws[i];
			float u0 = draw.rect.min.x, v0 = draw.rect.min.y, u1 = draw.rect.max.x, v1 = draw.rect.max.y;
			if (draw.useTransform)
			{
				// pixel perfect correction
				draw.transform.m30 = MathF.Round(draw.transform.m30 * 16) / 16;
				draw.transform.m31 = MathF.Round(draw.transform.m31 * 16) / 16;

				additiveBatch.draw(
					draw.size.x, draw.size.y, 0.0f - i / (float)additiveDraws.Count * 0.0001f,
					draw.transform,
					draw.texture, uint.MaxValue,
					u0, v0, u1, v1,
					draw.color, draw.solid ? 0.0f : 1.0f, true);
			}
			else
			{
				// pixel perfect correction
				draw.position.x = MathF.Round(draw.position.x * 16) / 16;
				draw.position.y = MathF.Round(draw.position.y * 16) / 16;

				additiveBatch.draw(
					draw.position.x, draw.position.y, draw.position.z - i / (float)additiveDraws.Count * 0.0001f,
					draw.size.x, draw.size.y,
					draw.rotation,
					draw.texture, uint.MaxValue,
					u0, v0, u1, v1,
					draw.color, draw.solid ? 0.0f : 1.0f, true);
			}
		}
		for (int i = 0; i < additiveExDraws.Count; i++)
		{
			SpriteExDraw draw = additiveExDraws[i];
			float u0 = draw.rect.min.x, v0 = draw.rect.min.y, u1 = draw.rect.max.x, v1 = draw.rect.max.y;
			additiveBatch.draw(draw.vertex0, draw.vertex1, draw.vertex2, draw.vertex3, draw.texture, uint.MaxValue, u0, v0, u1, v1, draw.color, draw.solid ? 0.0f : 1.0f);
		}
		additiveBatch.end();

		for (int i = 0; i < additiveBatch.getNumDrawCalls(); i++)
		{
			graphics.setCullState(CullState.None);
			graphics.setBlendState(BlendState.Additive);
			graphics.setDepthWrite(false);

			additiveBatch.submitDrawCall(i, spriteShader);
		}

		lineRenderer.begin(lineDraws.Count);
		for (int i = 0; i < lineDraws.Count; i++)
		{
			lineRenderer.draw(lineDraws[i].vertex0, lineDraws[i].vertex1, lineDraws[i].color);
		}
		lineRenderer.end((int)RenderPass.Geometry, lineShader, graphics);
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

		for (int it = 0; it < Math.Max((lightDraws.Count + 15) / 16, 1); it++)
		{
			int offset = it * 16;

			graphics.setRenderTarget(lighting);
			graphics.setBlendState(BlendState.Additive);
			graphics.setDepthTest(DepthTest.None);

			graphics.setVertexBuffer(quad);

			graphics.setTexture(lightingShader, "s_frame", 0, gbuffer.getAttachmentTexture(0));

			graphics.setUniform(lightingShader, "u_cameraBounds", new Vector4(left, right, bottom, top));
			graphics.setUniform(lightingShader, "u_ambientLight", it == 0 ? new Vector4(ambientLight, 0.0f) : Vector4.Zero);

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

		if (lightMask != null)
		{
			graphics.resetState();
			graphics.setPass((int)RenderPass.Lighting);

			graphics.setRenderTarget(lighting);
			graphics.setBlendState(BlendState.Alpha);
			graphics.setDepthTest(DepthTest.None);

			graphics.setVertexBuffer(quad);

			graphics.setTexture(lightMaskShader, "s_lightMask", 0, lightMask);
			graphics.setUniform(lightMaskShader, "u_lightMaskRect", new Vector4(lightMaskRect.position, lightMaskRect.size));
			graphics.setUniform(lightMaskShader, "u_cameraBounds", new Vector4(left, right, bottom, top));

			graphics.draw(lightMaskShader);
		}
	}

	static void ParallaxPass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Parallax);
		graphics.setRenderTarget(parallax);

		//graphics.blit(parallax.getAttachmentTexture(2), gbuffer.getAttachmentTexture(2));

		graphics.setViewTransform(perspective, perspectiveView);

		parallaxBatch.begin(parallaxDraws.Count);
		for (int i = 0; i < parallaxDraws.Count; i++)
		{
			SpriteDraw draw = parallaxDraws[i];
			float u0 = draw.rect.min.x, v0 = draw.rect.min.y, u1 = draw.rect.max.x, v1 = draw.rect.max.y;

			u0 += 0.00001f;
			v0 += 0.00001f;
			u1 -= 0.00001f;
			v1 -= 0.00001f;

			if (draw.useTransform)
			{
				float pixelSize = (perspectiveDistance - draw.position.z) / perspectiveDistance / 16;
				draw.transform.m30 = MathF.Round(draw.transform.m30 / pixelSize) * pixelSize;
				draw.transform.m31 = MathF.Round(draw.transform.m31 / pixelSize) * pixelSize;

				spriteBatch.draw(
					draw.size.x, draw.size.y, 0.0f - i * 0.0000001f,
					draw.transform,
					draw.texture, uint.MaxValue,
					u0, v0, u1, v1,
					draw.color, draw.solid ? 0.0f : 1.0f);
			}
			else
			{
				// pixel perfect correction
				float pixelSize = (perspectiveDistance - draw.position.z) / perspectiveDistance / 16;
				draw.position.x = MathF.Round(draw.position.x / pixelSize) * pixelSize;
				draw.position.y = MathF.Round(draw.position.y / pixelSize) * pixelSize;

				parallaxBatch.draw(
					draw.position.x, draw.position.y, draw.position.z - i * 0.0000001f,
					draw.size.x, draw.size.y,
					draw.rotation,
					draw.texture, uint.MaxValue,
					u0, v0, u1, v1,
					draw.color, draw.solid ? 0.0f : 1.0f);
			}
		}
		parallaxBatch.end();

		for (int i = 0; i < parallaxBatch.getNumDrawCalls(); i++)
		{
			graphics.setCullState(CullState.None);
			graphics.setBlendState(BlendState.Alpha);

			parallaxBatch.submitDrawCall(i, spriteShader);
		}
	}

	static void CompositePass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Composite);

		graphics.setRenderTarget(composite);

		graphics.setDepthTest(DepthTest.None);
		graphics.setCullState(CullState.ClockWise);

		graphics.setTexture(compositeShader.getUniform("s_midground", UniformType.Sampler), 0, gbuffer.getAttachmentTexture(0));
		graphics.setTexture(compositeShader.getUniform("s_parallax", UniformType.Sampler), 1, parallax.getAttachmentTexture(0));
		graphics.setTexture(compositeShader.getUniform("s_material", UniformType.Sampler), 2, gbuffer.getAttachmentTexture(1));
		graphics.setTexture(compositeShader.getUniform("s_lighting", UniformType.Sampler), 3, lighting.getAttachmentTexture(0));

		graphics.setUniform(compositeShader, "u_cameraSettings", new Vector4(cameraFractX, cameraFractY, 0, 0));

		graphics.setVertexBuffer(quad);
		graphics.draw(compositeShader);
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
		if (bloomEnabled)
		{
			BloomDownsample(0, composite.getAttachmentTexture(0), bloomDownsampleChain[0]);
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
		else
		{
			graphics.resetState();
			graphics.setPass((int)RenderPass.Bloom);

			graphics.setRenderTarget(bloomUpsampleChain[0], true);
		}
	}

	static void BlitPass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.UI);

		graphics.setRenderTarget(null);

		graphics.setDepthTest(DepthTest.None);
		graphics.setCullState(CullState.ClockWise);

		graphics.setUniform(blitShader, "u_cameraSettings", new Vector4(UIWidth, UIHeight, 0, 0 /*cameraFractX, cameraFractY*/));
		graphics.setUniform(blitShader, "u_cameraSettings1", new Vector4(right - left, top - bottom, 0, 0));

		graphics.setTexture(blitShader.getUniform("s_frame", UniformType.Sampler), 0, composite.getAttachmentTexture(0));
		graphics.setTexture(blitShader.getUniform("s_bloom", UniformType.Sampler), 1, bloomUpsampleChain[0].getAttachmentTexture(0));

		graphics.setUniform(blitShader, "u_vignetteColor", new Vector4(vignetteColor, vignetteFalloff));
		graphics.setUniform(blitShader, "u_bloomSettings", new Vector4(bloomStrength, bloomFalloff, 0, 0));

		graphics.setVertexBuffer(quad);
		graphics.draw(blitShader);
	}

	static void UIPass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.UI);
		graphics.setRenderTarget(null);

		int scale = (int)MathF.Round(Display.width / (float)UIWidth);
		graphics.setViewTransform(
			Matrix.CreateOrthographic(0, UIWidth, 0, UIHeight, 1.0f, -1.0f),
			Matrix.CreateScale(UIWidth * scale / (float)Display.width, UIHeight * scale / (float)Display.height, 1)
		);

		uiBatch.begin(uiDraws.Count);
		for (int i = 0; i < uiDraws.Count; i++)
		{
			UIDraw draw = uiDraws[i];
			Texture texture = draw.texture;

			float u0 = draw.rect.min.x;
			float v0 = draw.rect.min.y;
			float u1 = draw.rect.max.x;
			float v1 = draw.rect.max.y;

			//u0 += 0.00001f;
			//v0 += 0.00001f;
			//u1 -= 0.00001f;
			//v1 -= 0.00001f;

			uiBatch.draw(
				draw.position.x, UIHeight - draw.position.y - draw.size.y, 0.0f,
				draw.size.x, draw.size.y,
				draw.rotation,
				texture, uint.MaxValue,
				u0, v0, u1, v1,
				MathHelper.ARGBToVector(draw.color), draw.solid ? 0.0f : 1.0f);
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
				UIHeight,
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
		ParallaxPass();
		CompositePass();
		PostProcessingPass();
		BlitPass();
		UIPass();

		draws.Clear();
		exDraws.Clear();
		additiveDraws.Clear();
		additiveExDraws.Clear();
		verticalDraws.Clear();
		parallaxDraws.Clear();
		uiDraws.Clear();
		textDraws.Clear();
		lineDraws.Clear();
		lightDraws.Clear();
		lightMask = null;
	}
}
