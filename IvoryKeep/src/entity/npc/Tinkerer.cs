﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class Tinkerer : NPC
{
	public Tinkerer()
		: base("tinkerer")
	{
		displayName = "Tinker";

		sprite = new Sprite(Resource.GetTexture("sprites/merchant6.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 1, true);
		animator.setAnimation("idle");

		buysItems = true;
		buyTax = 0.55f;
		canCraft = true;
	}

	public override void init(Level level)
	{
		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_TINKERER_MET))
		{
			initialDialogue = new Dialogue();
			initialDialogue.addVoiceLine("\\cAh, a customer!");
			initialDialogue.addVoiceLine("You'd be surprised how good business is down here.");
			initialDialogue.addVoiceLine("But don't ask me how I get by wares. Just know that they can be yours...\\3 For a price, of course.").addCallback(() =>
			{
				GameState.instance.save.setFlag(SaveFile.FLAG_NPC_TINKERER_MET);
			});
		}

		{
			Dialogue dialogue = new Dialogue();
			dialogue.addVoiceLine("You're looking for the lost sigil, aren't you? Everyone is. Let me know if you find it.");
			addDialogue(dialogue);
		}
		{
			Dialogue dialogue = new Dialogue();
			dialogue.addVoiceLine("Let's talk trade.");
			addDialogue(dialogue);
		}

		populateShop(GameState.instance.generator.random, 8, 14, level.avgLootValue * 2, ItemType.Food, ItemType.Potion, ItemType.Scroll, ItemType.Utility, ItemType.Weapon, ItemType.Armor, ItemType.Ammo);
	}

	public override Item craftItem(Item item1, Item item2)
	{
		Item craftedItem = Crafting.CraftItem(item1, item2);
		if (craftedItem != null)
		{
			player.removeItemSingle(item1);
			player.removeItemSingle(item2);
		}
		return craftedItem;
	}
}
