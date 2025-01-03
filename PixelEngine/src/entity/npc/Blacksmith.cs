using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Blacksmith : NPC
{
	public Blacksmith(Random random, Level level)
		: base("blacksmith")
	{
		displayName = "Blacksmith";

		sprite = new Sprite(Resource.GetTexture("sprites/merchant5.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		buysItems = true;
		buyTax = 0.5f;
		canUpgrade = true;

		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_BLACKSMITH_MET))
		{
			initialDialogue = new Dialogue();
			initialDialogue.addVoiceLine("A thousand souls, and yet none strong enough to escape this \\bwretched\\0 place.");
			initialDialogue.addVoiceLine("What makes you think you'll fare any better?").addCallback(() =>
			{
				GameState.instance.save.setFlag(SaveFile.FLAG_NPC_BLACKSMITH_MET);
			});
		}
		else
		{
			initialDialogue = new Dialogue();
			initialDialogue.addVoiceLine("Take what you need, if you can bear the weight.");
		}

		Dialogue dialogue = new Dialogue();
		dialogue.addVoiceLine("Hmm?");
		dialogue.addVoiceLine("I'm not up for chatting.");
		addDialogue(dialogue);

		populateShop(random, 2, 8, level.avgLootValue, ItemType.Weapon, ItemType.Shield, ItemType.Armor, ItemType.Ammo);
	}

	public Blacksmith()
		: this(Random.Shared, GameState.instance.level)
	{
	}
}
