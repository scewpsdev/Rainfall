using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Spider : Mob
{
	public Spider()
	{
		sprite = new Sprite(Resource.GetTexture("res/sprites/spider.png", false));

		ai = new WanderAI();
	}
}
