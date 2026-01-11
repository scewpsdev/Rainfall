using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BuilderMerchant : NPC
{
	public BuilderMerchant()
		: base("builder_merchant")
	{
		displayName = "John";

		sprite = new Sprite(Resource.GetTexture("sprites/merchant.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 1, true);
		animator.setAnimation("idle");

		buysItems = true;

		save.setInititalDialogue("""
			Howdy!
			""");
	}

	public override void init(Level level)
	{
		populateShop(GameState.instance.generator.random, 8, 14, level.avgLootValue, ItemType.Weapon, ItemType.Armor, ItemType.Food, ItemType.Utility, ItemType.Ammo, ItemType.Scroll, ItemType.Potion);
	}
}
