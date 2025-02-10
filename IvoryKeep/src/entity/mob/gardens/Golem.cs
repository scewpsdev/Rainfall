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

		sprite = new Sprite(Resource.GetTexture("sprites/golem.png", false), 0, 0, 48, 48);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 4, 2, true);
		animator.addAnimation("run", 4, 1, true);
		animator.addAnimation("jump", 1, 1, false);
		animator.addAnimation("charge", 1, 1, false);
		animator.addAnimation("attack", 2, 1, false);
		animator.addAnimation("cooldown", 1, 1, false);
		animator.addAnimation("dead", 1, 1, true);
		animator.addAnimation("dead_falling", 1, 1, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.5f, 0.0f, 1.0f, 1.8f);
		rect = new FloatRect(-1.5f, 0, 3, 3);

		health = 15;
		poise = 3;
		speed = 0.7f;
		damage = 1.5f;
		jumpPower = 10;
		gravity = -20;
		itemDropChance = 0.8f;
		//relicDropChance = 0.5f;

		//ai = new GolemAI(this);
	}
}
