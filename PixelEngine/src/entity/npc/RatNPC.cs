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

		addVoiceLine("WOAH!");
		addVoiceLine("You look... intense!");
		addVoiceLine("Ah, you must be here to try some of my wondrous cheese?");
		addVoiceLine("It's very exquisite, yes yes. Truly wunderbar.");
		addVoiceLine("Very fine taste you have, little creature.");
		addVoiceLine("I'll give you one for free, good?");

		Cheese wondrousCheese = new Cheese();
		wondrousCheese.name = "wondrous_cheese";
		wondrousCheese.displayName = "Wondrous Cheese";
		addShopItem(wondrousCheese, 0);
		addShopItem(new Cheese());
	}
}
