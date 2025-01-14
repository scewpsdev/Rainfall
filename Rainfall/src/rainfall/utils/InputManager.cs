using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


public class InputBinding
{
	public KeyCode[] keys = null;
	public MouseButton button = MouseButton.None;
	public GamepadButton gamepadButton = GamepadButton.None;
	public int scrollDelta = 0;


	public bool isDown()
	{
		bool keyDown = false;
		if (keys != null)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				if (Input.IsKeyDown(keys[i]))
					keyDown = true;
			}
		}
		return InputManager.inputEnabled && (keyDown
			|| button != MouseButton.None && Input.IsMouseButtonDown(button)
			|| gamepadButton != GamepadButton.None && Input.IsGamepadButtonDown(gamepadButton));
	}

	public bool isPressed(bool consume)
	{
		bool keyPressed = false;
		if (keys != null)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				if (Input.IsKeyPressed(keys[i]))
					keyPressed = true;
			}
		}
		bool result = InputManager.inputEnabled && (keyPressed
			|| button != MouseButton.None && Input.IsMouseButtonPressed(button)
			|| gamepadButton != GamepadButton.None && Input.IsGamepadButtonPressed(gamepadButton)
			|| scrollDelta != 0 && scrollDelta == Math.Sign(Input.scrollMove));
		if (result && consume)
			consumeEvent();
		return result;
	}

	public bool isReleased(bool consume)
	{
		bool keyReleased = false;
		if (keys != null)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				if (Input.IsKeyReleased(keys[i]))
					keyReleased = true;
			}
		}
		bool result = InputManager.inputEnabled && (keyReleased
			|| button != MouseButton.None && Input.IsMouseButtonReleased(button)
			|| gamepadButton != GamepadButton.None && Input.IsGamepadButtonReleased(gamepadButton)
			|| scrollDelta != 0 && scrollDelta == Math.Sign(Input.scrollMove));
		if (result && consume)
			consumeEvent();
		return result;
	}

	public void consumeEvent()
	{
		if (keys != null)
		{
			for (int i = 0; i < keys.Length; i++)
				Input.ConsumeKeyEvent(keys[i]);
		}
		if (button != MouseButton.None)
			Input.ConsumeMouseButtonEvent(button);
		if (gamepadButton != GamepadButton.None)
			Input.ConsumeGamepadButtonEvent(gamepadButton);
		if (scrollDelta != 0)
			Input.ConsumeScrollEvent();
	}

	public override string ToString()
	{
		StringBuilder result = new StringBuilder();
		if (Input.gamepadConnected)
		{
			if (gamepadButton != GamepadButton.None)
				result.Append((result.Length > 2 ? " / " : "") + "Gamepad " + gamepadButton.ToString());
		}
		else
		{
			if (keys != null && keys.Length > 0)
				result.Append((result.Length > 2 ? " / " : "") + keys[0].ToString());
			if (button != MouseButton.None)
				result.Append((result.Length > 0 ? " / " : "") + "M" + ((int)button).ToString());
			if (scrollDelta != 0)
				result.Append((result.Length > 0 ? " / " : "") + "Scroll " + (scrollDelta > 0 ? "Up" : "Down"));
		}
		return result.ToString();
	}

	public string ToStringElaborate()
	{
		StringBuilder result = new StringBuilder();
		result.Append("[ ");
		if (keys != null)
		{
			for (int i = 0; i < keys.Length; i++)
				result.Append((result.Length > 2 ? " / " : "") + keys[i].ToString());
		}
		if (button != MouseButton.None)
			result.Append((result.Length > 2 ? " / " : "") + "M" + ((int)button).ToString());
		if (gamepadButton != GamepadButton.None)
			result.Append((result.Length > 2 ? " / " : "") + "Gamepad " + gamepadButton.ToString());
		if (scrollDelta != 0)
			result.Append((result.Length > 2 ? " / " : "") + "Scroll " + (scrollDelta > 0 ? "Up" : "Down"));
		result.Append(" ]");
		return result.ToString();
	}
}

public static class InputManager
{
	const string INPUT_BINDINGS_FILE = "settings/InputBindings.config";

	const float MOUSE_SENSITIVITY = 0.0015f;
	const float TURN_SPEED = 2.5f;

	static Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();
	static Dictionary<string, MouseButton> buttons = new Dictionary<string, MouseButton>();
	static Dictionary<string, GamepadButton> gamepadButtons = new Dictionary<string, GamepadButton>();

	static Dictionary<string, InputBinding> bindings = new Dictionary<string, InputBinding>();

	public static bool inputEnabled = true;


