using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


public static class InteractableMenu
{
	public static int Render(Vector2i pos, string title, List<string> options, out bool closed, ref int selectedOption)
	{
		int lineHeight = 12;
		int headerHeight = 12 + 1;
		int width = 120;
		int height = headerHeight + options.Count * lineHeight;
		int x = Math.Min(pos.x, Renderer.UIWidth - width - 2);
		int y = Math.Max(pos.y - height, 2);

		Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, UIColors.WINDOW_FRAME);

		Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, UIColors.WINDOW_BACKGROUND);
		Renderer.DrawUITextBMP(x + 2, y + 2, title, 1, UIColors.TEXT);
		y += headerHeight;

		int option = -1;
		if (InputManager.IsPressed("Down", true) || InputManager.IsPressed("UIDown", true))
		{
			selectedOption = (selectedOption + 1) % options.Count;
			Audio.PlayBackground(UISound.uiClick);
		}
		if (InputManager.IsPressed("Up", true) || InputManager.IsPressed("UIUp", true))
		{
			selectedOption = (selectedOption + options.Count - 1) % options.Count;
			Audio.PlayBackground(UISound.uiClick);
		}

		for (int i = 0; i < options.Count; i++)
		{
			if (Renderer.IsHovered(x, y, width, lineHeight) && Input.cursorHasMoved)
				selectedOption = i;

			bool selected = selectedOption == i;

			Renderer.DrawUISprite(x, y, width, lineHeight, null, false, selected ? UIColors.ITEM_SLOT_BACKGROUND_HIGHLIGHT : UIColors.ITEM_SLOT_BACKGROUND);
			Renderer.DrawUITextBMP(x + 4, y + 2, options[i], 1, UIColors.TEXT);

			if (selected && (InputManager.IsPressed("UIConfirm", true) || Input.IsMouseButtonPressed(MouseButton.Left, true)))
			{
				option = i;
				Audio.PlayBackground(UISound.uiConfirm2);
				break;
			}

			y += lineHeight;
		}

		closed = InputManager.IsPressed("UIBack", true) || InputManager.IsPressed("UIQuit", true);
		if (closed)
			Audio.PlayBackground(UISound.uiBack);
		return option;
	}
}
