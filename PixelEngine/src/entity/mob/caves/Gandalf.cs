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

		sprite = new Sprite(Resource.GetTexture("sprites/gandalf.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 1, true);
		animator.addAnimation("charge", 1, 1, true);
		animator.addAnimation("attack", 1, 1, true);
		animator.addAnimation("dead", 1, 1, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.25f, 0.0f, 0.5f, 0.75f);

		health = 5;
		speed = 2;

		ai = new GandalfAI(this);
	}
}
