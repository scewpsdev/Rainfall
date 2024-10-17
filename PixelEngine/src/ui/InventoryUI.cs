using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;


public class InventoryUI
{
	public static Sprite weaponSprite, shieldSprite, helmetSprite, bagSprite, ringSprite;
	public static Sprite backpackSprite, armorSprite, glovesSprite, bootsSprite;

	static InventoryUI()
	{
		weaponSprite = new Sprite(HUD.tileset, 0, 2, 2, 2);
		shieldSprite = new Sprite(HUD.tileset, 2, 2, 2, 2);
		bagSprite = new Sprite(HUD.tileset, 4, 2, 2, 2);
		helmetSprite = new Sprite(HUD.tileset, 6, 2, 2, 2);
		ringSprite = new Sprite(HUD.tileset, 6, 4, 2, 2);
		backpackSprite = new Sprite(HUD.tileset, 0, 6, 2, 2);
		armorSprite = new Sprite(HUD.tileset, 2, 6, 2, 2);
		glovesSprite = new Sprite(HUD.tileset, 4, 6, 2, 2);
		bootsSprite = new Sprite(HUD.tileset, 6, 6, 2, 2);
	}


	Player player;

	int selectedItem = 0;
	int sidePanelHeight = 40;
	int inventoryHeight = 120;
	int characterHeight = 150;

	int currentScroll = 0;


	public InventoryUI(Player player)
	{
		this.player = player;
	}

	static bool drawItemSlot(int x, int y, int size, Item item, Sprite background = null, bool selected = false)
	{
		return ItemSlotUI.Render(x, y, size, item, background, selected);
	}

	public static void DrawEquipment(int x, int y, int width, int height, Player player)
	{
		int xpadding = 4;
		int ypadding = 4;
		int slotSize = 16;

		y += 16;

		Renderer.DrawUISprite(x, y, 16, 16, shieldSprite);
		Renderer.DrawUISprite(x, y, 16, 16, weaponSprite);
		drawItemSlot(x + 16 + 2, y, slotSize, player.offhandItem);
		drawItemSlot(x + 16 + 2 + slotSize + xpadding, y, slotSize, player.handItem);
		y += slotSize + ypadding;

		Renderer.DrawUISprite(x, y, 16, 16, bagSprite);
		for (int i = 0; i < player.activeItems.Length; i++)
			drawItemSlot(x + 16 + 2 + i * (slotSize + xpadding), y, slotSize, player.activeItems[i]);
		y += slotSize + ypadding;

		int idx = 0;
		for (int i = 0; i < player.items.Count; i++)
		{
			Item item = player.items[i];
			if (!player.isEquipped(item))
			{
				drawItemSlot(x + 16 + 2 + idx * (slotSize + xpadding), y, slotSize, item);
				idx++;
			}
		}
		y += slotSize + ypadding;

		Renderer.DrawUISprite(x, y, 16, 16, helmetSprite);
		for (int i = 0; i < (int)ArmorSlot.Count; i++)
		{
			if (player.getArmorItem((ArmorSlot)i, out int slot))
				drawItemSlot(x + 16 + 2 + i * (slotSize + xpadding), y, slotSize, player.passiveItems[slot]);
			else
				drawItemSlot(x + 16 + 2 + i * (slotSize + xpadding), y, slotSize, null);
		}
		y += slotSize + ypadding;

		Renderer.DrawUISprite(x, y, 16, 16, ringSprite);
		{
			int xx = x + 16 + 2;
			for (int i = 0; i < player.passiveItems.Count; i++)
			{
				if (player.passiveItems[i].armorSlot == ArmorSlot.None)
				{
					drawItemSlot(xx, y, slotSize, player.passiveItems[i]);
					xx += slotSize + xpadding;
				}
			}
		}
	}

	public static int DrawEquipment2(int x, int y, int width, int height, Player player, ref int selectedItem, out Item item)
	{
		item = null;

		Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, UIColors.WINDOW_FRAME);
		Renderer.DrawUISprite(x, y, width, height, null, false, UIColors.WINDOW_BACKGROUND);

