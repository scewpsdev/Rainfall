﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;


public class InventoryUI
{
	Player player;

	bool inventoryOpen = false;
	ItemContainerEntity openContainer = null;
	int currentUIScale = 2;

	Item grabbedItem = null;
	int grabbedItemStackSize = 0;
	Vector2i grabbedItemOffset = Vector2i.Zero;

	//Vector2 hoveredSlotPosition = new Vector2(-100);
	ItemSlot hoveredSlot = null;
	Vector2 hoveredSlotFract = new Vector2(0.0f);

	int tooltipCurrentContentHeight = 400;

	bool equipmentTabSelectScreenOpen = false;
	int equipmentTabSelectScreenSelectedItem = 0;
	List<ItemSlot> equipmentTabSelectScreenSlots = new List<ItemSlot>();
	ItemSlot equipmentTabSelectScreenResult = null;

	int equipmentTabOverviewSelectedSlot = 0;

	Texture equipmentTabIcon;
	Texture equipmentHandRight, equipmentHandLeft;
	Texture leftHand, rightHand;

	Font mediumFont;
	Font stackSizeFont;
	Font tooltipFontBig, tooltipFontMedium;


	public InventoryUI(Player player)
	{
		this.player = player;

		equipmentTabIcon = Resource.GetTexture("res/texture/ui/inventory_tab_equipment.png");
		equipmentHandRight = Resource.GetTexture("res/texture/ui/equipment_hand_right.png");
		equipmentHandLeft = Resource.GetTexture("res/texture/ui/equipment_hand_left.png");
		leftHand = Resource.GetTexture("res/texture/ui/hand_left.png");
		rightHand = Resource.GetTexture("res/texture/ui/hand_right.png");

		mediumFont = FontManager.GetFont("baskerville", 24, true);
		stackSizeFont = FontManager.GetFont("baskerville", 20, true);
		tooltipFontBig = FontManager.GetFont("baskerville", 24, true);
		tooltipFontMedium = FontManager.GetFont("baskerville", 18, true);
	}

	public void update()
	{
		currentUIScale = Display.height / 450;
	}

	/*
	void openItemSelectScreen(params ItemCategory[] categories)
	{
		equipmentTabSelectScreenOpen = true;
		equipmentTabSelectScreenSelectedItem = 0;

		equipmentTabSelectScreenSlots.Clear();
		foreach (ItemCategory category in categories)
		{
			List<ItemSlot> slots = player.inventory.getItemListForCategory(category);
			equipmentTabSelectScreenSlots.AddRange(slots);
		}
	}
	*/

	public void openChestUI(ItemContainerEntity container)
	{
		openContainer = container;
		player.setCursorLocked(false);
	}

