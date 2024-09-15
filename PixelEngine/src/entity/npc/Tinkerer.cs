using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


public class Tinkerer : NPC
{
	public Tinkerer(Random random)
		: base("tinkerer")
	{
		displayName = "Tinker";

		sprite = new Sprite(Resource.GetTexture("res/sprites/merchant6.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		buysItems = true;
		canCraft = true;

		populateShop(random, 3, 9, 15, ItemType.Food, ItemType.Potion, ItemType.Scroll, ItemType.Gem, ItemType.Utility, ItemType.Ammo);
	}

	public Tinkerer()
		: this(Random.Shared)
	{
	}

	public override Item craftItem(Item item1, Item item2)
	{
		float combinedValue = item1.value + item2.value;

		Item craftedItem = null;

		bool hasName(string name) => item1.name == name || item2.name == name;
		bool hasType(ItemType type) => item2.type == type || item2.type == type;

		Random random = new Random((int)Hash.combine((uint)item1.id, (uint)item2.id));

		// do crafting
		if (hasName("stick") && hasType(ItemType.Gem))
			craftedItem = Item.CreateRandom(ItemType.Staff, random, combinedValue * 0.8f, combinedValue * 1.2f);
		if (hasName("scroll_blank") && hasType(ItemType.Gem))
			craftedItem = Item.CreateRandom(ItemType.Scroll, random, combinedValue * 0.8f, combinedValue * 1.2f);

		if (craftedItem != null)
		{
			player.removeItemSingle(item1);
			player.removeItemSingle(item2);
		}
		return craftedItem;
	}
}
