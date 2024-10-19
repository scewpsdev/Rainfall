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

	public static int Render(int x, int y, int width, int height, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, bool renderInfoPanel, Item compareItem, bool infoPanelShowCompareItem, bool takeInput, out bool secondary, out bool closed, ref int selectedItem)
	{
		secondary = false;
		closed = false;

		int lineHeight = 16;
		int headerHeight = 12 + 1;
		int sidePanelWidth = 90;
		int shopWidth = Math.Min(Math.Max(60, 1 + lineHeight + 5 + longestLineWidth + 1), width);
		int shopHeight = Math.Min(items.Count, maxItems) * lineHeight;

		int top = y;

		Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, UIColors.WINDOW_FRAME);

		Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, UIColors.WINDOW_BACKGROUND);
		Renderer.DrawUISprite(x, y + headerHeight, width, height - headerHeight, null, false, UIColors.WINDOW_BACKGROUND);
		Renderer.DrawUITextBMP(x + 2, y + 2, title, 1, UIColors.TEXT);
		if (money != -1)
		{
			Renderer.DrawUISprite(x + width - 1 - HUD.gold.width, y + 2, HUD.gold.width, HUD.gold.height, HUD.gold);
			string moneyStr = money.ToString();
			Renderer.DrawUITextBMP(x + width - 1 - HUD.gold.width - Renderer.MeasureUITextBMP(moneyStr, moneyStr.Length, 1).x - 2, y + 2, moneyStr, 1, UIColors.TEXT);
		}
		y += headerHeight;

		if (takeInput)
		{
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
			if (takeInput)
			{
				if (Renderer.IsHovered(x, y, shopWidth, lineHeight) && Input.cursorHasMoved && selectedItem != i)
				{
					selectedItem = i;
					Audio.PlayBackground(UISound.uiClick);
				}
			}
			bool selected = selectedItem == i;

			Item item = items[i];

			Renderer.DrawUISprite(x, y, shopWidth, lineHeight, null, false, selected ? UIColors.ITEM_SLOT_BACKGROUND_HIGHLIGHT : UIColors.ITEM_SLOT_BACKGROUND);
			Renderer.DrawUISprite(x, y, lineHeight, lineHeight, item.getIcon(), false, MathHelper.VectorToARGB(item.spriteColor));
			string name = item.fullDisplayName;
			Renderer.DrawUITextBMP(x + lineHeight + 5, y + 4, name, 1, UIColors.TEXT);

			int lineWidth = Renderer.MeasureUITextBMP(name).x + 5;

			if (prices != null)
			{
				int price = prices[i];
				string quantity = price.ToString();
				bool canAfford = money >= price;
				uint color = money == -1 ? UIColors.TEXT_MONEY : canAfford ? UIColors.TEXT : UIColors.TEXT_DOWNGRADE;
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
					Renderer.DrawUISprite(x + shopWidth - 3 - 16, y, 16, 16, item.type == ItemType.Relic ? InventoryUI.ringSprite : InventoryUI.helmetSprite);
					if (passiveSlot != -1)
						Renderer.DrawUITextBMP(x + shopWidth - 3 - 4, y + 16 - 8, (passiveSlot + 1 - (item.type == ItemType.Relic ? player.passiveItems.Count - 2 : 0)).ToString(), 1, 0xFF505050);
				}

				lineWidth += 16;
			}

			lineWidth += 3;

			longestLineWidth = Math.Max(longestLineWidth, lineWidth);

			if (takeInput)
			{
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
			Item item = items[selectedItem];
			if (infoPanelShowCompareItem)
				MathHelper.Swap(ref item, ref compareItem);
			sidePanelHeight = ItemInfoPanel.Render(item, x + shopWidth + 1, top + headerHeight, sidePanelWidth, Math.Max(shopHeight, sidePanelHeight), compareItem);
		}

		if (takeInput)
		{
			if (InputManager.IsPressed("UIBack", true) || InputManager.IsPressed("UIClose"))
			{
				closed = true;
				longestLineWidth = 60;
				sidePanelHeight = 40;
				Audio.PlayBackground(UISound.uiBack);
			}
		}

		return choice;
	}

	public static int Render(int x, int y, int width, int height, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, bool renderInfoPanel, Item compareItem, bool infoPanelShowCompareItem, out bool secondary, out bool closed, ref int selectedItem)
	{
		return Render(x, y, width, height, title, items, prices, money, renderSlotIcons, renderInfoPanel, compareItem, infoPanelShowCompareItem, true, out secondary, out closed, ref selectedItem);
	}

	public static int Render(Vector2i pos, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, bool renderInfoPanel, Item compareItem, bool infoPanelShowCompareItem, out bool secondary, out bool closed, ref int selectedItem)
	{
		int lineHeight = 16;
		int headerHeight = 12 + 1;
		int sidePanelWidth = 90;
		int shopWidth = Math.Max(60, lineHeight + 5 + longestLineWidth + 1);
		int shopHeight = Math.Min(items.Count, maxItems) * lineHeight;
		int width = shopWidth + (renderInfoPanel ? 1 + sidePanelWidth : 0);
		int height = headerHeight + shopHeight;
		int x = Math.Clamp(pos.x, 2, Renderer.UIWidth - width - 2);
		int y = Math.Clamp(pos.y - height, 2, Renderer.UIHeight - height - 2);

		return Render(x, y, width, height, title, items, prices, money, renderSlotIcons, renderInfoPanel, compareItem, infoPanelShowCompareItem, out secondary, out closed, ref selectedItem);
	}

	public static int Render(int x, int y, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, bool renderInfoPanel, Item compareItem, bool infoPanelShowCompareItem, out bool secondary, out bool closed, ref int selectedItem)
	{
		int lineHeight = 16;
		int headerHeight = 12 + 1;
		int sidePanelWidth = 90;
		int shopWidth = Math.Max(60, lineHeight + 5 + longestLineWidth + 1);
		int shopHeight = Math.Min(items.Count, maxItems) * lineHeight;
		int width = shopWidth + (renderInfoPanel ? 1 + sidePanelWidth : 0);
		int height = headerHeight + shopHeight;
		x = Math.Clamp(x, 2, Renderer.UIWidth - width - 2);
		y = Math.Clamp(y, 2, Renderer.UIHeight - height - 2);

		return Render(x, y, width, height, title, items, prices, money, renderSlotIcons, renderInfoPanel, compareItem, infoPanelShowCompareItem, out secondary, out closed, ref selectedItem);
	}

	public static int Render(Vector2i pos, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, Func<int, int, int, int, int> renderInfoPanel, bool takeInput, out bool secondary, out bool closed, ref int selectedItem)
	{
		int lineHeight = 16;
		int headerHeight = 12 + 1;
		int sidePanelWidth = 90;
		int shopWidth = Math.Max(60, lineHeight + 5 + longestLineWidth + 1);
		int shopHeight = Math.Min(items.Count, maxItems) * lineHeight;
		int width = shopWidth + (renderInfoPanel != null ? 1 + sidePanelWidth : 0);
		int height = headerHeight + shopHeight;
		int x = Math.Clamp(pos.x, 2, Renderer.UIWidth - width - 2);
		int y = Math.Clamp(pos.y - height, 2, Renderer.UIHeight - height - 2);

		int choice = Render(x, y, width, height, title, items, prices, money, renderSlotIcons, false, null, false, takeInput, out secondary, out closed, ref selectedItem);

		// Item info panel
		if (items.Count > 0 && renderInfoPanel != null)
		{
			int xx = x + shopWidth + 1;
			int yy = y + headerHeight;
			int ww = sidePanelWidth;
			int hh = Math.Max(shopHeight, sidePanelHeight);

			Renderer.DrawUISprite(xx - 1, yy - 1, ww + 2, hh + 2, null, false, 0xFFAAAAAA);
			Renderer.DrawUISprite(xx, yy, ww, hh, null, false, 0xFF222222);

			sidePanelHeight = renderInfoPanel(xx, yy, ww, hh);
		}

		return choice;
	}

	public static int Render(Vector2i pos, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, Func<int, int, int, int, int> renderInfoPanel, out bool secondary, out bool closed, ref int selectedItem)
	{
		return Render(pos, title, items, prices, money, renderSlotIcons, renderInfoPanel, true, out secondary, out closed, ref selectedItem);
	}

	public static Item GetCompareItem(Player player, Item item)
	{
		Item compareItem = null;
		if (item.isSecondaryItem && player.handItem == null  /*&& !handItem.twoHanded && offhandItem == null*/)
			compareItem = player.offhandItem;
		else if (item.isHandItem && (item.type == ItemType.Weapon || item.type == ItemType.Staff) /*&& handItem == null && (offhandItem == null || !item.twoHanded)*/)
			compareItem = player.handItem;
		else if (item.isPassiveItem && item.armorSlot != ArmorSlot.None)
		{
			if (player.getArmorItem(item.armorSlot, out int slotIdx))
				compareItem = player.passiveItems[slotIdx];
		}
		return compareItem;
	}
}
