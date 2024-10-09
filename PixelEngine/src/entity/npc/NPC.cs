using Microsoft.VisualBasic.FileIO;
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
	CraftingMenu,
	UpgradeMenu,
}

enum DialogueEffect : int
{
	None = 0,
	Shaking,
	Quivering,
	Dancing,
	Restless,
}

public struct DialogueLine
{
	public string[] words;
}

public struct Dialogue
{
	public DialogueLine[] lines;
}

public abstract class NPC : Mob, Interactable
{
	const int DEFAULT_DIALOGUE_SPEED = 15;


	NPCState state = NPCState.None;
	protected Player player;

	protected List<Dialogue> voiceLines = new List<Dialogue>();
	long lastCharacterTime;
	int currentCharacter = 0;
	bool dialogueFinished = false;
	int dialogueSpeed = DEFAULT_DIALOGUE_SPEED;

	int selectedOption = 0;

	public bool buysItems = false;
	protected bool canCraft = false;
	protected bool canUpgrade = false;

	protected List<Tuple<Item, int>> shopItems = new List<Tuple<Item, int>>();
	int selectedItem = 0;
	protected float saleTax = 0.3f;
	protected float buyTax = 0.0f;
	//int longestItemName = 80;
	//int sidePanelHeight = 40;
	List<Item> craftingItems = new List<Item>();
	Item craftingItem1, craftingItem2;

	List<Item> upgradeItems = new List<Item>();
	List<int> upgradePrices = new List<int>();

	Sound[] tradeSound;
	Sound upgradeSound;

	//Sprite gem;

	Simplex simplex = new Simplex();


	public NPC(string name)
		: base(name)
	{
		//gem = HUD.gold;

		tradeSound = Resource.GetSounds("res/sounds/trade", 12);
		upgradeSound = Resource.GetSound("res/sounds/upgrade.ogg");
	}

	public override void destroy()
	{
		closeScreen();
	}

	public override void onLevelSwitch(Level newLevel)
	{
		closeScreen();
	}

	protected void populateShop(Random random, int minItems, int maxItems, float meanValue, params ItemType[] types)
	{
		int numItems = MathHelper.RandomInt(minItems, maxItems, random);

		float[] distribution = new float[DropRates.shop.Length];
		float cumulativeRate = 0;
		for (int i = 0; i < types.Length; i++)
		{
			float rate = DropRates.shop[(int)types[i]];
			distribution[(int)types[i]] = rate;
			cumulativeRate += rate;
		}
		for (int i = 0; i < distribution.Length; i++)
			distribution[i] /= cumulativeRate;

		for (int i = 0; i < numItems; i++)
		{
			Item[] items = Item.CreateRandom(random, distribution, meanValue);
			for (int j = 0; j < items.Length; j++)
			{
				Item item = items[j];
				if (item.canDrop && (item.stackable || !hasShopItem(item.name)))
					addShopItem(item.copy());
				else
					i--;
			}

			/*
			float value = MathF.Max(meanValue + meanValue * MathHelper.RandomGaussian(random), 0.0f);
			List<Item> items = Item.GetItemPrototypesOfType(type);
			items.Sort((Item item1, Item item2) =>
			{
				float r1 = MathF.Abs(item1.value - value);
				float r2 = MathF.Abs(item2.value - value);
				return r1 > r2 ? 1 : r1 < r2 ? -1 : 0;
			});
			Item item = items[0];
			*/
			/*
			ItemType type = types[random.Next() % types.Length];
			Item item = Item.CreateRandom(type, random, meanValue);
			if (item.canDrop && (item.stackable || !hasShopItem(item.name)))
				addShopItem(item.copy());
			else
				i--;
			*/
		}
	}

