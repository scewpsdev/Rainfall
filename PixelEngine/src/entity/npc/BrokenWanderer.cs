using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BrokenWanderer : NPC
{
	public BrokenWanderer(Random random, Level level)
		: base("builder_merchant")
	{
		displayName = "Broken Wanderer";

		sprite = new Sprite(Resource.GetTexture("sprites/npc/traveller.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 0.7f, true);
		animator.setAnimation("idle");

		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_TRAVELLER_MET))
		{
			initialDialogue = new Dialogue();
			initialDialogue.addVoiceLine("Oh, a traveler? It's rare to find one still breathing.");
			initialDialogue.addVoiceLine("The air down here has a way of\\1... \\3thinning your chances.");
			initialDialogue.screens[initialDialogue.screens.Count - 1].addCallback(() =>
			{
				GameState.instance.save.setFlag(SaveFile.FLAG_NPC_TRAVELLER_MET);
			});
		}
		else if (GameState.instance.save.hasFlag(SaveFile.FLAG_CAVES_FOUND))
		{
			initialDialogue = new Dialogue();
			initialDialogue.addVoiceLine("You've been below, haven't you? I can see it in your eyes.");
			initialDialogue.addVoiceLine("The caves... they leave their mark, even if you come back whole.");
		}

		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_CAVES_FOUND))
		{
			{
				Dialogue dialogue = new Dialogue();
				dialogue.addVoiceLine("That entrance over there? It leads into the caverns below. Tunnels that twist and turn...");
				dialogue.addVoiceLine("The deeper you go, the less sense it all makes.");
				dialogue.addVoiceLine("So many travellers got lost down there...");
				addDialogue(dialogue);
			}
			{
				Dialogue dialogue = new Dialogue();
				dialogue.addVoiceLine("I've ventured down before. Not far, mind you. It wasn't the creatures that turned me back.");
				dialogue.addVoiceLine("Something's wrong with the air down there... It gets thick, like it's pressing against you.");
				addDialogue(dialogue);
			}
			{
				Dialogue dialogue = new Dialogue();
				dialogue.addVoiceLine("Have you seen that big stone gate in the back? Nobody knows how to open it.");
				dialogue.addVoiceLine("It's been like this since I got here. I wonder where it could lead...");
				addDialogue(dialogue);
			}
		}
		{
			Dialogue dialogue = new Dialogue();
			dialogue.addVoiceLine("This land was once golden, they say. Glorious and prosperous.");
			dialogue.addVoiceLine("But now? Only relics of a kingdom that died with its pride.");
			dialogue.addVoiceLine("\\3And yet, people still come here... chasing whispers of treasure and glory...");
			addDialogue(dialogue);
		}
		{
			Dialogue dialogue = new Dialogue();
			dialogue.addVoiceLine("I once knew a way out. Or thought I did... But the caves - they shift and change.");
			dialogue.addVoiceLine("They're \\balive\\0 you know. Mocking us.");
			addDialogue(dialogue);
		}
		{
			Dialogue dialogue = new Dialogue();
			dialogue.addVoiceLine("But it's not so bad up here.");
			dialogue.addVoiceLine("I for one am done risking my neck for some trinkets.");
			addDialogue(dialogue);
		}
		{
			Dialogue dialogue = new Dialogue();
			dialogue.addVoiceLine("Still here? You're either brave or a fool. I wonder - are they so different in the end?");
			addDialogue(dialogue);
		}
	}

	public BrokenWanderer()
		: this(Random.Shared, GameState.instance.level)
	{
	}
}
