using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GreenSpider : Mob
{
	public GreenSpider()
	{
		sprite = new Sprite(Resource.GetTexture("res/sprites/green_spider.png", false));

		collider = new FloatRect(-0.4f, 0, 0.8f, 0.5f);

		speed = 6;
		jumpPower = 9;

		ai = new GreenSpiderAI();
	}
}
