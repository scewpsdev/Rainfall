using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Rat : Mob
{
	public Rat()
		: base("rat")
	{
		displayName = "Rat";

		sprite = new Sprite(Resource.GetTexture("sprites/rat.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation_("idle", 0, 0, 16, 0, 1, 1, true);
		animator.addAnimation_("dead", 1 * 16, 0, 16, 0, 1, 1, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.4f, 0, 0.8f, 0.55f);

		ai = new WanderAI(this);

		health = 2;
		poise = 0;
		speed = 3;
	}
}
