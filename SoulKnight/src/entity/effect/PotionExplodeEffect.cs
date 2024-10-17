using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PotionExplodeEffect : Entity
{
	float radius;
	Sprite sprite;
	Vector3 color;

	float duration = 0.35f;

	long startTime;


	public PotionExplodeEffect(float radius, Vector3 color)
	{
		this.radius = radius;
		this.color = color;

		sprite = new Sprite(effectsTileset, 0, 2, 2, 2);
	}

	public override void init(Level level)
	{
		startTime = Time.currentTime;
	}

	public override void update()
	{
		if ((Time.currentTime - startTime) / 1e9f > duration)
			remove();
	}

	public override void render()
	{
		float size = (Time.currentTime - startTime) / 1e9f / duration * radius;
		float alpha = 1 - (Time.currentTime - startTime) / 1e9f / duration;
		Renderer.DrawSprite(position.x - 0.5f * size, position.y - 0.5f * size, size, size, sprite, false, new Vector4(color, alpha));
	}
}
