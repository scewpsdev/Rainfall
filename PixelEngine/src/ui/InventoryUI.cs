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


	public InventoryUI(Player player)
	{
		this.player = player;
	}

	static void drawItemSlot(int x, int y, int size, int border, Item item)
	{
		Renderer.DrawUISprite(x, y, size, size, null, false, 0xFF333333);
		Renderer.DrawUISprite(x + border, y + border, size - 2 * border, size - 2 * border, null, false, 0xFF111111);
		if (item != null)
			Renderer.DrawUISprite(x, y, size, size, item.sprite);
	}

	public static void DrawInventory(int x, int y, int width, int height, Player player)
	{
		Renderer.DrawUISprite(x, y, width, height, null, false, 0xFF111111);

		int handItemSlotSize = 16 * 2;
		drawItemSlot(x + 16, y + 16, handItemSlotSize, 2, player.handItem);

		int xpadding = 2;
		int ypadding = 4;
		int slotSize = 24;

		for (int i = 0; i < player.quickItems.Length; i++)
			drawItemSlot(x + 16 + i * (slotSize + xpadding), y + 16 + handItemSlotSize + ypadding, slotSize, 2, player.quickItems[i]);

		for (int i = 0; i < player.passiveItems.Length; i++)
			drawItemSlot(x + 16 + i * (slotSize + xpadding), y + 16 + handItemSlotSize + ypadding + slotSize + ypadding, slotSize, 2, player.passiveItems[i]);

	}

	public void render()
	{
		if (InputManager.IsPressed("Inventory") && player.numOverlaysOpen == 0)
		{
			player.inventoryOpen = !player.inventoryOpen;
		}

		if (player.inventoryOpen)
		{
			if (InputManager.IsPressed("UIQuit") || InputManager.IsPressed("UIBack"))
			{
				player.inventoryOpen = false;
			}

			List<Item> sellItems = new List<Item>();
			if (player.handItem != null)
				sellItems.Add(player.handItem);
			for (int i = 0; i < player.quickItems.Length; i++)
			{
				if (player.quickItems[i] != null)
					sellItems.Add(player.quickItems[i]);
			}
			for (int i = 0; i < player.passiveItems.Length; i++)
			{
				if (player.passiveItems[i] != null)
					sellItems.Add(player.passiveItems[i]);
			}

			int lineHeight = 16;
			int headerHeight = 12 + 1;
			int sidePanelWidth = 80;
			int shopWidth = 150;
			int width = shopWidth + 1 + sidePanelWidth;
			int height = headerHeight + sellItems.Count * lineHeight;
			int x = Renderer.UIWidth / 2 - width / 2;
			int y = Renderer.UIHeight / 2 - Math.Max(height, headerHeight + sidePanelHeight) / 2;
			int top = y;

			Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);

			Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, 0xFF222222);
			Renderer.DrawUITextBMP(x + 2, y + 2, "Inventory", 1, 0xFFAAAAAA);
			Renderer.DrawUISprite(x + width - 1 - HUD.gem.width, y + 2, HUD.gem.width, HUD.gem.height, HUD.gem);
			string moneyStr = GameState.instance.player.money.ToString();
			Renderer.DrawUITextBMP(x + width - 1 - HUD.gem.width - Renderer.MeasureUITextBMP(moneyStr, moneyStr.Length, 1).x - 2, y + 2, moneyStr, 1, 0xFFAAAAAA);
			y += headerHeight;

			if (sellItems.Count > 0)
			{
				if (InputManager.IsPressed("Down"))
					selectedItem = (selectedItem + 1) % sellItems.Count;
				if (InputManager.IsPressed("Up"))
					selectedItem = (selectedItem + sellItems.Count - 1) % sellItems.Count;

				for (int i = 0; i < sellItems.Count; i++)
				{
					bool selected = selectedItem == i;

					Item item = sellItems[i];

					Renderer.DrawUISprite(x, y, shopWidth, lineHeight, null, false, selected ? 0xFF333333 : 0xFF222222);
					Renderer.DrawUISprite(x + 1, y + 1, lineHeight, lineHeight, item.sprite);
					string name = item.fullDisplayName;
					Renderer.DrawUITextBMP(x + 1 + lineHeight + 5, y + 4, name, 1, 0xFFAAAAAA);

					/*
					if (selected && InputManager.IsPressed("Interact"))
					{
						InputManager.ConsumeEvent("Interact");

						if (item.stackable && item.stackSize > 1)
						{
							item.stackSize--;
							Item copy = item.copy();
							copy.stackSize = 1;
							addShopItem(copy);
							GameState.instance.player.money += price;
						}
						else
						{
							if (GameState.instance.player.removeItem(item))
							{
								addShopItem(item);
								GameState.instance.player.money += price;
								sellItems.RemoveAt(i--);

								if (selectedItem == sellItems.Count)
									selectedItem--;
								if (sellItems.Count == 0)
									initMenu();
							}
						}
					}
					*/

					y += lineHeight;
				}

				// Item info panel
				sidePanelHeight = ItemInfoPanel.Render(sellItems[selectedItem], x + shopWidth + 1, top + headerHeight, sidePanelWidth, Math.Max(sellItems.Count * lineHeight, sidePanelHeight));
			}
		}
	}
}
