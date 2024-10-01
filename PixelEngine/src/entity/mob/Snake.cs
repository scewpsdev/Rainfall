using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Snake : Mob
{
	public Snake()
		: base("snake")
	{
		displayName = "Snake";

		sprite = new Sprite(Resource.GetTexture("res/sprites/snake.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 0, 0, 1, 1, true);
		animator.addAnimation("charge", 16, 0, 0, 0, 1, 1, true);
		animator.addAnimation("attack", 2 * 16, 0, 0, 0, 1, 1, true);
		animator.addAnimation("dead", 3 * 16, 0, 16, 0, 1, 1, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.3f, 0, 0.6f, 1.0f);

		ai = new SnakeAI(this);

		health = 4;
		speed = 2;
	}
}