	void drawEquipmentTabOverview(int x, int y)
	{
		int padding = 4;
		int slotSize = 128;

		int totalSlotIndex = 0;

		if (equipmentTabOverviewSelectedSlot >= 0 && equipmentTabOverviewSelectedSlot < 2)
		{
			if (Input.IsKeyPressed(KeyCode.Down))
				equipmentTabOverviewSelectedSlot = (equipmentTabOverviewSelectedSlot + 2) % 4;
			else if (Input.IsKeyPressed(KeyCode.Up))
				equipmentTabOverviewSelectedSlot = (equipmentTabOverviewSelectedSlot + 2) % 4;
			else if (Input.IsKeyPressed(KeyCode.Right))
				equipmentTabOverviewSelectedSlot = (equipmentTabOverviewSelectedSlot + 1) % 2;
			else if (Input.IsKeyPressed(KeyCode.Left))
				equipmentTabOverviewSelectedSlot = (equipmentTabOverviewSelectedSlot + 1) % 2;
		}
		else if (equipmentTabOverviewSelectedSlot >= 2 && equipmentTabOverviewSelectedSlot < 4)
		{
			if (Input.IsKeyPressed(KeyCode.Down))
				equipmentTabOverviewSelectedSlot = (equipmentTabOverviewSelectedSlot + 2) % 4;
			else if (Input.IsKeyPressed(KeyCode.Up))
				equipmentTabOverviewSelectedSlot = (equipmentTabOverviewSelectedSlot + 2) % 4;
			else if (Input.IsKeyPressed(KeyCode.Right))
				equipmentTabOverviewSelectedSlot = (equipmentTabOverviewSelectedSlot - 2 + 1) % 2 + 2;
			else if (Input.IsKeyPressed(KeyCode.Left))
				equipmentTabOverviewSelectedSlot = (equipmentTabOverviewSelectedSlot - 2 + 1) % 2 + 2;
		}

		var drawHandItems = (int x, int y, ItemSlot[] slots, Texture icon, int handID) =>
		{
			int slotY = y + padding;
			int handIconSize = 80;

			GUI.Texture(x + 2 * padding, slotY + (slotSize - handIconSize) / 2, handIconSize, handIconSize, icon);

			for (int i = 0; i < slots.Length; i++)
			{
				int slotX = x + 2 * padding + handIconSize + i * (slotSize + padding);

				if (totalSlotIndex == equipmentTabOverviewSelectedSlot && equipmentTabSelectScreenResult != null)
				{
					//if (player.equipHandItem(handID, i, equipmentTabSelectScreenResult))
					//	equipmentTabSelectScreenResult = null;
				}

				bool hovered = Input.IsHovered(slotX, slotY, slotSize, slotSize);
				if (hovered && Input.cursorHasMoved)
					equipmentTabOverviewSelectedSlot = totalSlotIndex;
				bool selected = totalSlotIndex == equipmentTabOverviewSelectedSlot;
				if (selected)
				{
					if (hovered && Input.IsMouseButtonPressed(MouseButton.Left) || Input.IsKeyPressed(KeyCode.E))
					{
						//openItemSelectScreen(ItemCategory.Weapon, ItemCategory.Shield, ItemCategory.Utility, ItemCategory.Consumable);
					}
					else if (Input.IsKeyPressed(KeyCode.F))
					{
						//if (player.equipHandItem(handID, i, null))
						//{ }
					}
				}

				GUI.Rect(slotX, slotY, slotSize, slotSize, selected ? 0xff444444 : 0xff222222);
				if (slots[i] != null)
				{
					GUI.Texture(slotX, slotY, slotSize, slotSize, slots[i].item.icon);
				}

				totalSlotIndex++;
			}
		};

		//drawHandItems(x, y, player.inventory.rightHandSlots, equipmentHandRight, 0);
		//drawHandItems(x, y + padding + slotSize, player.inventory.leftHandSlots, equipmentHandLeft, 1);
	}

	int isEquippedRightHand(ItemSlot slot)
	{
		/*
		for (int i = 0; i < player.inventory.rightHandSlots.Length; i++)
		{
			if (player.inventory.rightHandSlots[i] == slot)
				return i;
		}
		*/
		return -1;
	}

	int isEquippedLeftHand(ItemSlot slot)
	{
		/*
		for (int i = 0; i < player.inventory.leftHandSlots.Length; i++)
		{
			if (player.inventory.leftHandSlots[i] == slot)
				return i;
		}
		*/
		return -1;
	}

	void drawItemSlot(int x, int y, int width, int height, ItemSlot slot, bool selected)
	{
		int iconSize = 80;

		GUI.Rect(x, y, width, height, selected ? 0xff444444 : 0xff222222);
		GUI.Texture(x, y, iconSize, iconSize, slot.item.icon);

		GUI.Text(x + iconSize + 24, y + height / 2 - (int)mediumFont.size / 2, 1.0f, slot.item.displayName, mediumFont, 0xffcccccc);

		if (slot.item.stackable)
			GUI.Text(x + width - iconSize - 24 - 8, y + height / 2 - (int)mediumFont.size / 2, 1.0f, slot.stackSize.ToString(), mediumFont, 0xffcccccc);

		int rightHandSlot = isEquippedRightHand(slot);
		int leftHandSlot = isEquippedLeftHand(slot);

		Debug.Assert(!(rightHandSlot != -1 && leftHandSlot != -1));

		if (rightHandSlot != -1)
			GUI.Texture(x + width - iconSize - 8, y, iconSize, iconSize, rightHand);
		else if (leftHandSlot != -1)
			GUI.Texture(x + width - iconSize - 8, y, iconSize, iconSize, leftHand);
	}

