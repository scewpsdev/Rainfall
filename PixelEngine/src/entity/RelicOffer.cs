using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RelicOffer : Entity
{
	List<Item> items = new List<Item>();

	long startTime;


	public RelicOffer()
	{
	}

	public override void init(Level level)
	{
		// TODO modify count?
		for (int i = 0; i < 3; i++)
		{
			Item item = Item.CreateRandom(ItemType.Relic, Random.Shared, level.lootValue);
			while (hasItem(item.name))
				item = Item.CreateRandom(ItemType.Relic, Random.Shared, level.lootValue);

			items.Add(item);
		}

		GameState.instance.onscreenPrompt = true;
		Input.cursorMode = CursorMode.Normal;

		startTime = Time.timestamp;
	}

	public override void destroy()
	{
		GameState.instance.onscreenPrompt = false;
		Input.cursorMode = CursorMode.Hidden;
	}

	bool hasItem(string name)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].name == name)
				return true;
		}
		return false;
	}

	int selectedItem;
	public override void render()
	{
		Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, null, false, MathHelper.ColorAlpha(0xFF000000, MathF.Min((Time.timestamp - startTime) / 1e9f, 0.5f)));

		if ((Time.timestamp - startTime) / 1e9f > 0.5f)
		{
			int width = 160;
			int height = 50 * items.Count;
			int x = Renderer.UIWidth / 2 - width / 2;
			int y = Renderer.UIHeight / 2 - height / 2;

			int choice = ItemSelector.Render(x, y, "Select upgrade", items, null, -1, null, true, null, false, out bool secondary, out bool closed, ref selectedItem);
			if (choice != -1)
			{
				GameState.instance.player.giveItem(items[choice]);
				GameState.instance.onscreenPrompt = false;
				remove();
			}
		}
	}
}
