using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;


public class Golem : Mob
{
	public Golem()
		: base("golem")
	{
		displayName = "Golem";

		sprite = new Sprite(Resource.GetTexture("res/sprites/golem.png", false), 0, 0, 48, 48);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 48, 0, 4, 2, true);
		animator.addAnimation("run", 4 * 48, 0, 48, 0, 4, 3, true);
		animator.addAnimation("jump", 8 * 48, 0, 48, 0, 1, 1, false);
		animator.addAnimation("charge", 9 * 48, 0, 48, 0, 1, 1, false);
		animator.addAnimation("attack", 10 * 48, 0, 48, 0, 2, 6, false);
		animator.addAnimation("cooldown", 12 * 48, 0, 48, 0, 1, 1, false);
		animator.addAnimation("dead", 13 * 48, 0, 48, 0, 1, 1, true);
		animator.addAnimation("dead_falling", 14 * 48, 0, 48, 0, 1, 1, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.5f, 0.0f, 1.0f, 2.0f);
		rect = new FloatRect(-1.5f, 0, 3, 3);

		health = 15;
		speed = 0.7f;
		damage = 1.5f;
		jumpPower = 10;
		gravity = -20;
		itemDropChance = 0.8f;

		ai = new GolemAI(this);
	}
}
