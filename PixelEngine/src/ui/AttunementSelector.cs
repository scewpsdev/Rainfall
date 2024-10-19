using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class AttunementSelector
{
	public static int Render(int x, int y, int width, int height, Staff staff, out bool secondary, out bool closed, ref int selectedItem)
	{
		secondary = false;
		closed = false;

		int choice = -1;

		int padding = 4;
		int slotSize = 16;

		Renderer.DrawUISprite(x, y, width, height, null, false, UIColors.WINDOW_BACKGROUND);

		int columns = (width - 2 * padding + 2) / (slotSize + 2);

		if (InputManager.IsPressed("UIRight", true))
		{
			selectedItem++;
			Audio.PlayBackground(UISound.uiClick);
		}
		if (InputManager.IsPressed("UILeft", true))
		{
			selectedItem--;
			Audio.PlayBackground(UISound.uiClick);
		}
		if (InputManager.IsPressed("UIDown", true))
		{
			selectedItem += columns;
			Audio.PlayBackground(UISound.uiClick);
		}
		if (InputManager.IsPressed("UIUp", true))
		{
			selectedItem -= columns;
			Audio.PlayBackground(UISound.uiClick);
		}

		selectedItem = MathHelper.Clamp(selectedItem, 0, staff.staffAttunementSlots - 1);

		for (int i = 0; i < staff.staffAttunementSlots; i++)
		{
			int xx = x + padding + i % columns;
			int yy = y + padding + i / columns;

			bool selected = selectedItem == i;
			if (ItemSlotUI.Render(xx, yy, slotSize, staff.attunedSpells[i], null, selected))
				selectedItem = i;

			if (selected && (InputManager.IsPressed("UIConfirm", true) || Input.IsMouseButtonPressed(MouseButton.Left, true)))
			{
				choice = i;
				Audio.PlayBackground(UISound.uiConfirm2);
			}
			else if (selected && (InputManager.IsPressed("UIConfirm2", true) || Input.IsMouseButtonPressed(MouseButton.Right, true)))
			{
				choice = i;
				secondary = true;
				Audio.PlayBackground(UISound.uiConfirm2);
			}
		}

		if (InputManager.IsPressed("UIBack", true) || InputManager.IsPressed("UIQuit", true))
		{
			closed = true;
			Audio.PlayBackground(UISound.uiBack);
		}

		return choice;
	}
}
