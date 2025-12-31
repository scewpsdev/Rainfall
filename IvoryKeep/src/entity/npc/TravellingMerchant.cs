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
	public TravellingMerchant(Random random, Level level)
		: base("travelling_merchant")
	{
		displayName = "Siko";

		sprite = new Sprite(Resource.GetTexture("sprites/merchant2.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 1, true);
		animator.setAnimation("idle");

		voicePitch = 0;
		canUncurse = true;

		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_GATEKEEPER_MET))
		{
			progression.initialDialogue = new Dialogue();
			progression.initialDialogue.addVoiceLine("So, you've found your way here. Curious.");
			progression.initialDialogue.addVoiceLine("Many wander, but few arrive.");
			progression.initialDialogue.screens[progression.initialDialogue.screens.Count - 1].addCallback(() =>
			{
				GameState.instance.save.setFlag(SaveFile.FLAG_NPC_GATEKEEPER_MET);
			});
		}
		else
		{
			progression.initialDialogue = new Dialogue();
			progression.initialDialogue.addVoiceLine("\\d...");
		}

		if (level == GameState.instance.hub)
		{
			{
				Dialogue dialogue = new Dialogue();
				dialogue.addVoiceLine("The castle looms beyond, doesn't it? I wonder what's left of it...");
				addDialogue(dialogue);
			}
		}
		else
		{
			{
				Dialogue dialogue = new Dialogue();
				dialogue.addVoiceLine("After all that's happened, the castle walls still stand tall...");
				dialogue.addVoiceLine("What? Sorry, I was just talking to myself.");
				addDialogue(dialogue);
			}
		}

		if (level != GameState.instance.hub)
		{
			buysItems = true;
			//canAttune = true;
			//populateShop(random, 7, 12, level.avgLootValue * 2, ItemType.Weapon, ItemType.Armor, ItemType.Staff, ItemType.Relic);
		}
	}

	public TravellingMerchant()
		: this(Random.Shared, GameState.instance.level)
	{
	}

	public override void init(Level level)
	{
		base.init(level);

		populateShop(GameState.instance.generator.random, 7, 12, level.avgLootValue * 2, ItemType.Potion, ItemType.Scroll, ItemType.Spell, ItemType.Relic);
	}
}
