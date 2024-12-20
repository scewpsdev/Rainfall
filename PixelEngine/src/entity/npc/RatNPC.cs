using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RatNPC : NPC
{
	Dialogue questlineDialogue;


	public RatNPC(Random random)
		: base("rat_npc")
	{
		displayName = "Jack";

		voicePitch = 1.6f;
		voicePitchVariation = 3;

		sprite = new Sprite(Resource.GetTexture("res/sprites/merchant3.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		var questCompletion = (Quest cheeseQuest) =>
		{
			initialDialogue = new Dialogue();
			initialDialogue.addVoiceLine("You did it! Here is your reward, as promised.").addCallback(() =>
			{
				GameState.instance.level.addEntity(new ItemEntity(new Cheese() { name = "wondrous_cheese", displayName = "Wondrous Cheese", stackable = false }), position + Vector2.Up);
				closeScreen();

				cheeseQuest.collect();
				GameState.instance.save.setFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_COMPLETED);
			});
		};
		if (GameState.instance.save.tryGetQuest(name, "cheese_quest", out Quest cheeseQuest))
			questCompletion(cheeseQuest);
		else
			GameState.instance.save.addQuestCompletionCallback(name, "cheese_quest", questCompletion);

		if (cheeseQuest != null)
		{
			if (cheeseQuest.isRunning || cheeseQuest.isCollected)
			{
				initialDialogue = new Dialogue();
				initialDialogue.addVoiceLine("How goes the hunt?");
			}

			if (GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_COMPLETED))
			{
				Cheese wondrousCheese = new Cheese();
				wondrousCheese.name = "wondrous_cheese";
				wondrousCheese.displayName = "Wondrous Cheese";
				addShopItem(wondrousCheese, 0);

				populateShop(random, 1, 4, 5, ItemType.Food);
				//addShopItem(new Cheese() { stackSize = 5 });
			}
		}
		else if (GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_MET))
		{
			initialDialogue = new Dialogue();
			initialDialogue.addVoiceLine("\\cYou again!");

			questlineDialogue = new Dialogue();
			questlineDialogue.addVoiceLine("There's something fierce about you. \\3I'm wondering...");
			questlineDialogue.addVoiceLine("Hear me out, will you?");
			questlineDialogue.addVoiceLine("I'm buying the milk I'm using for my cheese from this guy Siko. He wants some snake venom in return but I'm no good at hunting snakes, they \\bscare\\0 me.");
			questlineDialogue.addVoiceLine("You seem to have no problem of that kind. Would you mind helping me out?");
			questlineDialogue.addVoiceLine("The cheese is very important to me. Nowhere else have I ever found such an \\cexquisite\\0 type of milk...");
			addDialogue(questlineDialogue);
		}
		else
		{
			initialDialogue = new Dialogue();
			initialDialogue.addVoiceLine("\\aWOAH!");
			initialDialogue.addVoiceLine("\\3You look... \\5\\bintense!");
			initialDialogue.addVoiceLine("Ah, you must be here to try some of my \\cwondrous cheese?");
			initialDialogue.addVoiceLine("Yes, yes. That must be it. I'll give you one for free, good?");

			Cheese wondrousCheese = new Cheese();
			wondrousCheese.name = "wondrous_cheese";
			wondrousCheese.displayName = "Wondrous Cheese";
			addShopItem(wondrousCheese, 0);
		}
	}

	public RatNPC()
		: this(null)
	{
	}

	public override void onDialogueComplete(Dialogue dialogue)
	{
		if (dialogue == initialDialogue)
			GameState.instance.save.setFlag(SaveFile.FLAG_NPC_RAT_MET);
		else if (dialogue == questlineDialogue)
		{
			GameState.instance.save.addQuest(name, new CheeseQuest());
		}
	}

	public override void update()
	{
		base.update();

		if (GameState.instance.save.tryGetQuest(name, "cheese_quest", out Quest q))
		{
			CheeseQuest quest = q as CheeseQuest;
			if (quest.completionRequirementsMet())
			{

			}
		}
	}
}
