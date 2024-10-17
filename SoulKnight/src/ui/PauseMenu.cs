using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


enum PauseMenuState
{
	Selection,
	Options,
}

public static class PauseMenu
{
	static PauseMenuState state = PauseMenuState.Selection;
	static int currentButton = 0;


	public static void OnPause()
	{
		currentButton = 0;
		state = PauseMenuState.Selection;
		Input.cursorMode = CursorMode.Normal;
	}

	public static void OnUnpause()
	{
		if (state == PauseMenuState.Options)
			OptionsMenu.OnClose();
	}

	public static void OnKeyEvent(KeyCode key, KeyModifier modifiers, bool down)
	{
		if (state == PauseMenuState.Options)
			OptionsMenu.OnKeyEvent(key, modifiers, down);
	}

	public static void OnMouseButtonEvent(MouseButton button, bool down)
	{
		if (state == PauseMenuState.Options)
			OptionsMenu.OnMouseButtonEvent(button, down);
	}

	public static void OnGamepadButtonEvent(GamepadButton button, bool down)
	{
		if (state == PauseMenuState.Options)
			OptionsMenu.OnGamepadButtonEvent(button, down);
	}

	public static bool Render(GameState game)
	{
		Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, null, false, 0xAF000000);

		if (state == PauseMenuState.Selection)
		{
			string[] labels = [
				"Resume",
				"Options",
				"Quit"
			];

			bool[] enabled = [
				true,
				true,
				true
			];

			int selection = FullscreenMenu.Render(labels, enabled, ref currentButton);
			if (selection != -1)
			{
				switch (selection)
				{
					case 0: // Resume
						game.isPaused = false;
						OnUnpause();
						break;

					case 1: // Options
						state = PauseMenuState.Options;
						OptionsMenu.OnOpen();
						break;

					case 2: // Quit
						SoulKnight.instance.popState();
						break;

					default:
						Debug.Assert(false);
						break;
				}
			}

			if (InputManager.IsPressed("UIBack", true))
			{
				Audio.PlayBackground(UISound.uiBack);
				return false;
			}
		}
		else if (state == PauseMenuState.Options)
		{
			if (!OptionsMenu.Render())
				state = PauseMenuState.Selection;
		}

		return true;
	}
}
