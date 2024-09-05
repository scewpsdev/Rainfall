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
		: base("tinkerer_merchant")
	{
		displayName = "Tinker";

		sprite = new Sprite(Resource.GetTexture("res/sprites/merchant6.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		buysItems = true;
		canCraft = true;

		populateShop(random, 9, ItemType.Food, ItemType.Potion, ItemType.Scroll, ItemType.Gem, ItemType.Utility);
	}

	public override Item craftItem(Item item1, Item item2)
	{
		item1 = player.removeItemSingle(item1);
		item2 = player.removeItemSingle(item2);

		if (item1.type > item2.type)
			MathHelper.Swap(ref item1, ref item2);

		Random random = new Random((int)Hash.combine((uint)item1.id, (uint)item2.id));
		float value = item1.value + item2.value;

		for (int i = 0; i < 100; i++)
		{
			Item item = null;

			if (item1.id == item2.id)
			{
				item = Item.CreateRandom(item1.type, random, value, value * 1.5f);
				// return buffed item
			}
			else if (item1.type == item2.type)
			{
				item = Item.CreateRandom(item1.type, random, value, 1.5f * value);
			}
			else
			{
				if (item2.type == ItemType.Gem)
				{
					item = Item.CreateRandom(item1.type, random, value, 1.5f * value);
				}
				else
				{
					item = Item.CreateRandom(item1.type, random, value, 1.5f * value);
				}
			}

			if (item != null)
				return item;
			value *= 0.9f;
		}

		Debug.Assert(false);
		return new Emerald();
	}
}
