using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Raya : Mob
{
	public Raya()
		: base("raya")
	{
		displayName = "Raya the Explorer";

		sprite = new Sprite(Resource.GetTexture("res/sprites/mob/gardens/raya.png", false), 0, 0, 64, 64);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 64, 0, 2, 1, true);
		animator.addAnimation("run", 2 * 64, 0, 64, 0, 8, 8, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.25f, 0, 0.5f, 1.4f);
		rect = new FloatRect(-2, 0, 4, 4);

		ai = new RayaAI(this);

		health = 65;
		poise = 10;
		speed = 1.5f;
		damage = 2;
		jumpPower = 16;
		gravity = -30;
		awareness = 1;
		itemDropChance = 2;
	}
}