	ItemSlot drawEquipmentTabItemSelectScreen(int x, int y)
	{
		int padding = 8;

		int slotX = x + 2 * padding;
		int slotY = y + 2 * padding;

		int slotWidth = 1000;
		int slotHeight = 80;

		int totalItemIndex = 0;
		//int totalItemCount = player.inventory.weapons.Count + player.inventory.shields.Count + player.inventory.utilities.Count + player.inventory.consumables.Count;

		/*
		if (Input.IsKeyPressed(KeyCode.Down))
			equipmentTabSelectScreenSelectedItem = (equipmentTabSelectScreenSelectedItem + 1) % totalItemCount;
		else if (Input.IsKeyPressed(KeyCode.Up))
			equipmentTabSelectScreenSelectedItem = (equipmentTabSelectScreenSelectedItem - 1 + totalItemCount) % totalItemCount;
		*/

		if (Input.IsKeyPressed(KeyCode.Esc) || Input.IsKeyPressed(KeyCode.Q))
		{
			equipmentTabSelectScreenOpen = false;
			return null;
		}

		for (int i = 0; i < equipmentTabSelectScreenSlots.Count; i++)
		{
			bool hovered = Input.IsHovered(slotX, slotY, slotWidth, slotHeight);
			if (hovered && Input.cursorHasMoved)
				equipmentTabSelectScreenSelectedItem = totalItemIndex;
			bool selected = totalItemIndex == equipmentTabSelectScreenSelectedItem;
			if (selected)
			{
				if (hovered && Input.IsMouseButtonPressed(MouseButton.Left) || Input.IsKeyPressed(KeyCode.E))
					return equipmentTabSelectScreenSlots[i];
			}

			drawItemSlot(slotX, slotY, slotWidth, slotHeight, equipmentTabSelectScreenSlots[i], totalItemIndex == equipmentTabSelectScreenSelectedItem);
			slotY += slotHeight + padding;
			totalItemIndex++;
		}

		return null;
	}

	void drawEquipmentTab(int x, int y)
	{
		if (equipmentTabSelectScreenOpen)
		{
			ItemSlot selectedSlot = drawEquipmentTabItemSelectScreen(x, y);
			if (selectedSlot != null)
			{
				equipmentTabSelectScreenOpen = false;
				equipmentTabSelectScreenResult = selectedSlot;
			}
		}
		else
		{
			drawEquipmentTabOverview(x, y);
		}
	}

	void grabItem(Item item, int amount)
	{
		grabbedItem = item;
		grabbedItemStackSize = amount;
		//grabbedItemOffset = new Vector2i((gridX - topLeftSlot.gridX) * 64 + (Input.cursorPosition.x - x), (gridY - topLeftSlot.gridY) * 64 + (Input.cursorPosition.y - y));
		if (item != null)
			grabbedItemOffset = item.inventorySize * 32 * currentUIScale / 2;
		else
			grabbedItemOffset = Vector2i.Zero;
	}

