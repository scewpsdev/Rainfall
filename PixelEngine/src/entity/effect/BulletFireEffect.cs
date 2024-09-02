using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BulletFireEffect : Entity
{
	Sprite sprite;

	Entity follow;
	Vector2 offset;

	float duration = 0.1f;

	long startTime;


	public BulletFireEffect(Entity follow = null)
	{
		this.follow = follow;

		sprite = new Sprite(effectsTileset, 1, 0);
	}

	public override void init()
	{
		startTime = Time.currentTime;
		if (follow != null)
			offset = position - follow.position;
	}

	public override void update()
	{
		if (follow != null)
			position = follow.position + offset;

		if ((Time.currentTime - startTime) / 1e9f > duration)
			remove();
	}

	public override void render()
	{
		float size = (Time.currentTime - startTime) / 1e9f / duration;
		float alpha = 1 - (Time.currentTime - startTime) / 1e9f / duration;
		alpha = 1;
		uint color = (uint)((byte)(0xFF * alpha) << 24) | 0x00FFFFFF;
		Renderer.DrawSprite(position.x - 0.5f * size, position.y - 0.5f * size, size, size, sprite, false, color);
	}
}
