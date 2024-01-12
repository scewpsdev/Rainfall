using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Torch : Entity
{
	static Texture shadow;

	static Torch()
	{
		shadow = Resource.GetTexture("res/sprites/shadow.png", true);
	}


	SpriteSheet sheet;
	Sprite sprite;
	SpriteAnimator animator;

	AudioSource audio;
	Sound sfxBurn;

	Vector3 color;
	float radius;


	public Torch(int x, int y)
	{
		position = new Vector2(x, y) + 0.5f;
		color = new Vector3(1.0f, 0.6f, 0.2f) * 12;
		radius = 5;

		sheet = new SpriteSheet(Resource.GetTexture("res/sprites/torch.png", false), 32, 32);
		sprite = new Sprite(sheet, 0, 0);
		animator = new SpriteAnimator();
		animator.addAnimation("default", 0, 0, 1, 0, 8, 12, true);
		animator.setAnimation("default");

		collider = new FloatRect(-0.5f, -0.5f, 1, 1);
		staticCollider = true;

		audio = Audio.CreateSource(new Vector3(position, 1.0f));
		sfxBurn = Resource.GetSound("res/sounds/fire.ogg");
		audio.playSound(sfxBurn, 1.0f);
		audio.isLooping = true;
	}

	public override void update()
	{
		animator.update(sprite);
	}

	public override void draw()
	{
		Renderer.DrawVerticalSprite(position.x - 2, position.y, 4, 4, sprite, false);
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.25f, 0.01f, 1, 1, shadow, 0, 0, 8, 8, 0xFFFFFFFF);
		Renderer.DrawLight(position + new Vector2(0.0f, 2.5f), color, radius);
	}
}