	void drawItemSlotGrid(int x, int y, int width, int height, ItemSlot slot, int gridX, int gridY, ItemContainer container, ItemContainer lootDst)
	{
		bool isHovered = hoveredSlot == slot;
		bool isHighlighted = isHovered;
		ItemSlot topLeftSlot = container.getItemAtPos(gridX, gridY);
		if (topLeftSlot != null)
		{
			if (hoveredSlot != null && hoveredSlot.container == container && hoveredSlot.gridX != -1 &&
				hoveredSlot.gridX >= topLeftSlot.gridX && hoveredSlot.gridX < topLeftSlot.gridX + topLeftSlot.item.inventorySize.x &&
				hoveredSlot.gridY >= topLeftSlot.gridY && hoveredSlot.gridY < topLeftSlot.gridY + topLeftSlot.item.inventorySize.y)
			{
				isHighlighted = true;
			}
		}
		if (grabbedItem != null && hoveredSlot != null && hoveredSlot.container == container && hoveredSlot.gridX != -1)
		{
			int x0 = (int)MathF.Round(hoveredSlot.gridX + hoveredSlotFract.x - grabbedItemOffset.x / (float)width);
			int x1 = x0 + grabbedItem.inventorySize.x - 1;
			int y0 = (int)MathF.Round(hoveredSlot.gridY + hoveredSlotFract.y - grabbedItemOffset.y / (float)height);
			int y1 = y0 + grabbedItem.inventorySize.y - 1;
			if (gridX >= x0 && gridX <= x1 && gridY >= y0 && gridY <= y1)
				isHighlighted = true;
			else
				isHighlighted = false;
		}
		if (topLeftSlot != null && isHovered && Input.IsMouseButtonPressed(MouseButton.Left) && grabbedItem == null)
		{
			if (lootDst != null)
			{
				ItemSlot addedTo = lootDst.addItem(topLeftSlot.item, topLeftSlot.stackSize);
				if (addedTo == null)
					grabItem(topLeftSlot.item, topLeftSlot.stackSize);
			}
			else
			{
				grabItem(topLeftSlot.item, topLeftSlot.stackSize);
			}
			topLeftSlot.setItem(null);
			topLeftSlot.stackSize = 0;
		}
		else if (isHovered && Input.IsMouseButtonPressed(MouseButton.Left) && grabbedItem != null)
		{
			int x0 = (int)MathF.Round(hoveredSlot.gridX + hoveredSlotFract.x - grabbedItemOffset.x / (float)width);
			int x1 = x0 + grabbedItem.inventorySize.x - 1;
			int y0 = (int)MathF.Round(hoveredSlot.gridY + hoveredSlotFract.y - grabbedItemOffset.y / (float)height);
			int y1 = y0 + grabbedItem.inventorySize.y - 1;

			if (x0 >= 0 && x1 < container.width && y0 >= 0 && y1 < container.height)
			{
				List<ItemSlot> overlappingItems = new List<ItemSlot>();
				for (int xx = x0; xx <= x1; xx++)
				{
					for (int yy = y0; yy <= y1; yy++)
					{
						ItemSlot item = container.getItemAtPos(xx, yy);
						if (item != null && !overlappingItems.Contains(item))
							overlappingItems.Add(item);
					}
				}

				ItemSlot targetSlot = container.items[x0 + y0 * container.width];
				if (overlappingItems.Count == 0)
				{
					targetSlot.setItem(grabbedItem);
					targetSlot.stackSize = grabbedItemStackSize;
					grabItem(null, 0);
				}
				else if (overlappingItems.Count == 1)
				{
					if (overlappingItems[0].item == grabbedItem && grabbedItem.stackable)
					{
						overlappingItems[0].stackSize += grabbedItemStackSize;
						grabItem(null, 0);
					}
					else
					{
						Item tmp = overlappingItems[0].item;
						int tmp2 = overlappingItems[0].stackSize;
						overlappingItems[0].setItem(null);
						overlappingItems[0].stackSize = 0;
						targetSlot.setItem(grabbedItem);
						targetSlot.stackSize = grabbedItemStackSize;
						grabItem(tmp, tmp2);
					}
				}
			}
		}

		uint background = isHighlighted ? 0xFF666666 : 0xFF333333;
		GUI.Rect(x + 1, y + 1, width - 2, height - 2, background);
		if (slot.item != null)
		{
			int iconSize = Math.Max(slot.item.inventorySize.x, slot.item.inventorySize.y);
			int iconWidth = iconSize * width;
			int iconHeight = iconSize * height;
			int xoffset = (slot.item.inventorySize.x * width) / 2 - iconWidth / 2;
			int yoffset = (slot.item.inventorySize.y * height) / 2 - iconHeight / 2;
			GUI.Texture(x + xoffset, y + yoffset, iconWidth, iconHeight, slot.item.icon);
			if (slot.item.stackable && slot.stackSize > 1)
			{
				string stackSizeStr = slot.stackSize.ToString();
				int stackSizeWidth = stackSizeFont.measureText(stackSizeStr);
				GUI.Text(x + iconWidth - stackSizeWidth, y + iconHeight - (int)stackSizeFont.size, 1.0f, stackSizeStr, stackSizeFont, 0xffaaaaaa);
			}
		}
	}

	void drawItemSlot(int x, int y, int width, int height, ItemSlot slot)
	{
		bool isHovered = Input.IsHovered(x, y, width, height);
		if (slot.item != null && isHovered && Input.IsMouseButtonPressed(MouseButton.Left) && grabbedItem == null)
		{
			grabItem(slot.item, slot.stackSize);
			slot.setItem(null);
			slot.stackSize = 0;
		}
		else if (isHovered && Input.IsMouseButtonPressed(MouseButton.Left) && grabbedItem != null)
		{
			if (slot.canPlaceItem(grabbedItem))
			{
				if (slot.item == null)
				{
					slot.setItem(grabbedItem);
					slot.stackSize = grabbedItemStackSize;
					grabItem(null, 0);
				}
				else
				{
					if (slot.item == grabbedItem && grabbedItem.stackable)
					{
						slot.stackSize += grabbedItemStackSize;
						grabItem(null, 0);
					}
					else
					{
						Item tmp = slot.item;
						int tmp2 = slot.stackSize;
						slot.setItem(grabbedItem);
						slot.stackSize = grabbedItemStackSize;
						grabItem(tmp, tmp2);
					}
				}
			}
		}

		uint background = isHovered ? 0xFF666666 : 0xFF333333;
		GUI.Rect(x, y, width, height, background);
		if (slot.item != null)
		{
			GUI.Texture(x, y, width, height, slot.item.icon);
			if (slot.item.stackable && slot.stackSize > 1)
			{
				string stackSizeStr = slot.stackSize.ToString();
				int stackSizeWidth = stackSizeFont.measureText(stackSizeStr);
				GUI.Text(x + width - stackSizeWidth, y + height - (int)stackSizeFont.size, 1.0f, stackSizeStr, stackSizeFont, 0xffaaaaaa);
			}
		}
	}

