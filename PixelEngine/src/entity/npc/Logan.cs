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
