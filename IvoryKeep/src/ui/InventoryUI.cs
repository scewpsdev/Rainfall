using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

public class InventoryUI
{
	Player player;

	public bool inventoryOpen = false;
	ItemContainerEntity openContainer = null;
	static int currentUIScale = 2;
	int slotSize = 48;

	Item grabbedItem = null;
	int grabbedItemStackSize = 0;
	Vector2i grabbedItemOffset = Vector2i.Zero;

	//Vector2 hoveredSlotPosition = new Vector2(-100);
	ItemSlot hoveredSlot = null;

	static int tooltipCurrentContentHeight = 100;

	Texture equipmentHandRight, equipmentHandLeft;
	Texture equipmentArmorHelmet, equipmentArmorTorso, equipmentArmorPants, equipmentArmorBoots;
	Texture equipmentHotbarItem;
	Texture equipmentArrows, equipmentSpells;

	Font mediumFont;
	Font stackSizeFont;
	static Font tooltipFontBig, tooltipFontMedium;


	public InventoryUI(Player player)
	{
		this.player = player;

		equipmentHandRight = Resource.GetTexture("res/texture/ui/equipment_hand_right.png");
		equipmentHandLeft = Resource.GetTexture("res/texture/ui/equipment_hand_left.png");
		equipmentArmorHelmet = Resource.GetTexture("res/texture/ui/equipment_armor_helmet.png");
		equipmentArmorTorso = Resource.GetTexture("res/texture/ui/equipment_armor_torso.png");
		equipmentArmorPants = Resource.GetTexture("res/texture/ui/equipment_armor_pants.png");
		equipmentArmorBoots = Resource.GetTexture("res/texture/ui/equipment_armor_boots.png");
		equipmentHotbarItem = Resource.GetTexture("res/texture/ui/equipment_hotbar_item.png");
		equipmentArrows = Resource.GetTexture("res/texture/ui/equipment_arrows.png");
		equipmentSpells = Resource.GetTexture("res/texture/ui/equipment_spells.png");

		mediumFont = FontManager.GetFont("default", 24, true);
		stackSizeFont = FontManager.GetFont("default", 20, true);
		tooltipFontBig = FontManager.GetFont("default", 24, true);
		tooltipFontMedium = FontManager.GetFont("default", 18, true);
	}

	public void openChestUI(ItemContainerEntity container)
	{
		openContainer = container;
		inventoryOpen = true;
		Input.mouseLocked = false;
	}

	void grabItem(Item item, int amount)
	{
		grabbedItem = item;
		grabbedItemStackSize = amount;
		//grabbedItemOffset = new Vector2i((gridX - topLeftSlot.gridX) * 64 + (Input.cursorPosition.x - x), (gridY - topLeftSlot.gridY) * 64 + (Input.cursorPosition.y - y));
		if (item != null)
			grabbedItemOffset = Vector2i.One * 32 / 2;
		else
			grabbedItemOffset = Vector2i.Zero;
	}

