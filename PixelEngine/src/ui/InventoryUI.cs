using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
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
		for (int i = 0; i < player.passiveItems.Length - 2; i++)
			drawItemSlot(x + 16 + 2 + i * (slotSize + xpadding), y, slotSize, player.passiveItems[i]);
		y += slotSize + ypadding;

		Renderer.DrawUISprite(x, y, 16, 16, ringSprite);
		for (int i = 0; i < 2; i++)
			drawItemSlot(x + 16 + 2 + i * (slotSize + xpadding), y, slotSize, player.passiveItems[player.passiveItems.Length - 2 + i]);
	}

	void openScreen()
	{
		player.inventoryOpen = true;
		player.numOverlaysOpen++;
		selectedItem = 0;
	}

	void closeScreen()
	{
		player.inventoryOpen = false;
		player.numOverlaysOpen--;
	}

	public void render()
	{
		if (!player.inventoryOpen && player.numOverlaysOpen == 0 && InputManager.IsPressed("Inventory", true))
			openScreen();
		else if (player.inventoryOpen)
		{
			if (InputManager.IsPressed("Inventory", true) || InputManager.IsPressed("UIQuit", true) || InputManager.IsPressed("UIBack", true))
				closeScreen();
		}

		if (player.inventoryOpen)
		{
			int lineHeight = 16;
			int headerHeight = 12 + 1;
			int sidePanelWidth = 80;
			int shopWidth = 150;
			int width = shopWidth + 1 + sidePanelWidth;
			int height = headerHeight + player.items.Count * lineHeight;
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
					selectedItem = (selectedItem + 1) % player.items.Count;
				if (InputManager.IsPressed("Up", true) || InputManager.IsPressed("UIUp", true))
					selectedItem = (selectedItem + player.items.Count - 1) % player.items.Count;

				for (int i = 0; i < player.items.Count; i++)
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
						Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, weaponSprite);
					else if (player.offhandItem == item)
						Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, shieldSprite);
					else if (player.isActiveItem(item, out int activeSlot))
					{
						Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, bagSprite);
						Renderer.DrawUITextBMP(x + shopWidth - 1 - 4, y + 16 - 8, (activeSlot + 1).ToString(), 1, 0xFF505050);
					}
					else if (player.isPassiveItem(item, out int passiveSlot))
					{
						Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, item.type == ItemType.Ring ? ringSprite : armorSprite);
						Renderer.DrawUITextBMP(x + shopWidth - 1 - 4, y + 16 - 8, (passiveSlot + 1 - (item.type == ItemType.Ring ? player.passiveItems.Length - 2 : 0)).ToString(), 1, 0xFF505050);
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
							}
							if (InputManager.IsPressed("UIConfirm2", true))
								if (player.useActiveItem(item))
								{
									i--;
									if (selectedItem == player.items.Count)
										selectedItem--;
								}

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

				// Item info panel
				if (player.items.Count > 0)
					sidePanelHeight = ItemInfoPanel.Render(player.items[selectedItem].Item2, x + shopWidth + 1, top + headerHeight, sidePanelWidth, Math.Max(player.items.Count * lineHeight, sidePanelHeight));
			}
		}
	}
}
