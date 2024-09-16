using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class DebugConsole
{
	static StringBuilder line = new StringBuilder();
	static List<string> history = new List<string>();
	static List<string> responses = new List<string>();
	static int currentHistoryIdx = 0;

	public static void OnOpen()
	{
		line.Clear();
		currentHistoryIdx = 0;
		InputManager.inputEnabled = false;
	}

	public static void OnClose()
	{
		InputManager.inputEnabled = true;
	}

	static string RunCommand(string cmd, string[] args)
	{
		if (cmd == "create_entity")
		{
			if (args.Length > 0)
			{
				Entity entity = EntityType.CreateInstance(args[0]);
				if (entity != null)
				{
					Vector2 position = GameSettings.aimMode == AimMode.Directional ? GameState.instance.player.position + GameState.instance.player.collider.center + GameState.instance.player.lookDirection
						: GameState.instance.camera.screenToWorld(Renderer.cursorPosition);
					GameState.instance.level.addEntity(entity, position);
					return "Spawned entity in position " + position.ToString();
				}
				return "No entity of type " + args[0];
			}
		}
		if (cmd == "create_item")
		{
			if (args.Length > 0)
			{
				Item item = Item.GetItemPrototype(args[0]);
				if (item != null)
				{
					Vector2 position = GameSettings.aimMode == AimMode.Directional ? GameState.instance.player.position + GameState.instance.player.collider.center + GameState.instance.player.lookDirection
						: GameState.instance.camera.screenToWorld(Renderer.cursorPosition);
					int count = 1;
					if (args.Length >= 2 && int.TryParse(args[1], out int _count))
						count = _count;
					Item copy = item.copy();
					copy.stackSize = count;
					GameState.instance.level.addEntity(new ItemEntity(copy), position);
					return "Spawned item " + args[0] + " in position" + position.ToString();
				}
				return "No item of type " + args[0];
			}
		}
		if (cmd == "set_floor")
		{
			if (args.Length > 0)
			{
				if (int.TryParse(args[0], out int floor))
				{
					if (floor >= 1 && floor <= GameState.instance.floors.Length)
					{
						Level level = GameState.instance.floors[floor - 1];
						GameState.instance.switchLevel(level, level.entrance.position);
					}
				}
			}
		}

		return null;
	}

	public static void Render()
	{
		int width = 240;
		int height = 160;
		int x = Renderer.UIWidth / 2 - width / 2;
		int y = Renderer.UIHeight / 2 - height / 2;

		Renderer.DrawUISprite(x, y, width, height, null, false, 0x7F111111);

		Renderer.DrawUISprite(x + 2, y + height - 2 - Renderer.smallFont.size, width - 4, Renderer.smallFont.size, null, false, 0x7F333333);
		bool caretVisible = Time.currentTime / 1e9f % 2 > 1;
		Renderer.DrawUITextBMP(x + 2, y + height - 2 - Renderer.smallFont.size, line.ToString() + (caretVisible ? "_" : ""));

		int maxLines = 16;
		int lineHeight = Renderer.smallFont.size;
		int yy = y + height - 2 - lineHeight - 4 - lineHeight;
		for (int i = history.Count - 1; i >= Math.Max(0, history.Count - maxLines); i--)
		{
			if (responses[i] != null)
			{
				Renderer.DrawUITextBMP(x + 2, yy, responses[i], 1, 0xFF777777);
				yy -= lineHeight;
			}
			Renderer.DrawUITextBMP(x + 2, yy, history[i]);
			yy -= lineHeight;
		}
	}

	public static void OnKeyEvent(KeyCode key, KeyModifier modifiers, bool down)
	{
		if (key == KeyCode.Backspace && modifiers == KeyModifier.None && down && line.Length > 0)
			line.Remove(line.Length - 1, 1);
		if (key == KeyCode.Return && modifiers == KeyModifier.None && down)
		{
			string txt = line.ToString();
			history.Add(txt);

			string[] elements = txt.Split(' ');
			string cmd = elements[0];
			string[] args = ArrayUtils.Slice(elements, 1);
			string response = RunCommand(cmd, args);
			responses.Add(response);
			line.Clear();
			currentHistoryIdx = 0;
		}
		if (key == KeyCode.Esc && modifiers == KeyModifier.None && down)
		{
			GameState.instance.consoleOpen = false;
			OnClose();
		}
		if (key == KeyCode.Up && modifiers == KeyModifier.None && down && history.Count > 0)
		{
			line.Clear();
			line.Append(history[history.Count - 1 - currentHistoryIdx++]);
		}
		Input.ConsumeKeyEvent(key);
	}

	public static void OnCharEvent(char c)
	{
		line.Append(c);
	}
}
