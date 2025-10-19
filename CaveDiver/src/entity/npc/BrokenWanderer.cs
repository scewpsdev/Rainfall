using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BrokenWanderer : NPC
{
	public BrokenWanderer()
		: base("builder_merchant")
	{
		displayName = "James";

		sprite = new Sprite(Resource.GetTexture("sprites/npc/traveller.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 3, true);
		animator.setAnimation("idle");
	}

	public override void init(Level level)
	{
		setInititalDialogue("""
			Oh, a traveler? It's rare to find one still breathing.
			""");
	}
}
