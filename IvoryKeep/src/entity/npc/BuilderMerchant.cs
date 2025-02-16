using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BuilderMerchant : NPC
{
	public BuilderMerchant(Random random, Level level)
		: base("builder_merchant")
	{
		displayName = "John";

		sprite = new Sprite(Resource.GetTexture("sprites/merchant.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 1, true);
		animator.setAnimation("idle");

		buysItems = true;

		initialDialogue = new Dialogue();
		initialDialogue.addVoiceLine("Howdy!");

		populateShop(random, 5, 8, level.avgLootValue * 2, ItemType.Weapon, ItemType.Armor, ItemType.Scroll, ItemType.Food, ItemType.Utility, ItemType.Ammo);
	}

	public BuilderMerchant()
		: this(Random.Shared, GameState.instance.level)
	{
	}
}