	void drawGrabbedItem(int paneX, int paneY, int paneWidth, int paneHeight)
	{
		// Grabbed item
		{
			if (grabbedItem != null)
			{
				int slotSize = 32 * currentUIScale;

				Vector2i cursorPosition = Input.cursorPosition;
				int xx = cursorPosition.x - grabbedItemOffset.x;
				int yy = cursorPosition.y - grabbedItemOffset.y;
				int iconSize = Math.Max(grabbedItem.inventorySize.x, grabbedItem.inventorySize.y) * slotSize;

				int xoffset = (grabbedItem.inventorySize.x * slotSize) / 2 - iconSize / 2;
				int yoffset = (grabbedItem.inventorySize.y * slotSize) / 2 - iconSize / 2;

				GUI.Texture(xx + xoffset, yy + yoffset, iconSize, iconSize, grabbedItem.icon);
				if (grabbedItem.stackable && grabbedItemStackSize > 1)
				{
					string stackSizeStr = grabbedItemStackSize.ToString();
					int stackSizeWidth = stackSizeFont.measureText(stackSizeStr);
					GUI.Text(xx + xoffset + iconSize - stackSizeWidth, yy + yoffset + iconSize - (int)stackSizeFont.size, 1.0f, stackSizeStr, stackSizeFont, 0xffaaaaaa);
				}

				if (Input.IsMouseButtonPressed(MouseButton.Left) &&
					(cursorPosition.x < paneX || cursorPosition.x >= paneX + paneWidth || cursorPosition.y < paneY || cursorPosition.y >= paneY + paneHeight))
				{
					player.throwItem(grabbedItem, grabbedItemStackSize);
					grabItem(null, 0);
				}
			}
		}
	}

