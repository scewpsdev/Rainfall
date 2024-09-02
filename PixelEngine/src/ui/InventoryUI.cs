using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class InventoryUI
{
	Player player;

	int selectedItem = 0;
	int sidePanelHeight = 40;

	Sprite weaponSprite, shieldSprite, armorSprite, bagSprite;


	public InventoryUI(Player player)
	{
		this.player = player;

		weaponSprite = new Sprite(HUD.tileset, 0, 2, 2, 2);
		shieldSprite = new Sprite(HUD.tileset, 4, 2, 2, 2);
		armorSprite = new Sprite(HUD.tileset, 6, 4, 2, 2);
		bagSprite = new Sprite(HUD.tileset, 2, 4, 2, 2);
	}

	static void drawItemSlot(int x, int y, int size, int border, Item item)
	{
		Renderer.DrawUISprite(x, y, size, size, null, false, 0xFF333333);
		Renderer.DrawUISprite(x + border, y + border, size - 2 * border, size - 2 * border, null, false, 0xFF111111);
		if (item != null)
			Renderer.DrawUISprite(x, y, size, size, item.sprite);
	}

	public static void DrawEquipment(int x, int y, int width, int height, Player player)
	{
		//Renderer.DrawUISprite(x, y, width, height, null, false, 0xFF111111);

		int handItemSlotSize = 16 * 2;
		drawItemSlot(x + 16, y + 16, handItemSlotSize, 2, player.handItem);

		int xpadding = 2;
		int ypadding = 4;
		int slotSize = 24;

		for (int i = 0; i < player.activeItems.Length; i++)
			drawItemSlot(x + 16 + i * (slotSize + xpadding), y + 16 + handItemSlotSize + ypadding, slotSize, 2, player.activeItems[i]);

		for (int i = 0; i < player.passiveItems.Length; i++)
			drawItemSlot(x + 16 + i * (slotSize + xpadding), y + 16 + handItemSlotSize + ypadding + slotSize + ypadding, slotSize, 2, player.passiveItems[i]);

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
			Renderer.DrawUISprite(x + width - 1 - HUD.gem.width, y + 2, HUD.gem.width, HUD.gem.height, HUD.gem);
			string moneyStr = GameState.instance.player.money.ToString();
			Renderer.DrawUITextBMP(x + width - 1 - HUD.gem.width - Renderer.MeasureUITextBMP(moneyStr, moneyStr.Length, 1).x - 2, y + 2, moneyStr, 1, 0xFFAAAAAA);
			y += headerHeight;

			if (player.items.Count > 0)
			{
				if (InputManager.IsPressed("Down"))
					selectedItem = (selectedItem + 1) % player.items.Count;
				if (InputManager.IsPressed("Up"))
					selectedItem = (selectedItem + player.items.Count - 1) % player.items.Count;

				for (int i = 0; i < player.items.Count; i++)
				{
					if (Renderer.IsHovered(x, y, shopWidth, lineHeight) && Input.cursorHasMoved)
						selectedItem = i;

					bool selected = selectedItem == i;

					Item item = player.items[i].Item2;

					Renderer.DrawUISprite(x, y, shopWidth, lineHeight, null, false, selected ? 0xFF333333 : 0xFF222222);
					Renderer.DrawUISprite(x + 1, y + 1, 16, 16, item.sprite);
					string name = item.fullDisplayName;
					Renderer.DrawUITextBMP(x + 1 + 16 + 5, y + 4, name, 1, 0xFFAAAAAA);

					if (player.handItem == item)
						Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, weaponSprite);
					else if (player.offhandItem == item)
						Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, shieldSprite);
					else if (player.isActiveItem(item))
						Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, bagSprite);
					else if (player.isPassiveItem(item))
						Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, armorSprite);

					if (selected && (InputManager.IsPressed("Interact", true) || Input.IsMouseButtonPressed(MouseButton.Left)))
					{
						if (Input.IsMouseButtonPressed(MouseButton.Left))
							Input.ConsumeMouseButtonEvent(MouseButton.Left);

						if (item.isHandItem)
						{
							if (player.handItem == item)
								player.unequipItem(item);
							else
								player.equipItem(item);
						}
						else if (item.isActiveItem)
						{
							if (player.useActiveItem(item))
							{
								i--;

								if (selectedItem == player.items.Count)
									selectedItem--;
							}
						}
						else if (item.isPassiveItem)
						{
							if (player.isPassiveItem(item))
								player.unequipItem(item);
							else
								player.equipItem(item);
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
