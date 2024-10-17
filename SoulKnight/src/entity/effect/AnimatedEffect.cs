using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AnimatedEffect : Entity
{
	Sprite sprite;
	SpriteAnimator animator;
	protected Vector4 color = Vector4.One;
	protected bool additive = false;

	Entity follow;
	Vector2 offset;

	FloatRect rect;
	float duration;

	long startTime;


	public AnimatedEffect(Texture texture, int fps, Entity follow = null)
	{
		this.follow = follow;

		sprite = new Sprite(texture, 0, 0, texture.height, texture.height);
		animator = new SpriteAnimator();
		animator.addAnimation("default", 0, 0, texture.height, 0, texture.width / texture.height, fps, false);
		animator.setAnimation("default");

		float size = texture.height / 16.0f;
		rect = new FloatRect(-0.5f * size, -0.5f * size, size, size);
		duration = (texture.width / texture.height) / (float)fps;
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

		animator.update(sprite);

		if ((Time.currentTime - startTime) / 1e9f > duration)
			remove();
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x + rect.position.x, position.y + rect.position.y, LAYER_FG, rect.size.x, rect.size.y, 0, sprite, false, color, additive);
	}
}
