using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ParallaxObject : Entity
{
	Sprite sprite;
	float layer;

	public ParallaxObject(Texture texture, float layer)
	{
		sprite = new Sprite(texture, 0, 0, texture.width, texture.height);
		this.layer = layer;
	}

	public ParallaxObject(Texture texture, int x, int y, int width, int height, float layer)
	{
		sprite = new Sprite(texture, x, y, width, height);
		this.layer = layer;
	}

	public override void render()
	{
		float width = sprite.size.x / 16.0f;
		float height = sprite.size.y / 16.0f;

		float distance = LayerToZ(layer);
		float fog = MathF.Exp(-distance * GameState.instance.level.fogFalloff);

		Vector3 vertex = ParallaxEffect(position, layer);

		//Renderer.DrawSprite(vertex.x - 0.5f * width, vertex.y - 0.5f * height, vertex.z, width, height, 0, sprite, false, Vector4.One);
		//Renderer.DrawSpriteSolid(vertex.x - 0.5f * width, vertex.y - 0.5f * height, vertex.z - 0.001f, width, height, 0, sprite, false, MathHelper.VectorToARGB(new Vector4(GameState.instance.level.fogColor, 1 - fog)));
	}

	public static Vector3 ParallaxEffect(Vector2 vertex, float layer)
	{
		float parallax = MathF.Pow(0.5f, layer);
		float xoffset = (GameState.instance.camera.position.x - vertex.x) * (1 - parallax);
		float yoffset = (GameState.instance.camera.position.y - vertex.y) * (1 - parallax);

		float z = 0.9f + 0.01f * layer;

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