	void drawItemSlotGrid(int x, int y, ItemSlot slot, int gridX, int gridY, ItemContainer container, ItemContainer lootDst)
	{
		bool isHovered = GUI.IsHovered(x, y, slotSize, slotSize);
		if (isHovered)
			hoveredSlot = slot;

		bool isHighlighted = isHovered;
		if (slot != null)
		{
			if (hoveredSlot != null && hoveredSlot.container == container && hoveredSlot.gridX != -1 &&
				hoveredSlot.gridX == slot.gridX && hoveredSlot.gridY == slot.gridY)
			{
				isHighlighted = true;
			}
		}
		/*
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
		*/
		if (slot != null && slot.item != null && isHovered && Input.IsMouseButtonPressed(MouseButton.Left) && grabbedItem == null)
		{
			if (lootDst != null)
			{
				ItemSlot addedTo = lootDst.addItem(slot.item, slot.stackSize);
				if (addedTo == null)
					grabItem(slot.item, slot.stackSize);
			}
			else
			{
				grabItem(slot.item, slot.stackSize);
			}
			slot.setItem(null);
			slot.stackSize = 0;
		}
		else if (slot != null && slot.item != null && isHovered && Input.IsMouseButtonPressed(MouseButton.Right) && grabbedItem == null && lootDst == null)
		{
			if (container is Inventory)
			{
				Inventory inventory = container as Inventory;
				inventory.equipItem(slot);
			}
		}
		else if (isHovered && Input.IsMouseButtonPressed(MouseButton.Left) && grabbedItem != null)
		{
			{
				bool overlaps = hoveredSlot.item != null;
				if (!overlaps)
				{
					hoveredSlot.setItem(grabbedItem);
					hoveredSlot.stackSize = grabbedItemStackSize;
					grabItem(null, 0);
				}
				else
				{
					if (hoveredSlot.item == grabbedItem && grabbedItem.stackable)
					{
						hoveredSlot.stackSize += grabbedItemStackSize;
						grabItem(null, 0);
					}
					else
					{
						Item tmp = hoveredSlot.item;
						int tmp2 = hoveredSlot.stackSize;
						hoveredSlot.setItem(grabbedItem);
						hoveredSlot.stackSize = grabbedItemStackSize;
						grabItem(tmp, tmp2);
					}
				}
			}
		}

		uint background = isHighlighted ? 0xFF666666 : 0xFF333333;
		GUI.Rect(x + 1, y + 1, slotSize - 2, slotSize - 2, background);
		if (slot.item != null)
		{
			GUI.Texture(x, y, slotSize, slotSize, slot.item.icon);
			if (slot.item.stackable && slot.stackSize > 1)
			{
				string stackSizeStr = slot.stackSize.ToString();
				int stackSizeWidth = stackSizeFont.measureText(stackSizeStr);
				GUI.Text(x + slotSize - stackSizeWidth, y + slotSize - (int)stackSizeFont.size, 1.0f, stackSizeStr, stackSizeFont, 0xffaaaaaa);
			}
		}
	}

	void drawItemSlot(int x, int y, int width, int height, ItemSlot slot, Texture emptyTexture = null)
	{
		bool isHovered = GUI.IsHovered(x, y, width, height);
		if (isHovered)
			hoveredSlot = slot;

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
		GUI.Rect(x + 1, y + 1, width - 2, height - 2, background);
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
		else if (emptyTexture != null)
		{
			GUI.Texture(x, y, width, height, emptyTexture, 0, 0, emptyTexture.width, emptyTexture.height, 0xFF000000);
		}
	}

	void drawGrabbedItem(/*int paneX, int paneY, int paneWidth, int paneHeight*/)
	{
		// Grabbed item
		{
			if (grabbedItem != null)
			{
				Vector2i cursorPosition = Input.cursorPosition;
				int xx = cursorPosition.x - grabbedItemOffset.x;
				int yy = cursorPosition.y - grabbedItemOffset.y;
				int iconSize = slotSize;

				GUI.Texture(xx, yy, iconSize, iconSize, grabbedItem.icon);
				if (grabbedItem.stackable && grabbedItemStackSize > 1)
				{
					string stackSizeStr = grabbedItemStackSize.ToString();
					int stackSizeWidth = stackSizeFont.measureText(stackSizeStr);
					GUI.Text(xx + iconSize - stackSizeWidth, yy + iconSize - (int)stackSizeFont.size, 1.0f, stackSizeStr, stackSizeFont, 0xffaaaaaa);
				}

				/*
				if (Input.IsMouseButtonPressed(MouseButton.Left) &&
					(cursorPosition.x < paneX || cursorPosition.x >= paneX + paneWidth || cursorPosition.y < paneY || cursorPosition.y >= paneY + paneHeight))
				{
					//player.throwItem(grabbedItem, grabbedItemStackSize);
					grabItem(null, 0);
				}
				*/
			}
		}
	}

