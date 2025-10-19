using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


public static class ItemSlotUI
{
	public static void Render(float x, float y, int size, Sprite icon, uint iconColor, int stackSize, Sprite background, Vector2i cellPosition, ref Vector2i selectedCell)
	{
		bool selected = cellPosition == selectedCell;

		Renderer.DrawUISprite(x - 1, y - 1, size + 2, size + 2, null, false, selected ? UIColors.ITEM_SLOT_FRAME_HIGHLIGHT : UIColors.ITEM_SLOT_FRAME);
		Renderer.DrawUISprite(x, y, size, size, null, false, selected ? UIColors.ITEM_SLOT_BACKGROUND_HIGHLIGHT : UIColors.ITEM_SLOT_BACKGROUND);
		if (icon != null)
		{
			Renderer.DrawUIOutline(x, y, size, size, icon, false, 0xFF000000);
			Renderer.DrawUISprite(x, y, size, size, icon, false, iconColor);

			if (stackSize > 1)
				Renderer.DrawUITextBMP(x + size - size / 4, y + size - Renderer.smallFont.size + 2, stackSize.ToString(), 1, 0xFFBBBBBB);
		}
		else if (background != null)
		{
			Renderer.DrawUISprite(x, y, size, size, background, false, 0x7FFFFFFF);
		}

		if (Input.cursorHasMoved && Renderer.IsHovered(x, y, size, size) && selectedCell != cellPosition)
		{
			selectedCell = cellPosition;
			Audio.PlayBackgroundClocked(UISound.uiClick);
		}
	}
}
