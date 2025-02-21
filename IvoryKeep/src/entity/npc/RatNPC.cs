using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RatNPC : NPC
{
	public CheeseQuest cheeseQuest = new CheeseQuest();


	public RatNPC()
		: base("rat_npc")
	{
		displayName = "Jack";

		voicePitch = 1.6f;
		voicePitchVariation = 3;

		sprite = new Sprite(Resource.GetTexture("sprites/merchant3.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 1, true);
		animator.setAnimation("idle");
	}

	public override void load(DatObject obj)
	{
		base.load(obj);
		SaveFile.LoadQuest(obj, cheeseQuest, this);
	}

	public override void save(DatObject obj)
	{
		base.save(obj);
		SaveFile.SaveQuest(obj, cheeseQuest, this);
	}

	public override void init(Level level)
	{
		setOneTimeInititalDialogue("""
			\aWOAH!
			\3You look... \5\bintense!
			Ah, you must be here to try some of my \cwondrous cheese?
			Yes, yes. That must be it. I'll give you one for free, good?
			""")?.addCallback(() =>
		{
			Cheese wondrousCheese = new Cheese();
			wondrousCheese.name = "wondrous_cheese";
			wondrousCheese.displayName = "Wondrous Cheese";
			addShopItem(wondrousCheese, 0);

			GameState.instance.save.setFlag(SaveFile.FLAG_NPC_RAT_MET);
		});

		if (initialDialogue == null)
			setOneTimeInititalDialogue("""
				\cYou again!
				""");


		if (GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) && cheeseQuest.state == QuestState.Uninitialized)
		{
			addDialogue("""
				There's something fierce about you. \3I'm wondering...
				Hear me out, will you?
				I'm buying the milk I'm using for my cheese from this guy Siko. He wants some snake venom in return but I'm no good at hunting snakes. They \bscare\0 me.
				You seem to have no problem of that kind. Would you mind helping me out?
				The milk is very important to me. Nowhere else have I ever found such an \cexquisite\0 kind...
				""").addCallback(() =>
			{
				QuestManager.AddActiveQuest(name, cheeseQuest);
			});
		}
		QuestManager.addQuestCompletionCallback(name, cheeseQuest.name, (Quest _) =>
		{
			setInititalDialogue("""
				You did it! Here is your reward, as promised.
				""").addCallback(() =>
			{
				GameState.instance.level.addEntity(new ItemEntity(new Cheese() { name = "wondrous_cheese", displayName = "Wondrous Cheese", stackable = false }), position + Vector2.Up);
				GameState.instance.save.unlockStartingClass(StartingClass.fool);
				closeScreen();

				cheeseQuest.collect();
				GameState.instance.save.setFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_COMPLETED);
			});
		});

		if (initialDialogue == null)
			setInititalDialogue("""
				How goes the hunt?
				""");

		if (level.floor != -1)
		{
			populateShop(GameState.instance.generator.random, 3, 5, 5, ItemType.Food);
			addShopItem(new Cheese() { name = "wondrous_cheese", displayName = "Wondrous Cheese", stackable = false }, 0);
		}
	}
}
