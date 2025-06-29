﻿using Rainfall;
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

		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_GATEKEEPER_MET))
		{
			initialDialogue = new Dialogue();
			initialDialogue.addVoiceLine("So, you've found your way here. Curious.");
			initialDialogue.addVoiceLine("Many wander, but few arrive.");
			initialDialogue.screens[initialDialogue.screens.Count - 1].addCallback(() =>
			{
				GameState.instance.save.setFlag(SaveFile.FLAG_NPC_GATEKEEPER_MET);
			});
		}
		else
		{
			initialDialogue = new Dialogue();
			initialDialogue.addVoiceLine("\\d...");
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
				dialogue.addVoiceLine("After all that's happened, the castle still stands tall...");
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

		populateShop(new Random(12345), 7, 12, level.avgLootValue * 2, ItemType.Weapon, ItemType.Armor, ItemType.Staff, ItemType.Relic);
	}
}