	public static void LoadBindings()
	{
		foreach (KeyCode key in Enum.GetValues<KeyCode>())
		{
			keys.Add(key.ToString(), key);
		}
		foreach (MouseButton button in Enum.GetValues<MouseButton>())
		{
			buttons.Add("M" + ((int)button).ToString(), button);
		}
		foreach (GamepadButton button in Enum.GetValues<GamepadButton>())
		{
			gamepadButtons.Add("Gamepad" + button.ToString(), button);
		}

		if (File.Exists(INPUT_BINDINGS_FILE))
		{
			DatFile bindingsFile = new DatFile(File.ReadAllText(INPUT_BINDINGS_FILE), INPUT_BINDINGS_FILE);
			foreach (DatField binding in bindingsFile.root.fields)
			{
				MouseButton button = MouseButton.None;
				List<KeyCode> keys = new List<KeyCode>();
				GamepadButton gamepadButton = GamepadButton.None;
				int scrollDelta = 0;

				if (binding.value.type == DatValueType.Identifier)
				{
					string valueStr = binding.value.identifier;
					TryGetButton(valueStr, ref button);
					if (TryGetKey(valueStr, out KeyCode key))
						keys.Add(key);
					TryGetGamepadButton(valueStr, ref gamepadButton);
					TryGetScroll(valueStr, ref scrollDelta);
				}
				else if (binding.value.type == DatValueType.Array)
				{
					foreach (DatValue value in binding.value.array.values)
					{
						string valueStr = value.identifier;
						TryGetButton(valueStr, ref button);
						if (TryGetKey(valueStr, out KeyCode key))
							keys.Add(key);
						TryGetGamepadButton(valueStr, ref gamepadButton);
						TryGetScroll(valueStr, ref scrollDelta);
					}
				}

				if (button != MouseButton.None || keys.Count > 0 || gamepadButton != GamepadButton.None || scrollDelta != 0)
				{
					bindings.Add(binding.name, new InputBinding() { button = button, keys = keys.ToArray(), gamepadButton = gamepadButton, scrollDelta = scrollDelta });
					Console.WriteLine("Registered binding " + binding.name);
				}
			}
		}
	}

	static bool TryGetKey(string value, out KeyCode key)
	{
		if (keys.ContainsKey(value))
		{
			key = keys[value];
			return true;
		}
		key = KeyCode.None;
		return false;
	}

	static void TryGetButton(string value, ref MouseButton button)
	{
		if (buttons.ContainsKey(value))
			button = buttons[value];
	}

	static void TryGetGamepadButton(string value, ref GamepadButton button)
	{
		if (gamepadButtons.ContainsKey(value))
			button = gamepadButtons[value];
	}

	static void TryGetScroll(string value, ref int scroll)
	{
		if (value == "ScrollUp")
			scroll = 1;
		if (value == "ScrollDown")
			scroll = -1;
	}

	public static void SaveBindings()
	{
		FileStream stream = File.Open(INPUT_BINDINGS_FILE, FileMode.Create);
		DatFile bindingsFile = new DatFile();

		foreach (var pair in bindings)
		{
			InputBinding binding = pair.Value;
			List<DatValue> values = new List<DatValue>();
			if (binding.keys != null)
			{
				DatValue[] keyValues = new DatValue[binding.keys.Length];
				for (int i = 0; i < binding.keys.Length; i++)
					keyValues[i] = new DatValue(binding.keys[i].ToString(), DatValueType.Identifier);
				values.Add(new DatValue(new DatArray(keyValues)));
			}
			if (binding.button != MouseButton.None)
				values.Add(new DatValue("M" + ((int)binding.button).ToString(), DatValueType.Identifier));
			if (binding.gamepadButton != GamepadButton.None)
				values.Add(new DatValue("Gamepad" + binding.gamepadButton.ToString(), DatValueType.Identifier));
			if (binding.scrollDelta != 0)
				values.Add(new DatValue(binding.scrollDelta == 1 ? "ScrollUp" : "ScrollDown", DatValueType.Identifier));
			bindingsFile.addField(new DatField(pair.Key, new DatValue(new DatArray(values.ToArray()))));
		}

		bindingsFile.serialize(stream);
		stream.Close();

#if DEBUG
		//Utils.RunCommand("xcopy", "/y \"" + INPUT_BINDINGS_FILE + "\" \"..\\..\\..\\\"");
#endif

		Console.WriteLine("Saved bindings");
	}

	public static InputBinding GetBinding(string name)
	{
		if (bindings.TryGetValue(name, out InputBinding binding))
			return binding;
		return null;
	}

	public static bool IsDown(string name)
	{
		if (bindings.ContainsKey(name))
			return bindings[name].isDown();
		return false;
	}

	public static bool IsPressed(string name, bool consume = false)
	{
		if (bindings.ContainsKey(name))
			return bindings[name].isPressed(consume);
		return false;
	}

	public static bool IsReleased(string name, bool consume = false)
	{
		if (bindings.ContainsKey(name))
			return bindings[name].isReleased(consume);
		return false;
	}

	public static void ConsumeEvent(string name)
	{
		if (bindings.ContainsKey(name))
			bindings[name].consumeEvent();
	}

	public static Vector2 moveVector
	{
		get
		{
			float x = 0
				+ (Input.IsKeyDown(KeyCode.D) ? 1.0f : 0.0f)
				+ (Input.IsKeyDown(KeyCode.A) ? -1.0f : 0.0f)
				;
			float y = 0
				+ (Input.IsKeyDown(KeyCode.W) ? 1.0f : 0.0f)
				+ (Input.IsKeyDown(KeyCode.S) ? -1.0f : 0.0f)
				;
			return new Vector2(x, y);
		}
	}

	public static Vector2 lookVector
	{
		get
		{
			float x = Input.cursorMove.x * MOUSE_SENSITIVITY
				+ (Input.IsKeyDown(KeyCode.L) ? TURN_SPEED * Time.deltaTime : 0.0f)
				+ (Input.IsKeyDown(KeyCode.J) ? -TURN_SPEED * Time.deltaTime : 0.0f)
				;
			float y = Input.cursorMove.y * MOUSE_SENSITIVITY
				+ (Input.IsKeyDown(KeyCode.K) ? TURN_SPEED * Time.deltaTime : 0.0f)
				+ (Input.IsKeyDown(KeyCode.I) ? -TURN_SPEED * Time.deltaTime : 0.0f)
				;
			return new Vector2(x, y);
		}
	}
}
