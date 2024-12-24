using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public static class NPCSelector
{
	static int longestLineWidth = 60;
	static int sidePanelHeight = 40;
	static int maxItems = 10;
	static int currentScroll = 0;

	public static int Render(float x, float y, int width, int height, string title, List<string> items, Func<float, float, int, int, int> renderInfoPanel, out bool secondary, out bool closed, ref int selectedItem)
	{
		secondary = false;

		int lineHeight = 16;
		int headerHeight = 12 + 1;
		int sidePanelWidth = 80;
		int shopWidth = Math.Max(60, 4 + longestLineWidth + 1);
		int shopHeight = Math.Min(items.Count, maxItems) * lineHeight;

		float top = y;

		Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);

		Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, 0xFF222222);
		Renderer.DrawUITextBMP(x + 2, y + 2, title, 1, 0xFFAAAAAA);
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

			string name = items[i];

			Renderer.DrawUISprite(x, y, shopWidth, lineHeight, null, false, selected ? 0xFF333333 : 0xFF222222);
			//Renderer.DrawUISprite(x + 1, y, lineHeight, lineHeight, item.getIcon(), false, MathHelper.VectorToARGB(item.spriteColor));
			Renderer.DrawUITextBMP(x + 4, y + 4, name, 1, 0xFFAAAAAA);

			int lineWidth = Renderer.MeasureUITextBMP(name).x + 5;

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
		if (items.Count > 0 && renderInfoPanel != null)
		{
			float xx = x + shopWidth + 1;
			float yy = top + headerHeight;
			int ww = sidePanelWidth;
			int hh = Math.Max(shopHeight, sidePanelHeight);

			Renderer.DrawUISprite(xx - 1, yy - 1, ww + 2, hh + 2, null, false, 0xFFAAAAAA);
			Renderer.DrawUISprite(xx, yy, ww, hh, null, false, 0xFF222222);

			sidePanelHeight = renderInfoPanel(xx, yy, ww, hh);
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

	public static int Render(Vector2 pos, string title, List<string> items, Func<float, float, int, int, int> renderInfoPanel, out bool secondary, out bool closed, ref int selectedItem)
	{
		int lineHeight = 16;
		int headerHeight = 12 + 1;
		int sidePanelWidth = 80;
		int shopWidth = Math.Max(60, 4 + longestLineWidth + 1);
		int shopHeight = Math.Min(items.Count, maxItems) * lineHeight;
		int width = shopWidth + (renderInfoPanel != null ? 1 + sidePanelWidth : 0);
		int height = headerHeight + shopHeight;
		float x = Math.Clamp(pos.x, 2, Renderer.UIWidth - width - 2);
		float y = Math.Clamp(pos.y - height, 2, Renderer.UIHeight - height - 2);

		return Render(x, y, width, height, title, items, renderInfoPanel, out secondary, out closed, ref selectedItem);
	}
}
