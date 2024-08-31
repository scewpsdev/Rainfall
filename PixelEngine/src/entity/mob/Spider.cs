using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Spider : Mob
{
	public Spider()
		: base("spider")
	{
		displayName = "Spider";

		sprite = new Sprite(Resource.GetTexture("res/sprites/spider.png", false));

		collider = new FloatRect(-0.4f, 0, 0.8f, 0.5f);

		ai = new SpiderAI(this);

		health = 4;
	}
}
