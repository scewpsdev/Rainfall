using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class Logan : NPC
{
	public Logan(Random random, Level level)
		: base("logan")
	{
		displayName = "Big Fat Logan";

		sprite = new Sprite(Resource.GetTexture("res/sprites/merchant4.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		addVoiceLine("Mm, you seem quite lucid! A \\drare\\0 thing in these times.");
		addVoiceLine("\\cBuy my shit.");

		populateShop(random, 2, 5, level.lootValue, ItemType.Potion, ItemType.Ring, ItemType.Staff, ItemType.Scroll);
		buysItems = true;
	}

	public Logan()
		: this(Random.Shared, GameState.instance.level)
	{
	}
}
