using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class FireProjectileCastEffect : Entity
{
	Sprite sprite;

	Entity follow;
	Vector2 offset;

	float duration = 0.2f;

	long startTime;


	public FireProjectileCastEffect(Entity follow = null)
	{
		this.follow = follow;

		sprite = new Sprite(effectsTileset, 0, 1);
	}

	public override void init(Level level)
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
		uint color = (uint)((byte)(0xFF * alpha) << 24) | 0x00FFFFFF;
		Renderer.DrawSprite(position.x - 0.5f * size, position.y - 0.5f * size, size, size, sprite, false, color);
	}
}
