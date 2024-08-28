using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GreenSpider : Mob
{
	public GreenSpider()
		: base("green_spider")
	{
		displayName = "Green Spider";

		sprite = new Sprite(Resource.GetTexture("res/sprites/green_spider.png", false));

		collider = new FloatRect(-0.4f, 0, 0.8f, 0.5f);

		speed = 6;
		jumpPower = 9;

		health = 6;

		itemDropChance = 0.8f;

		ai = new SpiderAI
		{
			aggroRange = 12,
			loseRange = 15,
			jumpChargeTime = 0.5f,
		};
	}
}
