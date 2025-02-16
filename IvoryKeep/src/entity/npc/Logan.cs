using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class Logan : NPC
{
	public LoganQuest loganQuest = new LoganQuest();


	public Logan()
			: base("logan")
	{
		displayName = "Big Fat Logan";

		sprite = new Sprite(Resource.GetTexture("sprites/merchant4.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 1, true);
		animator.setAnimation("idle");
	}

	public override void load(DatObject obj)
	{
		base.load(obj);
		SaveFile.LoadQuest(obj, loganQuest, this);
	}

	public override void save(DatObject obj)
	{
		base.save(obj);
		SaveFile.SaveQuest(obj, loganQuest, this);
	}

	public override void init(Level level)
	{
		setOneTimeInititalDialogue("""
			Mm, you seem quite lucid! A \drare\0 thing in these times.
			\cBuy my shit.
			""")?.addCallback(() =>
		{
			GameState.instance.save.setFlag(SaveFile.FLAG_NPC_LOGAN_MET);
		});

		if (initialDialogue == null && loganQuest.state == QuestState.Uninitialized && GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_LOGAN_MET) && GameState.instance.save.hasFlag(SaveFile.FLAG_DUNGEONS_FOUND))
		{
			setOneTimeInititalDialogue("""
				\1Aha... \0You again.
				Color me impressed, you're not as hopeless as you look, making it this far.
				Makes me wonder... maybe you're the type who can handle something a bit more interesting.
				""");

			addDialogue("""
				Fine then, let's see if you're up to a real challenge.
				Word is, there's an ancient magic staff hidden somewhere deep in those weeping catacombs.
				Find it for me and you might even earn yourself a proper wizard's robes!
				That's if the beast guarding it doesn't rip you to pieces first. But you look sturdy enough, I suppose!
				Hehe.
				""").addCallback(() =>
			{
				loganQuest.state = QuestState.InProgress;
			});
		}
		QuestManager.addQuestCompletionCallback(name, loganQuest.name, (Quest _) =>
		{
			setInititalDialogue("""
				Ha, look at you! You actually did it! Maybe you're not as useless as the rest of the rabble.
				Hooray, or something. Here, take this.
				""").addCallback(() =>
			{
				Item staff = player.getItem("questline_logan_staff");
				if (staff != null)
					player.removeItem(staff);

				closeScreen();
				GameState.instance.save.unlockStartingClass(StartingClass.wizard);

				loganQuest.collect();
			});
		});

		if (initialDialogue == null && GameState.instance.save.hasFlag(SaveFile.FLAG_CASTLE_UNLOCKED))
		{
			setOneTimeInititalDialogue("""
				\1Oh ho!\0 Look at you, finding all the fancy trinkets. That thing's got \dpower\0, no doubt about it.
				Let me see that. \1Hmm... \0Yep, it's magic all right. Old magic. Twisted.
				""");
		}

		if (initialDialogue == null && loganQuest.state == QuestState.InProgress && level == GameState.instance.hub)
		{
			setOneTimeInititalDialogue("""
				Found that staff yet? Come see me if you do.
				""");
		}

		if (initialDialogue == null && Random.Shared.NextSingle() < 0.1f)
		{
			setOneTimeInititalDialogue("""
					The king, the archives, the underworld... everyone's so serious about it all.
					Me? I just want to blow things up.
					""");
		}
		if (initialDialogue == null)
		{
			int i = Random.Shared.Next();
			if (i % 4 == 0)
			{
				setInititalDialogue("""
					Ah, \byou again\0! My best - and only - customer.
					""");
			}
			else if (i % 4 == 1)
			{
				setInititalDialogue("""
					Well, well, still breathing are we?
					Very good.
					""");
			}
			else if (i % 4 == 2)
			{
				setInititalDialogue("""
					Go ahead, take a look.
					""");
			}
			else
			{
				setInititalDialogue("""
					Still alive, eh? I'll admit, I'm impressed. Most of my customers are more ghostly by now.
					""");
			}
		}

		if (level != GameState.instance.hub)
		{
			populateShop(GameState.instance.generator.random, 5, 7, level.avgLootValue * 2, ItemType.Potion, ItemType.Staff, ItemType.Spell, ItemType.Scroll);
			buysItems = true;
			//canAttune = true;
		}
	}
}
