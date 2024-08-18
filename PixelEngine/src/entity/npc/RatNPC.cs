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

		sprite = new Sprite(Resource.GetTexture("res/sprites/rat_npc.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		addVoiceLine("WOAH!");
		addVoiceLine("You look...", "intense!");
		addVoiceLine("Did you come to try some of my wondrous cheese?");
		addVoiceLine("It's very exquisit, yes yes.");
		addVoiceLine("I'll give you one for free, good?");
	}

	public override void populateShop(Random random)
	{
		addShopItem(new Cheese());
	}
}
