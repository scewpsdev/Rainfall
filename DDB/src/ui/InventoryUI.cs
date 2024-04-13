using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;


internal class InventoryUI
{
	Player player;

	bool open = false;

	bool equipmentTabSelectScreenOpen = false;
	int equipmentTabSelectScreenSelectedItem = 0;
	List<ItemSlot> equipmentTabSelectScreenSlots = new List<ItemSlot>();
	ItemSlot equipmentTabSelectScreenResult = null;

	int equipmentTabOverviewSelectedSlot = 0;

	Texture equipmentTabIcon;
	Texture equipmentHandRight, equipmentHandLeft;
	Texture leftHand, rightHand;

	Font mediumFont;


	public InventoryUI(Player player)
	{
		this.player = player;

		equipmentTabIcon = Resource.GetTexture("res/texture/ui/inventory_tab_equipment.png");
		equipmentHandRight = Resource.GetTexture("res/texture/ui/equipment_hand_right.png");
		equipmentHandLeft = Resource.GetTexture("res/texture/ui/equipment_hand_left.png");
		leftHand = Resource.GetTexture("res/texture/ui/hand_left.png");
		rightHand = Resource.GetTexture("res/texture/ui/hand_right.png");

		mediumFont = Resource.GetFontData("res/fonts/libre-baskerville.regular.ttf").createFont(24);
	}

	public void update()
	{
	}

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

			Renderer.DrawUITexture(x + 2 * padding, slotY + (slotSize - handIconSize) / 2, handIconSize, handIconSize, icon);

			for (int i = 0; i < slots.Length; i++)
			{
				int slotX = x + 2 * padding + handIconSize + i * (slotSize + padding);

				if (totalSlotIndex == equipmentTabOverviewSelectedSlot && equipmentTabSelectScreenResult != null)
				{
					if (player.equipHandItem(handID, i, equipmentTabSelectScreenResult))
						equipmentTabSelectScreenResult = null;
				}

				bool hovered = Input.IsHovered(slotX, slotY, slotSize, slotSize);
				if (hovered && Input.cursorHasMoved)
					equipmentTabOverviewSelectedSlot = totalSlotIndex;
				bool selected = totalSlotIndex == equipmentTabOverviewSelectedSlot;
				if (selected)
				{
					if (hovered && Input.IsMouseButtonPressed(MouseButton.Left) || Input.IsKeyPressed(KeyCode.KeyE))
					{
						openItemSelectScreen(ItemCategory.Weapon, ItemCategory.Shield, ItemCategory.Utility, ItemCategory.Consumable);
					}
					else if (Input.IsKeyPressed(KeyCode.KeyF))
					{
						if (player.equipHandItem(handID, i, null))
						{ }
					}
				}

				Renderer.DrawUIRect(slotX, slotY, slotSize, slotSize, selected ? 0xff444444 : 0xff222222);
				if (slots[i] != null)
				{
					Renderer.DrawUITexture(slotX, slotY, slotSize, slotSize, slots[i].item.icon);
				}

				totalSlotIndex++;
			}
		};

		drawHandItems(x, y, player.inventory.rightHandSlots, equipmentHandRight, 0);
		drawHandItems(x, y + padding + slotSize, player.inventory.leftHandSlots, equipmentHandLeft, 1);
	}

	int isEquippedRightHand(ItemSlot slot)
	{
		for (int i = 0; i < player.inventory.rightHandSlots.Length; i++)
		{
			if (player.inventory.rightHandSlots[i] == slot)
				return i;
		}
		return -1;
	}

	int isEquippedLeftHand(ItemSlot slot)
	{
		for (int i = 0; i < player.inventory.leftHandSlots.Length; i++)
		{
			if (player.inventory.leftHandSlots[i] == slot)
				return i;
		}
		return -1;
	}

	void drawItemSlot(int x, int y, int width, int height, ItemSlot slot, bool selected)
	{
		int iconSize = 80;

		Renderer.DrawUIRect(x, y, width, height, selected ? 0xff444444 : 0xff222222);
		Renderer.DrawUITexture(x, y, iconSize, iconSize, slot.item.icon);

		Renderer.DrawText(x + iconSize + 24, y + height / 2 - (int)mediumFont.size / 2, 1.0f, slot.item.displayName, mediumFont, 0xffcccccc);

		if (slot.item.stackable)
			Renderer.DrawText(x + width - iconSize - 24 - 8, y + height / 2 - (int)mediumFont.size / 2, 1.0f, slot.stackSize.ToString(), mediumFont, 0xffcccccc);

		int rightHandSlot = isEquippedRightHand(slot);
		int leftHandSlot = isEquippedLeftHand(slot);

		Debug.Assert(!(rightHandSlot != -1 && leftHandSlot != -1));

		if (rightHandSlot != -1)
			Renderer.DrawUITexture(x + width - iconSize - 8, y, iconSize, iconSize, rightHand);
		else if (leftHandSlot != -1)
			Renderer.DrawUITexture(x + width - iconSize - 8, y, iconSize, iconSize, leftHand);
	}

	ItemSlot drawEquipmentTabItemSelectScreen(int x, int y)
	{
		int padding = 8;

		int slotX = x + 2 * padding;
		int slotY = y + 2 * padding;

		int slotWidth = 1000;
		int slotHeight = 80;

		int totalItemIndex = 0;
		int totalItemCount = player.inventory.weapons.Count + player.inventory.shields.Count + player.inventory.utilities.Count + player.inventory.consumables.Count;

		if (Input.IsKeyPressed(KeyCode.Down))
			equipmentTabSelectScreenSelectedItem = (equipmentTabSelectScreenSelectedItem + 1) % totalItemCount;
		else if (Input.IsKeyPressed(KeyCode.Up))
			equipmentTabSelectScreenSelectedItem = (equipmentTabSelectScreenSelectedItem - 1 + totalItemCount) % totalItemCount;

		if (Input.IsKeyPressed(KeyCode.Esc) || Input.IsKeyPressed(KeyCode.KeyQ))
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
				if (hovered && Input.IsMouseButtonPressed(MouseButton.Left) || Input.IsKeyPressed(KeyCode.KeyE))
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

	void drawInventory(int x, int y, int width, int height)
	{
		Renderer.DrawUIRect(x, y, width, height, 0xff111111);

		int tabIconSize = 64;
		int padding = 4;

		// Tab icons
		{
			// Equipment tab
			{
				int iconX = x + padding;
				int iconY = y + padding;

				Renderer.DrawUITexture(iconX, iconY, tabIconSize, tabIconSize, equipmentTabIcon);
			}
		}

		drawEquipmentTab(x, y + padding + tabIconSize + padding);

	}

	public void draw(GraphicsDevice graphics)
	{
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
	}
}
