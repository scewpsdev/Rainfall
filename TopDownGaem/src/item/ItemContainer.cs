using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ItemContainer
{
	public readonly int width, height;
	public readonly ItemSlot[] items;


	public ItemContainer(int width, int height)
	{
		this.width = width;
		this.height = height;
		items = new ItemSlot[width * height];
		for (int i = 0; i < items.Length; i++)
			items[i] = new ItemSlot(this, i, i % width, i / width);
	}

	ItemSlot getItemSlotWithItem(Item item)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].item == item)
				return items[i];
		}
		return null;
	}

	public ItemSlot getItemAtPos(int gridX, int gridY)
	{
		if (gridX >= 0 && gridX < width && gridY >= 0 && gridY < height)
			return items[gridX + gridY * width];
		return null;
	}

	public virtual ItemSlot addItem(Item item, int amount = 1)
	{
		//List<ItemSlot> items = getItemListForCategory(item.category);
		if (item.stackable)
		{
			ItemSlot existingSlot = getItemSlotWithItem(item);
			if (existingSlot != null)
			{
				existingSlot.stackSize += amount;
				return existingSlot;
			}
		}

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int i = x + y * width;
				if (getItemAtPos(x, y).item == null)
				{
					if (item.stackable)
					{
						items[i].setItem(item);
						items[i].stackSize = amount;
						return items[i];
					}
					else
					{
						items[i].setItem(item);
						items[i].stackSize = 1;
						amount--;
						if (amount == 0)
							return items[i];
					}
				}
			}
		}

		return null;
	}

	public void removeItem(ItemSlot existingSlot, int amount = 1)
	{
		existingSlot.stackSize -= amount;
		if (existingSlot.stackSize <= 0)
		{
			existingSlot.stackSize = 0;
			existingSlot.setItem(null);
		}
	}

	public void removeItem(Item item, int amount = 1)
	{
		ItemSlot slot = findSlot(item);
		if (slot != null)
		{
			slot.stackSize -= amount;
			if (slot.stackSize <= 0)
			{
				slot.stackSize = 0;
				slot.setItem(null);
			}
		}
	}

	public void swapSlots(ItemSlot slot0, ItemSlot slot1)
	{
		Item tmp = slot0.item;
		int tmp2 = slot0.stackSize;
		slot0.setItem(slot1.item);
		slot0.stackSize = slot1.stackSize;
		slot1.setItem(tmp);
		slot1.stackSize = tmp2;
	}

	public ItemSlot findSlot(Item item)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].item == item)
				return items[i];
		}
		return null;
	}
}
