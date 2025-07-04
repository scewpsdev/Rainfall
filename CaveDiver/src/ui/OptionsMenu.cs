﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Option
{
	public string name;
	public Action<string> callback;

	public string[] items;
	public int selectedItem;

	public InputBinding input;
}

class InputOption : Option
{
	public InputOption(string name, string displayName)
	{
		this.name = displayName;
		input = InputManager.GetBinding(name);
	}
}

public static class OptionsMenu
{
	static string[] tabs = { "General", "Graphics", "Controls" };
	static int selectedTab = 0;
	static int selectedOption = -1;
	static int currentScroll = 0;

	static Option awaitingInputOption;

	static Option[] generalOptions;
	static Option[] graphicsOptions;
	static Option[] controlOptions;

	public static void Init()
	{
		generalOptions = [
			new Option {name = "Aim Mode", items = ["Simple", "Directional", "Crosshair"], selectedItem = (int)Settings.game.aimMode, callback = (string str) => { Settings.game.aimMode = Utils.ParseEnum<AimMode>(str); } },
		];
		graphicsOptions = [
			new Option {name = "Bloom", items = ["On", "Off"], selectedItem = Renderer.bloomEnabled ? 0 : 1, callback = (string str) => { Renderer.bloomEnabled = str == "On"; } },
		];
		controlOptions = [
			new InputOption("Left", "Move Left"),
			new InputOption("Right", "Move Right"),
			new InputOption("Up", "Move Up"),
			new InputOption("Down", "Move Down"),
			new InputOption("Interact", "Interact"),
			new InputOption("Jump", "Jump"),
			new InputOption("Attack", "Attack"),
			new InputOption("Attack2", "Attack Secondary"),
			new InputOption("UseItem", "Use Quick Item"),
			new InputOption("SwitchItem", "Switch Quick Item"),
			new InputOption("SwitchSpell", "Switch Spell Item"),
			new InputOption("Inventory", "Open Inventory"),
			new InputOption("UIConfirm", "Menu Confirm"),
			new InputOption("UIConfirm2", "Menu Confirm Secondary"),
			new InputOption("UIBack", "Menu Back"),
			new InputOption("UIQuit", "Menu Close"),
			new InputOption("UILeft", "Menu Left"),
			new InputOption("UIRight", "Menu Right"),
			new InputOption("UIUp", "Menu Up"),
			new InputOption("UIDown", "Menu Down"),
		];
	}

	public static void OnOpen()
	{
		selectedTab = 0;
		selectedOption = 0;
		currentScroll = 0;
	}

	public static void OnClose()
	{
		awaitingInputOption = null;
		Settings.Save();
		InputManager.SaveBindings();
	}

	static void DrawLeft(int x, int y, string txt, bool selected)
	{
		uint color = selected ? 0xFFFFFFFF : 0xFF7F7F7F;
		Renderer.DrawUITextBMP(x, y, txt, 1, color);
	}

	static void DrawSelection(int x, int y, string[] items, int selectedItem, bool selected)
	{
		string item = items[selectedItem];

		if (selected)
			item = "< " + item;

		int width = 320;
		x += width - Renderer.MeasureUITextBMP(item).x;

		if (selected)
			item = item + " >";

		uint color = selected ? 0xFFFFFFFF : 0xFF7F7F7F;
		Renderer.DrawUITextBMP(x, y, item, 1, color);
	}

	static void DrawInput(int x, int y, InputBinding binding, bool awaitingInput, bool selected)
	{
		string item = awaitingInput ? "..." : binding.ToString();

		if (selected)
			item = "< " + item;

		int width = 320;
		x += width - Renderer.MeasureUITextBMP(item).x;

		if (selected)
			item = item + " >";

		uint color = selected ? 0xFFFFFFFF : 0xFF7F7F7F;
		Renderer.DrawUITextBMP(x, y, item, 1, color);
	}

