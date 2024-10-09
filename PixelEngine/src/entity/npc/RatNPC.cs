using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RatNPC : NPC
{
	public RatNPC()
		: base("rat_npc")
	{
		displayName = "Jack";

		sprite = new Sprite(Resource.GetTexture("res/sprites/merchant3.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		if (GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_INIT))
		{
			addVoiceLine("How goes the hunt?");
		}
		else
		{
			addVoiceLine("\\aWOAH!");
			addVoiceLine("\\3You look... \\5\\bintense!");
			addVoiceLine("Ah, you must be here to try some of my \\cwondrous cheese?");
			addVoiceLine("Yes, yes. That must be it. I'll give you one for free, good?");

			Cheese wondrousCheese = new Cheese();
			wondrousCheese.name = "wondrous_cheese";
			wondrousCheese.displayName = "Wondrous Cheese";
			addShopItem(wondrousCheese, 0);
			addShopItem(new Cheese());
		}
	}

	public override void update()
	{
		base.update();

		if (voiceLines.Count == 0)
		{
			GameState.instance.save.setFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_INIT);
		}
	}
}
