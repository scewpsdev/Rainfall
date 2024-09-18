using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


public static class ItemSelector
{
	static int longestLineWidth = 150;
	static int sidePanelHeight = 40;

	public static int Render(Vector2i pos, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, out bool closed, ref int selectedItem)
	{
		int lineHeight = 16;
		int headerHeight = 12 + 1;
		int sidePanelWidth = 80;
		int shopWidth = Math.Max(120, 1 + lineHeight + 5 + longestLineWidth + 1);
		int width = shopWidth + 1 + sidePanelWidth;
		int height = headerHeight + items.Count * lineHeight;
		int x = Math.Min(pos.x, Renderer.UIWidth - width - 2);
		int y = Math.Max(pos.y - height, 2);

		Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);

		Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, 0xFF222222);
		Renderer.DrawUITextBMP(x + 2, y + 2, title, 1, 0xFFAAAAAA);
		Renderer.DrawUISprite(x + width - 1 - HUD.gold.width, y + 2, HUD.gold.width, HUD.gold.height, HUD.gold);
		string moneyStr = GameState.instance.player.money.ToString();
		Renderer.DrawUITextBMP(x + width - 1 - HUD.gold.width - Renderer.MeasureUITextBMP(moneyStr, moneyStr.Length, 1).x - 2, y + 2, moneyStr, 1, 0xFFAAAAAA);
		y += headerHeight;

		if (InputManager.IsPressed("Down", true) || InputManager.IsPressed("UIDown", true))
			selectedItem = (selectedItem + 1) % items.Count;
		if (InputManager.IsPressed("Up", true) || InputManager.IsPressed("UIUp", true))
			selectedItem = (selectedItem + items.Count - 1) % items.Count;

		int choice = -1;
		for (int i = 0; i < items.Count; i++)
		{
			if (Renderer.IsHovered(x, y, shopWidth, lineHeight) && Input.cursorHasMoved)
				selectedItem = i;
			bool selected = selectedItem == i;

			Item item = items[i];

			Renderer.DrawUISprite(x, y, shopWidth, lineHeight, null, false, selected ? 0xFF333333 : 0xFF222222);
			Renderer.DrawUISprite(x + 1, y + 1, lineHeight, lineHeight, item.getIcon(), false, MathHelper.VectorToARGB(item.spriteColor));
			string name = item.fullDisplayName;
			Renderer.DrawUITextBMP(x + 1 + lineHeight + 5, y + 4, name, 1, 0xFFAAAAAA);

			int lineWidth = Renderer.MeasureUITextBMP(name, name.Length, 1).x + 5;

			if (prices != null)
			{
				int price = prices[i];
				string quantity = price.ToString();
				bool canAfford = money >= price;
				uint color = money == -1 ? 0xFFf4d16b : canAfford ? 0xFFAAAAAA : 0xFFAA3333;
				Renderer.DrawUITextBMP(x + shopWidth - (renderSlotIcons != null ? 1 + 16 : 4) - Renderer.MeasureUITextBMP(quantity, quantity.Length, 1).x, y + 4, quantity, 1, color);

				lineWidth += Renderer.MeasureUITextBMP(quantity).x;
			}

			if (renderSlotIcons != null)
			{
				Player player = renderSlotIcons;
				if (player.handItem == item)
					Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, InventoryUI.weaponSprite);
				else if (player.offhandItem == item)
					Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, InventoryUI.shieldSprite);
				else if (player.isActiveItem(item, out int activeSlot))
				{
					Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, InventoryUI.bagSprite);
					Renderer.DrawUITextBMP(x + shopWidth - 1 - 4, y + 16 - 8, (activeSlot + 1).ToString(), 1, 0xFF505050);
				}
				else if (player.isPassiveItem(item, out int passiveSlot))
				{
					Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, item.type == ItemType.Ring ? InventoryUI.ringSprite : InventoryUI.armorSprite);
					Renderer.DrawUITextBMP(x + shopWidth - 1 - 4, y + 16 - 8, (passiveSlot + 1 - (item.type == ItemType.Ring ? player.passiveItems.Length - 2 : 0)).ToString(), 1, 0xFF505050);
				}

				lineWidth += 16;
			}

			longestLineWidth = Math.Max(longestLineWidth, lineWidth);

			if (selected && (InputManager.IsPressed("Interact", true) || Input.IsMouseButtonPressed(MouseButton.Left, true)))
				choice = i;

			y += lineHeight;
		}

		// Item info panel
		if (items.Count > 0)
			sidePanelHeight = ItemInfoPanel.Render(items[selectedItem], x + shopWidth + 1, Math.Max(pos.y - height, 2) + headerHeight, sidePanelWidth, Math.Max(items.Count * lineHeight, sidePanelHeight));

		closed = InputManager.IsPressed("UIBack", true) || InputManager.IsPressed("UIClose");
		if (closed)
		{
			longestLineWidth = 150;
			sidePanelHeight = 40;
		}
		return choice;
	}
}
