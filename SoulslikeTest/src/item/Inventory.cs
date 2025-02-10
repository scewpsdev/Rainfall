using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
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
		Player player = ((Inventory)container).player;
		player.setHandItem(handID, item);

		if (item != null && item.equipSound != null)
			player.playSoundOrganic(item.equipSound);
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

		if (item != null && item.equipSound != null)
		{
			Player player = ((Inventory)container).player;
			player.playSoundOrganic(item.equipSound);
		}
	}

	public override bool canPlaceItem(Item item)
	{
		return item.category == ItemCategory.Armor && item.armorType == armorType;
	}
}

public class ConsumableSlot : ItemSlot
{
	public ConsumableSlot(Inventory inventory, int index)
		: base(inventory, index)
	{
	}

	public override void setItem(Item item)
	{
		base.setItem(item);

		if (item != null && item.equipSound != null)
		{
			Player player = ((Inventory)container).player;
			player.playSoundOrganic(item.equipSound);
		}
	}

	public override bool canPlaceItem(Item item)
	{
		return item.category == ItemCategory.Consumable;
	}
}

public class SpellSlot : ItemSlot
{
	public SpellSlot(Inventory inventory, int index)
		: base(inventory, index)
	{
	}

	public override void setItem(Item item)
	{
		base.setItem(item);

		if (item != null && item.equipSound != null)
		{
			Player player = ((Inventory)container).player;
			player.playSoundOrganic(item.equipSound);
		}
	}

	public override bool canPlaceItem(Item item)
	{
		return item.category == ItemCategory.Spell;
	}
}

public class ArrowSlot : ItemSlot
{
	public ArrowSlot(Inventory inventory)
		: base(inventory, 0)
	{
	}

	public override void setItem(Item item)
	{
		base.setItem(item);

		if (item != null && item.equipSound != null)
		{
			Player player = ((Inventory)container).player;
			player.playSoundOrganic(item.equipSound);
		}
	}

	public override bool canPlaceItem(Item item)
	{
		return item.category == ItemCategory.Arrow;
	}
}

public class Inventory : ItemContainer
{
	const int WIDTH = 6;
	const int HEIGHT = 5;


	public readonly Player player;

	public HandSlot[] rightHand = new HandSlot[2];
	public HandSlot[] leftHand = new HandSlot[2];
	public ArmorSlot[] armor = new ArmorSlot[4];
	public ConsumableSlot[] hotbar = new ConsumableSlot[6];
	public SpellSlot[] spells = new SpellSlot[6];
	public ArrowSlot arrows;

