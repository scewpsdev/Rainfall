using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Effect : Entity
{
	SpriteSheet sheet;
	Sprite sprite;
	SpriteAnimator animator;
	float rotation;
	uint color;


	public Effect(string name, float rotation, uint color, Vector2 position)
	{
		this.position = position;
		this.rotation = rotation;
		this.color = color;

		sheet = new SpriteSheet(Resource.GetTexture("res/sprites/effect_" + name + ".png", false), 32, 32);
		sprite = new Sprite(sheet, 0, 0);

		animator = new SpriteAnimator();
		animator.addAnimation("default", 0, 0, 1, 0, sheet.texture.width / 32, 12, false);
		animator.setAnimation("default");
	}

	public override void update()
	{
		animator.update(sprite);
		if (animator.finished)
			removed = true;
	}

	public override void draw()
	{
		Renderer.DrawVerticalSprite(position.x - 2, position.y + 0.01f, -1, 4, 4, sprite, false, rotation, color);
	}
}
