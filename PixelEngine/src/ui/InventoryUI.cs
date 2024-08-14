using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class InventoryUI
{
	Player player;


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

	static void drawInventory(int x, int y, int width, int height, Player player)
	{
		Renderer.DrawUISprite(x, y, width, height, null, false, 0xFF111111);

		int handItemSlotSize = 16 * 3;
		drawItemSlot(x + 16, y + 16, handItemSlotSize, 3, player.handItem);

		int xpadding = 2;
		int ypadding = 4;
		int slotSize = 16 * 2;

		for (int i = 0; i < player.quickItems.Length; i++)
			drawItemSlot(x + 16 + i * (slotSize + xpadding), y + 16 + handItemSlotSize + ypadding, slotSize, 2, player.quickItems[i]);

		for (int i = 0; i < player.passiveItems.Length; i++)
			drawItemSlot(x + 16 + i * (slotSize + xpadding), y + 16 + handItemSlotSize + ypadding + slotSize + ypadding, slotSize, 2, player.passiveItems[i]);

	}

	public void render()
	{
		if (InputManager.IsPressed("Inventory"))
			player.inventoryOpen = !player.inventoryOpen;

		if (player.inventoryOpen)
		{
			int x = 16;
			int y = 16;
			int width = Renderer.UIWidth / 2 - 16;
			int height = Renderer.UIHeight - 2 * 16;

			drawInventory(x, y, width, height, player);
		}
	}
}
