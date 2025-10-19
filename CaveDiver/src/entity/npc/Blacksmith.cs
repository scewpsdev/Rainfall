using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Blacksmith : NPC, WorldEventListener
{
	Sound[] smithSound;


	public Blacksmith()
		: base("blacksmith")
	{
		displayName = "Blacksmith";

		smithSound = Resource.GetSounds("sounds/smith", 3);

		sprite = new Sprite(Resource.GetTexture("sprites/merchant5.png", false), 0, 0, 64, 32);
		rect = new FloatRect(-2, 0, 4, 2);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 1, true);
		animator.addAnimation("smith", 11, 1, true);
		animator.addAnimationEvent("smith", 2, () =>
		{
			Vector2 particleOrigin = position + new Vector2(11 * direction, 7) / 16.0f;
			level.addEntity(ParticleEffects.CreateSmithEffect(), particleOrigin);
			Audio.PlayOrganic(smithSound, new Vector3(particleOrigin, 0), 2, 1, 0, 0.2f);
		});
		animator.setAnimation("smith");
		turnTowardsPlayer = false;

		buyTax = 0.5f;
		voicePitch = 0.75f;

		canUpgrade = true;
	}

	public override void init(Level level)
	{
		addDialogue("""
			Hmm?
			I'm not up for chatting.
			""");

		GameState.instance.worldEventListeners.Add(this);
	}

	public override void update()
	{
		base.update();

		if (state == NPCState.None)
			animator.setAnimation("smith");
		else
			animator.setAnimation("idle");
	}

	public void onBossKilled(Mob[] boss)
	{
		clearShop();

		if (initialDialogue == null)
		{
			setOneTimeInititalDialogue("""
				Guess I could see about sharpening that blade of yours.
				Go on, let me have a look.
				""");
		}

		if (initialDialogue == null)
		{
			setOneTimeInititalDialogue("""
				Still alive, eh?
				Let's see about getting that gear of yours in shape.
				""");
		}

		if (initialDialogue == null)
		{
			setOneTimeInititalDialogue("""
				Serious about this, huh? Reckon you might even stand a chance.
				Now give me your weapons.
				""");
		}
	}
}
