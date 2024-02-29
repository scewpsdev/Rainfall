using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class InputBinding
{
	public KeyCode key = KeyCode.None;
	public MouseButton button = MouseButton.None;
	public int scrollDelta = 0;


	public bool isDown()
	{
		return key != KeyCode.None && Input.IsKeyDown(key)
			|| button != MouseButton.None && Input.IsMouseButtonDown(button);
	}

	public bool isPressed()
	{
		return key != KeyCode.None && Input.IsKeyPressed(key)
			|| button != MouseButton.None && Input.IsMouseButtonPressed(button)
			|| scrollDelta != 0 && scrollDelta == Math.Sign(Input.scrollMove);
	}

	public bool isReleased()
	{
		return key != KeyCode.None && Input.IsKeyReleased(key)
			|| button != MouseButton.None && Input.IsMouseButtonReleased(button)
			|| scrollDelta != 0 && scrollDelta == Math.Sign(Input.scrollMove);
	}
}

public static class InputManager
{
	const string INPUT_BINDINGS_FILE = "InputBindings.config";

	const float MOUSE_SENSITIVITY = 0.0015f;
	const float TURN_SPEED = 2.5f;

	static Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();
	static Dictionary<string, MouseButton> buttons = new Dictionary<string, MouseButton>();

	static Dictionary<string, InputBinding> bindings = new Dictionary<string, InputBinding>();


	public static void Init()
	{
		foreach (KeyCode key in Enum.GetValues<KeyCode>())
		{
			keys.Add(key.ToString(), key);
		}
		foreach (MouseButton button in Enum.GetValues<MouseButton>())
		{
			buttons.Add(button.ToString(), button);
		}

		if (File.Exists(INPUT_BINDINGS_FILE))
		{
			DatFile bindingsFile = new DatFile(File.ReadAllText(INPUT_BINDINGS_FILE), INPUT_BINDINGS_FILE);
			foreach (DatField binding in bindingsFile.root.fields)
			{
				MouseButton button = MouseButton.None;
				KeyCode key = KeyCode.None;
				int scrollDelta = 0;

				if (binding.value.type == DatValueType.Identifier)
				{
					string valueStr = binding.value.identifier;
					TryGetButton(valueStr, ref button);
					TryGetKey(valueStr, ref key);
					TryGetScroll(valueStr, ref scrollDelta);
				}
				else if (binding.value.type == DatValueType.Array)
				{
					foreach (DatValue value in binding.value.array.values)
					{
						string valueStr = value.identifier;
						TryGetButton(valueStr, ref button);
						TryGetKey(valueStr, ref key);
						TryGetScroll(valueStr, ref scrollDelta);
					}
				}

				if (button != MouseButton.None || key != KeyCode.None || scrollDelta != 0)
				{
					bindings.Add(binding.name, new InputBinding() { button = button, key = key, scrollDelta = scrollDelta });
					Console.WriteLine("Registered binding " + binding.name);
				}
			}
		}
	}

	static void TryGetKey(string value, ref KeyCode key)
	{
		if (keys.ContainsKey(value))
			key = keys[value];
	}

	static void TryGetButton(string value, ref MouseButton button)
	{
		if (value.Length == 2 && value[0] == 'M' && value[1] >= '0' && value[1] <= '9')
		{
			int buttonID = value[1] - '0' - 1;
			button = MouseButton.Left + buttonID;
		}
	}

	static void TryGetScroll(string value, ref int scroll)
	{
		if (value == "ScrollUp")
			scroll = 1;
		if (value == "ScrollDown")
			scroll = -1;
	}

	static void AddBinding(string name, KeyCode key)
	{
		bindings.Add(name, new InputBinding() { key = key });
	}

	static void AddBinding(string name, MouseButton button, KeyCode key = KeyCode.None, int scrollDelta = 0)
	{
		bindings.Add(name, new InputBinding() { button = button, key = key, scrollDelta = scrollDelta });
	}

	public static bool IsDown(string name)
	{
		if (bindings.ContainsKey(name))
			return bindings[name].isDown();
		return false;
	}

	public static bool IsPressed(string name)
	{
		if (bindings.ContainsKey(name))
			return bindings[name].isPressed();
		return false;
	}

	public static bool IsReleased(string name)
	{
		if (bindings.ContainsKey(name))
			return bindings[name].isReleased();
		return false;
	}

	public static Vector2 moveVector
	{
		get
		{
			float x = 0
				+ (Input.IsKeyDown(KeyCode.KeyD) ? 1.0f : 0.0f)
				+ (Input.IsKeyDown(KeyCode.KeyA) ? -1.0f : 0.0f)
				;
			float y = 0
				+ (Input.IsKeyDown(KeyCode.KeyW) ? 1.0f : 0.0f)
				+ (Input.IsKeyDown(KeyCode.KeyS) ? -1.0f : 0.0f)
				;
			return new Vector2(x, y);
		}
	}

	public static Vector2 lookVector
	{
		get
		{
			float x = Input.cursorMove.x * MOUSE_SENSITIVITY
				+ (Input.IsKeyDown(KeyCode.KeyL) ? TURN_SPEED * Time.deltaTime : 0.0f)
				+ (Input.IsKeyDown(KeyCode.KeyJ) ? -TURN_SPEED * Time.deltaTime : 0.0f)
				;
			float y = Input.cursorMove.y * MOUSE_SENSITIVITY
				+ (Input.IsKeyDown(KeyCode.KeyK) ? TURN_SPEED * Time.deltaTime : 0.0f)
				+ (Input.IsKeyDown(KeyCode.KeyI) ? -TURN_SPEED * Time.deltaTime : 0.0f)
				;
			return new Vector2(x, y);
		}
	}
}
