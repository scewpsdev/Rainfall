using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;


public class InventoryUI
{
	public static Sprite weaponSprite, shieldSprite, armorSprite, bagSprite, ringSprite;

	static InventoryUI()
	{
		weaponSprite = new Sprite(HUD.tileset, 0, 2, 2, 2);
		shieldSprite = new Sprite(HUD.tileset, 2, 2, 2, 2);
		bagSprite = new Sprite(HUD.tileset, 4, 2, 2, 2);
		armorSprite = new Sprite(HUD.tileset, 6, 2, 2, 2);
		ringSprite = new Sprite(HUD.tileset, 6, 4, 2, 2);
	}


	Player player;

	int selectedItem = 0;
	int sidePanelHeight = 40;

	int currentScroll = 0;


	public InventoryUI(Player player)
	{
		this.player = player;
	}

	static void drawItemSlot(int x, int y, int size, Item item)
	{
		Renderer.DrawUISprite(x - 1, y - 1, size + 2, size + 2, null, false, 0xFF333333);
		Renderer.DrawUISprite(x, y, size, size, null, false, 0xFF111111);
		if (item != null)
			Renderer.DrawUISprite(x, y, size, size, item.getIcon(), false, MathHelper.VectorToARGB(item.spriteColor));
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

		Renderer.DrawUISprite(x, y, 16, 16, armorSprite);
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

			List<Item> items = player.items;
			int choice = ItemSelector.Render(10, 50, "Inventory", items, null, player.money, player, false, null, false, out bool secondary, out bool closed, ref selectedItem);

			if (choice != -1)
			{
				Item item = items[choice];

				/*
				if (item.isHandItem && !secondary)
				{
					if (player.handItem == item)
						player.unequipItem(item);
					else
					{
						if (player.offhandItem == item)
							player.unequipItem(item);
						player.equipHandItem(item);
					}
				}
				if (item.isSecondaryItem && secondary)
				{
					if (player.offhandItem == item)
						player.unequipItem(item);
					else
					{
						if (player.handItem == item)
							player.unequipItem(item);
						player.equipOffhandItem(item);
					}
				}
				*/
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
				/*
				if (item.isPassiveItem && !secondary)
				{
					if (player.isPassiveItem(item, out _))
						player.unequipItem(item);
					else
						player.equipItem(item);
				}
				*/
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

			/*
			int lineHeight = 16;
			int headerHeight = 12 + 1;
			int sidePanelWidth = 80;
			int maxItems = 12;
			int shopWidth = 150;
			int shopHeight = Math.Min(player.items.Count, maxItems) * lineHeight;
			int width = shopWidth + 1 + sidePanelWidth;
			int height = headerHeight + shopHeight;
			int x = Renderer.UIWidth / 2 - width / 2;
			int y = Math.Min(50, Renderer.UIHeight / 2 - Math.Max(height, headerHeight + sidePanelHeight) / 2);
			int top = y;

			Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);

			Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, 0xFF222222);
			Renderer.DrawUITextBMP(x + 2, y + 2, "Inventory", 1, 0xFFAAAAAA);
			Renderer.DrawUISprite(x + width - 1 - HUD.gold.width, y + 2, HUD.gold.width, HUD.gold.height, HUD.gold);
			string moneyStr = GameState.instance.player.money.ToString();
			Renderer.DrawUITextBMP(x + width - 1 - HUD.gold.width - Renderer.MeasureUITextBMP(moneyStr, moneyStr.Length, 1).x - 2, y + 2, moneyStr, 1, 0xFFAAAAAA);
			y += headerHeight;

			if (player.items.Count > 0)
			{
				if (InputManager.IsPressed("Down", true) || InputManager.IsPressed("UIDown", true))
				{
					selectedItem = (selectedItem + 1) % player.items.Count;
					Audio.PlayBackground(UISound.uiClick);
				}
				if (InputManager.IsPressed("Up", true) || InputManager.IsPressed("UIUp", true))
				{
					selectedItem = (selectedItem + player.items.Count - 1) % player.items.Count;
					Audio.PlayBackground(UISound.uiClick);
				}

				if (selectedItem >= currentScroll + maxItems)
					currentScroll = selectedItem - maxItems + 1;
				else if (selectedItem >= 0 && selectedItem < currentScroll)
					currentScroll = selectedItem;

				if (Input.scrollMove != 0 && player.items.Count > maxItems)
				{
					currentScroll = Math.Clamp(currentScroll - Input.scrollMove, 0, player.items.Count - maxItems);
					selectedItem = Math.Clamp(selectedItem, currentScroll, currentScroll + maxItems - 1);
				}

				for (int i = currentScroll; i < Math.Min(player.items.Count, currentScroll + maxItems); i++)
				{
					if (Renderer.IsHovered(x, y, shopWidth, lineHeight) && Input.cursorHasMoved)
						selectedItem = i;

					bool selected = selectedItem == i;

					Item item = player.items[i].Item2;

					Renderer.DrawUISprite(x, y, shopWidth, lineHeight, null, false, selected ? 0xFF333333 : 0xFF222222);
					Renderer.DrawUISprite(x + 1, y + 1, 16, 16, item.getIcon(), false, MathHelper.VectorToARGB(item.spriteColor));
					string name = item.fullDisplayName;
					Renderer.DrawUITextBMP(x + 1 + 16 + 5, y + 4, name, 1, 0xFFAAAAAA);

					if (player.handItem == item)
						Renderer.DrawUISprite(x + shopWidth - 3 - 16, y, 16, 16, weaponSprite);
					else if (player.offhandItem == item)
						Renderer.DrawUISprite(x + shopWidth - 3 - 16, y, 16, 16, shieldSprite);
					else if (player.isActiveItem(item, out int activeSlot))
					{
						Renderer.DrawUISprite(x + shopWidth - 3 - 16, y, 16, 16, bagSprite);
						Renderer.DrawUITextBMP(x + shopWidth - 3 - 4, y + 16 - 8, (activeSlot + 1).ToString(), 1, 0xFF505050);
					}
					else if (player.isPassiveItem(item, out int passiveSlot))
					{
						Renderer.DrawUISprite(x + shopWidth - 3 - 16, y, 16, 16, item.type == ItemType.Ring ? ringSprite : armorSprite);
						Renderer.DrawUITextBMP(x + shopWidth - 3 - 4, y + 16 - 8, (passiveSlot + 1 - (item.type == ItemType.Ring ? player.passiveItems.Length - 2 : 0)).ToString(), 1, 0xFF505050);
					}

					if (selected)
					{
						if (item.isHandItem)
						{
							if (InputManager.IsPressed("UIConfirm", true) || Input.IsMouseButtonPressed(MouseButton.Left, true))
							{
								if (player.handItem == item)
									player.unequipItem(item);
								else
								{
									if (player.offhandItem == item)
										player.unequipItem(item);
									player.equipHandItem(item);
								}
								Audio.PlayBackground(UISound.uiConfirm2);
							}
						}
						if (item.isSecondaryItem)
						{
							if (InputManager.IsPressed("UIConfirm2", true) || Input.IsMouseButtonPressed(MouseButton.Right, true))
							{
								if (player.offhandItem == item)
									player.unequipItem(item);
								else
								{
									if (player.handItem == item)
										player.unequipItem(item);
									player.equipOffhandItem(item);
								}
								Audio.PlayBackground(UISound.uiConfirm2);
							}
						}
						if (item.isActiveItem)
						{
							if (InputManager.IsPressed("UIConfirm", true) || Input.IsMouseButtonPressed(MouseButton.Left, true))
							{
								if (player.isActiveItem(item, out _))
									player.unequipItem(item);
								else
									player.equipItem(item);
								Audio.PlayBackground(UISound.uiClick);
							}
							if (InputManager.IsPressed("UIConfirm2", true) || Input.IsMouseButtonPressed(MouseButton.Right, true))
							{
								if (player.useActiveItem(item))
								{
									i--;
									if (selectedItem == player.items.Count)
										selectedItem--;
								}
							}
							//Audio.PlayBackground(UISound.uiClick);

						}
						if (item.isPassiveItem)
						{
							if (InputManager.IsPressed("UIConfirm", true) || Input.IsMouseButtonPressed(MouseButton.Left, true))
							{
								if (player.isPassiveItem(item, out _))
									player.unequipItem(item);
								else
									player.equipItem(item);
							}
						}
					}

					y += lineHeight;
				}

				// Scroll bar
				if (player.items.Count > maxItems)
				{
					float fraction = maxItems / (float)player.items.Count;
					float offset = currentScroll / (float)player.items.Count;
					Renderer.DrawUISprite(x + shopWidth - 2, top + headerHeight + 1 + (int)(offset * shopHeight), 1, (int)(fraction * shopHeight) - 2, 0, null, 0xFF777777);
				}

				// Item info panel
				if (player.items.Count > 0)
					sidePanelHeight = ItemInfoPanel.Render(player.items[selectedItem].Item2, x + shopWidth + 1, top + headerHeight, sidePanelWidth, Math.Max(shopHeight, sidePanelHeight));
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

			//Renderer.DrawUISprite(x, y, width, height, null, false, 0xFF000000);

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
