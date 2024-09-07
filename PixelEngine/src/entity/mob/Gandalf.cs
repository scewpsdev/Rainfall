using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Gandalf : Mob
{
	public Gandalf()
		: base("gandalf")
	{
		displayName = "Gandalf";

		sprite = new Sprite(Resource.GetTexture("res/sprites/gandalf.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.addAnimation("charge", 2 * 16, 0, 16, 0, 1, 1, true);
		animator.addAnimation("attack", 3 * 16, 0, 16, 0, 1, 1, true);
		animator.addAnimation("dead", 4 * 16, 0, 16, 0, 1, 1, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.25f, 0.0f, 0.5f, 0.5f);

		ai = new GandalfAI(this);

		health = 5;
		speed = 2;
	}
}
