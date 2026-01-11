using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class SikoSave : NPCSaveData
{
	public override void init(SaveFile save)
	{
		if (!save.hasFlag(SaveFile.FLAG_NPC_GATEKEEPER_MET))
		{
			setOneTimeInititalDialogue("""
				So you've found your way here. Curious.
				Many wander, but few arrive.
				""").addCallback(() =>
			{
				save.setFlag(SaveFile.FLAG_NPC_GATEKEEPER_MET);
			});
		}
		else
		{
			setInititalDialogue("\\d...");
		}
	}
}

public class TravellingMerchant : NPC
{
	public TravellingMerchant()
		: base("travelling_merchant")
	{
		displayName = "Siko";

		sprite = new Sprite(Resource.GetTexture("sprites/merchant2.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 1, true);
		animator.setAnimation("idle");

		voicePitch = 0;
		canUncurse = true;
	}

	public override void init(Level level)
	{
		base.init(level);

		if (level == GameState.instance.hub)
		{
			save.addDialogue("The castle looms beyond, doesn't it? I wonder what's left of it...");
		}
		else
		{
			save.addOneTimeDialogue("""
			   After all that's happened, the castle walls still stand tall...
			   """);
		}

		save.addDialogue("\\1...?");

		if (!level.name.StartsWith("caves"))
		{
			populateShop(GameState.instance.generator.random, 7, 12, level.avgLootValue * 2, ItemType.Potion, ItemType.Scroll, /*ItemType.Spell, */ItemType.Relic);

			float bottleChance = 0.3f;
			if (GameState.instance.generator.random.NextSingle() < bottleChance)
			{
				addShopItem(new GlassBottle());
			}
		}

		if (level.areaName != null)
		{
			const float mapChance = 0.2f;
			if (GameState.instance.generator.random.NextSingle() < mapChance)
			{
				DungeonMap map = new DungeonMap();
				map.setArea(level);
				addShopItem(map);
			}
		}

		if (level != GameState.instance.hub)
		{
			buysItems = true;
			//canAttune = true;
			//populateShop(random, 7, 12, level.avgLootValue * 2, ItemType.Weapon, ItemType.Armor, ItemType.Staff, ItemType.Relic);
		}
	}
}
