using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class Tinkerer : NPC
{
	public Tinkerer()
		: base("tinkerer")
	{
		displayName = "Bob";

		sprite = new Sprite(Resource.GetTexture("sprites/merchant6.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 1, true);
		animator.setAnimation("idle");

		buysItems = true;
	}

	public override void init(Level level)
	{
		setInititalDialogue("""
			\cAh, a customer!
			""");
	}
}