	void drawTooltip()
	{
		if (grabbedItem == null && hoveredSlot != null)
		{
			ItemSlot hoveredItem = hoveredSlot;
			if (hoveredSlot.item == null && hoveredSlot.gridX != -1)
				hoveredItem = hoveredSlot.container.getItemAtPos(hoveredSlot.gridX, hoveredSlot.gridY);
			if (hoveredItem != null && hoveredItem.item != null)
			{
				int padding = 2 * currentUIScale;
				int tooltipX = Input.cursorPosition.x;
				int tooltipY = Input.cursorPosition.y;
				int tooltipWidth = 200 * currentUIScale;
				int tooltipHeight = tooltipCurrentContentHeight + 2 * 2 + 2 * padding + 2 * 10;

				int yscroll = 2 + padding + 5 * currentUIScale;

				GUI.Rect(tooltipX, tooltipY, tooltipWidth, tooltipHeight, 0xFFAAAAAA);
				GUI.Rect(tooltipX + 1, tooltipY + 1, tooltipWidth - 2, tooltipHeight - 2, 0xFF000000);
				GUI.Rect(tooltipX + 2, tooltipY + 2, tooltipWidth - 4, tooltipHeight - 4, 0xFF222222);

				Texture icon = hoveredItem.item.icon;
				int iconSize = 64 * currentUIScale;
				GUI.Texture(tooltipX + tooltipWidth / 2 - iconSize / 2, tooltipY + yscroll, iconSize, iconSize, icon);
				yscroll += iconSize + 5 * currentUIScale;

				string nameStr = hoveredItem.item.displayName;
				int nameStrWidth = tooltipFontBig.measureText(nameStr);
				GUI.Text(tooltipX + tooltipWidth / 2 - nameStrWidth / 2, tooltipY + yscroll, 1.0f, nameStr, tooltipFontBig, 0xFFAAAAAA);
				yscroll += (int)tooltipFontBig.size + 2 * currentUIScale;

				string categoryStr = hoveredItem.item.typeSpecifier;
				int categoryStrWidth = tooltipFontMedium.measureText(categoryStr);
				GUI.Text(tooltipX + tooltipWidth / 2 - categoryStrWidth / 2, tooltipY + yscroll, 1.0f, categoryStr, tooltipFontMedium, 0xFF777777);
				yscroll += (int)tooltipFontMedium.size + 2 * currentUIScale;

				if (hoveredItem.item.twoHanded)
				{
					string twoHandedStr = "Two-handed";
					int twoHandedStrWidth = tooltipFontMedium.measureText(twoHandedStr);
					GUI.Text(tooltipX + tooltipWidth / 2 - twoHandedStrWidth / 2, tooltipY + yscroll, 1.0f, twoHandedStr, tooltipFontMedium, 0xFF777777);
					yscroll += (int)tooltipFontMedium.size + 2 * currentUIScale;
				}

				yscroll += 10 * currentUIScale;

				int lineHeight = (int)tooltipFontMedium.size + 5 * currentUIScale;

				if (hoveredItem.item.description != null)
				{
					int descriptionLines = GUI.TextWrapped(tooltipX + 2 + 5 + padding, tooltipY + yscroll, 1.0f, tooltipWidth - 14 - 2 * padding, 1.5f, hoveredItem.item.description, tooltipFontMedium, 0xFF777777);
					yscroll += descriptionLines * lineHeight;
					yscroll += 10 * currentUIScale;
				}

				void drawTooltipInfo(string value)
				{
					GUI.Text(tooltipX + 2 + 5 + padding, tooltipY + yscroll, 1.0f, value, tooltipFontMedium, 0xFF777777);
				}
				void drawTooltipInfoRight(string value)
				{
					int valueStrWidth = tooltipFontMedium.measureText(value);
					GUI.Text(tooltipX + tooltipWidth - 2 - 5 - padding - valueStrWidth, tooltipY + yscroll, 1.0f, value, tooltipFontMedium, 0xFF777777);
				}

				if (hoveredItem.item.category == ItemCategory.Weapon)
				{
					drawTooltipInfo("Damage");
					drawTooltipInfoRight(hoveredItem.item.baseDamage.ToString());
					yscroll += lineHeight;

					if (hoveredItem.item.blockDamageAbsorption > 0)
					{
						drawTooltipInfo("Blocking");
						drawTooltipInfoRight(hoveredItem.item.blockDamageAbsorption.ToString());
						yscroll += lineHeight;
					}

					if (hoveredItem.item.parryFramesCount > 0)
					{
						drawTooltipInfo("Parry");
						drawTooltipInfoRight(hoveredItem.item.parryFramesCount.ToString());
						yscroll += lineHeight;
					}
				}
				else if (hoveredItem.item.category == ItemCategory.Shield)
				{
					drawTooltipInfo("Protection");
					drawTooltipInfoRight(hoveredItem.item.blockDamageAbsorption.ToString());
					yscroll += lineHeight;

					drawTooltipInfo("Stability");
					drawTooltipInfoRight((10 - hoveredItem.item.shieldHitStaminaCost).ToString());
					yscroll += lineHeight;

					drawTooltipInfo("Parry");
					drawTooltipInfoRight(hoveredItem.item.parryFramesCount.ToString());
					yscroll += lineHeight;
				}
				else if (hoveredItem.item.category == ItemCategory.Armor)
				{
					drawTooltipInfo("Protection");
					drawTooltipInfoRight(hoveredItem.item.blockDamageAbsorption.ToString());
					yscroll += lineHeight;
				}

				tooltipCurrentContentHeight = yscroll - 5 * currentUIScale;
			}
		}
	}

	void drawItemContainer(int x, int y, int slotSize, ItemContainer container, ItemContainer lootDst)
	{
		// Items
		{
			for (int i = container.width - 1; i >= 0; i--)
			{
				for (int j = container.height - 1; j >= 0; j--)
				{
					int xx = x + i * slotSize;
					int yy = y + j * slotSize;
					if (Input.IsHovered(xx, yy, slotSize, slotSize))
					{
						float xfract = (Input.cursorPosition.x - xx) / (float)slotSize;
						float yfract = (Input.cursorPosition.y - yy) / (float)slotSize;
						hoveredSlot = container.items[i + j * container.width];
						hoveredSlotFract = new Vector2(xfract, yfract);
					}
				}
			}

			for (int i = container.width - 1; i >= 0; i--)
			{
				for (int j = container.height - 1; j >= 0; j--)
				{
					int xx = x + i * slotSize;
					int yy = y + j * slotSize;
					drawItemSlotGrid(xx, yy, slotSize, slotSize, container.items[i + j * container.width], i, j, container, lootDst);
				}
			}
		}
	}

	void drawItemSlotArray(int x, int y, int width, int height, int dx, int dy, ItemSlot[] slots)
	{
		for (int i = 0; i < slots.Length; i++)
		{
			int xx = x + i * dx;
			int yy = y + i * dy;
			if (Input.IsHovered(xx, yy, width, height))
			{
				float xfract = (Input.cursorPosition.x - xx) / (float)width;
				float yfract = (Input.cursorPosition.y - yy) / (float)height;
				hoveredSlot = slots[i];
				hoveredSlotFract = new Vector2(xfract, yfract);
			}
		}

		for (int i = 0; i < slots.Length; i++)
		{
			int xx = x + i * dx;
			int yy = y + i * dy;
			drawItemSlot(xx, yy, width, height, slots[i]);
		}
	}

