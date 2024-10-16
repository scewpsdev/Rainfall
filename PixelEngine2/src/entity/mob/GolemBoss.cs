using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GolemBoss : Mob
{
	public GolemBoss()
		: base("golem_boss")
	{
		displayName = "Golem Boss";

		sprite = new Sprite(Resource.GetTexture("res/sprites/golem_boss.png", false), 0, 0, 64, 64);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 64, 0, 4, 2, true);
		animator.addAnimation("run", 4 * 64, 0, 64, 0, 4, 3, true);
		animator.addAnimation("jump", 8 * 64, 0, 64, 0, 1, 1, false);
		animator.addAnimation("charge", 9 * 64, 0, 64, 0, 1, 1, false);
		animator.addAnimation("attack", 10 * 64, 0, 64, 0, 2, 6, false);
		animator.addAnimation("cooldown", 12 * 64, 0, 64, 0, 1, 1, false);
		animator.addAnimation("dead", 13 * 64, 0, 64, 0, 1, 1, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.5f, 0.0f, 1.0f, 1.8f);
		rect = new FloatRect(-2, 0, 4, 4);

		ai = new GolemAI(this);
		ai.loseRange = 100;

		health = 30;
		poise = 4;
		speed = 2.0f;
		damage = 1.5f;
		jumpPower = 11;
		awareness = 1;
		itemDropChance = 1;
		relicDropChance = 1;
	}
}
