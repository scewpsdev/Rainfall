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
	AttuneMenu,
	QuestList,
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

public class DialogueScreen
{
	public DialogueLine[] lines;

	public List<Action> callbacks = new List<Action>();

	public void addCallback(Action callback)
	{
		callbacks.Add(callback);
	}
}

public class Dialogue
{
	public List<DialogueScreen> screens = new List<DialogueScreen>();


	public DialogueScreen addVoiceLine(string txt)
	{
		int maxWidth = 120 - 2 * 4;
		string[] lines = Renderer.SplitMultilineText(txt, maxWidth);
		DialogueScreen dialogue = new DialogueScreen();
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
		screens.Add(dialogue);
		return dialogue;
	}
}

public abstract class NPC : Mob, Interactable
{
	const int DEFAULT_DIALOGUE_SPEED = 25;


	NPCState state = NPCState.None;
	protected Player player;

	protected Dialogue initialDialogue;
	protected List<Dialogue> dialogues = new List<Dialogue>();
	Dialogue currentDialogue;
	long lastCharacterTime;
	int currentCharacter = 0;
	bool dialogueFinished = false;
	int dialogueSpeed = DEFAULT_DIALOGUE_SPEED;

	int selectedOption = 0;

	public bool buysItems = false;
	protected bool canCraft = false;
	protected bool canUpgrade = false;
	//protected bool canAttune = false;
	public float voicePitch = 1.0f;
	public int voicePitchVariation = 2;

	protected List<Tuple<Item, int>> shopItems = new List<Tuple<Item, int>>();
	int selectedItem = 0;
	//int infoPanelHeight = 90;
	protected float buyTax = 0.25f;
	List<Item> craftingItems = new List<Item>();
	Item craftingItem1, craftingItem2;

	List<Item> upgradeItems = new List<Item>();
	List<int> upgradePrices = new List<int>();

	List<Item> attuneStaffs = new List<Item>();
	List<Item> attuneSpells = new List<Item>();
	//Staff attuneStaff = null;
	//int attuneSlotID = -1;
	//Spell attuneSpell = null;

	Sound[] tradeSound;
	Sound upgradeSound;

	//Sprite gem;

	Simplex simplex = new Simplex();


	public NPC(string name)
		: base(name)
	{
		//gem = HUD.gold;

		tradeSound = Resource.GetSounds("sounds/trade", 12);
		upgradeSound = Resource.GetSound("sounds/upgrade.ogg");
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

		shopItems.Sort((Tuple<Item, int> item1, Tuple<Item, int> item2) =>
		{
			int getScore(Item item) => item.isHandItem && !item.isSecondaryItem ? 1 :
				item.isHandItem ? 2 :
				item.isSecondaryItem ? 3 :
				item.isActiveItem ? 4 :
				item.isPassiveItem && item.armorSlot != ArmorSlot.None ? 5 + (int)item.armorSlot :
				item.isPassiveItem ? 5 + (int)ArmorSlot.Count : 100;
			int score1 = getScore(item1.Item1);
			int score2 = getScore(item2.Item1);
			return score1 > score2 ? 1 : score1 < score2 ? -1 : item1.Item1.value > item2.Item1.value ? 1 : item1.Item1.value < item2.Item1.value ? -1 : 0;
		});
	}

