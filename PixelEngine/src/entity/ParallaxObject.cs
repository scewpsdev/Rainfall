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

	public override void render()
	{
		float width = sprite.size.x / 16.0f;
		float height = sprite.size.y / 16.0f;
		float parallax = MathF.Pow(0.5f, layer);
		float xoffset = (GameState.instance.camera.position.x - position.x) * (1 - parallax);
		float yoffset = (GameState.instance.camera.position.y - position.y) * (1 - parallax);

		float distance = MathF.Pow(2, layer);
		float fog = MathF.Exp(-distance * GameState.instance.level.fogFalloff);
		Vector3 color = Vector3.Lerp(GameState.instance.level.fogColor, Vector3.One, fog);

		float z = 0.9f + 0.01f * layer;

		Renderer.DrawSprite(position.x + xoffset - 0.5f * width, position.y + yoffset - 0.5f * height, z, width, height, 0, sprite, false, new Vector4(color, 1));
	}
}