	public void addShopItem(Item item, int price = -1)
	{
		if (price == -1)
			price = (int)MathF.Round(item.value * (1 + saleTax));

		if (item.stackable)
		{
			for (int i = 0; i < shopItems.Count; i++)
			{
				if (shopItems[i].Item1.name == item.name && shopItems[i].Item2 == price)
				{
					shopItems[i].Item1.stackSize++;
					return;
				}
			}
		}

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
		Dialogue dialogue;
		dialogue.lines = new DialogueLine[lines.Length];
		for (int i = 0; i < lines.Length; i++)
		{
			DialogueLine line;
			line.words = lines[i].Split(' ');
			/*
			for (int j = 0; j < line.words.Length; j++)
			{
				string word = line.words[j];
				for (int k = 0; k < word.Length; k++)
				{
					char c = word[i];
					if (c == '\\' && k < word.Length - 1)
					{

						k++;
					}
				}
				if (word.Length >= 3 && word[0] == '\\')
				{
					line.effects[j] = (DialogueEffect)(word[1] - '0');
					line.words[j] = word.Substring(2);
				}
			}
			*/
			dialogue.lines[i] = line;
		}
		voiceLines.Add(dialogue);
	}

	public virtual Item craftItem(Item item1, Item item2)
	{
		return null;
	}

