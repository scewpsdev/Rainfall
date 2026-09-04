using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ShopMenu
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
		shopHeight = Math.Min(numItems, maxItems) * lineHeight;
		width = shopWidth + (renderInfoPanel ? 1 + sidePanelWidth : 0);
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

	public static int Render(float x, float y, int width, int ____height, string title, List<string> items, List<Sprite> icons, List<int> prices, int money, bool renderInfoPanel, Item infoPanelItem, Item compareItem, out bool secondary, out bool closed, ref int selectedItem)
	{
		secondary = false;
		closed = false;

		int shopWidth = Math.Min(Math.Max(60, 1 + lineHeight + 5 + longestLineWidth + 1), width);
		int shopHeight = Math.Min(items.Count, maxItems) * lineHeight;

		float top = y;

		y += RenderTitle(x, y, width, title, money);

		Renderer.DrawUISprite(x - 1, y - 1, width + 2, shopHeight + 2, null, false, UIColors.WINDOW_FRAME);
		Renderer.DrawUISprite(x, y, width, shopHeight, null, false, UIColors.WINDOW_BACKGROUND);

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
			if (Renderer.IsHovered(x, y, shopWidth, lineHeight) && Input.cursorHasMoved && selectedItem != i)
			{
				selectedItem = i;
				Audio.PlayBackground(UISound.uiClick);
			}
			bool selected = selectedItem == i;

			string name = items[i];
			Sprite icon = icons != null ? icons[i] : null;

			Renderer.DrawUISprite(x, y, shopWidth, lineHeight, null, false, selected ? UIColors.ITEM_SLOT_BACKGROUND_HIGHLIGHT : UIColors.ITEM_SLOT_BACKGROUND);
			if (icon != null)
			{
				Renderer.DrawUISprite(x, y, lineHeight, lineHeight, icon, false);
				Renderer.DrawUITextBMP(x + lineHeight + 5, y + 4, name, 1, UIColors.TEXT);
			}
			else
			{
				Renderer.DrawUITextBMP(x + 5, y + 4, name, 1, UIColors.TEXT);
			}

			int lineWidth = Renderer.MeasureUITextBMP(name).x + 5;

			if (prices != null)
			{
				int price = prices[i];
				string quantity = price.ToString();
				bool canAfford = money >= price;
				uint color = money < 0 ? UIColors.TEXT_MONEY : canAfford ? UIColors.TEXT : UIColors.TEXT_DOWNGRADE;
				Renderer.DrawUITextBMP(x + shopWidth - 4 - Renderer.MeasureUITextBMP(quantity, quantity.Length, 1).x, y + 4, quantity, 1, color);

				lineWidth += Renderer.MeasureUITextBMP(quantity).x;
			}

			lineWidth += 3;

			longestLineWidth = Math.Max(longestLineWidth, lineWidth);

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
			Item item = infoPanelItem;
			sidePanelHeight = (int)ItemInfoPanel.Render(item, x + shopWidth + 1, top + headerHeight, sidePanelWidth, Math.Max(shopHeight, sidePanelHeight), compareItem);
		}

		if (InputManager.IsPressed("UIBack", true) || InputManager.IsPressed("UIClose"))
		{
			closed = true;
			longestLineWidth = 60;
			sidePanelHeight = 40;
			Audio.PlayBackground(UISound.uiBack);
		}

		return choice;
	}

	public static int Render(Vector2 pos, string title, List<string> items, List<Sprite> icons, List<int> prices, int money, bool renderInfoPanel, Item infoPanelItem, Item compareItem, out bool secondary, out bool closed, ref int selectedItem)
	{
		GetSize(items.Count, renderInfoPanel, out int width, out int height, out int shopWidth, out int shopHeight);
		float x = Math.Clamp(pos.x, 2, Renderer.UIWidth - width - 2);
		float y = Math.Clamp(pos.y - height, 2, Renderer.UIHeight - height - 2);

		return Render(x, y, width, height, title, items, icons, prices, money, renderInfoPanel, infoPanelItem, compareItem, out secondary, out closed, ref selectedItem);
	}
}
