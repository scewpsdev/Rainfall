using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ParallaxObject : Entity
{
	public Sprite sprite;
	public FloatRect rect;
	public float z;


	public ParallaxObject()
	{
	}

	public ParallaxObject(Texture texture, FloatRect rect, float z)
	{
		sprite = new Sprite(texture, 0, 0, texture.width, texture.height);
		this.rect = rect;
		this.z = z;
	}

	public ParallaxObject(Texture texture, int x, int y, int width, int height, FloatRect rect, float z)
	{
		sprite = new Sprite(texture, x, y, width, height);
		this.rect = rect;
		this.z = z;
	}

	public ParallaxObject(Sprite sprite, FloatRect rect, float z)
	{
		this.sprite = sprite;
		this.rect = rect;
		this.z = z;
	}

	public override void render()
	{
		float distance = -z;
		float fog = MathF.Exp(-distance * GameState.instance.level.fogFalloff);

		//Vector3 vertex = ParallaxEffect(position, MathF.Log2(1 - 0.1f * z));

		//Renderer.DrawSprite(vertex.x + rect.min.x, vertex.y + rect.min.y, z, rect.size.x, rect.size.y, 0, sprite, false, Vector4.One);
		//Renderer.DrawSpriteSolid(vertex.x + rect.min.x, vertex.y + rect.min.y, z + 0.01f, rect.size.x, rect.size.y, 0, sprite, false, MathHelper.VectorToARGB(new Vector4(GameState.instance.level.fogColor, 1 - fog)));

		float scale = (10 - z) * 0.1f;
		Vector2 center = rect.center;
		float width = rect.size.x; // * scale; //sprite.spriteSheet.texture.width / 16.0f * scale;
		float height = rect.size.y; // * scale; //sprite.spriteSheet.texture.height / 16.0f * scale;

		Renderer.DrawParallaxSprite(position.x + center.x - 0.5f * width, position.y + center.y - 0.5f * height, z, width, height, rotation, sprite, Vector4.One);
	}

	public static Vector3 ParallaxEffect(Vector2 vertex, float layer)
	{
		float parallax = MathF.Pow(0.5f, layer);
		float xoffset = (GameState.instance.camera.position.x - vertex.x) * (1 - parallax);
		float yoffset = (GameState.instance.camera.position.y - vertex.y) * (1 - parallax);

		float z = layer >= 0 ? 0.9f + 0.002f * layer : -0.9f + 0.002f * layer;

		return new Vector3(vertex.x + xoffset, vertex.y + yoffset, z);
	}

	public static Vector3 ParallaxEffect(Vector3 vertex)
	{
		return ParallaxEffect(vertex.xy, ZToLayer(vertex.z));
	}

	public static float LayerToZ(float layer)
	{
		return MathF.Pow(2, layer) - 1;
	}

	public static float ZToLayer(float z)
	{
		return MathF.Log2(1 + z);
	}
}