	public void addShopItem(Item item, int price = -1)
	{
		if (price == -1)
			price = (int)MathF.Round(item.value);

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

	public void addDialogue(Dialogue dialogue)
	{
		dialogues.Add(dialogue);
	}

	public virtual Item craftItem(Item item1, Item item2)
	{
		return null;
	}

	public bool isInteractable(Player player)
	{
		return state == NPCState.None && (shopItems.Count > 0 || initialDialogue != null || dialogues.Count > 0 || (buysItems && player.items.Count > 0) || (canCraft && player.items.Count >= 2)) /*|| (canAttune && player.hasItemOfType(ItemType.Staff))*/ || GameState.instance.save.getQuestList(name, out _);
	}

	public float getRange()
	{
		return 2;
	}

	public void interact(Player player)
	{
		openScreen();
		this.player = player;

		if (initialDialogue != null)
		{
			initDialogue(initialDialogue);
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
			state = initialDialogue != null ? NPCState.Dialogue : NPCState.Menu;
			GameState.instance.player.numOverlaysOpen++;
		}
	}

	protected void closeScreen()
	{
		if (state != NPCState.None)
		{
			state = NPCState.None;
			GameState.instance.player.numOverlaysOpen--;
			selectedOption = 0;
		}
	}

	void initDialogue(Dialogue dialogue)
	{
		state = NPCState.Dialogue;
		currentDialogue = dialogue;
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
				upgradePrices.Add(player.items[i].upgradeCost);
			}
		}
	}

	void initAttuneMenu()
	{
		state = NPCState.AttuneMenu;
		selectedItem = 0;
		attuneStaffs.Clear();
		attuneSpells.Clear();
		//attuneStaff = null;
		//attuneSlotID = -1;
		//attuneSpell = null;
		for (int i = 0; i < player.items.Count; i++)
		{
			if (player.items[i].type == ItemType.Staff)
				attuneStaffs.Add(player.items[i]);
			else if (player.items[i].type == ItemType.Spell)
				attuneSpells.Add(player.items[i]);
		}
	}

	void initQuestList()
	{
		state = NPCState.QuestList;
		selectedItem = 0;
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
			Vector2 pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));
			DialogueScreen voiceLine = currentDialogue.screens[0];

			int lineHeight = 8;
			int headerHeight = 12 + 1;
			int width = 120;
			int height = headerHeight + 4 + voiceLine.lines.Length * lineHeight + 4;
			float x = Math.Min(pos.x, Renderer.UIWidth - width - 2);
			float y = Math.Max(pos.y - height, 2);

			Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);

			// speech bubble thingy
			for (int i = 0; i < 5; i++)
			{
				float xx = pos.x + 4;
				float yy = y + height + i;
				int ww = 5 - i;

				Renderer.DrawUISprite(xx, yy, ww, 1, null, false, 0xFF222222);
			}

			Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, 0xFF222222);
			Renderer.DrawUITextBMP(x + 2, y + 2, displayName, 1, 0xFFAAAAAA);
			y += headerHeight;

			Renderer.DrawUISprite(x, y, width, voiceLine.lines.Length * lineHeight + 4 + 4, null, false, 0xFF222222);
			y += 4;

			float characterFreq = dialogueSpeed * (InputManager.IsDown("Interact") && currentCharacter >= 5 ? 12 : 1);
			int numChars = (int)((Time.currentTime - lastCharacterTime) / 1e9f * characterFreq);
			for (int i = 0; i < numChars; i++)
			{
				currentCharacter++;
				lastCharacterTime = Time.currentTime;
			}
			if (numChars > 0 && !dialogueFinished && currentCharacter % 2 == 0)
			{
				float pitch = voicePitch;
				float noteMultiplier = MathF.Pow(2.0f, 1 / 7.0f);
				int variation = MathHelper.RandomInt(-voicePitchVariation, voicePitchVariation);
				pitch *= MathF.Pow(noteMultiplier, variation);
				Audio.PlayBackground(UISound.uiClick, 0.2f, pitch);
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

						cursor += Renderer.DrawUITextBMP(x + 4 + cursor + offset.x, y + offset.y, word[k], false, false, 1, 0xFFAAAAAA);
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
				DialogueScreen screen = currentDialogue.screens[0];
				currentDialogue.screens.RemoveAt(0);

				lastCharacterTime = Time.currentTime;
				currentCharacter = 0;
				dialogueFinished = false;
				dialogueSpeed = DEFAULT_DIALOGUE_SPEED;
				if (currentDialogue.screens.Count == 0)
				{
					initMenu();
					if (currentDialogue == initialDialogue)
						initialDialogue = null;
					else
						dialogues.Remove(currentDialogue);
					currentDialogue = null;
				}

				for (int i = 0; i < screen.callbacks.Count; i++)
					screen.callbacks[i]();
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
			{
				bool hasUpgradable = false;
				for (int i = 0; i < player.items.Count; i++)
				{
					if (player.items[i].upgradable)
					{
						hasUpgradable = true;
						break;
					}
				}
				if (hasUpgradable)
					options.Add("Upgrade");
			}
			//if (canAttune && player.hasItemOfType(ItemType.Staff))
			//	options.Add("Attune");
			if (dialogues.Count > 0)
				options.Add("Talk");
			if (GameState.instance.save.getQuestList(name, out _))
				options.Add("Quests");
			options.Add("Quit");

			Vector2 pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

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
				else if (options[option] == "Attune")
				{
					initAttuneMenu();
				}
				else if (options[option] == "Talk")
				{
					initDialogue(dialogues[0]);
				}
				else if (options[option] == "Quests")
				{
					initQuestList();
				}
				else if (options[option] == "Quit")
					closeScreen();
			}

			if (closed)
				closeScreen();
		}
		else if (state == NPCState.Shop)
		{
			Vector2 pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			List<Item> items = new List<Item>(shopItems.Count);
			List<int> prices = new List<int>(shopItems.Count);
			for (int i = 0; i < shopItems.Count; i++)
			{
				items.Add(shopItems[i].Item1);
				prices.Add(shopItems[i].Item2);
			}

			int choice = ItemSelector.Render(pos, "Buy", items, prices, player.money, null, true, ItemSelector.GetCompareItem(player, items[selectedItem]), false, out bool secondary, out bool closed, ref selectedItem);
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
		}
		else if (state == NPCState.SellMenu)
		{
			Vector2 pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			List<Item> items = new List<Item>(player.items.Count);
			List<int> prices = new List<int>(player.items.Count);
			for (int i = 0; i < player.items.Count; i++)
			{
				items.Add(player.items[i]);
				prices.Add(Math.Max((int)MathF.Round(player.items[i].value * buyTax), 1));
			}

			int itemIdx = ItemSelector.Render(pos, "Sell", items, prices, -player.money, player, true, null, false, out bool secondary, out bool closed, ref selectedItem);
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
		}
		else if (state == NPCState.CraftingMenu)
		{
			Vector2 pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

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
		}
		else if (state == NPCState.UpgradeMenu)
		{
			Vector2 pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			Item upgradedItem = upgradeItems[selectedItem].copy();
			upgradedItem.upgrade();

			int choice = ItemSelector.Render(pos, "Upgrade", upgradeItems, upgradePrices, player.money, player, true, upgradedItem, true, out bool secondary, out bool closed, ref selectedItem);
			if (choice != -1)
			{
				if (upgradePrices[selectedItem] <= player.money)
				{
					Item item = upgradeItems[choice];
					item.upgrade();
					player.money -= upgradePrices[choice];
					upgradePrices[choice] = item.upgradeCost;
					Audio.Play(upgradeSound, new Vector3(position, 0));
				}
			}

			if (closed)
				initMenu();
		}
		else if (state == NPCState.AttuneMenu)
		{
			/*
			Vector2 pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));
			pos.x = Math.Min(pos.x, Renderer.UIWidth - 1 - 120 - 90 - 90);

			if (attuneStaff == null)
			{
				int choice = ItemSelector.Render(pos, "Attune", attuneStaffs, null, -1, player, false, null, false, out bool secondary, out bool closed, ref selectedItem);
				if (choice != -1)
				{
					attuneStaff = (Staff)attuneStaffs[choice];
					while (attuneStaff.attunedSpells.Count < attuneStaff.staffAttunementSlots)
						attuneStaff.attunedSpells.Add(null);
					selectedItem = 0;
				}

				if (closed)
					initMenu();
			}
			if (attuneStaff != null)
			{
				if (attuneSlotID == -1)
				{
					int renderAttunementSelector(float x, float y, int width, int height)
					{
						int choice = AttunementSelector.Render(x, y, width, height, attuneStaff, out bool secondary, out bool closed, ref selectedItem);

						Spell selectedSpell = attuneStaff.attunedSpells[selectedItem];
						if (selectedSpell != null)
							infoPanelHeight = (int)ItemInfoPanel.Render(selectedSpell, x + width + 1, y, 90, infoPanelHeight);

						if (choice != -1)
						{
							if (secondary)
							{
								if (attuneStaff.attunedSpells[choice] != null)
								{
									Spell oldSpell = attuneStaff.attuneSpell(choice, null);
									player.giveItem(oldSpell);
									attuneSpells.Add(oldSpell);
								}
							}
							else
							{
								attuneSlotID = choice;
								selectedItem = 0;
							}
						}
						if (closed)
						{
							attuneStaff = null;
						}

						return height;
					}

					int staffIdx = attuneStaffs.IndexOf(attuneStaff);
					ItemSelector.Render(pos, "Attune", attuneStaffs, null, -1, player, renderAttunementSelector, false, out bool secondary, out bool closed, ref staffIdx);
				}
				if (attuneSlotID != -1)
				{
					int renderSpellSelector(float x, float y, int width, int height)
					{
						int choice = ItemSelector.Render(x, y, width, height, "Select spell", attuneSpells, null, -1, null, true, null, false, out bool secondary, out bool closed, ref selectedItem);
						if (choice != -1)
						{
							attuneSpell = (Spell)attuneSpells[choice];

							player.removeItem(attuneSpell);
							Spell oldSpell = attuneStaff.attuneSpell(attuneSlotID, attuneSpell);
							attuneSpells.Remove(attuneSpell);
							if (oldSpell != null)
							{
								player.giveItem(oldSpell);
								attuneSpells.Add(oldSpell);
							}

							attuneSlotID = -1;
						}
						if (closed)
						{
							attuneSlotID = -1;
						}

						int lineHeight = 16;
						int headerHeight = 12 + 1;
						int shopHeight = Math.Min(attuneSpells.Count, 10) * lineHeight;
						return headerHeight + shopHeight;
					}

					int staffIdx = attuneStaffs.IndexOf(attuneStaff);
					ItemSelector.Render(pos, "Attune", attuneStaffs, null, -1, player, renderSpellSelector, false, out bool secondary, out bool closed, ref staffIdx);
				}
			}
			*/
		}
		else if (state == NPCState.QuestList)
		{
			Vector2 pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			List<string> labels = new List<string>();
			if (GameState.instance.save.getQuestList(name, out List<Quest> quests))
			{
				for (int i = 0; i < quests.Count; i++)
					labels.Add(quests[i].displayName);
			}

			int renderInfoPanel(float x, float y, int width, int height)
			{
				float top = y;

				y += 4;

				Quest quest = quests[selectedItem];
				Renderer.DrawUITextBMP(x + width / 2 - Renderer.MeasureUITextBMP(quest.displayName).x / 2, y, quest.displayName, 1, 0xFFFFFFFF);
				y += Renderer.smallFont.size;
				y += 5;

				if (quest.description != null)
				{
					string[] descriptionLines = Renderer.SplitMultilineText(quest.description, width - 2);
					for (int i = 0; i < descriptionLines.Length; i++)
					{
						Renderer.DrawUITextBMP(x + 2, y, descriptionLines[i], 1, 0xFFAAAAAA);
						y += Renderer.smallFont.size;
					}
				}

				y += 5;
				string progress = quest.getProgressString();
				Renderer.DrawUITextBMP(x + width / 2 - Renderer.MeasureUITextBMP(progress).x / 2, y, progress, 1, 0xFFf4d16b);
				y += Renderer.smallFont.size;

				y += 4;

				return (int)MathF.Round(y - top);
			};
			NPCSelector.Render(pos, "Quests", labels, renderInfoPanel, out bool secondary, out bool closed, ref selectedItem);

			if (closed)
				initMenu();
		}
	}
}
