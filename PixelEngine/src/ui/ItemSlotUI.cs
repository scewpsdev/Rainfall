using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class ItemSlotUI
{
	public static bool Render(int x, int y, int size, Item item, Sprite background = null, bool selected = false)
	{
		Renderer.DrawUISprite(x - 1, y - 1, size + 2, size + 2, null, false, selected ? UIColors.ITEM_SLOT_FRAME_HIGHLIGHT : UIColors.ITEM_SLOT_FRAME);
		Renderer.DrawUISprite(x, y, size, size, null, false, selected ? UIColors.ITEM_SLOT_BACKGROUND_HIGHLIGHT : UIColors.ITEM_SLOT_BACKGROUND);
		if (item != null)
		{
			Renderer.DrawUIOutline(x, y, size, size, item.getIcon(), false, 0xFF000000);
			Renderer.DrawUISprite(x, y, size, size, item.getIcon(), false, MathHelper.VectorToARGB(item.spriteColor));
		}
		else if (background != null)
		{
			Renderer.DrawUISprite(x, y, size, size, background, false, 0x7FFFFFFF);
		}

		bool newSelected = selected;
		if (Input.cursorHasMoved && Renderer.IsHovered(x, y, size, size))
			newSelected = true;
		return newSelected;
	}
}
