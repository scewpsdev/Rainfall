using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;


enum NPCState
{
	None = 0,

	Dialogue,
	Menu,
	Shop,
	SellMenu,
}

struct VoiceLine
{
	public string[] lines;
}

public abstract class NPC : Mob, Interactable
{
	NPCState state = NPCState.None;
	Player player;

	List<VoiceLine> voiceLines = new List<VoiceLine>();

	int selectedOption = 0;

	protected bool buysItems = false;

	protected List<Tuple<Item, int>> shopItems = new List<Tuple<Item, int>>();
	int selectedItem = 0;
	protected float tax = 0.2f;
	int longestItemName = 80;
	int sidePanelHeight = 40;

	Sprite gem;


	public NPC(string name)
		: base(name)
	{
		gem = HUD.gem;
	}

	public override void destroy()
	{
		closeScreen();
	}

	public override void onLevelSwitch()
	{
		closeScreen();
	}

	protected void populateShop(Random random, int maxItems, params ItemType[] types)
	{
		int numItems = MathHelper.RandomInt(1, maxItems, random);
		for (int i = 0; i < numItems; i++)
		{
			ItemType type = types[random.Next() % types.Length];
			Item item = Item.CreateRandom(type, random);
			if (item.stackable || !hasShopItem(item.name))
				addShopItem(item);
		}
	}

	public void addShopItem(Item item, int price = -1)
	{
		if (item.stackable)
		{
			for (int i = 0; i < shopItems.Count; i++)
			{
				if (shopItems[i].Item1.name == item.name)
				{
					shopItems[i].Item1.stackSize++;
					return;
				}
			}
		}

		if (price == -1)
			price = (int)MathF.Round(item.value * (1 + tax));
		shopItems.Add(new Tuple<Item, int>(item, price));
	}

	public bool hasShopItem(string name)
	{
		for (int i = 0; i < shopItems.Count; i++)
		{
			if (shopItems[i].Item1.name == name)
				return true;
		}
		return false;
	}

	public void clearShop()
	{
		shopItems.Clear();
	}

	public void addVoiceLine(string txt)
	{
		int maxWidth = 120 - 2 * 4;
		string[] lines = Renderer.SplitMultilineText(txt, maxWidth);
		voiceLines.Add(new VoiceLine { lines = lines });
	}

	public bool canInteract(Player player)
	{
		return state == NPCState.None && (shopItems.Count > 0 || voiceLines.Count > 0 || buysItems);
	}

	public float getRange()
	{
		return 2;
	}

	public void interact(Player player)
	{
		openScreen();
		this.player = player;

		if (voiceLines.Count > 0)
		{
			state = NPCState.Dialogue;
		}
		else
		{
			initMenu();
		}
	}

	public void onFocusEnter(Player player)
	{
		outline = OUTLINE_COLOR;
	}

	public void onFocusLeft(Player player)
	{
		outline = 0;
	}

	void openScreen()
	{
		if (state == NPCState.None)
		{
			state = voiceLines.Count > 0 ? NPCState.Dialogue : NPCState.Menu;
			GameState.instance.player.numOverlaysOpen++;
		}
	}

	void closeScreen()
	{
		if (state != NPCState.None)
		{
			state = NPCState.None;
			GameState.instance.player.numOverlaysOpen--;
		}
	}

	void initMenu()
	{
		state = NPCState.Menu;
		selectedOption = 0;
	}

	void initShop()
	{
		state = NPCState.Shop;
		selectedItem = 0;
	}

	void initSellMenu()
	{
		state = NPCState.SellMenu;
		selectedItem = 0;
	}

	public override void update()
	{
		Player player = GameState.instance.player;

		float maxDistance = 2;
		if (InputManager.IsPressed("UIQuit") || (player.position - position).lengthSquared > maxDistance * maxDistance)
		{
			closeScreen();
		}

		const float lookRange = 3;
		if (player.position.y >= position.y - 1.0f && (player.position - position).lengthSquared < lookRange * lookRange)
		{
			direction = MathF.Sign(player.position.x - position.x);
		}

		animator.update(sprite);
	}

