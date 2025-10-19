using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ItemSlot
{
	public readonly ItemContainer container;
	public readonly int index;
	public readonly int gridX, gridY;
	public Item item { get; private set; }
	public int stackSize;

	public ItemSlot(ItemContainer container, int index, int gridX = -1, int gridY = -1)
	{
		this.container = container;
		this.index = index;
		this.gridX = gridX;
		this.gridY = gridY;
		item = null;
		stackSize = 0;
	}

	public virtual void setItem(Item item)
	{
		this.item = item;
	}

	public virtual bool canPlaceItem(Item item)
	{
		return true;
	}
}

public class HandSlot : ItemSlot
{
	public readonly int handID;

	public HandSlot(Inventory inventory, int index, int handID)
		: base(inventory, index)
	{
		this.handID = handID;
	}

	public override void setItem(Item item)
	{
		base.setItem(item);
		((Inventory)container).player.onHandItemUpdate(this, handID);
	}

	public override bool canPlaceItem(Item item)
	{
		return item.moveset != null;
	}
}

public class ArmorSlot : ItemSlot
{
	ArmorType armorType;

	public ArmorSlot(Inventory inventory, int index, ArmorType armorType)
		: base(inventory, index)
	{
		this.armorType = armorType;
	}

	public override void setItem(Item item)
	{
		base.setItem(item);
	}

