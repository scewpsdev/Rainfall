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

	void drawItemSlot(int x, int y, int size, Item item)
	{
		int border = 4;
		Renderer.DrawUISprite(x - border, y - border, size + 2 * border, size + 2 * border, null, false, 0xFF333333);
		Renderer.DrawUISprite(x, y, size, size, null, false, 0xFF111111);
		if (item != null)
			Renderer.DrawUISprite(x, y, size, size, item.sprite);
	}

	public void render()
	{
		if (InputManager.IsPressed("Inventory"))
			player.inventoryOpen = !player.inventoryOpen;

		if (player.inventoryOpen)
		{
			int x = 50;
			int y = 50;
			int width = 800;
			int height = Renderer.UIHeight - 2 * 50;
			Renderer.DrawUISprite(x, y, width, height, null, false, 0xFF111111);

			int handItemSlotSize = 16 * 6;
			drawItemSlot(x + 50, y + 50, handItemSlotSize, player.handItem);

			int xpadding = 12;
			int ypadding = 20;
			int slotSize = 16 * 4;

			for (int i = 0; i < player.quickItems.Length; i++)
				drawItemSlot(x + 50 + i * (slotSize + xpadding), y + 50 + handItemSlotSize + ypadding, slotSize, player.quickItems[i]);

			for (int i = 0; i < player.passiveItems.Length; i++)
				drawItemSlot(x + 50 + i * (slotSize + xpadding), y + 50 + handItemSlotSize + ypadding + slotSize + ypadding, slotSize, player.passiveItems[i]);
		}
	}
}
