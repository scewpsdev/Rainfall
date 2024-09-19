using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Crafting
{
	public static Item CraftItem(Item item1, Item item2)
	{
		float combinedValue = item1.value + item2.value;

		Item craftedItem = null;

		Item hasName(string name) => item1.name == name ? item1 : item2.name == name ? item2 : null;
		Item hasType(ItemType type) => item1.type == type ? item1 : item2.type == type ? item2 : null;

		Random random = new Random((int)Hash.combine((uint)item1.id, (uint)item2.id));

		// do crafting
		if (hasName("stick") != null && hasType(ItemType.Gem) != null)
			craftedItem = Item.CreateRandom(ItemType.Staff, random, combinedValue * 0.8f, combinedValue * 1.2f);
		if (hasName("scroll_blank") != null && hasType(ItemType.Gem) != null)
			craftedItem = Item.CreateRandom(ItemType.Scroll, random, combinedValue * 0.8f, combinedValue * 1.2f);
		if (hasType(ItemType.Potion) != null && hasName("rope") != null)
		{
			Potion potion = hasType(ItemType.Potion).copy() as Potion;
			potion.makeThrowable();
			craftedItem = potion;
		}

		return craftedItem;
	}
}
