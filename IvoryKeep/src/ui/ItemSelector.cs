using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class ItemSelector
{
	const int lineHeight = 14;
	const int headerHeight = 12 + 1;
	const int sidePanelWidth = 90;
	const int minShopWidth = 60;

	static int longestLineWidth = 60;
	static int sidePanelHeight = 40;
	static int maxItems = 10;
	static int currentScroll = 0;


	static void GetSize(int numItems, bool renderInfoPanel, out int width, out int height, out int shopWidth, out int shopHeight)
	{
		shopWidth = Math.Max(minShopWidth, lineHeight + 5 + longestLineWidth + 1);
		shopHeight = Math.Max(Math.Min(numItems, maxItems), 1) * lineHeight;
		width = shopWidth + (renderInfoPanel ? 2 + sidePanelWidth : 0);
		height = headerHeight + shopHeight;
	}

	public static void GetSize(int numItems, bool renderInfoPanel, out int width, out int height)
	{
		GetSize(numItems, renderInfoPanel, out width, out height, out _, out _);
	}

	static int RenderTitle(float x, float y, int width, string title, int money)
	{
		Renderer.DrawUISprite(x - 1, y - 1, width + 2, headerHeight + 1, null, false, UIColors.WINDOW_FRAME);
		Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, UIColors.WINDOW_BACKGROUND);
		Renderer.DrawUITextBMP(x + 2, y + 2, title, 1, UIColors.TEXT);
		//if (money != -1)
		{
			Renderer.DrawUISprite(x + width - 1 - HUD.gold.width, y + 2, HUD.gold.width, HUD.gold.height, HUD.gold);
			string moneyStr = Math.Abs(money).ToString();
			Renderer.DrawUITextBMP(x + width - 1 - HUD.gold.width - Renderer.MeasureUITextBMP(moneyStr, moneyStr.Length, 1).x - 2, y + 2, moneyStr, 1, UIColors.TEXT);
		}
		return headerHeight;
	}

	public static int Render(float x, float y, int width, int ____height, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, bool renderInfoPanel, Item compareItem, bool infoPanelShowCompareItem, bool takeInput, out bool secondary, out bool closed, ref int selectedItem, Func<float, float, int, int> renderTabIcons = null)
	{
		secondary = false;
		closed = false;

		int shopWidth = Math.Min(Math.Max(60, 1 + lineHeight + 5 + longestLineWidth + 1), width);
		int shopHeight = Math.Max(Math.Min(items.Count, maxItems), 1) * lineHeight;

		float top = y;

		y += RenderTitle(x, y, width, title, money);

		if (renderTabIcons != null)
			y += renderTabIcons(x, y, width) + 1;

		Renderer.DrawUISprite(x - 1, y - 1, width + 2, shopHeight + 2, null, false, UIColors.WINDOW_FRAME);
		Renderer.DrawUISprite(x, y, width, shopHeight, null, false, UIColors.WINDOW_BACKGROUND);

		if (takeInput)
		{
			if ((InputManager.IsPressed("Down", true) || InputManager.IsPressed("UIDown", true) || Input.IsKeyPressed(KeyCode.K)) && items.Count > 0)
			{
				selectedItem = (selectedItem + 1) % items.Count;
				Audio.PlayBackground(UISound.uiClick);
			}
			if ((InputManager.IsPressed("Up", true) || InputManager.IsPressed("UIUp", true) || Input.IsKeyPressed(KeyCode.I)) && items.Count > 0)
			{
				selectedItem = (selectedItem + items.Count - 1) % items.Count;
				Audio.PlayBackground(UISound.uiClick);
			}
		}
		if (items.Count > 0)
			selectedItem = Math.Clamp(selectedItem, 0, items.Count - 1);

		if (selectedItem >= currentScroll + maxItems)
			currentScroll = selectedItem - maxItems + 1;
		else if (selectedItem >= 0 && selectedItem < currentScroll)
			currentScroll = selectedItem;

		if (Input.scrollMove != 0 && items.Count > maxItems)
		{
			currentScroll = Math.Clamp(currentScroll - Input.scrollMove, 0, items.Count - maxItems);
			selectedItem = Math.Clamp(selectedItem, currentScroll, currentScroll + maxItems - 1);
		}

		if (items.Count == 0)
		{
			string txt = "No items";
			Renderer.DrawUISprite(x, y, width, lineHeight, null, false, UIColors.ITEM_SLOT_BACKGROUND);
			Renderer.DrawUITextBMP(x + width / 2 - Renderer.MeasureUITextBMP(txt).x / 2, y + 4, txt, 1, UIColors.TEXT);
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
			Renderer.DrawUISprite(x, y, lineHeight, lineHeight, item.icon, false, Mathf.VectorToARGB(item.spriteColor));
			string name = item.fullDisplayName;
			Renderer.DrawUITextBMP(x + lineHeight + 5, y + 4, name, 1, UIColors.TEXT);

			int lineWidth = Renderer.MeasureUITextBMP(name).x + 5;

			if (prices != null)
			{
				int price = prices[i];
				string quantity = price.ToString();
				bool canAfford = money >= price;
				uint color = money < 0 ? UIColors.TEXT_MONEY : canAfford ? UIColors.TEXT : UIColors.TEXT_DOWNGRADE;
				Renderer.DrawUITextBMP(x + shopWidth - (renderSlotIcons != null ? 1 + 16 : 4) - Renderer.MeasureUITextBMP(quantity, quantity.Length, 1).x, y + 4, quantity, 1, color);

				lineWidth += Renderer.MeasureUITextBMP(quantity).x;
			}

			if (renderSlotIcons != null)
			{
				Player player = renderSlotIcons;
				if (player.handItem == item)
					Renderer.DrawUISprite(x + shopWidth - 3 - 16, y - (16 - lineHeight) / 2, 16, 16, InventoryUI.weaponSprite);
				else if (player.offhandItem == item)
					Renderer.DrawUISprite(x + shopWidth - 3 - 16, y - (16 - lineHeight) / 2, 16, 16, InventoryUI.shieldSprite);
				else if (player.isActiveItem(item, out int activeSlot))
				{
					Renderer.DrawUISprite(x + shopWidth - 3 - 16, y - (16 - lineHeight) / 2, 16, 16, InventoryUI.bagSprite);
					Renderer.DrawUITextBMP(x + shopWidth - 3 - 4, y + 16 - 8, (activeSlot + 1).ToString(), 1, 0xFF505050);
				}
				else if (player.isPassiveItem(item, out int passiveSlot))
				{
					Renderer.DrawUISprite(x + shopWidth - 3 - 16, y - (16 - lineHeight) / 2, 16, 16, item.type == ItemType.Relic ? InventoryUI.ringSprite : InventoryUI.helmetSprite);
					if (passiveSlot != -1)
						Renderer.DrawUITextBMP(x + shopWidth - 3 - 4, y + 16 - 8, (passiveSlot + 1 - (item.type == ItemType.Relic ? player.passiveItems.Count - 2 : 0)).ToString(), 1, 0xFF505050);
				}

				lineWidth += 16;
			}

			lineWidth += 3;

			longestLineWidth = Math.Max(longestLineWidth, lineWidth);

			if (takeInput)
			{
				if (selected && (InputManager.IsPressed("UIConfirm", true) || Input.IsMouseButtonPressed(MouseButton.Left, true) && Renderer.IsHovered(x, y, shopWidth, lineHeight)))
				{
					choice = i;
					Audio.PlayBackground(UISound.uiConfirm2);
				}
				if (selected && (InputManager.IsPressed("UIConfirm2", true) || Input.IsMouseButtonPressed(MouseButton.Right, true) && Renderer.IsHovered(x, y, shopWidth, lineHeight)))
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
				Mathf.Swap(ref item, ref compareItem);
			sidePanelHeight = (int)ItemInfoPanel.Render(item, x + shopWidth + 1, top + headerHeight, sidePanelWidth, Math.Max(shopHeight, sidePanelHeight), compareItem);
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

	public static int Render(float x, float y, int width, int height, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, bool renderInfoPanel, Item compareItem, bool infoPanelShowCompareItem, out bool secondary, out bool closed, ref int selectedItem, Func<float, float, int, int> renderTabIcons = null)
	{
		return Render(x, y, width, height, title, items, prices, money, renderSlotIcons, renderInfoPanel, compareItem, infoPanelShowCompareItem, true, out secondary, out closed, ref selectedItem, renderTabIcons);
	}

	public static int Render(Vector2 pos, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, bool renderInfoPanel, Item compareItem, bool infoPanelShowCompareItem, out bool secondary, out bool closed, ref int selectedItem)
	{
		GetSize(items.Count, renderInfoPanel, out int width, out int height);
		float x = Math.Clamp(pos.x, 2, Renderer.UIWidth - width - 2);
		float y = Math.Clamp(pos.y - height, 2, Renderer.UIHeight - height - 2);

		return Render(x, y, width, height, title, items, prices, money, renderSlotIcons, renderInfoPanel, compareItem, infoPanelShowCompareItem, out secondary, out closed, ref selectedItem);
	}

	public static int Render(int x, int y, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, bool renderInfoPanel, Item compareItem, bool infoPanelShowCompareItem, out bool secondary, out bool closed, ref int selectedItem, Func<float, float, int, int> renderTabIcons = null)
	{
		GetSize(items.Count, renderInfoPanel, out int width, out int height);
		x = Math.Clamp(x, 2, Renderer.UIWidth - width - 2);
		y = Math.Clamp(y, 2, Renderer.UIHeight - height - 2);

		return Render(x, y, width, height, title, items, prices, money, renderSlotIcons, renderInfoPanel, compareItem, infoPanelShowCompareItem, out secondary, out closed, ref selectedItem, renderTabIcons);
	}

	public static int Render(Vector2 pos, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, Func<float, float, int, int, int> renderInfoPanel, bool takeInput, out bool secondary, out bool closed, ref int selectedItem)
	{
		GetSize(items.Count, renderInfoPanel != null, out int width, out int height, out int shopWidth, out int shopHeight);
		float x = Math.Clamp(pos.x, 2, Renderer.UIWidth - width - 2);
		float y = Math.Clamp(pos.y - height, 2, Renderer.UIHeight - height - 2);

		int choice = Render(x, y, width, height, title, items, prices, money, renderSlotIcons, false, null, false, takeInput, out secondary, out closed, ref selectedItem);

		// Item info panel
		if (items.Count > 0 && renderInfoPanel != null)
		{
			float xx = x + shopWidth + 1;
			float yy = y + headerHeight;
			int ww = sidePanelWidth;
			int hh = Math.Max(shopHeight, sidePanelHeight);

			Renderer.DrawUISprite(xx - 1, yy - 1, ww + 2, hh + 2, null, false, 0xFFAAAAAA);
			Renderer.DrawUISprite(xx, yy, ww, hh, null, false, 0xFF222222);

			sidePanelHeight = renderInfoPanel(xx, yy, ww, hh);
		}

		return choice;
	}

	public static int Render(Vector2 pos, string title, List<Item> items, List<int> prices, int money, Player renderSlotIcons, Func<float, float, int, int, int> renderInfoPanel, out bool secondary, out bool closed, ref int selectedItem)
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
