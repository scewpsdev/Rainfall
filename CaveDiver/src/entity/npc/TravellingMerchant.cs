using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class TravellingMerchant : NPC
{
	public TravellingMerchant()
		: base("travelling_merchant")
	{
		displayName = "Siko";

		sprite = new Sprite(Resource.GetTexture("sprites/merchant2.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 1, true);
		animator.setAnimation("idle");

		buysItems = true;
	}

	public override void init(Level level)
	{
		setInititalDialogue("""
			So, you've found your way here. Curious.
			""");
	}
}