	public static void DrawTooltip(ItemSlot hoveredSlot)
	{
		ItemSlot hoveredItem = hoveredSlot;
		if (hoveredSlot.item == null && hoveredSlot.gridX != -1)
			hoveredItem = hoveredSlot.container.getItemAtPos(hoveredSlot.gridX, hoveredSlot.gridY);
		if (hoveredItem != null && hoveredItem.item != null)
		{
			int padding = 4;
			int tooltipWidth = 400;
			int tooltipHeight = tooltipCurrentContentHeight + 2 * 2 + 2 * padding + 2 * 10;
			int tooltipX = Math.Min(Input.cursorPosition.x, Display.width - tooltipWidth);
			int tooltipY = Math.Min(Input.cursorPosition.y, Display.height - tooltipHeight);

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

				if (hoveredItem.item.baseAbsorption > 0)
				{
					drawTooltipInfo("Blocking");
					drawTooltipInfoRight(hoveredItem.item.baseAbsorption.ToString());
					yscroll += lineHeight;
				}

				if (hoveredItem.item.blockStability > 0)
				{
					drawTooltipInfo("Stability");
					drawTooltipInfoRight(hoveredItem.item.blockStability.ToString());
					yscroll += lineHeight;
				}
			}
			else if (hoveredItem.item.category == ItemCategory.Armor)
			{
				drawTooltipInfo("Protection");
				drawTooltipInfoRight(hoveredItem.item.baseAbsorption.ToString());
				yscroll += lineHeight;
			}

			tooltipCurrentContentHeight = yscroll - 5 * currentUIScale;
		}
	}

	void drawItemContainer(int x, int y, ItemContainer container, ItemContainer lootDst)
	{
		// Items
		{
			for (int i = container.width - 1; i >= 0; i--)
			{
				for (int j = container.height - 1; j >= 0; j--)
				{
					int xx = x + i * slotSize;
					int yy = y + j * slotSize;
					drawItemSlotGrid(xx, yy, container.items[i + j * container.width], i, j, container, lootDst);
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
			if (GUI.IsHovered(xx, yy, width, height))
				hoveredSlot = slots[i];
		}

		for (int i = 0; i < slots.Length; i++)
		{
			int xx = x + i * dx;
			int yy = y + i * dy;
			drawItemSlot(xx, yy, width, height, slots[i]);
		}
	}

	void drawInventory(int x, int y)
	{
		GUI.PushLayer();
		{
			int left = 10;// width / 2 - totalWidth / 2;
			int top = 10; // height - totalHeight - 20 * currentUIScale;
			drawItemContainer(x + left, y + top, player.inventory, null);
		}
		GUI.PopLayer();
	}

	void drawChest(int x, int y)
	{
		GUI.PushLayer();

		int left = 10;
		int top = 10;
		drawItemContainer(x + left, y + top, openContainer.getContainer(), player.inventory);

		GUI.PopLayer();
	}

	void drawEquipment(int x, int y)
	{
		int padding = 10;

		GUI.PushLayer();
		{
			drawItemSlot(x + padding, y + padding, slotSize, slotSize, player.inventory.rightHand[0], equipmentHandRight);
			drawItemSlot(x + padding, y + padding + slotSize, slotSize, slotSize, player.inventory.leftHand[0], equipmentHandLeft);
			drawItemSlot(x + padding, y + padding + 2 * slotSize, slotSize, slotSize, player.inventory.rightHand[1]);
			drawItemSlot(x + padding, y + padding + 3 * slotSize, slotSize, slotSize, player.inventory.leftHand[1]);

			drawItemSlot(x + padding + 5 * slotSize, y + padding + 0 * slotSize, slotSize, slotSize, player.inventory.armor[0], equipmentArmorHelmet);
			drawItemSlot(x + padding + 5 * slotSize, y + padding + 1 * slotSize, slotSize, slotSize, player.inventory.armor[1], equipmentArmorTorso);
			drawItemSlot(x + padding + 5 * slotSize, y + padding + 2 * slotSize, slotSize, slotSize, player.inventory.armor[2], equipmentArmorPants);
			drawItemSlot(x + padding + 5 * slotSize, y + padding + 3 * slotSize, slotSize, slotSize, player.inventory.armor[3], equipmentArmorBoots);

			drawItemSlot(x + padding + 4 * slotSize, y + padding + 3 * slotSize, slotSize, slotSize, player.inventory.arrows, equipmentArrows);

			drawItemSlot(x + padding, y + padding + 4 * slotSize + padding, slotSize, slotSize, player.inventory.hotbar[0], equipmentHotbarItem);
			drawItemSlot(x + padding + slotSize, y + padding + 4 * slotSize + padding, slotSize, slotSize, player.inventory.hotbar[1]);
			drawItemSlot(x + padding + 2 * slotSize, y + padding + 4 * slotSize + padding, slotSize, slotSize, player.inventory.hotbar[2]);
			drawItemSlot(x + padding + 3 * slotSize, y + padding + 4 * slotSize + padding, slotSize, slotSize, player.inventory.hotbar[3]);
			drawItemSlot(x + padding + 4 * slotSize, y + padding + 4 * slotSize + padding, slotSize, slotSize, player.inventory.hotbar[4]);
			drawItemSlot(x + padding + 5 * slotSize, y + padding + 4 * slotSize + padding, slotSize, slotSize, player.inventory.hotbar[5]);

			drawItemSlot(x + padding, y + padding + 4 * slotSize + padding + slotSize + padding, slotSize, slotSize, player.inventory.spells[0], equipmentSpells);
			drawItemSlot(x + padding + slotSize, y + padding + 4 * slotSize + padding + slotSize + padding, slotSize, slotSize, player.inventory.spells[1]);
			drawItemSlot(x + padding + 2 * slotSize, y + padding + 4 * slotSize + padding + slotSize + padding, slotSize, slotSize, player.inventory.spells[2]);
			drawItemSlot(x + padding + 3 * slotSize, y + padding + 4 * slotSize + padding + slotSize + padding, slotSize, slotSize, player.inventory.spells[3]);
			drawItemSlot(x + padding + 4 * slotSize, y + padding + 4 * slotSize + padding + slotSize + padding, slotSize, slotSize, player.inventory.spells[4]);
			drawItemSlot(x + padding + 5 * slotSize, y + padding + 4 * slotSize + padding + slotSize + padding, slotSize, slotSize, player.inventory.spells[5]);


			/*
			// Armor slots
			{
				int slotSize = 32 * currentUIScale;
				int topPadding = 20 * currentUIScale;
				int padding = 12 * currentUIScale;

				drawItemSlotArray(x + width / 2 - slotSize / 2, y + topPadding, slotSize, slotSize, 0, slotSize + padding, player.inventory.armor);
			}

			// Left weapon slots
			{
				int slotSize = 64 * currentUIScale;
				int padding = 2 * currentUIScale;

				int xx = x + width / 2 - 26 * currentUIScale - padding - slotSize;
				int yy = y + 42 * currentUIScale;

				drawItemSlotArray(xx, yy, slotSize, slotSize, -(slotSize + padding), 0, player.inventory.leftHand);
			}
			// Right weapon slots
			{
				int slotSize = 64 * currentUIScale;
				int padding = 2 * currentUIScale;

				int xx = x + width / 2 + 26 * currentUIScale + padding;
				int yy = y + 42 * currentUIScale;

				drawItemSlotArray(xx, yy, slotSize, slotSize, slotSize + padding, 0, player.inventory.rightHand);
			}

			// Hotbar items
			{
				int slotSize = 32 * currentUIScale;
				int padding = 2 * currentUIScale;
				int totalWidth = padding + player.inventory.hotbar.Length * (slotSize + padding);
				int xx = x + width / 2 - totalWidth / 2 + padding;
				int yy = y + 196 * currentUIScale;

				drawItemSlotArray(xx, yy, slotSize, slotSize, slotSize + padding, 0, player.inventory.hotbar);
			}
			*/
		}
		GUI.PopLayer();
	}

	void drawStats(int x, int y, int width, int height)
	{
		GUI.PushLayer();

		GUI.Text(x + width / 2 - tooltipFontBig.measureText(player.name) / 2, y, 1, player.name, tooltipFontBig, 0xFFAAAAAA);
		y += (int)tooltipFontBig.size + 30;

		void drawTooltipInfo(string value, uint color = 0xFFAAAAAA)
		{
			GUI.Text(x, y, 1.0f, value, tooltipFontMedium, color);
		}
		void drawTooltipInfoRight(string value, uint color = 0xFFFFFFFF)
		{
			int valueStrWidth = tooltipFontMedium.measureText(value);
			GUI.Text(x + width - valueStrWidth, y, 1.0f, value, tooltipFontMedium, color);
		}
		void drawLevelBackground(int x, int y, int width, int height, ref int lvl)
		{
			if (player.stats.availablePoints > 0)
			{
				bool hovered = GUI.IsHovered(x, y, width, height);
				if (hovered)
				{
					int padding = 3;
					GUI.Rect(x - padding, y - padding, width + 2 * padding, height + 2 * padding, 0xFF444444);
					if (Input.IsMouseButtonPressed(MouseButton.Left))
					{
						lvl++;
						player.stats.availablePoints--;
					}
				}
			}
		}

		int lineHeight = (int)tooltipFontMedium.size + 10;

		drawTooltipInfo("Health", 0xffCC7C7C);
		drawTooltipInfoRight(player.stats.health.ToString() + "/" + player.stats.getMaxHealth().ToString());
		y += lineHeight;

		drawTooltipInfo("Stamina", 0xff7EA07E);
		drawTooltipInfoRight((player.stats.stamina).ToString() + "/" + player.stats.getMaxStamina().ToString());
		y += lineHeight;

		drawTooltipInfo("Mana", 0xffB2B8F4);
		drawTooltipInfoRight(player.stats.mana.ToString() + "/" + player.stats.getMaxMana().ToString());
		y += lineHeight;

		y += lineHeight;

		if (player.stats.availablePoints > 0)
		{
			drawTooltipInfo("Available Points:");
			drawTooltipInfoRight(player.stats.availablePoints.ToString(), 0xFF00FF00);
			y += lineHeight;
			y += lineHeight;
		}

		drawLevelBackground(x, y, width, (int)tooltipFontMedium.size, ref player.stats.levels.vitality);
		drawTooltipInfo("Vitality");
		drawTooltipInfoRight(player.stats.levels.vitality.ToString());
		y += lineHeight;

		drawLevelBackground(x, y, width, (int)tooltipFontMedium.size, ref player.stats.levels.endurance);
		drawTooltipInfo("Endurance");
		drawTooltipInfoRight(player.stats.levels.endurance.ToString());
		y += lineHeight;

		drawLevelBackground(x, y, width, (int)tooltipFontMedium.size, ref player.stats.levels.agility);
		drawTooltipInfo("Agility");
		drawTooltipInfoRight(player.stats.levels.agility.ToString());
		y += lineHeight;

		drawLevelBackground(x, y, width, (int)tooltipFontMedium.size, ref player.stats.levels.strength);
		drawTooltipInfo("Strength");
		drawTooltipInfoRight(player.stats.levels.strength.ToString());
		y += lineHeight;

		drawLevelBackground(x, y, width, (int)tooltipFontMedium.size, ref player.stats.levels.finesse);
		drawTooltipInfo("Finesse");
		drawTooltipInfoRight(player.stats.levels.finesse.ToString());
		y += lineHeight;

		drawLevelBackground(x, y, width, (int)tooltipFontMedium.size, ref player.stats.levels.intelligence);
		drawTooltipInfo("Intelligence");
		drawTooltipInfoRight(player.stats.levels.intelligence.ToString());
		y += lineHeight;

		y += lineHeight;

		drawTooltipInfo("Gold");
		drawTooltipInfoRight(player.stats.xp.ToString());
		y += lineHeight;

		GUI.PopLayer();
	}

	public void draw()
	{
		if (inventoryOpen /*&& openContainer == null*/)
		{
			hoveredSlot = null;

			currentUIScale = Display.height / 480;
			if (currentUIScale == 0)
			{
				slotSize = 32;
			}
			else if (currentUIScale == 1)
			{
				slotSize = 48;
			}
			else if (currentUIScale >= 2)
			{
				slotSize = 64;
			}

			bool inventoryHovered, equipmentHovered;
			int inventoryWidth, inventoryHeight;

			// Inventory screen
			{
				int padding = 10;
				int width = player.inventory.width * slotSize + 2 * padding;
				int height = player.inventory.height * slotSize + 2 * padding;
				int x = 32;
				int y = Display.height - 32 - height;

				GUI.Rect(x, y, width, height, 0xff111111);
				drawInventory(x, y);

				inventoryHovered = GUI.IsHovered(x, y, width, height);
				inventoryWidth = width;
				inventoryHeight = height;
			}

			// Equipment screen
			{
				int padding = 10;
				int width = 6 * slotSize + 2 * padding;
				int height = 6 * slotSize + 4 * padding;
				int x = 32;
				int y = Display.height - 32 - height - padding - inventoryHeight;

				GUI.Rect(x, y, width, height, 0xff111111);
				drawEquipment(x, y);

				equipmentHovered = GUI.IsHovered(x, y, width, height);
			}

			// Item container screen
			if (openContainer != null)
			{
				int padding = 10;
				int width = openContainer.getContainer().width * slotSize + 2 * padding;
				int height = openContainer.getContainer().height * slotSize + 2 * padding;
				int x = 32 + inventoryWidth + padding;
				int y = Display.height - 32 - height;

				GUI.Rect(x, y, width, height, 0xff111111);
				drawChest(x, y);
			}

			// Player stats screen
			{
				int padding = 20;
				int width = 280;
				int height = 470;
				int x = Display.width - 32 - width;
				int y = Display.height - 32 - height;

				GUI.Rect(x, y, width, height, 0xff111111);
				drawStats(x + padding, y + padding, width - 2 * padding, height - 2 * padding);
			}

			if (grabbedItem != null && Input.IsMouseButtonPressed(MouseButton.Left) && !inventoryHovered && !equipmentHovered)
			{
				//player.dropItem(grabbedItem, grabbedItemStackSize);
				grabItem(null, 0);
			}

			GUI.PushLayer();
			drawGrabbedItem(/*x, y, width, height*/);
			if (grabbedItem == null && hoveredSlot != null)
				DrawTooltip(hoveredSlot);
			GUI.PopLayer();

			if (InputManager.IsPressed("UIBack") || InputManager.IsPressed("UIClose") || InputManager.IsPressed("Inventory"))
			{
				inventoryOpen = false;
				if (openContainer != null)
				{
					openContainer.onClose();
					openContainer = null;
				}
				Input.mouseLocked = true;

				if (grabbedItem != null)
				{
					//player.dropItem(grabbedItem, grabbedItemStackSize);
					grabItem(null, 0);
				}
			}
		}
		/*
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
		*/
		else
		{
			if (InputManager.IsPressed("Inventory"))
			{
				inventoryOpen = true;
				Input.mouseLocked = false;
			}
		}
	}
}
