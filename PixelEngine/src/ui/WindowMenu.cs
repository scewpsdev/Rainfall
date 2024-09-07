using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class WindowMenu
{
	public static int Render(string[] options, ref int selectedOption, int x, int y, int width, int lineHeight)
	{
		if (InputManager.IsPressed("Down"))
			selectedOption = (selectedOption + 1) % options.Length;
		if (InputManager.IsPressed("Up"))
			selectedOption = (selectedOption + options.Length - 1) % options.Length;

		for (int i = 0; i < options.Length; i++)
		{
			if (Renderer.IsHovered(x, y, width, lineHeight) && Input.cursorHasMoved)
				selectedOption = i;

			bool selected = selectedOption == i;

			Renderer.DrawUISprite(x, y, width, lineHeight, null, false, selected ? 0xFF333333 : 0xFF222222);
			Renderer.DrawUITextBMP(x + 4, y + 2, options[i], 1, 0xFFAAAAAA);

			if (selected && (InputManager.IsPressed("Interact") || Input.IsMouseButtonPressed(MouseButton.Left)))
			{
				if (InputManager.IsPressed("Interact"))
					InputManager.ConsumeEvent("Interact");
				if (Input.IsMouseButtonPressed(MouseButton.Left))
					Input.ConsumeMouseButtonEvent(MouseButton.Left);

				return i;
			}

			y += lineHeight;
		}

		return -1;
	}
}
