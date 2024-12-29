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

		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_LOGAN_MET))
		{
			initialDialogue = new Dialogue();
			initialDialogue.addVoiceLine("Mm, you seem quite lucid! A \\drare\\0 thing in these times.");
			initialDialogue.addVoiceLine("\\cBuy my shit.").addCallback(() =>
			{
				GameState.instance.save.setFlag(SaveFile.FLAG_NPC_LOGAN_MET);
			});
		}
		else
		{
			if (GameState.instance.save.hasFlag(SaveFile.FLAG_DUNGEONS_FOUND) && !GameState.instance.save.tryGetQuest(name, "logan_quest", out _))
			{
				initialDialogue = new Dialogue();
				initialDialogue.addVoiceLine("\\1Aha... \\0You again.");
				initialDialogue.addVoiceLine("Color me impressed, you're not as hopeless as you look, making it this far.");
				initialDialogue.addVoiceLine("Makes me wonder... maybe you're the type who can handle something a bit more interesting.");

				Dialogue dialogue = new Dialogue();
				dialogue.addVoiceLine("Fine then, let's see if you're up to a real challenge.");
				dialogue.addVoiceLine("Word is, there's an ancient magic staff hidden somewhere deep in those weeping catacombs.");
				dialogue.addVoiceLine("Find it for me and you might even earn yourself a proper wizard's robes!");
				dialogue.addVoiceLine("That's if the beast guarding it doesn't rip you to pieces first. But you look sturdy enough, I suppose!");
				dialogue.addVoiceLine("Hehe.").addCallback(() =>
				{
					GameState.instance.save.addQuest(name, new LoganQuest());
				});
				addDialogue(dialogue);
			}
			if (GameState.instance.save.tryGetQuest(name, "logan_quest", out Quest quest))
			{
				GameState.instance.save.addQuestCompletionCallback(name, "logan_quest", (Quest quest) =>
				{
					initialDialogue = new Dialogue();
					initialDialogue.addVoiceLine("Ha, look at you! You actually did it. Maybe you're not as useless as the rest of the rabble.");
					initialDialogue.addVoiceLine("Hooray, or something. Here, take this.").addCallback(() =>
					{
						closeScreen();
						GameState.instance.save.unlockStartingClass(StartingClass.wizard);
					});
				});
				if (!quest.isCompleted && level == GameState.instance.areaDungeons[0])
				{
					initialDialogue = new Dialogue();
					initialDialogue.addVoiceLine("I'll be waiting here in case you find it.");
				}
			}
			else
			{
				int i = random.Next();
				if (i % 4 == 0)
				{
					initialDialogue = new Dialogue();
					initialDialogue.addVoiceLine("Ah, \\byou again\\0! My best - and only - customer.");
				}
				else if (i % 4 == 1)
				{
					initialDialogue = new Dialogue();
					initialDialogue.addVoiceLine("Well, well, still breathing are we?");
					initialDialogue.addVoiceLine("Very good.");
				}
				else if (i % 4 == 2)
				{
					initialDialogue = new Dialogue();
					initialDialogue.addVoiceLine("Go ahead, take a look. Just don't blame me if your eyebrows fall off.");
				}
				else
				{
					initialDialogue = new Dialogue();
					initialDialogue.addVoiceLine("Still alive, eh? I'll admit, I'm impressed. Most of my customers are more ghostly by now.");
				}
			}

			if (!GameState.instance.save.hasFlag(SaveFile.FLAG_CASTLE_UNLOCKED))
			{
				if (random.NextSingle() < 0.9f)
				{
					{
						Dialogue dialogue = new Dialogue();
						dialogue.addVoiceLine("Heading for the castle, eh? You're braver than I thought.");
						dialogue.addVoiceLine("Rumour has it something \\bterrible\\0 happened there.");
						dialogue.addVoiceLine("I do admit, I would love to visit the king's archives again after all this time... But now? That would be \\dsuicide\\0.");
						addDialogue(dialogue);
					}
					{
						Dialogue dialogue = new Dialogue();
						dialogue.addVoiceLine("Magic ran through those halls back in the days. Until something changed. Something quite \\dsinister\\...");
						addDialogue(dialogue);
					}
					{
						Dialogue dialogue = new Dialogue();
						dialogue.addVoiceLine("If you're really heading there... \\3do stay safe, friend.");
						addDialogue(dialogue);
					}
				}
				else
				{
					{
						Dialogue dialogue = new Dialogue();
						dialogue.addVoiceLine("The king, the archives, the underworld... everyone's so serious about it all.");
						dialogue.addVoiceLine("Me? I just want to blow things up.");
						addDialogue(dialogue);
					}
				}
			}
			else
			{
				{
					Dialogue dialogue = new Dialogue();
					dialogue.addVoiceLine("Oh-ho! Look at you, finding all the fancy trinkets. That thing's got power, no doubt about it.");
					dialogue.addVoiceLine("Let me see that. Hmm... yep, it's magic all right. Old magic. Twisted.");
					addDialogue(dialogue);
				}
			}
		}

		populateShop(random, 2, 9, level.avgLootValue, ItemType.Potion, ItemType.Staff, ItemType.Spell, ItemType.Scroll);
		buysItems = true;
		canAttune = true;
	}

	public Logan()
		: this(Random.Shared, GameState.instance.level)
	{
	}
}