		int xpadding = 4;
		int ypadding = 4;
		int slotSize = 16;

		int top = y;

		y += 8;

		Renderer.DrawUITextBMP(x + width / 2 - Renderer.MeasureUITextBMP("Inventory").x / 2, y, "Inventory", 1, 0xFFAAAAAA);

		y += Renderer.smallFont.size + 8;

		List<Item> storedItems = new List<Item>();
		for (int i = 0; i < player.items.Count; i++)
		{
			if (!player.isEquipped(player.items[i]))
				storedItems.Add(player.items[i]);
		}

		int firstEquipmentItem = 0;
		int firstActiveItem = 7;
		int firstStoredItem = firstActiveItem + player.activeItems.Length;
		int firstPassiveItem = firstStoredItem + storedItems.Count;

		if (drawItemSlot(x + width / 2 - slotSize / 2 - xpadding - slotSize, y, slotSize, player.offhandItem, shieldSprite, selectedItem == firstEquipmentItem + 0))
			selectedItem = firstEquipmentItem + 0;
		if (drawItemSlot(x + width / 2 - slotSize / 2, y, slotSize, player.getArmorItem(ArmorSlot.Helmet), helmetSprite, selectedItem == firstEquipmentItem + 1))
			selectedItem = firstEquipmentItem + 1;
		if (drawItemSlot(x + width / 2 + slotSize / 2 + xpadding, y, slotSize, player.handItem, weaponSprite, selectedItem == firstEquipmentItem + 2))
			selectedItem = firstEquipmentItem + 2;

		if (selectedItem == firstEquipmentItem + 0)
			item = player.offhandItem;
		else if (selectedItem == firstEquipmentItem + 1)
			item = player.getArmorItem(ArmorSlot.Helmet);
		else if (selectedItem == firstEquipmentItem + 2)
			item = player.handItem;

		y += slotSize + ypadding;

		if (drawItemSlot(x + width / 2 - slotSize / 2 - xpadding - slotSize, y, slotSize, player.getArmorItem(ArmorSlot.Back), backpackSprite, selectedItem == firstEquipmentItem + 3))
			selectedItem = firstEquipmentItem + 3;
		if (drawItemSlot(x + width / 2 - slotSize / 2, y, slotSize, player.getArmorItem(ArmorSlot.Body), armorSprite, selectedItem == firstEquipmentItem + 4))
			selectedItem = firstEquipmentItem + 4;
		if (drawItemSlot(x + width / 2 + slotSize / 2 + xpadding, y, slotSize, player.getArmorItem(ArmorSlot.Gloves), glovesSprite, selectedItem == firstEquipmentItem + 5))
			selectedItem = firstEquipmentItem + 5;

		if (selectedItem == firstEquipmentItem + 3)
			item = player.getArmorItem(ArmorSlot.Back);
		else if (selectedItem == firstEquipmentItem + 4)
			item = player.getArmorItem(ArmorSlot.Body);
		else if (selectedItem == firstEquipmentItem + 5)
			item = player.getArmorItem(ArmorSlot.Gloves);

		y += slotSize + ypadding;

		if (drawItemSlot(x + width / 2 - slotSize / 2, y, slotSize, player.getArmorItem(ArmorSlot.Boots), bootsSprite, selectedItem == firstEquipmentItem + 6))
			selectedItem = firstEquipmentItem + 6;

		if (selectedItem == firstEquipmentItem + 6)
			item = player.getArmorItem(ArmorSlot.Boots);

