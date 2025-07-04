﻿using Rainfall;
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

		sprite = new Sprite(Resource.GetTexture("sprites/green_spider.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 1, 1, true);
		animator.addAnimation("dead", 1, 1, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.4f, 0, 0.8f, 0.5f);

		speed = 6;
		jumpPower = 9;

		health = 8;
		poisonResistant = true;

		itemDropChance = 0.8f;

		ai = new SpiderAI(this, 0.5f)
		{
			aggroRange = 12,
			loseRange = 15,
		};
	}
}
