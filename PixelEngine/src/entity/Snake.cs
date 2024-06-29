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
	}
}
