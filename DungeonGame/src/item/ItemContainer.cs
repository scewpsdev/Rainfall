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
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				ItemSlot slot = items[x + y * width];
				if (slot.item != null)
				{
					int x0 = x;
					int x1 = x + slot.item.inventorySize.x;
					int y0 = y;
					int y1 = y + slot.item.inventorySize.y;
					if (gridX >= x0 && gridX < x1 && gridY >= y0 && gridY < y1)
						return slot;
				}
			}
		}
		return null;
	}

	public bool isSlotEmpty(int gridX, int gridY, int gridWidth, int gridHeight)
	{
		for (int x = gridX; x < gridX + gridWidth; x++)
		{
			for (int y = gridY; y < gridY + gridHeight; y++)
			{
				if (x < 0 || x >= width || y < 0 || y >= height)
					return false;
				if (getItemAtPos(x, y) != null)
					return false;
			}
		}
		return true;
	}

	public ItemSlot addItem(Item item, int amount = 1)
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
				ItemSlot topLeft = getItemAtPos(x, y);
				if (topLeft == null)
				{
					if (isSlotEmpty(x, y, item.inventorySize.x, item.inventorySize.y))
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