	void drawInventory(int x, int y, int width, int height)
	{
		GUI.PushLayer();

		// Armor slots
		{
			int slotSize = 32 * currentUIScale;
			int topPadding = 20 * currentUIScale;
			int padding = 12 * currentUIScale;

			drawItemSlotArray(x + width / 2 - slotSize / 2, y + topPadding, slotSize, slotSize, 0, slotSize + padding, player.inventory.armor);

			/*
			for (int i = 0; i < player.inventory.armor.Length; i++)
			{
				int xx = x + width / 2 - slotSize / 2;
				int yy = y + topPadding + i * (slotSize + padding);
				if (Input.IsHovered(xx, yy, slotSize, slotSize))
				{
					float xfract = (Input.cursorPosition.x - xx) / (float)slotSize;
					float yfract = (Input.cursorPosition.y - yy) / (float)slotSize;
					hoveredSlot = player.inventory.armor[i];
					hoveredSlotFract = new Vector2(xfract, yfract);
				}
			}

			for (int i = 0; i < player.inventory.armor.Length; i++)
			{
				int xx = x + width / 2 - slotSize / 2;
				int yy = y + topPadding + i * (slotSize + padding);
				drawItemSlot(xx, yy, slotSize, slotSize, player.inventory.armor[i]);
			}
			*/
		}

		// Left weapon slots
		{
			int slotSize = 64 * currentUIScale;
			int padding = 2 * currentUIScale;

			int xx = x + width / 2 - 26 * currentUIScale - padding - slotSize;
			int yy = y + 42 * currentUIScale;

			drawItemSlotArray(xx, yy, slotSize, slotSize, -(slotSize + padding), 0, player.inventory.leftHand);

			/*
			for (int i = 0; i < player.inventory.leftHand.Length; i++)
			{
				int xx = x + width / 2 - 52 - padding - slotSize - i * (slotSize + padding);
				int yy = y + 84;
				if (Input.IsHovered(xx, yy, slotSize, slotSize))
				{
					float xfract = (Input.cursorPosition.x - xx) / (float)slotSize;
					float yfract = (Input.cursorPosition.y - yy) / (float)slotSize;
					hoveredSlot = player.inventory.armor[i];
					hoveredSlotFract = new Vector2(xfract, yfract);
				}
			}

			for (int i = 0; i < player.inventory.leftHand.Length; i++)
			{
				int xx = x + width / 2 - 52 - padding - slotSize - i * (slotSize + padding);
				int yy = y + 84;
				drawItemSlot(xx, yy, slotSize, slotSize, player.inventory.leftHand[i]);
			}
			*/
		}
		// Right weapon slots
		{
			int slotSize = 64 * currentUIScale;
			int padding = 2 * currentUIScale;

			int xx = x + width / 2 + 26 * currentUIScale + padding;
			int yy = y + 42 * currentUIScale;

			drawItemSlotArray(xx, yy, slotSize, slotSize, slotSize + padding, 0, player.inventory.rightHand);

			/*
			for (int i = 0; i < player.inventory.rightHand.Length; i++)
			{
				int xx = x + width / 2 + 52 + padding + i * (slotSize + padding);
				int yy = y + 84;
				if (Input.IsHovered(xx, yy, slotSize, slotSize))
				{
					float xfract = (Input.cursorPosition.x - xx) / (float)slotSize;
					float yfract = (Input.cursorPosition.y - yy) / (float)slotSize;
					hoveredSlot = player.inventory.armor[i];
					hoveredSlotFract = new Vector2(xfract, yfract);
				}
			}

			for (int i = 0; i < player.inventory.rightHand.Length; i++)
			{
				int xx = x + width / 2 + 52 + padding + i * (slotSize + padding);
				int yy = y + 84;
				drawItemSlot(xx, yy, slotSize, slotSize, player.inventory.rightHand[i]);
			}
			*/
		}

		// Hotbar items
		{
			int slotSize = 32 * currentUIScale;
			int padding = 2 * currentUIScale;
			int totalWidth = padding + player.inventory.hotbar.Length * (slotSize + padding);
			int xx = x + width / 2 - totalWidth / 2 + padding;
			int yy = y + 196 * currentUIScale;

			drawItemSlotArray(xx, yy, slotSize, slotSize, slotSize + padding, 0, player.inventory.hotbar);

			/*
			for (int i = 0; i < player.inventory.hotbar.Length; i++)
			{
				int xx = x + width / 2 - totalWidth / 2 + padding + i * (slotSize + padding);
				int yy = y + 392;
				drawItemSlot(xx, yy, slotSize, slotSize, player.inventory.hotbar[i]);
			}
			*/
		}

		{
			int slotSize = 32 * currentUIScale;
			int totalWidth = player.inventory.width * slotSize;
			int totalHeight = player.inventory.height * slotSize;
			int left = width / 2 - totalWidth / 2;
			int top = height - totalHeight - 20 * currentUIScale;
			drawItemContainer(x + left, y + top, slotSize, player.inventory, null);
		}

		GUI.PopLayer();

		/*
		int tabIconSize = 64;
		int padding = 4;

		// Tab icons
		{
			// Equipment tab
			{
				int iconX = x + padding;
				int iconY = y + padding;

				GUI.Texture(iconX, iconY, tabIconSize, tabIconSize, equipmentTabIcon);
			}
		}
		*/

		//drawEquipmentTab(x, y + padding + tabIconSize + padding);
	}

