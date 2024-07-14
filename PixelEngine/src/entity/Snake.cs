using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Snake : Mob
{
	public Snake()
	{
		sprite = new Sprite(Resource.GetTexture("res/sprites/snake.png", false));

		collider = new FloatRect(-0.3f, 0, 0.6f, 0.8f);

		ai = new SnakeAI();

		health = 4;
		speed = 2;
	}
}