	static void UpdateOptions(Option[] options)
	{
		if (InputManager.IsPressed("Down", true) || InputManager.IsPressed("UIDown", true))
		{
			selectedOption = (selectedOption + 1) % options.Length;
			Audio.PlayBackground(UISound.uiClick);
		}
		if (InputManager.IsPressed("Up", true) || InputManager.IsPressed("UIUp", true))
		{
			selectedOption = (selectedOption + options.Length - 1) % options.Length;
			Audio.PlayBackground(UISound.uiClick);
		}

		const int maxOptions = 14;
		if (selectedOption >= currentScroll + maxOptions)
			currentScroll = selectedOption - maxOptions + 1;
		else if (selectedOption >= 0 && selectedOption < currentScroll)
			currentScroll = selectedOption;

		int y = 48;
		for (int i = currentScroll; i < Math.Min(options.Length, currentScroll + maxOptions); i++)
		{
			if (i == currentScroll && i > 0)
			{
				Renderer.DrawUITextBMP(Renderer.UIWidth / 2 + 160 - 10, y - Renderer.smallFont.size - 4, '^', false, false, 1, 0xFF7F7F7F);
			}

			Option option = options[i];
			bool selected = selectedOption == i;
			DrawLeft(Renderer.UIWidth / 2 - 160, y, option.name, selected);

			if (option.items != null)
			{
				if (selected && (InputManager.IsPressed("Right", true) || InputManager.IsPressed("UIRight", true)))
				{
					option.selectedItem = (option.selectedItem + 1) % option.items.Length;
					option.callback(option.items[option.selectedItem]);
					Audio.PlayBackground(UISound.uiSwitch);
				}
				if (selected && (InputManager.IsPressed("Left", true) || InputManager.IsPressed("UILeft", true)))
				{
					option.selectedItem = (option.selectedItem + option.items.Length - 1) % option.items.Length;
					option.callback(option.items[option.selectedItem]);
					Audio.PlayBackground(UISound.uiSwitch);
				}

				DrawSelection(Renderer.UIWidth / 2 - 160, y, option.items, option.selectedItem, selected);
			}
			else if (option.input != null)
			{
				if (selected && awaitingInputOption == null && InputManager.IsPressed("UIConfirm", true))
				{
					awaitingInputOption = option;
					Audio.PlayBackground(UISound.uiSwitch);
				}

				DrawInput(Renderer.UIWidth / 2 - 160, y, option.input, awaitingInputOption == option, selected);
			}

			y += Renderer.smallFont.size + 4;

			if (i == Math.Min(options.Length, currentScroll + maxOptions) - 1 && i < options.Length - 1)
			{
				Renderer.DrawUITextBMP(Renderer.UIWidth / 2 + 160 - 10, y, '^', false, true, 1, 0xFF7F7F7F);
			}
		}
	}

	public static void OnKeyEvent(KeyCode key, KeyModifier modifiers, bool down)
	{
		if (down && modifiers == KeyModifier.None && awaitingInputOption != null)
		{
			if (key == KeyCode.Esc)
			{
				awaitingInputOption = null;
				Input.ConsumeKeyEvent(key);
			}
			else
			{
				Input.ConsumeKeyEvent(key);
				if (awaitingInputOption.input.keys == null)
					awaitingInputOption.input.keys = new KeyCode[1];
				awaitingInputOption.input.keys[0] = key;
				awaitingInputOption = null;
			}
		}
	}

	public static void OnMouseButtonEvent(MouseButton button, bool down)
	{
		if (down && awaitingInputOption != null)
		{
			Input.ConsumeMouseButtonEvent(button);
			awaitingInputOption.input.button = button;
			awaitingInputOption = null;
		}
	}

	public static void OnGamepadButtonEvent(GamepadButton button, bool down)
	{
		if (down && awaitingInputOption != null)
		{
			Input.ConsumeGamepadButtonEvent(button);
			awaitingInputOption.input.gamepadButton = button;
			awaitingInputOption = null;
		}
	}

	static void General()
	{
		UpdateOptions(generalOptions);
	}

	static void Graphics()
	{
		UpdateOptions(graphicsOptions);
	}

	static void Controls()
	{
		UpdateOptions(controlOptions);
	}

	public static bool Render()
	{
		if (selectedOption == -1)
		{
			int choice = FullscreenMenu.Render(tabs, null, ref selectedTab);

			if (choice != -1)
			{
				selectedOption = 0;
				Audio.PlayBackground(UISound.uiClick);
			}

			if (InputManager.IsPressed("UIUp", true))
			{
				selectedTab = (selectedTab + tabs.Length - 1) % tabs.Length;
				currentScroll = 0;
				Audio.PlayBackground(UISound.uiClick);
			}
			if (InputManager.IsPressed("UIDown", true))
			{
				selectedTab = (selectedTab + 1) % tabs.Length;
				currentScroll = 0;
				Audio.PlayBackground(UISound.uiClick);
			}
			if (InputManager.IsPressed("UIBack", true))
			{
				OnClose();
				Audio.PlayBackground(UISound.uiBack);
				return false;
			}
		}
		else
		{
			//if (selectedTab == 0) // General
			//	General();
			//else if (selectedTab == 1) // Graphics
			//	Graphics();
			//else if (selectedTab == 2) // Controls
			Controls();

			if (InputManager.IsPressed("UIBack", true))
			{
				//selectedOption = -1;
				Audio.PlayBackground(UISound.uiBack);
			}
		}

		if (awaitingInputOption == null && InputManager.IsPressed("UIQuit", true))
		{
			OnClose();
			Audio.PlayBackground(UISound.uiBack);
			return false;
		}

		return true;
	}
}
