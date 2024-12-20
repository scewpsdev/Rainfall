using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class Tinkerer : NPC
{
	public Tinkerer(Random random, Level level)
		: base("tinkerer")
	{
		displayName = "Tinker";

		sprite = new Sprite(Resource.GetTexture("res/sprites/merchant6.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		buysItems = true;
		canCraft = true;

		populateShop(random, 3, 9, level.avgLootValue, ItemType.Food, ItemType.Potion, ItemType.Scroll, ItemType.Gem, ItemType.Utility, ItemType.Ammo);
	}

	public Tinkerer()
		: this(Random.Shared, GameState.instance.level)
	{
	}

	public override Item craftItem(Item item1, Item item2)
	{
		Item craftedItem = Crafting.CraftItem(item1, item2);
		if (craftedItem != null)
		{
			player.removeItemSingle(item1);
			player.removeItemSingle(item2);
		}
		return craftedItem;
	}
}