		if (selectedItem >= firstEquipmentItem && selectedItem < firstActiveItem)
		{
			if (Input.IsKeyPressed(KeyCode.L))
			{
				Input.ConsumeKeyEvent(KeyCode.L);
				selectedItem = (selectedItem / 3) * 3 + (selectedItem + 1) % 3;
				Audio.PlayBackground(UISound.uiClick);
			}
			if (Input.IsKeyPressed(KeyCode.J))
			{
				Input.ConsumeKeyEvent(KeyCode.J);
				selectedItem = (selectedItem / 3) * 3 + (selectedItem + 3 - 1) % 3;
				Audio.PlayBackground(UISound.uiClick);
			}
			if (Input.IsKeyPressed(KeyCode.K))
			{
				Input.ConsumeKeyEvent(KeyCode.K);
				selectedItem = (selectedItem / 3 + 1) * 3 + selectedItem % 3;
				Audio.PlayBackground(UISound.uiClick);
			}
			if (Input.IsKeyPressed(KeyCode.I))
			{
				Input.ConsumeKeyEvent(KeyCode.I);
				if (selectedItem == firstActiveItem - 1)
					selectedItem -= 2;
				else
					selectedItem = (selectedItem / 3 - 1) * 3 + selectedItem % 3;
				Audio.PlayBackground(UISound.uiClick);
			}
			if (selectedItem < firstEquipmentItem)
				selectedItem = firstEquipmentItem;
			else if (selectedItem >= firstEquipmentItem + 6 + 3)
				selectedItem = firstActiveItem;
			else if (selectedItem >= firstActiveItem)
				selectedItem = firstActiveItem - 1;
		}

		y += slotSize + 8;

		for (int i = 0; i < player.activeItems.Length; i++)
		{
			int xx = i % 4;
			int yy = i / 4;

			bool selected = selectedItem == firstActiveItem + i;

			if (drawItemSlot(x + width / 2 - xpadding / 2 - slotSize - xpadding - slotSize + xx * (slotSize + xpadding), y + yy * (slotSize + ypadding), slotSize, player.activeItems[i], bagSprite, selected))
				selectedItem = firstActiveItem + i;

			if (selected)
				item = player.activeItems[i];
		}

		if (selectedItem >= firstActiveItem && selectedItem < firstStoredItem)
		{
			selectedItem -= firstActiveItem;
			if (Input.IsKeyPressed(KeyCode.L))
			{
				Input.ConsumeKeyEvent(KeyCode.L);
				selectedItem = (selectedItem / 4) * 4 + (selectedItem + 1) % 4;
				Audio.PlayBackground(UISound.uiClick);
			}
			if (Input.IsKeyPressed(KeyCode.J))
			{
				Input.ConsumeKeyEvent(KeyCode.J);
				selectedItem = (selectedItem / 4) * 4 + (selectedItem + 4 - 1) % 4;
				Audio.PlayBackground(UISound.uiClick);
			}
			if (Input.IsKeyPressed(KeyCode.K))
			{
				Input.ConsumeKeyEvent(KeyCode.K);
				selectedItem = (selectedItem / 4 + 1) * 4 + selectedItem % 4;
				Audio.PlayBackground(UISound.uiClick);
			}
			if (Input.IsKeyPressed(KeyCode.I))
			{
				Input.ConsumeKeyEvent(KeyCode.I);
				selectedItem = (selectedItem / 4 - 1) * 4 + selectedItem % 4;
				Audio.PlayBackground(UISound.uiClick);
			}
			if (selectedItem < 0)
				selectedItem = -1;
			selectedItem += firstActiveItem;
		}

		y += (player.activeItems.Length + 3) / 4 * (slotSize + ypadding) + 8;

