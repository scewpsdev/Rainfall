using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Bob : Mob
{
	public Bob()
		: base("bob")
	{
		displayName = "Bob";

		sprite = new Sprite(Resource.GetTexture("res/sprites/bob.png", false), 0, 0, 32, 32);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 32, 0, 2, 1, true);
		animator.addAnimation("run", 2 * 32, 0, 32, 0, 8, 8, true);
		animator.addAnimation("charge", 24 * 32, 0, 32, 0, 1, 1, true);
		animator.addAnimation("attack", 25 * 32, 0, 32, 0, 1, 1, true);
		animator.addAnimation("cooldown", 26 * 32, 0, 32, 0, 5, 6, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.3f, 0, 0.6f, 0.8f);
		rect = new FloatRect(-1, 0, 2, 2);

		ai = new AdvancedAI();

		health = 12;
		speed = 1;
	}
}
