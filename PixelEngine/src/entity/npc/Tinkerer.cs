using Rainfall;
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
	public Tinkerer(Random random, Level level)
		: base("tinkerer")
	{
		displayName = "Tinker";

		sprite = new Sprite(Resource.GetTexture("sprites/merchant6.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		buysItems = true;
		buyTax = 0.35f;
		canCraft = true;

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

		populateShop(random, 3, 9, level.avgLootValue * 1.25f, ItemType.Food, ItemType.Potion, ItemType.Scroll, ItemType.Gem, ItemType.Utility, ItemType.Ammo);
	}

	public Tinkerer()
		: this(Random.Shared, GameState.instance.level)
	{
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
