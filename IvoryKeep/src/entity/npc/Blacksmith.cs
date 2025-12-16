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
	}

	public override void init(Level level)
	{
		setOneTimeInititalDialogue("""
			Another wanderer poking their nose in places it don't belong.
			If you have no interest in my wares keep walking. You disturb my focus.
			""")?.addCallback(() =>
		{
			GameState.instance.save.setFlag(SaveFile.FLAG_NPC_BLACKSMITH_MET);
		});

		if (progression.initialDialogue == null)
		{
			setInititalDialogue("""
				Take what you need, if you can bear the weight.
				""");
		}

		addOneTimeDialogue("""
			A thousand souls, and yet none strong enough to escape these \bforsaken\0 ruins. What makes you think you'll fare any better?
			""");

		addDialogue("""
			Hmm?
			I'm not up for chatting.
			""");

		GameState.instance.worldEventListeners.Add(this);
	}

	public override void update()
	{
		base.update();

		BossRoom bossRoom = level.getEntity<BossRoom>();
		if (bossRoom != null && bossRoom.boss.isAlive)
			animator.setAnimation("idle");
		else if (state == NPCState.None)
			animator.setAnimation("smith");
		else
			animator.setAnimation("idle");
	}

	public override void onLevelSwitch(Level newLevel)
	{
		base.onLevelSwitch(newLevel);
		if (newLevel != level && level != GameState.instance.hub)
			remove();
	}

	public void onBossKilled(Mob boss)
	{
		clearShop();
		Random random = new Random((int)Hash.combine(Hash.hash(GameState.instance.run.seed), (uint)boss.level.floor));
		populateShop(random, 8, 10, boss.level.avgLootValue * 2, ItemType.Weapon, ItemType.Shield, ItemType.Armor, ItemType.Ammo);
		buysItems = true;

		if (GameState.instance.areaCaves.Contains(boss.level))
		{
			setInititalDialogue("""
				Guess I could see about sharpening that blade of yours.
				Go on, let me have a look.
				""");
		}
		else if (GameState.instance.areaMines.Contains(boss.level))
		{
			setInititalDialogue("""
				You're still alive?
				Hah.
				Let's see about getting that gear of yours in shape.
				""");
		}
		else if (GameState.instance.areaDungeons.Contains(boss.level))
		{
			setInititalDialogue("""
				Serious about this, huh? Reckon you might even stand a chance.
				Now give me your weapons.
				""");
			addDialogue("""
				Back when the royal knights came to me, they wanted weapons that could slay giants.
				Look where it got them.
				\1Promise me you will do better...
				Ah, don't listen to me.
				""");
		}

		canUpgrade = true;
	}
}
