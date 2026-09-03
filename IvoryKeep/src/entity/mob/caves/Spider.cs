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

		collider = new Hitbox(-0.4f * 2, 0, 0.8f * 2, 0.45f * 2);

		health = 3;
		poisonResistant = true;
		hasNightVision = true;
		canClimb = true;
		jumpPower = 12 * 2;

		ai = new SpiderAI(this);
	}
}
