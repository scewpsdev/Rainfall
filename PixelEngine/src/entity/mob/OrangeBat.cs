using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class OrangeBat : Mob
{
	public OrangeBat()
		: base("orange_bat")
	{
		displayName = "Orange Bat";

		health = 4;
		gravity = 0;
		speed = 1.2f;
		canFly = true;

		ai = new BatAI(this)
		{
			preferVerticalMovement = false,
			canShoot = true
		};

		collider = new FloatRect(-0.4f, -0.4f, 0.8f, 0.8f);
		sprite = new Sprite(Resource.GetTexture("res/sprites/orange_bat.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 3, 6, true);
		animator.setAnimation("idle");

		rect = new FloatRect(-0.5f, -0.5f, 1, 1);
	}

	public override void update()
	{
		base.update();

		animator.getAnimation("idle").fps = 6 * speed / 2;
	}
}