	public int twoHandedWeapon = -1;
	public int rightHandSlotIdx = 0;
	public int leftHandSlotIdx = 0;
	public int quickSlotIdx = 0;
	public int spellSlotIdx = 0;


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
			hotbar[i] = new ConsumableSlot(this, i);
		for (int i = 0; i < spells.Length; i++)
			spells[i] = new SpellSlot(this, i);
		arrows = new ArrowSlot(this);
	}

	ItemSlot getItemSlotWithItem(Item item)
	{
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
		for (int i = 0; i < spells.Length; i++)
		{
			if (spells[i].item == item)
				return spells[i];
		}
		if (arrows.item == item)
			return arrows;
		return null;
	}

	public override ItemSlot addItem(Item item, int amount = 1)
	{
		if (item.category == ItemCategory.Weapon)
		{
			if ((item.weaponType == WeaponType.Melee || item.weaponType == WeaponType.Bow || item.weaponType == WeaponType.Staff) &&
				rightHand[0].item == null)
				return equipItem(item, amount);
			if (item.weaponType == WeaponType.Shield &&
				leftHand[0].item == null)
				return equipItem(item, amount);
		}
		if (item.category == ItemCategory.Armor)
		{
			if (armor[(int)item.armorType - 1].item == null)
				return equipItem(item, amount);
		}
		if (item.category == ItemCategory.Consumable)
		{
			if (findItem(item) == null)
				return equipItem(item, amount);
		}
		if (item.category == ItemCategory.Spell)
			return equipItem(item, amount);
		if (item.category == ItemCategory.Arrow)
		{
			if (arrows.item == null)
				return equipItem(item, amount);
		}

		if (item.stackable)
		{
			ItemSlot existingSlot = getItemSlotWithItem(item);
			if (existingSlot != null)
			{
				existingSlot.stackSize += amount;
				return existingSlot;
			}
		}

		return base.addItem(item, amount);
	}

	public ItemSlot equipItem(Item item, int amount)
	{
		switch (item.category)
		{
			case ItemCategory.Weapon:
				switch (item.weaponType)
				{
					case WeaponType.Melee:
					case WeaponType.Bow:
					case WeaponType.Staff:
						return addHandItem(0, item, amount);
					case WeaponType.Shield:
						return addHandItem(1, item, amount);
				}
				break;
			case ItemCategory.Consumable:
				return addHotbarItem(item, amount);
			case ItemCategory.Armor:
				return addArmorItem((int)item.armorType - 1, item, amount);
			case ItemCategory.Arrow:
				arrows.setItem(item);
				arrows.stackSize = amount;
				return arrows;
			case ItemCategory.Spell:
				return addSpellItem(item, amount);
			case ItemCategory.Key:
				break;
			default:
				Debug.Assert(false);
				break;
		}
		return null;
	}

	public void equipItem(ItemSlot slot)
	{
		switch (slot.item.category)
		{
			case ItemCategory.Weapon:
				switch (slot.item.weaponType)
				{
					case WeaponType.Melee:
					case WeaponType.Bow:
					case WeaponType.Staff:
						//if (rightHand[0].item == null || leftHand[0].item != null)
						swapSlots(rightHand[0], slot);
						//else if (leftHand[0].item == null)
						//	swapSlots(leftHand[0], slot);
						break;
					case WeaponType.Shield:
						//if (leftHand[0].item == null || rightHand[0].item != null)
						swapSlots(leftHand[0], slot);
						//else if (rightHand[0].item == null)
						//	swapSlots(rightHand[0], slot);
						break;
				}
				break;
			case ItemCategory.Consumable:
				addHotbarItem(slot.item, slot.stackSize);
				slot.setItem(null);
				slot.stackSize = 0;
				break;
			case ItemCategory.Armor:
				swapSlots(armor[(int)slot.item.armorType - 1], slot);
				break;
			case ItemCategory.Arrow:
				swapSlots(arrows, slot);
				break;
			case ItemCategory.Spell:
				addSpellItem(slot.item, slot.stackSize);
				slot.setItem(null);
				slot.stackSize = 0;
				break;
			case ItemCategory.Key:
				break;
			default:
				Debug.Assert(false);
				break;
		}
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

	public ItemSlot addSpellItem(Item item, int amount)
	{
		if (item.stackable)
		{
			for (int i = 0; i < spells.Length; i++)
			{
				if (spells[i].item == item)
				{
					spells[i].stackSize += amount;
					return spells[i];
				}
			}
		}
		for (int i = 0; i < spells.Length; i++)
		{
			if (spells[i].item == null)
			{
				spells[i].setItem(item);
				spells[i].stackSize = amount;
				return spells[i];
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

	public void cycleHotbar()
	{
		for (int i = 0; i < hotbar.Length; i++)
		{
			int ii = (quickSlotIdx + 1 + i) % hotbar.Length;
			ItemSlot slot = hotbar[ii];
			if (slot.item != null)
			{
				quickSlotIdx = ii;
				break;
			}
		}
	}

	public void cycleSpells()
	{
		for (int i = 0; i < spells.Length; i++)
		{
			int ii = (spellSlotIdx + 1 + i) % spells.Length;
			ItemSlot slot = spells[ii];
			if (slot.item != null)
			{
				spellSlotIdx = ii;
				break;
			}
		}
	}

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

	public ItemSlot getCurrentHotbarSlot()
	{
		return getHotbarSlot(quickSlotIdx);
	}

	public ItemSlot getHotbarSlot(int idx)
	{
		return hotbar[idx];
	}

	public ItemSlot getSpellSlot(int idx)
	{
		return spells[idx];
	}

	public ItemSlot getCurrentSpellSlot()
	{
		return getSpellSlot(spellSlotIdx);
	}

	public void consumeArrow()
	{
		removeItem(arrows, 1);
	}

	public float getArmorProtection()
	{
		float damageMultiplier = 1;
		for (int i = 0; i < armor.Length; i++)
		{
			if (armor[i].item != null)
				damageMultiplier *= armor[i].item.getAbsorptionDamageModifier();
		}
		return damageMultiplier;
	}

	public int numGold
	{
		get
		{
			//ItemSlot slot = findItem(Item.Get("gold"));
			//if (slot != null)
			//	return slot.stackSize;
			return 0;
		}
	}
}
