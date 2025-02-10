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

		sprite = new Sprite(Resource.GetTexture("sprites/spider.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 1, 1, true);
		animator.addAnimation("dead", 1, 1, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.4f, 0, 0.8f, 0.6f);

		health = 3;
		poisonResistant = true;
		jumpPower = 12;

		ai = new SpiderAI(this);
	}
}