	public override bool canPlaceItem(Item item)
	{
		return item.category == ItemCategory.Armor && item.armorType == armorType;
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

public class Inventory : ItemContainer
{
	const int WIDTH = 10;
	const int HEIGHT = 6;


	public readonly Player player;

	public HandSlot[] rightHand = new HandSlot[2];
	public HandSlot[] leftHand = new HandSlot[2];
	public ArmorSlot[] armor = new ArmorSlot[4];
	public ItemSlot[] hotbar = new ItemSlot[4];

	/*
	public List<ItemSlot> weapons = new List<ItemSlot>();
	public List<ItemSlot> shields = new List<ItemSlot>();
	public List<ItemSlot> utilities = new List<ItemSlot>();
	public List<ItemSlot> consumables = new List<ItemSlot>();
	public List<ItemSlot> arrows = new List<ItemSlot>();
	public List<ItemSlot> collectibles = new List<ItemSlot>();
	public List<ItemSlot> spells = new List<ItemSlot>();

	public ItemSlot[] rightHandSlots = new ItemSlot[2];
	public ItemSlot[] leftHandSlots = new ItemSlot[2];
	public int[] quickSlots = new int[10] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
	*/

	public Dictionary<ItemSlot, SpellSet> spellSlots = new Dictionary<ItemSlot, SpellSet>();

	public int twoHandedWeapon = -1;
	public int rightHandSlotIdx = 0;
	public int leftHandSlotIdx = 0;
	public int quickSlotIdx = 0;


	/*
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
			case ItemCategory.Collectible:
				return collectibles;
			case ItemCategory.Spell:
				return spells;
			default:
				Debug.Assert(false);
				return null;
		}
	}
	*/

	public Inventory(Player player)
		: base(WIDTH, HEIGHT)
	{
		this.player = player;

		for (int i = 0; i < rightHand.Length; i++)
			rightHand[i] = new HandSlot(this, i, 0);
		for (int i = 0; i < leftHand.Length; i++)
			leftHand[i] = new HandSlot(this, i, 1);
		for (int i = 0; i < armor.Length; i++)
			armor[i] = new ArmorSlot(this, i, ArmorType.Helmet + i);
		for (int i = 0; i < hotbar.Length; i++)
			hotbar[i] = new ItemSlot(this, i);
	}

	/*
	void updateItemSlotIndices(List<ItemSlot> slots, int startIndex)
	{
		for (int i = startIndex; i < slots.Count; i++)
		{
			slots[i].index = i;
		}
	}
	*/

	public override ItemSlot addItem(Item item, int amount = 1)
	{
		if (item.category == ItemCategory.Weapon && rightHand[0].item == null)
			return addHandItem(0, 0, item, amount);
		else if (item.category == ItemCategory.Weapon && rightHand[1].item == null)
			return addHandItem(0, 1, item, amount);
		else if (item.category == ItemCategory.Utility && leftHand[0].item == null)
			return addHandItem(1, 0, item, amount);
		else if (item.category == ItemCategory.Utility && leftHand[1].item == null)
			return addHandItem(1, 1, item, amount);
		else if (item.category == ItemCategory.Shield && leftHand[0].item == null)
			return addHandItem(1, 0, item, amount);
		else if (item.category == ItemCategory.Shield && leftHand[1].item == null)
			return addHandItem(1, 1, item, amount);
		else if (item.category == ItemCategory.Armor && item.armorType == ArmorType.Torso && armor[1].item == null)
			return addArmorItem(1, item, amount);
		else if (item.category == ItemCategory.Consumable)
		{
			if (item.stackable)
			{
				for (int i = 0; i < hotbar.Length; i++)
				{
					if (hotbar[i].item == item)
						return addHotbarItem(i, item, amount);
				}
			}
			for (int i = 0; i < hotbar.Length; i++)
			{
				if (hotbar[i].item == null)
					return addHotbarItem(i, item, amount);
			}
		}
		return base.addItem(item, amount);
	}

	public ItemSlot addHandItem(int handID, int idx, Item item, int amount)
	{
		ItemSlot slot = (handID == 0 ? rightHand : leftHand)[idx];
		slot.setItem(item);
		slot.stackSize = amount;
		return slot;
	}

	public ItemSlot addHandItem(int handID, Item item, int amount)
	{
		ItemSlot[] handItems = (handID == 0 ? rightHand : leftHand);
		int idx = handItems[0] != null ? 0 : handItems[1] != null ? 1 : -1;
		if (idx != -1)
		{
			handItems[idx].setItem(item);
			handItems[idx].stackSize = amount;
			return handItems[idx];
		}
		return null;
	}

	public ItemSlot addHotbarItem(int idx, Item item, int amount)
	{
		ItemSlot slot = hotbar[idx];
		if (slot.item == null)
		{
			slot.setItem(item);
			slot.stackSize = amount;
			return slot;
		}
		else if (slot.item == item && item.stackable)
		{
			slot.stackSize += amount;
			return slot;
		}
		return null;
	}

	public ItemSlot addHotbarItem(Item item, int amount)
	{
		if (item.stackable)
		{
			for (int i = 0; i < hotbar.Length; i++)
			{
				if (hotbar[i].item == item)
				{
					hotbar[i].stackSize += amount;
					return hotbar[i];
				}
			}
		}
		for (int i = 0; i < hotbar.Length; i++)
		{
			if (hotbar[i].item == null)
			{
				hotbar[i].setItem(item);
				hotbar[i].stackSize = amount;
				return hotbar[i];
			}
		}
		return null;
	}

	public ItemSlot addArmorItem(int idx, Item item, int amount)
	{
		ItemSlot slot = armor[idx];
		slot.setItem(item);
		slot.stackSize = amount;
		return slot;
	}

	/*
	public void selectHandItem(int handID, int itemIdx, ItemSlot item)
	{
		for (int i = 0; i < rightHand.Length; i++)
		{
			if (rightHand[i].item == item)
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
	*/

	public ItemSlot getSelectedHandSlot(int handID)
	{
		return handID == 0 ? rightHand[rightHandSlotIdx] : leftHand[leftHandSlotIdx];
	}

	public Item getSelectedHandItem(int handID)
	{
		ItemSlot slot = getSelectedHandSlot(handID);
		return slot != null ? slot.item : null;
	}

	public bool hasItemEquipped(Item item, out ItemSlot slot)
	{
		Item rightItem = getSelectedHandItem(0);
		Item leftItem = getSelectedHandItem(1);
		if (rightItem == item)
		{
			slot = getSelectedHandSlot(0);
			return true;
		}
		else if (leftItem == item)
		{
			slot = getSelectedHandSlot(1);
			return true;
		}
		slot = null;
		return false;
	}

	public bool hasItemEquipped(Item item)
	{
		return hasItemEquipped(item, out _);
	}

	public bool hasItemInHand(Item item, out ItemSlot slot)
	{
		for (int i = 0; i < rightHand.Length; i++)
		{
			if (rightHand[i].item == item)
			{
				slot = rightHand[i];
				return true;
			}
		}
		for (int i = 0; i < leftHand.Length; i++)
		{
			if (leftHand[i].item == item)
			{
				slot = leftHand[i];
				return true;
			}
		}
		slot = null;
		return false;
	}

	public bool hasItemInOffhand(Item item)
	{
		if (hasItemInHand(item, out ItemSlot slot))
		{
			if (!hasItemEquipped(item))
				return true;
			else if (twoHandedWeapon != -1 && getSelectedHandSlot(twoHandedWeapon) != slot)
				return true;
			else
			{
				Item leftItem = getSelectedHandItem(1);
				if (leftItem != null && leftItem != item && leftItem.twoHanded)
					return true;

				Item rightItem = getSelectedHandItem(0);
				if (rightItem != null && rightItem != item && rightItem.twoHanded)
					return true;
			}
		}
		return false;
	}

	public ItemSlot findItemOfType(ItemCategory category)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].item != null && items[i].item.category == category)
				return items[i];
		}
		for (int i = 0; i < rightHand.Length; i++)
		{
			if (rightHand[i].item != null && rightHand[i].item.category == category)
				return rightHand[i];
		}
		for (int i = 0; i < leftHand.Length; i++)
		{
			if (leftHand[i].item != null && leftHand[i].item.category == category)
				return leftHand[i];
		}
		for (int i = 0; i < armor.Length; i++)
		{
			if (armor[i].item != null && armor[i].item.category == category)
				return armor[i];
		}
		for (int i = 0; i < hotbar.Length; i++)
		{
			if (hotbar[i].item != null && hotbar[i].item.category == category)
				return hotbar[i];
		}
		return null;
	}

	public ItemSlot findItem(Item item)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].item == item)
				return items[i];
		}
		for (int i = 0; i < rightHand.Length; i++)
		{
			if (rightHand[i].item == item)
				return rightHand[i];
		}
		for (int i = 0; i < leftHand.Length; i++)
		{
			if (leftHand[i].item == item)
				return leftHand[i];
		}
		for (int i = 0; i < armor.Length; i++)
		{
			if (armor[i].item == item)
				return armor[i];
		}
		for (int i = 0; i < hotbar.Length; i++)
		{
			if (hotbar[i].item == item)
				return hotbar[i];
		}
		return null;
	}

	/*
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
	*/

	public ItemSlot getCurrentQuickSlot()
	{
		return getQuickSlot(quickSlotIdx);
	}

	public Item getCurrentQuickSlotItem()
	{
		ItemSlot slot = hotbar[quickSlotIdx];
		return slot.item != null ? slot.item : null;
	}

	public ItemSlot getQuickSlot(int idx)
	{
		return hotbar[idx];
		/*
		if (itemID != -1)
		{
			Item item = Item.Get(itemID);
			List<ItemSlot> items = getItemListForCategory(item.category);
			return getItemSlotWithItem(item, items);
		}
		return null;
		*/
	}

	public Item getQuickSlotItem(int idx)
	{
		ItemSlot slot = hotbar[idx];
		return slot.item != null ? slot.item : null;
	}

	public ItemSlot getSpellSlot(ItemSlot staff)
	{
		return findItemOfType(ItemCategory.Spell);
		/*
		if (spellSlots.ContainsKey(staff))
		{
			SpellSet spellSet = spellSlots[staff];
			SpellSlot spellSlot = spellSet.slots[spellSet.selectedSlot];
			return spellSlot;
		}
		return null;
		*/
	}

	public void consumeArrow()
	{
		//Debug.Assert(arrows.Count > 0);
		removeItem(Item.Get("arrow"));
	}

	public int totalArrowCount
	{
		get
		{
			int result = 0;
			for (int i = 0; i < items.Length; i++)
			{
				if (items[i].item != null && items[i].item.category == ItemCategory.Arrow)
					result += items[i].stackSize;
			}
			return result;
		}
	}
}
