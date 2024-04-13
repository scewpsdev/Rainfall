using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ItemSlot
{
	public int index;
	public Item item;
	public int stackSize;

	public ItemSlot(int index, Item item, int stackSize)
	{
		this.index = index;
		this.item = item;
		this.stackSize = stackSize;
	}
}

public class SpellSlot
{
	public ItemSlot spell;
	public int numCharges;
}

public class SpellSet
{
	public SpellSlot[] slots;
	public int selectedSlot;
}

internal class Inventory
{
	public List<ItemSlot> weapons = new List<ItemSlot>();
	public List<ItemSlot> shields = new List<ItemSlot>();
	public List<ItemSlot> utilities = new List<ItemSlot>();
	public List<ItemSlot> consumables = new List<ItemSlot>();
	public List<ItemSlot> arrows = new List<ItemSlot>();
	public List<ItemSlot> spells = new List<ItemSlot>();

	public ItemSlot[] rightHandSlots = new ItemSlot[2];
	public ItemSlot[] leftHandSlots = new ItemSlot[2];
	public int[] quickSlots = new int[10] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

	public Dictionary<ItemSlot, SpellSet> spellSlots = new Dictionary<ItemSlot, SpellSet>();

	public int rightHandSlotIdx = 0;
	public int leftHandSlotIdx = 0;


	public List<ItemSlot> getItemListForCategory(ItemCategory category)
	{
		switch (category)
		{
			case ItemCategory.Weapon:
				return weapons;
			case ItemCategory.Shield:
				return shields;
			case ItemCategory.Utility:
				return utilities;
			case ItemCategory.Consumable:
				return consumables;
			case ItemCategory.Arrow:
				return arrows;
			case ItemCategory.Spell:
				return spells;
			default:
				Debug.Assert(false);
				return null;
		}
	}

	ItemSlot getItemSlotWithItem(Item item, List<ItemSlot> slots)
	{
		for (int i = 0; i < slots.Count; i++)
		{
			if (slots[i].item == item)
				return slots[i];
		}
		return null;
	}

	void updateItemSlotIndices(List<ItemSlot> slots, int startIndex)
	{
		for (int i = startIndex; i < slots.Count; i++)
		{
			slots[i].index = i;
		}
	}

	public ItemSlot addItem(Item item, int amount = 1)
	{
		List<ItemSlot> items = getItemListForCategory(item.category);
		if (item.stackable)
		{
			ItemSlot existingSlot = getItemSlotWithItem(item, items);
			if (existingSlot != null)
			{
				existingSlot.stackSize += amount;
				return existingSlot;
			}
		}

		for (int i = 0; i < items.Count + 1; i++)
		{
			if (i == items.Count)
			{
				if (item.stackable)
					items.Add(new ItemSlot(i, item, amount));
				else
				{
					for (int j = 0; j < amount; j++)
					{
						items.Add(new ItemSlot(i, item, 1));
					}
				}
				return items[items.Count - 1];
			}
			else if (item.stackable && items[i].item == item)
			{
				items[i].stackSize += amount;
				return items[i];
			}
			else if (items[i].item.id > item.id)
			{
				if (item.stackable)
				{
					items.Insert(i, new ItemSlot(i, item, amount));
					updateItemSlotIndices(items, i + 1);
					return items[i];
				}
				else
				{
					for (int j = 0; j < amount; j++)
					{
						items.Insert(i + j, new ItemSlot(i + j, item, 1));
					}
					updateItemSlotIndices(items, i + amount);
					return items[i + amount - 1];
				}
			}
		}

		Debug.Assert(false);
		return null;
	}

	public void removeItem(ItemSlot existingSlot, int amount = 1)
	{
		Item item = existingSlot.item;
		List<ItemSlot> items = getItemListForCategory(item.category);
		if (item.stackable)
		{
			existingSlot.stackSize -= amount;
			if (existingSlot.stackSize <= 0)
			{
				items.RemoveAt(existingSlot.index);
				updateItemSlotIndices(items, existingSlot.index);
			}
		}
		else
		{
			Debug.Assert(existingSlot.stackSize == 1);
			items.RemoveAt(existingSlot.index);
			updateItemSlotIndices(items, existingSlot.index);
		}
	}

	public ItemSlot findSlot(Item item)
	{
		List<ItemSlot> items = getItemListForCategory(item.category);
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].item == item)
				return items[i];
		}
		return null;
	}

	public void selectHandItem(int handID, int itemIdx, ItemSlot item)
	{
		for (int i = 0; i < rightHandSlots.Length; i++)
		{
			if (rightHandSlots[i] == item)
			{
				rightHandSlots[i] = null;
				break;
			}
		}
		for (int i = 0; i < leftHandSlots.Length; i++)
		{
			if (leftHandSlots[i] == item)
			{
				leftHandSlots[i] = null;
				break;
			}
		}
		for (int i = 0; i < quickSlots.Length; i++)
		{
			if (item != null && quickSlots[i] == item.item.id)
			{
				quickSlots[i] = -1;
				break;
			}
		}

		if (handID == 0)
			rightHandSlots[itemIdx] = item;
		else
			leftHandSlots[itemIdx] = item;
	}

	public ItemSlot getSelectedHandSlot(int handID)
	{
		return handID == 0 ? rightHandSlots[rightHandSlotIdx] : leftHandSlots[leftHandSlotIdx];
	}

	public Item getSelectedHandItem(int handID)
	{
		ItemSlot slot = getSelectedHandSlot(handID);
		return slot != null ? slot.item : null;
	}

	public bool selectQuickSlotItem(ItemSlot slot)
	{
		for (int i = 0; i < quickSlots.Length; i++)
		{
			if (quickSlots[i] == -1)
			{
				quickSlots[i] = slot.item.id;
				return true;
			}
		}
		return false;
	}

	public ItemSlot getQuickSlot(int idx)
	{
		int itemID = quickSlots[idx];
		if (itemID != -1)
		{
			Item item = Item.Get(itemID);
			List<ItemSlot> items = getItemListForCategory(item.category);
			return getItemSlotWithItem(item, items);
		}
		return null;
	}

	public Item getQuickSlotItem(int idx)
	{
		int itemID = quickSlots[idx];
		return itemID != -1 ? Item.Get(itemID) : null;
	}

	public SpellSlot getSpellSlot(ItemSlot staff)
	{
		if (spellSlots.ContainsKey(staff))
		{
			SpellSet spellSet = spellSlots[staff];
			SpellSlot spellSlot = spellSet.slots[spellSet.selectedSlot];
			return spellSlot;
		}
		return null;
	}

	public void consumeArrow()
	{
		//Debug.Assert(arrows.Count > 0);
		//removeItem(arrows[0]);
	}

	public int totalArrowCount
	{
		get
		{
			int result = 0;
			for (int i = 0; i < arrows.Count; i++)
				result += arrows[i].stackSize;
			return result;
		}
	}
}
