using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Blacksmith : NPC, WorldEventListener
{
	public Blacksmith()
		: base("blacksmith")
	{
		displayName = "Blacksmith";

		sprite = new Sprite(Resource.GetTexture("sprites/merchant5.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		buyTax = 0.5f;
		voicePitch = 0.75f;
	}

	public override void init(Level level)
	{
		setOneTimeInititalDialogue("""
			A thousand souls, and yet none strong enough to escape this \bwretched\0 place. What makes you think you'll fare any better?
			""")?.addCallback(() =>
		{
			GameState.instance.save.setFlag(SaveFile.FLAG_NPC_BLACKSMITH_MET);
		});

		addDialogue("""
			Hmm?
			I'm not up for chatting.
			""");

		GameState.instance.worldEventListeners.Add(this);
	}

	public void onBossKilled(Mob boss)
	{
		clearShop();
		populateShop(GameState.instance.generator.random, 5, 5, boss.level.avgLootValue * 1.5f, ItemType.Weapon, ItemType.Shield, ItemType.Armor, ItemType.Ammo);
		buysItems = true;
		canUpgrade = true;

		if (initialDialogue == null)
		{
			setInititalDialogue("""
				Take what you need, if you can bear the weight.
				""");
		}
	}
}