	public bool canInteract(Player player)
	{
		return state == NPCState.None && (shopItems.Count > 0 || voiceLines.Count > 0 || (buysItems && player.items.Count > 0) || (canCraft && player.items.Count >= 2));
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
			initDialogue();
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
			selectedOption = 0;
		}
	}

	void initDialogue()
	{
		state = NPCState.Dialogue;
		lastCharacterTime = Time.currentTime;
		currentCharacter = 0;
		dialogueFinished = false;
		dialogueSpeed = DEFAULT_DIALOGUE_SPEED;
	}

	void initMenu()
	{
		state = NPCState.Menu;
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

	void initCraftingMenu()
	{
		state = NPCState.CraftingMenu;
		selectedItem = 0;
		craftingItem1 = null;
		craftingItem2 = null;
		craftingItems.Clear();
		for (int i = 0; i < player.items.Count; i++)
			craftingItems.Add(player.items[i]);
	}

	void initUpgradeMenu()
	{
		state = NPCState.UpgradeMenu;
		selectedItem = 0;
		upgradeItems.Clear();
		for (int i = 0; i < player.items.Count; i++)
		{
			if (player.items[i].upgradable)
			{
				upgradeItems.Add(player.items[i]);
				upgradePrices.Add(player.items[i].upgradePrice);
			}
		}
	}

	public override void update()
	{
		Player player = GameState.instance.player;

		float maxDistance = getRange();
		if (state != NPCState.None && (InputManager.IsPressed("UIQuit") || (player.position + player.collider.center - position).lengthSquared > maxDistance * maxDistance))
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
			Dialogue voiceLine = voiceLines[0];

			int lineHeight = 8;
			int headerHeight = 12 + 1;
			int width = 120;
			int height = headerHeight + 4 + voiceLine.lines.Length * lineHeight + 4;
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

			Renderer.DrawUISprite(x, y, width, voiceLine.lines.Length * lineHeight + 4 + 4, null, false, 0xFF222222);
			y += 4;

			float characterFreq = dialogueSpeed * (InputManager.IsDown("Interact") && currentCharacter >= 2 ? 12 : 1);
			int numChars = (int)((Time.currentTime - lastCharacterTime) / 1e9f * characterFreq);
			for (int i = 0; i < numChars; i++)
			{
				currentCharacter++;
				lastCharacterTime = Time.currentTime;
			}

			DialogueEffect dialogueEffect = DialogueEffect.None;

			int characterIdx = 0;
			int spaceWidth = Renderer.MeasureUITextBMP(' ').x;
			for (int i = 0; i < voiceLine.lines.Length; i++)
			{
				Renderer.DrawUISprite(x, y, width, lineHeight, null, false, 0xFF222222);

				if (characterIdx > currentCharacter)
					break;

				int cursor = 0;
				for (int j = 0; j < voiceLine.lines[i].words.Length; j++)
				{
					if (characterIdx > currentCharacter)
						break;

					string word = voiceLine.lines[i].words[j];

					for (int k = 0; k < word.Length; k++)
					{
						if (characterIdx > currentCharacter)
							break;

						while (k < word.Length - 1 && word[k] == '\\')
						{
							if (characterIdx == currentCharacter)
							{
								if (word[k + 1] >= '1' && word[k + 1] <= '9')
									dialogueSpeed = DEFAULT_DIALOGUE_SPEED + (word[k + 1] - '5') * 3;
								else if (word[k + 1] == '0')
									dialogueSpeed = DEFAULT_DIALOGUE_SPEED;
							}
							if (word[k + 1] >= 'a' && word[k + 1] <= 'z')
								dialogueEffect = (DialogueEffect)(word[k + 1] - 'a' + 1);
							else if (word[k + 1] == '0')
								dialogueEffect = DialogueEffect.None;
							k += 2;
							continue;
						}
						if (k >= word.Length)
							continue;

						Vector2i offset = Vector2i.Zero;
						if (dialogueEffect == DialogueEffect.Shaking)
						{
							float elapsed = Time.currentTime / 1e9f * 10;
							float amplitude = 2;
							offset = (Vector2i)Vector2.Round(new Vector2(simplex.sample1f(elapsed + k * 1000), simplex.sample1f(-elapsed - k * 1000)) * amplitude);
						}
						else if (dialogueEffect == DialogueEffect.Quivering)
						{
							float elapsed = Time.currentTime / 1e9f * 4;
							float amplitude = 1;
							offset = (Vector2i)Vector2.Round(new Vector2(simplex.sample1f(elapsed + k * 1000), simplex.sample1f(-elapsed - k * 1000)) * amplitude);
						}
						else if (dialogueEffect == DialogueEffect.Dancing)
						{
							float elapsed = Time.currentTime / 1e9f * 1;
							float amplitude = 2;
							offset = (Vector2i)Vector2.Round(new Vector2(0, simplex.sample1f(elapsed + k)) * amplitude);
						}
						else if (dialogueEffect == DialogueEffect.Restless)
						{
							float elapsed = Time.currentTime / 1e9f * 0.5f;
							float amplitude = 1;
							offset = (Vector2i)Vector2.Round(new Vector2(simplex.sample1f(elapsed + k), simplex.sample1f(-elapsed - k)) * amplitude);
						}

						cursor += Renderer.DrawUITextBMP(x + 4 + cursor + offset.x, y + offset.y, word[k], 1, 0xFFAAAAAA);
						characterIdx++;

						if (k == word.Length - 1 && j == voiceLine.lines[i].words.Length - 1 && i == voiceLine.lines.Length - 1)
							dialogueFinished = true;
					}

					cursor += spaceWidth;
				}

				y += lineHeight;
			}

			if (dialogueFinished && InputManager.IsPressed("Interact", true))
			{
				voiceLines.RemoveAt(0);
				lastCharacterTime = Time.currentTime;
				currentCharacter = 0;
				dialogueFinished = false;
				dialogueSpeed = DEFAULT_DIALOGUE_SPEED;
				if (voiceLines.Count == 0)
					initMenu();
			}

			if (InputManager.IsPressed("UIBack", true))
				closeScreen();
		}
		else if (state == NPCState.Menu)
		{
			List<string> options = new List<string>();
			if (shopItems.Count > 0)
				options.Add("Buy");
			if (buysItems && player.items.Count > 0)
				options.Add("Sell");
			if (canCraft && player.items.Count >= 2)
				options.Add("Craft");
			if (canUpgrade && player.items.Count >= 1)
				options.Add("Upgrade");
			options.Add("Quit");

			Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			int option = InteractableMenu.Render(pos, displayName, options, out bool closed, ref selectedOption);

			if (option != -1)
			{
				if (options[option] == "Buy")
				{
					initShop();
				}
				else if (options[option] == "Sell")
				{
					initSellMenu();
				}
				else if (options[option] == "Craft")
				{
					initCraftingMenu();
				}
				else if (options[option] == "Upgrade")
				{
					initUpgradeMenu();
				}
				//else if (options[i] == "Talk")
				//	; // TODO
				else if (options[option] == "Quit")
					closeScreen();
			}

			if (closed)
				closeScreen();
		}
		else if (state == NPCState.Shop)
		{
			Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			List<Item> items = new List<Item>(shopItems.Count);
			List<int> prices = new List<int>(shopItems.Count);
			for (int i = 0; i < shopItems.Count; i++)
			{
				items.Add(shopItems[i].Item1);
				prices.Add(shopItems[i].Item2);
			}

			int choice = ItemSelector.Render(pos, displayName, items, prices, player.money, null, true, ItemSelector.GetCompareItem(player, items[selectedItem]), false, out bool secondary, out bool closed, ref selectedItem);
			if (choice != -1)
			{
				Item item = items[choice];
				int price = prices[choice];

				if (item.stackable && item.stackSize > 1 && !InputManager.IsDown("Sprint") && player.money >= price)
				{
					Item copy = item.copy();
					copy.stackSize = 1;
					GameState.instance.player.giveItem(copy);
					GameState.instance.player.money -= price;
					item.stackSize--;

					Audio.Play(tradeSound, new Vector3(position, 0));
				}
				else if (player.money >= price * item.stackSize)
				{
					GameState.instance.player.giveItem(item);
					GameState.instance.player.money -= price * item.stackSize;

					shopItems.RemoveAt(choice);

					if (selectedItem == shopItems.Count)
						selectedItem--;
					if (shopItems.Count == 0)
						initMenu();

					Audio.Play(tradeSound, new Vector3(position, 0));
				}
			}

			if (closed)
				initMenu();

			/*
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

			if (InputManager.IsPressed("Down", true) || InputManager.IsPressed("UIDown", true))
				selectedItem = (selectedItem + 1) % shopItems.Count;
			if (InputManager.IsPressed("Up", true) || InputManager.IsPressed("UIUp", true))
				selectedItem = (selectedItem + shopItems.Count - 1) % shopItems.Count;

			for (int i = 0; i < shopItems.Count; i++)
			{
				if (Renderer.IsHovered(x, y, shopWidth, lineHeight) && Input.cursorHasMoved)
					selectedItem = i;

				bool selected = selectedItem == i;

				Item item = shopItems[i].Item1;
				int price = shopItems[i].Item2;

				Renderer.DrawUISprite(x, y, shopWidth, lineHeight, null, false, selected ? 0xFF333333 : 0xFF222222);
				Renderer.DrawUISprite(x + 1, y + 1, 16, 16, item.getIcon(), false, MathHelper.VectorToARGB(item.spriteColor));
				string name = item.fullDisplayName;
				Renderer.DrawUITextBMP(x + 1 + 16 + 5, y + 4, name, 1, 0xFFAAAAAA);

				string quantity = price.ToString();
				bool canAfford = GameState.instance.player.money >= price;
				Renderer.DrawUITextBMP(x + shopWidth - 4 - Renderer.MeasureUITextBMP(quantity, quantity.Length, 1).x, y + 4, quantity, 1, canAfford ? 0xFFAAAAAA : 0xFFAA3333);

				longestItemName = Math.Max(longestItemName, Renderer.MeasureUITextBMP(name, name.Length, 1).x + 5 + Renderer.MeasureUITextBMP(quantity, quantity.Length, 1).x);

				if (selected && canAfford && (InputManager.IsPressed("Interact", true) || Input.IsMouseButtonPressed(MouseButton.Left, true)))
				{
					if (item.stackable && item.stackSize > 1)
					{
						Item copy = item.copy();
						copy.stackSize = 1;
						GameState.instance.player.giveItem(copy);
						GameState.instance.player.money -= price;
						item.stackSize--;
					}
					else
					{
						GameState.instance.player.giveItem(item);
						GameState.instance.player.money -= price;

						shopItems.RemoveAt(i);
						i--;

						if (selectedItem == shopItems.Count)
							selectedItem--;
						if (shopItems.Count == 0)
							initMenu();
					}
				}

				y += lineHeight;
			}

			// Item info panel
			if (shopItems.Count > 0)
			{
				sidePanelHeight = ItemInfoPanel.Render(shopItems[selectedItem].Item1, x + shopWidth + 1, Math.Max(pos.y - height, 2) + headerHeight, sidePanelWidth, Math.Max(shopItems.Count * lineHeight, sidePanelHeight));
			}

			if (InputManager.IsPressed("UIBack", true))
				initMenu();
			*/
		}
		else if (state == NPCState.SellMenu)
		{
			Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			List<Item> items = new List<Item>(player.items.Count);
			List<int> prices = new List<int>(player.items.Count);
			for (int i = 0; i < player.items.Count; i++)
			{
				items.Add(player.items[i]);
				prices.Add((int)MathF.Round(player.items[i].value));
			}

			int itemIdx = ItemSelector.Render(pos, displayName, items, prices, -1, player, true, null, false, out bool secondary, out bool closed, ref selectedItem);
			if (itemIdx != -1)
			{
				Item item = items[itemIdx];
				int price = prices[itemIdx];

				if (item.stackable && item.stackSize > 1 && !InputManager.IsDown("Sprint"))
				{
					item.stackSize--;
					Item copy = item.copy();
					copy.stackSize = 1;
					addShopItem(copy);
					GameState.instance.player.money += price;
				}
				else
				{
					GameState.instance.player.removeItem(item);
					addShopItem(item);
					GameState.instance.player.money += price * item.stackSize;

					if (selectedItem == player.items.Count)
						selectedItem--;
					if (player.items.Count == 0)
						initMenu();
				}

				Audio.Play(tradeSound, new Vector3(position, 0));
			}

			if (closed)
				initMenu();

			/*
			int lineHeight = 16;
			int headerHeight = 12 + 1;
			int sidePanelWidth = 80;
			int shopWidth = Math.Max(120, 1 + 16 + 5 + longestItemName + 1);
			int width = shopWidth + 1 + sidePanelWidth;
			int height = headerHeight + player.items.Count * lineHeight;
			int x = Math.Min(pos.x, Renderer.UIWidth - width - 2);
			int y = Math.Max(pos.y - height, 2);

			Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);

			Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, 0xFF222222);
			Renderer.DrawUITextBMP(x + 2, y + 2, displayName, 1, 0xFFAAAAAA);
			Renderer.DrawUISprite(x + width - 1 - gem.width, y + 2, gem.width, gem.height, gem);
			string moneyStr = GameState.instance.player.money.ToString();
			Renderer.DrawUITextBMP(x + width - 1 - gem.width - Renderer.MeasureUITextBMP(moneyStr, moneyStr.Length, 1).x - 2, y + 2, moneyStr, 1, 0xFFAAAAAA);
			y += headerHeight;

			if (InputManager.IsPressed("Down", true) || InputManager.IsPressed("UIDown", true))
				selectedItem = (selectedItem + 1) % player.items.Count;
			if (InputManager.IsPressed("Up", true) || InputManager.IsPressed("UIUp", true))
				selectedItem = (selectedItem + player.items.Count - 1) % player.items.Count;

			for (int i = 0; i < player.items.Count; i++)
			{
				if (Renderer.IsHovered(x, y, shopWidth, lineHeight) && Input.cursorHasMoved)
					selectedItem = i;
				bool selected = selectedItem == i;

				Item item = player.items[i].Item2;
				int price = (int)MathF.Round(item.value);

				Renderer.DrawUISprite(x, y, shopWidth, lineHeight, null, false, selected ? 0xFF333333 : 0xFF222222);
				Renderer.DrawUISprite(x + 1, y + 1, lineHeight, lineHeight, item.getIcon(), false, MathHelper.VectorToARGB(item.spriteColor));
				string name = item.fullDisplayName;
				Renderer.DrawUITextBMP(x + 1 + lineHeight + 5, y + 4, name, 1, 0xFFAAAAAA);

				string quantity = price.ToString();
				Renderer.DrawUITextBMP(x + shopWidth - 1 - 16 - Renderer.MeasureUITextBMP(quantity, quantity.Length, 1).x, y + 4, quantity, 1, 0xFFf4d16b);

				if (player.handItem == item)
					Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, InventoryUI.weaponSprite);
				else if (player.offhandItem == item)
					Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, InventoryUI.shieldSprite);
				else if (player.isActiveItem(item, out int activeSlot))
				{
					Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, InventoryUI.bagSprite);
					Renderer.DrawUITextBMP(x + shopWidth - 1 - 4, y + 16 - 8, (activeSlot + 1).ToString(), 1, 0xFF505050);
				}
				else if (player.isPassiveItem(item, out int passiveSlot))
				{
					Renderer.DrawUISprite(x + shopWidth - 1 - 16, y, 16, 16, item.type == ItemType.Ring ? InventoryUI.ringSprite : InventoryUI.armorSprite);
					Renderer.DrawUITextBMP(x + shopWidth - 1 - 4, y + 16 - 8, (passiveSlot + 1 - (item.type == ItemType.Ring ? player.passiveItems.Length - 2 : 0)).ToString(), 1, 0xFF505050);
				}

				longestItemName = Math.Max(longestItemName, Renderer.MeasureUITextBMP(name, name.Length, 1).x + 5 + 16 + Renderer.MeasureUITextBMP(quantity, quantity.Length, 1).x);

				if (selected && (InputManager.IsPressed("Interact", true) || Input.IsMouseButtonPressed(MouseButton.Left, true)))
				{
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
						GameState.instance.player.removeItem(item);
						addShopItem(item);
						GameState.instance.player.money += price;
						i--;

						if (selectedItem == player.items.Count)
							selectedItem--;
						if (player.items.Count == 0)
							initMenu();
					}
				}

				y += lineHeight;
			}

			// Item info panel
			if (player.items.Count > 0)
				sidePanelHeight = ItemInfoPanel.Render(player.items[selectedItem].Item2, x + shopWidth + 1, Math.Max(pos.y - height, 2) + headerHeight, sidePanelWidth, Math.Max(player.items.Count * lineHeight, sidePanelHeight));

			if (InputManager.IsPressed("UIBack", true))
				initMenu();
			*/
		}
		else if (state == NPCState.CraftingMenu)
		{
			Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			int choice = ItemSelector.Render(pos, craftingItem1 != null ? "Select item 2" : "Select item 1", craftingItems, null, -1, player, true, null, false, out bool secondary, out bool closed, ref selectedItem);
			if (choice != -1)
			{
				Item item = craftingItems[choice];

				if (craftingItem1 == null)
				{
					craftingItem1 = item;
					craftingItems.Remove(item);
					if (selectedItem == craftingItems.Count)
						selectedItem--;
				}
				else if (craftingItem2 == null)
				{
					craftingItem2 = item;
					Item craftedItem = craftItem(craftingItem1, craftingItem2);
					if (craftedItem != null)
					{
						GameState.instance.level.addEntity(new ItemEntity(craftedItem, null, new Vector2(direction, 1) * 3), position + new Vector2(0, 0.5f));
						craftingItem1 = null;
						craftingItem2 = null;
						closeScreen();
					}
					else
					{
						player.hud.showMessage("Could not craft anything out of " + craftingItem1.displayName + " and " + craftingItem2.displayName + ".");
						craftingItem1 = null;
						craftingItem2 = null;
						closeScreen();
					}
				}
			}

			if (closed)
				initMenu();

			/*
			int lineHeight = 16;
			int headerHeight = 12 + 1;
			int sidePanelWidth = 80;
			int shopWidth = Math.Max(120, 1 + lineHeight + 5 + longestItemName + 1);
			int width = shopWidth + 1 + sidePanelWidth;
			int height = headerHeight + craftingItems.Count * lineHeight;
			int x = Math.Min(pos.x, Renderer.UIWidth - width - 2);
			int y = Math.Max(pos.y - height, 2);

			Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);

			Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, 0xFF222222);
			Renderer.DrawUITextBMP(x + 2, y + 2, craftingItem1 != null ? "Select item 2" : "Select item 1", 1, 0xFFAAAAAA);
			Renderer.DrawUISprite(x + width - 1 - gem.width, y + 2, gem.width, gem.height, gem);
			string moneyStr = GameState.instance.player.money.ToString();
			Renderer.DrawUITextBMP(x + width - 1 - gem.width - Renderer.MeasureUITextBMP(moneyStr, moneyStr.Length, 1).x - 2, y + 2, moneyStr, 1, 0xFFAAAAAA);
			y += headerHeight;

			if (InputManager.IsPressed("Down", true) || InputManager.IsPressed("UIDown", true))
				selectedItem = (selectedItem + 1) % craftingItems.Count;
			if (InputManager.IsPressed("Up", true) || InputManager.IsPressed("UIUp", true))
				selectedItem = (selectedItem + craftingItems.Count - 1) % craftingItems.Count;

			for (int i = 0; i < craftingItems.Count; i++)
			{
				if (Renderer.IsHovered(x, y, shopWidth, lineHeight) && Input.cursorHasMoved)
					selectedItem = i;
				bool selected = selectedItem == i;

				Item item = craftingItems[i].Item2;

				Renderer.DrawUISprite(x, y, shopWidth, lineHeight, null, false, selected ? 0xFF333333 : 0xFF222222);
				Renderer.DrawUISprite(x + 1, y + 1, lineHeight, lineHeight, item.getIcon(), false, MathHelper.VectorToARGB(item.spriteColor));
				string name = item.fullDisplayName;
				Renderer.DrawUITextBMP(x + 1 + lineHeight + 5, y + 4, name, 1, 0xFFAAAAAA);

				longestItemName = Math.Max(longestItemName, Renderer.MeasureUITextBMP(name, name.Length, 1).x + 5);

				if (selected && (InputManager.IsPressed("Interact", true) || Input.IsMouseButtonPressed(MouseButton.Left, true)))
				{
					if (craftingItem1 == null)
					{
						craftingItem1 = item;
						craftingItems.Remove(item.type, item);
						if (selectedItem == craftingItems.Count)
							selectedItem--;
					}
					else if (craftingItem2 == null)
					{
						craftingItem2 = item;
						Item craftedItem = craftItem(craftingItem1, craftingItem2);
						if (craftedItem != null)
						{
							GameState.instance.level.addEntity(new ItemEntity(craftedItem, this, new Vector2(direction, 1) * 3), position + new Vector2(0, 0.5f));
							craftingItem1 = null;
							craftingItem2 = null;
							closeScreen();
						}
						else
						{
							player.hud.showMessage("Could not craft anything out of " + craftingItem1.displayName + " and " + craftingItem2.displayName + ".");
							craftingItem1 = null;
							craftingItem2 = null;
							closeScreen();
						}
					}
				}

				y += lineHeight;
			}

			// Item info panel
			if (craftingItems.Count > 0)
				sidePanelHeight = ItemInfoPanel.Render(craftingItems[selectedItem].Item2, x + shopWidth + 1, Math.Max(pos.y - height, 2) + headerHeight, sidePanelWidth, Math.Max(player.items.Count * lineHeight, sidePanelHeight));

			if (InputManager.IsPressed("UIBack", true))
				initMenu();
			*/
		}
		else if (state == NPCState.UpgradeMenu)
		{
			Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			Item upgradedItem = upgradeItems[selectedItem].copy();
			upgradedItem.upgrade();

			int choice = ItemSelector.Render(pos, "Upgrade", upgradeItems, upgradePrices, player.money, player, true, upgradedItem, true, out bool secondary, out bool closed, ref selectedItem);
			if (choice != -1)
			{
				Item item = upgradeItems[choice];
				item.upgrade();
				player.money -= upgradePrices[choice];
				upgradePrices[choice] = item.upgradePrice;
				Audio.Play(upgradeSound, new Vector3(position, 0));
			}

			if (closed)
				initMenu();
		}
	}
}