	public override void render()
	{
		base.render();

		if (state == NPCState.Dialogue)
		{
			Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));
			VoiceLine voiceLine = voiceLines[0];

			int lineHeight = 8;
			int headerHeight = 12 + 1;
			int width = 120;
			int height = headerHeight + 4 + voiceLine.lines.Length * lineHeight + 2;
			int x = Math.Min(pos.x, Renderer.UIWidth - width - 2);
			int y = Math.Max(pos.y - height, 2);

			Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);

			// speech bubble thingy
			for (int i = 0; i < 5; i++)
			{
				int xx = pos.x + 4;
				int yy = y + height + i;
				int ww = 5 - i;

				Renderer.DrawUISprite(xx, yy, ww, 1, null, false, 0xFF222222);
			}

			Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, 0xFF222222);
			Renderer.DrawUITextBMP(x + 2, y + 2, displayName, 1, 0xFFAAAAAA);
			y += headerHeight;

			Renderer.DrawUISprite(x, y, width, voiceLine.lines.Length * lineHeight + 4, null, false, 0xFF222222);
			y += 4;

			for (int i = 0; i < voiceLine.lines.Length; i++)
			{
				Renderer.DrawUISprite(x, y, width, lineHeight, null, false, 0xFF222222);
				Renderer.DrawUITextBMP(x + 4, y, voiceLine.lines[i], 1, 0xFFAAAAAA);
				y += lineHeight;
			}

			if (InputManager.IsPressed("Interact"))
			{
				InputManager.ConsumeEvent("Interact");
				voiceLines.RemoveAt(0);
				if (voiceLines.Count == 0)
				{
					initMenu();
				}
			}
		}
		else if (state == NPCState.Menu)
		{
			Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			int lineHeight = 12;
			int headerHeight = 12 + 1;
			int width = 120;
			int numOptions = (shopItems.Count > 0 ? 1 : 0) + (buysItems && player.numTotalItems > 0 ? 1 : 0) + 1;
			int height = headerHeight + numOptions * lineHeight;
			int x = Math.Min(pos.x, Renderer.UIWidth - width - 2);
			int y = Math.Max(pos.y - height, 2);

			Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);

			Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, 0xFF222222);
			Renderer.DrawUITextBMP(x + 2, y + 2, displayName, 1, 0xFFAAAAAA);
			y += headerHeight;

			string[] options = new string[numOptions];
			if (shopItems.Count > 0)
			{
				options[0] = "Buy";
				if (buysItems && player.numTotalItems > 0)
				{
					options[1] = "Sell";
					options[2] = "Quit";
				}
				else
				{
					options[1] = "Quit";
				}
			}
			else if (buysItems && player.numTotalItems > 0)
			{
				options[0] = "Sell";
				options[1] = "Quit";
			}
			else
			{
				options[0] = "Quit";
			}

			if (InputManager.IsPressed("Down"))
				selectedOption = (selectedOption + 1) % options.Length;
			if (InputManager.IsPressed("Up"))
				selectedOption = (selectedOption + options.Length - 1) % options.Length;

			for (int i = 0; i < options.Length; i++)
			{
				bool selected = selectedOption == i;

				Renderer.DrawUISprite(x, y, width, lineHeight, null, false, selected ? 0xFF333333 : 0xFF222222);
				Renderer.DrawUITextBMP(x + 4, y + 2, options[i], 1, 0xFFAAAAAA);

				if (selected && InputManager.IsPressed("Interact"))
				{
					InputManager.ConsumeEvent("Interact");

					if (options[i] == "Buy")
					{
						initShop();
					}
					else if (options[i] == "Sell")
					{
						initSellMenu();
					}
					//else if (options[i] == "Talk")
					//	; // TODO
					else if (options[i] == "Quit")
						closeScreen();
				}

				y += lineHeight;
			}

			if (InputManager.IsPressed("UIBack"))
				closeScreen();
		}
		else if (state == NPCState.Shop)
		{
			Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			int lineHeight = 16;
			int headerHeight = 12 + 1;
			int sidePanelWidth = 80;
			int shopWidth = Math.Max(120, 1 + 16 + 5 + longestItemName + 4);
			int width = shopWidth + 1 + sidePanelWidth;
			int height = headerHeight + shopItems.Count * lineHeight;
			int x = Math.Min(pos.x, Renderer.UIWidth - width - 2);
			int y = Math.Max(pos.y - height, 2);

			Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);

			Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, 0xFF222222);
			Renderer.DrawUITextBMP(x + 2, y + 2, displayName, 1, 0xFFAAAAAA);
			Renderer.DrawUISprite(x + width - 1 - gem.width, y + 2, gem.width, gem.height, gem);
			string moneyStr = GameState.instance.player.money.ToString();
			Renderer.DrawUITextBMP(x + width - 1 - gem.width - Renderer.MeasureUITextBMP(moneyStr, moneyStr.Length, 1).x - 2, y + 2, moneyStr, 1, 0xFFAAAAAA);
			y += headerHeight;

			if (InputManager.IsPressed("Down"))
				selectedItem = (selectedItem + 1) % shopItems.Count;
			if (InputManager.IsPressed("Up"))
				selectedItem = (selectedItem + shopItems.Count - 1) % shopItems.Count;

			for (int i = 0; i < shopItems.Count; i++)
			{
				bool selected = selectedItem == i;

				Item item = shopItems[i].Item1;
				int price = shopItems[i].Item2;

				Renderer.DrawUISprite(x, y, shopWidth, lineHeight, null, false, selected ? 0xFF333333 : 0xFF222222);
				Renderer.DrawUISprite(x + 1, y + 1, 16, 16, item.sprite);
				string name = item.fullDisplayName;
				Renderer.DrawUITextBMP(x + 1 + 16 + 5, y + 4, name, 1, 0xFFAAAAAA);

				string quantity = price.ToString();
				bool canAfford = GameState.instance.player.money >= price;
				Renderer.DrawUITextBMP(x + shopWidth - 4 - Renderer.MeasureUITextBMP(quantity, quantity.Length, 1).x, y + 4, quantity, 1, canAfford ? 0xFFAAAAAA : 0xFFAA3333);

				longestItemName = Math.Max(longestItemName, Renderer.MeasureUITextBMP(name, name.Length, 1).x + 5 + Renderer.MeasureUITextBMP(quantity, quantity.Length, 1).x);

				if (selected && canAfford && InputManager.IsPressed("Interact"))
				{
					InputManager.ConsumeEvent("Interact");

					if (item.stackable && item.stackSize > 1)
					{
						Item copy = item.copy();
						copy.stackSize = 1;
						if (GameState.instance.player.giveItem(copy))
						{
							GameState.instance.player.money -= price;
							item.stackSize--;
						}
					}
					else
					{
						if (GameState.instance.player.giveItem(item))
						{
							GameState.instance.player.money -= price;

							shopItems.RemoveAt(i);
							i--;

							if (selectedItem == shopItems.Count)
								selectedItem--;
							if (shopItems.Count == 0)
								initMenu();
						}
					}
				}

				y += lineHeight;
			}

			// Item info panel
			if (shopItems.Count > 0)
			{
				sidePanelHeight = ItemInfoPanel.Render(shopItems[selectedItem].Item1, x + shopWidth + 1, Math.Max(pos.y - height, 2) + headerHeight, sidePanelWidth, Math.Max(shopItems.Count * lineHeight, sidePanelHeight));
			}

			if (InputManager.IsPressed("UIBack"))
				initMenu();
		}
		else if (state == NPCState.SellMenu)
		{
			Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			List<Item> sellItems = new List<Item>();
			if (player.handItem != null)
				sellItems.Add(player.handItem);
			for (int i = 0; i < player.quickItems.Length; i++)
			{
				if (player.quickItems[i] != null)
					sellItems.Add(player.quickItems[i]);
			}
			for (int i = 0; i < player.passiveItems.Length; i++)
			{
				if (player.passiveItems[i] != null)
					sellItems.Add(player.passiveItems[i]);
			}

			int lineHeight = 16;
			int headerHeight = 12 + 1;
			int sidePanelWidth = 80;
			int shopWidth = Math.Max(120, 1 + lineHeight + 5 + longestItemName + 1);
			int width = shopWidth + 1 + sidePanelWidth;
			int height = headerHeight + sellItems.Count * lineHeight;
			int x = Math.Min(pos.x, Renderer.UIWidth - width - 2);
			int y = Math.Max(pos.y - height, 2);

			Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);

			Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, 0xFF222222);
			Renderer.DrawUITextBMP(x + 2, y + 2, displayName, 1, 0xFFAAAAAA);
			Renderer.DrawUISprite(x + width - 1 - gem.width, y + 2, gem.width, gem.height, gem);
			string moneyStr = GameState.instance.player.money.ToString();
			Renderer.DrawUITextBMP(x + width - 1 - gem.width - Renderer.MeasureUITextBMP(moneyStr, moneyStr.Length, 1).x - 2, y + 2, moneyStr, 1, 0xFFAAAAAA);
			y += headerHeight;

			if (InputManager.IsPressed("Down"))
				selectedItem = (selectedItem + 1) % sellItems.Count;
			if (InputManager.IsPressed("Up"))
				selectedItem = (selectedItem + sellItems.Count - 1) % sellItems.Count;

			for (int i = 0; i < sellItems.Count; i++)
			{
				bool selected = selectedItem == i;

				Item item = sellItems[i];
				int price = (int)MathF.Round(item.value);

				Renderer.DrawUISprite(x, y, shopWidth, lineHeight, null, false, selected ? 0xFF333333 : 0xFF222222);
				Renderer.DrawUISprite(x + 1, y + 1, lineHeight, lineHeight, item.sprite);
				string name = item.fullDisplayName;
				Renderer.DrawUITextBMP(x + 1 + lineHeight + 5, y + 4, name, 1, 0xFFAAAAAA);

				string quantity = price.ToString();
				Renderer.DrawUITextBMP(x + shopWidth - 1 - Renderer.MeasureUITextBMP(quantity, quantity.Length, 1).x, y + 4, quantity, 1, 0xFFAAAAAA);

				longestItemName = Math.Max(longestItemName, Renderer.MeasureUITextBMP(name, name.Length, 1).x + 5 + Renderer.MeasureUITextBMP(quantity, quantity.Length, 1).x);

				if (selected && InputManager.IsPressed("Interact"))
				{
					InputManager.ConsumeEvent("Interact");

					if (item.stackable && item.stackSize > 1)
					{
						item.stackSize--;
						Item copy = item.copy();
						copy.stackSize = 1;
						addShopItem(copy);
						GameState.instance.player.money += price;
					}
					else
					{
						if (GameState.instance.player.removeItem(item))
						{
							addShopItem(item);
							GameState.instance.player.money += price;
							sellItems.RemoveAt(i--);

							if (selectedItem == sellItems.Count)
								selectedItem--;
							if (sellItems.Count == 0)
								initMenu();
						}
					}
				}

				y += lineHeight;
			}

			// Item info panel
			if (sellItems.Count > 0)
			{
				sidePanelHeight = ItemInfoPanel.Render(sellItems[selectedItem], x + shopWidth + 1, Math.Max(pos.y - height, 2) + headerHeight, sidePanelWidth, Math.Max(sellItems.Count * lineHeight, sidePanelHeight));
			}

			if (InputManager.IsPressed("UIBack"))
				initMenu();
		}
	}
}
