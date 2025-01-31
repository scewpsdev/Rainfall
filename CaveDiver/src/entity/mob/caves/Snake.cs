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

		sprite = new Sprite(Resource.GetTexture("sprites/snake.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 1, 1, true);
		animator.addAnimation("charge", 1, 1, true);
		animator.addAnimation("attack", 1, 1, true);
		animator.addAnimation("dead", 1, 1, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.3f, 0, 0.6f, 0.9f);

		health = 3;
		speed = 2;

		ai = new SnakeAI(this);
	}
}
