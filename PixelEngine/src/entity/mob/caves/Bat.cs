using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Bat : Mob
{
	public Bat()
		: base("bat")
	{
		displayName = "Bat";

		health = 3;
		gravity = 0;
		speed = 1.5f;
		canFly = true;

		ai = new BatAI(this);

		collider = new FloatRect(-0.4f, -0.4f, 0.8f, 0.8f);
		sprite = new Sprite(Resource.GetTexture("sprites/bat.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 3, 6, true);
		animator.addAnimation("dead", 3 * 16, 0, 16, 0, 1, 1, true);
		animator.setAnimation("idle");

		rect = new FloatRect(-0.5f, -0.5f, 1, 1);
	}

	public override void update()
	{
		base.update();

		animator.getAnimation("idle").fps = 6 * speed / 2;
	}
}
