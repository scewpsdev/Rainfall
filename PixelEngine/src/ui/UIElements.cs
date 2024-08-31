using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class UIElements
{
	public static int WindowMenu(string[] options, ref int selectedOption, int x, int y, int width, int lineHeight)
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

	public static int FullscreenMenu(string[] labels, bool[] enabled, ref int currentButton)
	{
		int linePadding = 3;

		if (InputManager.IsPressed("Down"))
		{
			do
			{
				currentButton = (currentButton + 1) % labels.Length;
			} while (!enabled[currentButton]);
		}
		if (InputManager.IsPressed("Up"))
		{
			do
			{
				currentButton = (currentButton + labels.Length - 1) % labels.Length;
			} while (!enabled[currentButton]);
		}

		for (int i = 0; i < labels.Length; i++)
		{
			string txt = labels[i];
			Vector2i size = Renderer.MeasureUITextBMP(txt, txt.Length, 1);
			int x = Renderer.UIWidth / 2 - size.x / 2;
			int y = Renderer.UIHeight / 2 - size.y / 2 + i * (size.y + linePadding);

			if (Renderer.IsHovered(x, y, size.x, size.y) && Input.cursorHasMoved)
				currentButton = i;
			bool selected = currentButton == i;

			uint color = enabled[i] ? (selected ? 0xFFFFFFFF : 0xFF666666) : 0xFF333333;
			Renderer.DrawUITextBMP(x, y, txt, 1, color);

			if (selected && (InputManager.IsPressed("Interact") || Input.IsMouseButtonPressed(MouseButton.Left)))
			{
				if (InputManager.IsPressed("Interact"))
					InputManager.ConsumeEvent("Interact");
				if (Input.IsMouseButtonPressed(MouseButton.Left))
					Input.ConsumeMouseButtonEvent(MouseButton.Left);

				return i;
			}
		}

		return -1;
	}
}
