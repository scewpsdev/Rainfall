﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class FullscreenMenu
{
	public static int Render(string[] labels, bool[] enabled, ref int currentButton, float[] fade = null)
	{
		int linePadding = 3;

		if (InputManager.IsPressed("Down", true) || InputManager.IsPressed("UIDown", true))
		{
			do
			{
				currentButton = (currentButton + 1) % labels.Length;
			} while (enabled != null && !enabled[currentButton]);
			Audio.PlayBackground(UISound.uiClick);
		}
		if (InputManager.IsPressed("Up", true) || InputManager.IsPressed("UIUp", true))
		{
			do
			{
				currentButton = (currentButton + labels.Length - 1) % labels.Length;
			} while (enabled != null && !enabled[currentButton]);
			Audio.PlayBackground(UISound.uiClick);
		}

		for (int i = 0; i < labels.Length; i++)
		{
			string txt = labels[i];
			Vector2i size = Renderer.MeasureUITextBMP(txt, txt.Length, 1);
			int x = Renderer.UIWidth / 2 - size.x / 2;
			int y = Renderer.UIHeight / 2 - size.y / 2 + i * (size.y + linePadding);

			if (Renderer.IsHovered(x, y, size.x, size.y) && Input.cursorHasMoved && currentButton != i)
			{
				currentButton = i;
				Audio.PlayBackground(UISound.uiClick);
			}
			bool selected = currentButton == i;

			uint color = 0xFF777777;
			if (enabled != null && !enabled[i])
				color = 0xFF444444;
			else if (selected)
				color = MathHelper.VectorToARGB(Vector4.Lerp(0xFFFFFFFF, 0xFF444444, MathF.Sin(Time.timestamp / 1e9f * 3) * 0.5f + 0.5f));
			uint bgColor = 0xFF444444;

			if (fade != null)
			{
				color = MathHelper.ColorAlpha(color, fade[i]);
				bgColor = MathHelper.ColorAlpha(bgColor, fade[i]);
			}

			if (selected)
			{
				Renderer.DrawUITextBMP(x, y, txt, 1, color);
			}
			else
			{
				Renderer.DrawUITextBMP(x, y, txt, 1, bgColor);
				Renderer.DrawUITextBMP(x, y - 1, txt, 1, color);
			}

			if (selected && (InputManager.IsPressed("UIConfirm", true) || Input.IsMouseButtonPressed(MouseButton.Left, true)))
			{
				Audio.PlayBackground(UISound.uiConfirm);
				return i;
			}
		}

		return -1;
	}
}
