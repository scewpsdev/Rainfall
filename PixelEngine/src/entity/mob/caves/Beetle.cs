using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Beetle : Mob
{
	public Beetle()
		: base("beetle")
	{
		displayName = "Beetle";

		sprite = new Sprite(Resource.GetTexture("sprites/mob/beetle.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 4, true);
		animator.addAnimation("dead", 1 * 16, 0, 16, 0, 1, 1, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.25f, -0.25f, 0.5f, 0.5f);
		rect = new FloatRect(-0.5f, -0.5f, 1, 1);

		ai = new BeetleAI(this);

		spawnRate = 0.1f;

		health = 3;
		poise = 0;

		speed = 2;
		gravity = 0;
	}
}
