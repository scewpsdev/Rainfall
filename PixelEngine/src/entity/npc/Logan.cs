using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Logan : NPC
{
	public Logan(Random random)
		: base("logan")
	{
		displayName = "Big Fat Logan";

		sprite = new Sprite(Resource.GetTexture("res/sprites/merchant4.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		addVoiceLine("Mm, you seem quite lucid! A rare thing in these times.");
		addVoiceLine("Buy my shit.");

		populateShop(random, 5, ItemType.Potion, ItemType.Ring, ItemType.Staff, ItemType.Scroll);
	}
}
