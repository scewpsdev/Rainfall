using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


public static class ItemSelector
{
	static int longestLineWidth = 60;
	static int sidePanelHeight = 40;
	static int maxItems = 10;
	static int currentScroll = 0;

	public static int Render(int x, int y, int width, int height, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, bool renderInfoPanel, out bool secondary, out bool closed, ref int selectedItem)
	{
		secondary = false;

		int lineHeight = 16;
		int headerHeight = 12 + 1;
		int sidePanelWidth = 80;
		int shopWidth = Math.Max(60, 1 + lineHeight + 5 + longestLineWidth + 1);
		int shopHeight = Math.Min(items.Count, maxItems) * lineHeight;

		int top = y;

		Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);

		Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, 0xFF222222);
		Renderer.DrawUITextBMP(x + 2, y + 2, title, 1, 0xFFAAAAAA);
		Renderer.DrawUISprite(x + width - 1 - HUD.gold.width, y + 2, HUD.gold.width, HUD.gold.height, HUD.gold);
		string moneyStr = GameState.instance.player.money.ToString();
		Renderer.DrawUITextBMP(x + width - 1 - HUD.gold.width - Renderer.MeasureUITextBMP(moneyStr, moneyStr.Length, 1).x - 2, y + 2, moneyStr, 1, 0xFFAAAAAA);
		y += headerHeight;

		if ((InputManager.IsPressed("Down", true) || InputManager.IsPressed("UIDown", true)) && items.Count > 0)
		{
			selectedItem = (selectedItem + 1) % items.Count;
			Audio.PlayBackground(UISound.uiClick);
		}
		if ((InputManager.IsPressed("Up", true) || InputManager.IsPressed("UIUp", true)) && items.Count > 0)
		{
			selectedItem = (selectedItem + items.Count - 1) % items.Count;
			Audio.PlayBackground(UISound.uiClick);
		}

		if (selectedItem >= currentScroll + maxItems)
			currentScroll = selectedItem - maxItems + 1;
		else if (selectedItem >= 0 && selectedItem < currentScroll)
			currentScroll = selectedItem;

		if (Input.scrollMove != 0 && items.Count > maxItems)
		{
			currentScroll = Math.Clamp(currentScroll - Input.scrollMove, 0, items.Count - maxItems);
			selectedItem = Math.Clamp(selectedItem, currentScroll, currentScroll + maxItems - 1);
		}

		int choice = -1;
		for (int i = currentScroll; i < Math.Min(items.Count, currentScroll + maxItems); i++)
		{
			if (Renderer.IsHovered(x, y, shopWidth, lineHeight) && Input.cursorHasMoved && selectedItem != i)
			{
				selectedItem = i;
				Audio.PlayBackground(UISound.uiClick);
			}
			bool selected = selectedItem == i;

			Item item = items[i];

			Renderer.DrawUISprite(x, y, shopWidth, lineHeight, null, false, selected ? 0xFF333333 : 0xFF222222);
			Renderer.DrawUISprite(x + 1, y, lineHeight, lineHeight, item.getIcon(), false, MathHelper.VectorToARGB(item.spriteColor));
			string name = item.fullDisplayName;
			Renderer.DrawUITextBMP(x + 1 + lineHeight + 5, y + 4, name, 1, 0xFFAAAAAA);

			int lineWidth = Renderer.MeasureUITextBMP(name).x + 5;

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
					Renderer.DrawUISprite(x + shopWidth - 3 - 16, y, 16, 16, InventoryUI.weaponSprite);
				else if (player.offhandItem == item)
					Renderer.DrawUISprite(x + shopWidth - 3 - 16, y, 16, 16, InventoryUI.shieldSprite);
				else if (player.isActiveItem(item, out int activeSlot))
				{
					Renderer.DrawUISprite(x + shopWidth - 3 - 16, y, 16, 16, InventoryUI.bagSprite);
					Renderer.DrawUITextBMP(x + shopWidth - 3 - 4, y + 16 - 8, (activeSlot + 1).ToString(), 1, 0xFF505050);
				}
				else if (player.isPassiveItem(item, out int passiveSlot))
				{
					Renderer.DrawUISprite(x + shopWidth - 3 - 16, y, 16, 16, item.type == ItemType.Ring ? InventoryUI.ringSprite : InventoryUI.armorSprite);
					if (passiveSlot != -1)
						Renderer.DrawUITextBMP(x + shopWidth - 3 - 4, y + 16 - 8, (passiveSlot + 1 - (item.type == ItemType.Ring ? player.passiveItems.Count - 2 : 0)).ToString(), 1, 0xFF505050);
				}

				lineWidth += 16;
			}

			lineWidth += 3;

			longestLineWidth = Math.Max(longestLineWidth, lineWidth);

			if (selected && (InputManager.IsPressed("UIConfirm", true) || Input.IsMouseButtonPressed(MouseButton.Left, true)))
			{
				choice = i;
				Audio.PlayBackground(UISound.uiConfirm2);
			}
			if (selected && (InputManager.IsPressed("UIConfirm2", true) || Input.IsMouseButtonPressed(MouseButton.Right, true)))
			{
				choice = i;
				secondary = true;
				Audio.PlayBackground(UISound.uiConfirm2);
			}

			y += lineHeight;
		}

		// Scroll bar
		if (items.Count > maxItems)
		{
			float fraction = maxItems / (float)items.Count;
			float offset = currentScroll / (float)items.Count;
			Renderer.DrawUISprite(x + shopWidth - 2, top + headerHeight + 1 + (int)(offset * shopHeight), 1, (int)(fraction * shopHeight) - 2, 0, null, 0xFF777777);
		}

		// Item info panel
		if (items.Count > 0 && renderInfoPanel)
		{
			sidePanelHeight = ItemInfoPanel.Render(items[selectedItem], x + shopWidth + 1, top + headerHeight, sidePanelWidth, Math.Max(shopHeight, sidePanelHeight));
		}

		closed = InputManager.IsPressed("UIBack", true) || InputManager.IsPressed("UIClose");
		if (closed)
		{
			longestLineWidth = 60;
			sidePanelHeight = 40;
			Audio.PlayBackground(UISound.uiBack);
		}

		return choice;
	}

	public static int Render(Vector2i pos, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, bool renderInfoPanel, out bool secondary, out bool closed, ref int selectedItem)
	{
		int lineHeight = 16;
		int headerHeight = 12 + 1;
		int sidePanelWidth = 80;
		int shopWidth = Math.Max(60, 1 + lineHeight + 5 + longestLineWidth + 1);
		int shopHeight = Math.Min(items.Count, maxItems) * lineHeight;
		int width = shopWidth + (renderInfoPanel ? 1 + sidePanelWidth : 0);
		int height = headerHeight + shopHeight;
		int x = Math.Clamp(pos.x, 2, Renderer.UIWidth - width - 2);
		int y = Math.Clamp(pos.y - height, 2, Renderer.UIHeight - height - 2);

		return Render(x, y, width, height, title, items, prices, money, renderSlotIcons, renderInfoPanel, out secondary, out closed, ref selectedItem);
	}

	public static int Render(int x, int y, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, bool renderInfoPanel, out bool secondary, out bool closed, ref int selectedItem)
	{
		int lineHeight = 16;
		int headerHeight = 12 + 1;
		int sidePanelWidth = 80;
		int shopWidth = Math.Max(60, 1 + lineHeight + 5 + longestLineWidth + 1);
		int shopHeight = Math.Min(items.Count, maxItems) * lineHeight;
		int width = shopWidth + (renderInfoPanel ? 1 + sidePanelWidth : 0);
		int height = headerHeight + shopHeight;
		x = Math.Clamp(x, 2, Renderer.UIWidth - width - 2);
		y = Math.Clamp(y, 2, Renderer.UIHeight - height - 2);

		return Render(x, y, width, height, title, items, prices, money, renderSlotIcons, renderInfoPanel, out secondary, out closed, ref selectedItem);
	}
}