		if (storedItems.Count > 0)
		{
			for (int i = 0; i < storedItems.Count; i++)
			{
				int xx = i % 4;
				int yy = i / 4;

				bool selected = selectedItem == firstStoredItem + i;

				if (drawItemSlot(x + width / 2 - xpadding / 2 - slotSize - xpadding - slotSize + xx * (slotSize + xpadding), y + yy * (slotSize + ypadding), slotSize, storedItems[i], bagSprite, selected))
					selectedItem = firstStoredItem + i;

				if (selected)
					item = storedItems[i];
			}

			if (selectedItem >= firstStoredItem && selectedItem < firstPassiveItem)
			{
				selectedItem -= firstStoredItem;
				if (Input.IsKeyPressed(KeyCode.L))
				{
					Input.ConsumeKeyEvent(KeyCode.L);
					selectedItem = (selectedItem / 4) * 4 + (selectedItem + 1) % 4;
					Audio.PlayBackground(UISound.uiClick);
				}
				if (Input.IsKeyPressed(KeyCode.J))
				{
					Input.ConsumeKeyEvent(KeyCode.J);
					selectedItem = (selectedItem / 4) * 4 + (selectedItem + 4 - 1) % 4;
					Audio.PlayBackground(UISound.uiClick);
				}
				if (Input.IsKeyPressed(KeyCode.K))
				{
					Input.ConsumeKeyEvent(KeyCode.K);
					selectedItem = (selectedItem / 4 + 1) * 4 + selectedItem % 4;
					Audio.PlayBackground(UISound.uiClick);
				}
				if (Input.IsKeyPressed(KeyCode.I))
				{
					Input.ConsumeKeyEvent(KeyCode.I);
					selectedItem = (selectedItem / 4 - 1) * 4 + selectedItem % 4;
					Audio.PlayBackground(UISound.uiClick);
				}
				if (selectedItem < 0)
					selectedItem = -1;
				selectedItem += firstStoredItem;
			}

			y += (storedItems.Count + 3) / 4 * (slotSize + ypadding) + 8;
		}

		int idx = 0;
		for (int i = 0; i < player.passiveItems.Count; i++)
		{
			if (player.passiveItems[i].armorSlot == ArmorSlot.None)
			{
				int xx = idx % 4;
				int yy = idx / 4;

				bool selected = selectedItem == firstPassiveItem + i;

				if (drawItemSlot(x + width / 2 - xpadding / 2 - slotSize - xpadding - slotSize + xx * (slotSize + xpadding), y + yy * (slotSize + ypadding), slotSize, player.passiveItems[i], ringSprite, selected))
					selectedItem = firstPassiveItem + i;

				if (selected)
					item = player.passiveItems[i];

				idx++;
			}
		}

		if (selectedItem >= firstPassiveItem && selectedItem < firstPassiveItem + idx)
		{
			selectedItem -= firstPassiveItem;
			if (Input.IsKeyPressed(KeyCode.L))
			{
				Input.ConsumeKeyEvent(KeyCode.L);
				selectedItem = (selectedItem / 4) * 4 + (selectedItem + 1) % 4;
				Audio.PlayBackground(UISound.uiClick);
			}
			if (Input.IsKeyPressed(KeyCode.J))
			{
				Input.ConsumeKeyEvent(KeyCode.J);
				selectedItem = (selectedItem / 4) * 4 + (selectedItem + 4 - 1) % 4;
				Audio.PlayBackground(UISound.uiClick);
			}
			if (Input.IsKeyPressed(KeyCode.K))
			{
				Input.ConsumeKeyEvent(KeyCode.K);
				selectedItem = (selectedItem / 4 + 1) * 4 + selectedItem % 4;
				Audio.PlayBackground(UISound.uiClick);
			}
			if (Input.IsKeyPressed(KeyCode.I))
			{
				Input.ConsumeKeyEvent(KeyCode.I);
				selectedItem = (selectedItem / 4 - 1) * 4 + selectedItem % 4;
				Audio.PlayBackground(UISound.uiClick);
			}
			if (selectedItem >= idx)
				selectedItem = idx - 1;
			selectedItem += firstPassiveItem;
		}

		y += (idx + 3) / 4 * (slotSize + ypadding) + 8;