	void drawChest(int x, int y, int width, int height)
	{
		GUI.PushLayer();

		int xx = x + 20 * currentUIScale;
		int yy = y + 20 * currentUIScale;
		int slotSize = 32 * currentUIScale;
		drawItemContainer(xx, yy, slotSize, openContainer.getContainer(), player.inventory);

		GUI.PopLayer();
	}

	public void draw(GraphicsDevice graphics)
	{
		if (inventoryOpen && openContainer == null)
		{
			hoveredSlot = null;

			int width = 360 * currentUIScale;
			int height = 460 * currentUIScale;
			int x = Display.width / 2 - width / 2;
			int y = Display.height / 2 - height / 2;

			GUI.Rect(x, y, width, height, 0xff111111);
			drawInventory(x, y, width, height);

			GUI.PushLayer();
			drawGrabbedItem(x, y, width, height);
			drawTooltip();
			GUI.PopLayer();

			if (InputManager.IsPressed("UIBack") || InputManager.IsPressed("UIClose") || InputManager.IsPressed("OpenInventory"))
			{
				inventoryOpen = false;
				equipmentTabSelectScreenOpen = false;
				player.setCursorLocked(true);

				if (grabbedItem != null)
				{
					player.throwItem(grabbedItem, grabbedItemStackSize);
					grabItem(null, 0);
				}
			}
		}
		else if (openContainer != null)
		{
			hoveredSlot = null;

			int inventoryWidth = 360 * currentUIScale;
			int chestPaneWidth = 360 * currentUIScale;
			int height = 460 * currentUIScale;
			int x = Display.width / 2 - inventoryWidth;
			int y = Display.height / 2 - height / 2;
			int width = inventoryWidth + chestPaneWidth;

			GUI.Rect(x, y, width, height, 0xff111111);

			drawInventory(x, y, inventoryWidth, height);
			drawChest(x + inventoryWidth, y, chestPaneWidth, height);

			GUI.PushLayer();
			drawGrabbedItem(x, y, width, height);
			drawTooltip();
			GUI.PopLayer();

			if (InputManager.IsPressed("UIBack") || InputManager.IsPressed("UIClose") || InputManager.IsPressed("OpenInventory"))
			{
				inventoryOpen = false;
				openContainer.onClose();
				openContainer = null;
				player.setCursorLocked(true);

				if (grabbedItem != null)
				{
					player.throwItem(grabbedItem, grabbedItemStackSize);
					grabItem(null, 0);
				}
			}
		}
		else
		{
			if (InputManager.IsPressed("OpenInventory"))
			{
				inventoryOpen = true;
				player.setCursorLocked(false);
			}
		}

		/*
		if (open)
		{
			bool hasFocus = !equipmentTabSelectScreenOpen;

			int width = 1400;
			int height = 800;
			int x = Display.viewportSize.x / 2 - width / 2;
			int y = Display.viewportSize.y / 2 - height / 2;

			drawInventory(x, y, width, height);

			if (hasFocus && (Input.IsKeyPressed(KeyCode.KeyQ) || Input.IsKeyPressed(KeyCode.Esc)) || Input.IsKeyPressed(KeyCode.Tab))
			{
				open = false;
				equipmentTabSelectScreenOpen = false;
				player.setCursorLocked(true);
			}
		}
		else
		{
			if (Input.IsKeyPressed(KeyCode.Tab))
			{
				open = true;
				player.setCursorLocked(false);
			}
		}
		*/
	}
}
