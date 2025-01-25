using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BrokenWanderer : NPC
{
	public BrokenWanderer()
		: base("builder_merchant")
	{
		displayName = "Broken Wanderer";

		sprite = new Sprite(Resource.GetTexture("sprites/npc/traveller.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation_("idle", 0, 0, 16, 0, 2, 0.7f, true);
		animator.setAnimation("idle");
	}

	public override void init(Level level)
	{
		setOneTimeInititalDialogue("""
			Oh, a traveler? It's rare to find one still breathing.
			The air down here has a way of\1... \3thinning your chances.
			""")?.addCallback(() =>
		{
			GameState.instance.save.setFlag(SaveFile.FLAG_NPC_TRAVELLER_MET);
		});

		if (initialDialogue == null && GameState.instance.save.hasFlag(SaveFile.FLAG_CAVES_FOUND))
			setOneTimeInititalDialogue("""
				You've been below, haven't you? I can see it in your eyes.
				The caves... they leave their mark, even if you come back whole.
				""");


		addOneTimeDialogue("""
			That entrance over there? It leads into the caverns below. Tunnels that twist and turn...
			The deeper you go, the less sense it all makes.
			So many travellers got lost down there...
			""");

		addOneTimeDialogue("""
			I've ventured down before. Not far, mind you. It wasn't the creatures that turned me back.
			Something's wrong with the air down there... It gets thick, like it's pressing against you.
			""");

		addOneTimeDialogue("""
			Have you seen that big stone gate in the back? Nobody knows how to open it.
			It's been like this since I got here. I wonder where it could lead...
			""");

		addOneTimeDialogue("""
			Are the stories true? Word is the ancient castle on the hill got overrun.
			A fortress so strong and mighty, I never thought it possible for anything to get over those walls...
			It must have happened quite some time ago, considering the inhabitants are long gone.
			I wonder where they all went...
			""");

		addOneTimeDialogue("""
			This land was once golden, they say. Glorious and prosperous.
			But now? Only relics of a kingdom that died with its pride.
			\3And yet, people still come here, chasing whispers of treasure and glory...
			""");

		addOneTimeDialogue("""
			I once knew a way out of here. Or thought I did... But the caves - they shift and change.
			They're \balive\0 you know. Mocking us.
			But it's not so bad up here.
			I for one am done risking my neck for some trinkets.
			""");

		addDialogue("""
			Still here? You're either brave or a fool.
			I wonder - are they so different in the end?
			""");
	}
}