		return y - top;
	}

	void openScreen()
	{
		player.inventoryOpen = true;
		player.numOverlaysOpen++;
		selectedItem = 0;
		currentScroll = 0;
	}

	void closeScreen()
	{
		player.inventoryOpen = false;
		player.numOverlaysOpen--;
	}

	public void render()
	{
		if (!player.inventoryOpen && player.numOverlaysOpen == 0 && InputManager.IsPressed("Inventory", true))
		{
			openScreen();
			Audio.PlayBackground(UISound.uiClick);
		}
		else if (player.inventoryOpen)
		{
			if (InputManager.IsPressed("Inventory", true) || InputManager.IsPressed("UIQuit", true) || InputManager.IsPressed("UIBack", true))
			{
				closeScreen();
				Audio.PlayBackground(UISound.uiBack);
			}
		}

		if (player.inventoryOpen)
		{
			Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, 0, null, 0x7F000000);

			characterHeight = CharacterInfoPanel.Render(10, 50, 140, characterHeight, player);

			int width = 90;
			int x = Renderer.UIWidth - 10 - width;
			int y = 50;

			inventoryHeight = DrawEquipment2(x, y, width, inventoryHeight, player, ref selectedItem, out Item selected);
			if (selected != null)
			{
				int sidePanelWidth = 90;
				sidePanelHeight = ItemInfoPanel.Render(selected, x - sidePanelWidth - 1, y, sidePanelWidth, sidePanelHeight);

				if (InputManager.IsPressed("UIConfirm2", true) || Input.IsMouseButtonPressed(MouseButton.Right, true))
				{
					if (selected.isHandItem || selected.isSecondaryItem || selected.isPassiveItem && selected.armorSlot != ArmorSlot.None || selected.isActiveItem)
					{
						player.throwItem(selected, true);
						player.removeItem(selected);
					}
					else
					{
						player.unequipItem(selected);
					}
				}
			}

			/*
			List<Item> items = player.items;
			int choice = ItemSelector.Render(10, 50, "Inventory", items, null, player.money, player, false, null, false, out bool secondary, out bool closed, ref selectedItem);

			if (choice != -1)
			{
				Item item = items[choice];

				if (item.isActiveItem)
				{
					if (!secondary)
					{
						if (player.isActiveItem(item, out _))
							player.unequipItem(item);
						else
							player.equipItem(item);
					}
					else
					{
						if (player.useActiveItem(item))
						{
							if (selectedItem == player.items.Count)
								selectedItem--;
						}
					}
				}
				if ((item.isHandItem || item.isPassiveItem) && secondary)
				{
					player.throwItem(item, true);
					player.removeItem(item);
					if (choice <= selectedItem)
						selectedItem--;
				}
				if (item.isHandItem && item.isSecondaryItem && !secondary)
				{
					if (item == player.handItem)
					{
						player.unequipItem(item);
						player.equipOffhandItem(item);
					}
					else if (item == player.offhandItem)
					{
						player.unequipItem(item);
						player.equipHandItem(item);
					}
				}

				selectedItem = Math.Max(Math.Min(selectedItem, items.Count - 1), 0);
			}

			// Item info panel
			if (items.Count > 0)
			{
				int sidePanelWidth = 80;
				int x = Renderer.UIWidth - 10 - sidePanelWidth;
				int y = 50;
				sidePanelHeight = ItemInfoPanel.Render(items[selectedItem], x, y, sidePanelWidth, sidePanelHeight);
			}
			*/
		}

		// Minimap
		if (player.inventoryOpen)
		{
			int width = 48;
			int height = 32;
			int x = Renderer.UIWidth - 10 - width;
			int y = 10;

			Vector2i playerTile = (Vector2i)Vector2.Floor(player.position + new Vector2(0, 0.5f));
			int scrollx = playerTile.x - width / 2;
			int scrolly = playerTile.y - height / 2;

			for (int yy = scrolly; yy < scrolly + height; yy++)
			{
				for (int xx = scrollx; xx < scrollx + width; xx++)
				{
					TileType tile = GameState.instance.level.getTile(xx, yy);
					if (tile != null)
					{
						Renderer.DrawUISprite(x + xx - scrollx, y + height - (yy - scrolly) - 1, 1, 1, null, false, tile.particleColor);
					}
				}
			}

			Renderer.DrawUISprite(x + playerTile.x - scrollx, y + height - (playerTile.y - scrolly) - 1, 1, 1, null, false, 0xFF00FF00);
		}
	}
}
