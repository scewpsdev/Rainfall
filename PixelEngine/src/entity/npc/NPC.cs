using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


struct VoiceLine
{
	public string[] lines;
}

public abstract class NPC : Mob, Interactable
{
	bool shopOpen = false;
	List<Tuple<Item, int>> shopItems = new List<Tuple<Item, int>>();
	int selectedItem = 0;
	protected float tax = 0.2f;
	int longestItemName = 80;
	int sidePanelHeight = 40;
	Sprite gem;

	bool dialogueOpen = false;
	List<VoiceLine> voiceLines = new List<VoiceLine>();


	public NPC(string name)
		: base(name)
	{
		gem = HUD.gem;
	}

	public override void destroy()
	{
		if (shopOpen)
			closeShop();
	}

	public override void onLevelSwitch()
	{
		if (shopOpen)
			closeShop();
	}

	public abstract void populateShop(Random random);

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
			price = (int)MathF.Ceiling(item.value * (1 + tax));
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

	public void addVoiceLine(string txt)
	{
		int maxWidth = 120 - 2 * 4;
		string[] lines = Renderer.SplitMultilineText(txt, maxWidth);
		voiceLines.Add(new VoiceLine { lines = lines });
	}

	public bool canInteract(Player player)
	{
		return !shopOpen && !dialogueOpen && (shopItems.Count > 0 || voiceLines.Count > 0);
	}

	public float getRange()
	{
		return 2;
	}

	public void interact(Player player)
	{
		if (voiceLines.Count > 0)
		{
			openDialogue();
		}
		else if (!shopOpen)
		{
			openShop();
		}
		else
		{
			Debug.Assert(false);
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

	void openShop()
	{
		shopOpen = true;
		GameState.instance.player.numOverlaysOpen++;
	}

	void closeShop()
	{
		shopOpen = false;
		GameState.instance.player.numOverlaysOpen--;
	}

	void openDialogue()
	{
		dialogueOpen = true;
		GameState.instance.player.numOverlaysOpen++;
	}

	void closeDialogue()
	{
		dialogueOpen = false;
		GameState.instance.player.numOverlaysOpen--;
	}

	public override void update()
	{
		Player player = GameState.instance.player;

		float maxDistance = 2;
		if (InputManager.IsPressed("UIQuit") || (player.position - position).lengthSquared > maxDistance * maxDistance)
		{
			if (shopOpen)
				closeShop();
			if (dialogueOpen)
				closeDialogue();
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

		if (shopOpen)
		{
			if (InputManager.IsPressed("Down"))
				selectedItem = (selectedItem + 1) % shopItems.Count;
			if (InputManager.IsPressed("Up"))
				selectedItem = (selectedItem + shopItems.Count - 1) % shopItems.Count;

			Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			int lineHeight = 16;
			int headerHeight = 12 + 1;
			int sidePanelWidth = 80;
			int shopWidth = 1 + lineHeight + 5 + longestItemName + 1;
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

			for (int i = 0; i < shopItems.Count; i++)
			{
				bool selected = selectedItem == i;

				Item item = shopItems[i].Item1;
				int price = shopItems[i].Item2;

				Renderer.DrawUISprite(x, y, shopWidth, lineHeight, null, false, selected ? 0xFF333333 : 0xFF222222);
				Renderer.DrawUISprite(x + 1, y + 1, lineHeight, lineHeight, item.sprite);
				string name = item.fullDisplayName;
				Renderer.DrawUITextBMP(x + 1 + lineHeight + 5, y + 4, name, 1, 0xFFAAAAAA);

				string quantity = price.ToString();
				bool canAfford = GameState.instance.player.money >= price;
				Renderer.DrawUITextBMP(x + shopWidth - 1 - Renderer.MeasureUITextBMP(quantity, quantity.Length, 1).x, y + 4, quantity, 1, canAfford ? 0xFFAAAAAA : 0xFFAA3333);

				longestItemName = Math.Max(longestItemName, Renderer.MeasureUITextBMP(name, name.Length, 1).x + 5 + Renderer.MeasureUITextBMP(quantity, quantity.Length, 1).x);

				if (i == selectedItem && InputManager.IsPressed("Interact") && canAfford)
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
								closeShop();
						}
					}
				}

				y += lineHeight;
			}

			// Item info panel
			{
				y = Math.Max(pos.y - height, 2) + headerHeight;

				Item item = shopItems[selectedItem].Item1;

				Renderer.DrawUISprite(x + shopWidth, y - 1, sidePanelWidth + 2, Math.Max(shopItems.Count * lineHeight, sidePanelHeight) + 2, null, false, 0xFFAAAAAA);
				Renderer.DrawUISprite(x + shopWidth + 1, y, sidePanelWidth, Math.Max(shopItems.Count * lineHeight, sidePanelHeight), null, false, 0xFF222222);

				y += 4;
				string[] nameLines = Renderer.SplitMultilineText(item.displayName, sidePanelWidth);
				foreach (string line in nameLines)
				{
					Renderer.DrawUITextBMP(x + shopWidth + 1 + sidePanelWidth / 2 - Renderer.MeasureUITextBMP(line).x / 2, y, line, 1, 0xFFAAAAAA);
					y += Renderer.smallFont.size;
				}
				y++;

				string rarityString = item.rarityString;
				string itemTypeStr = item.type == ItemType.Weapon ? "Weapon" : item.type == ItemType.Active ? "Consumable" : "Passive Item";
				string description = rarityString + " " + itemTypeStr;
				string[] descriptionLines = Renderer.SplitMultilineText(description, sidePanelWidth);
				foreach (string line in descriptionLines)
				{
					Renderer.DrawUITextBMP(x + shopWidth + 1 + sidePanelWidth / 2 - Renderer.MeasureUITextBMP(line).x / 2, y, line, 1, 0xFF666666);
					y += Renderer.smallFont.size;
				}
				y += 4;

				void drawLeft(string str, uint color = 0xFFAAAAAA)
				{
					if (str == null)
						str = "???";
					Renderer.DrawUITextBMP(x + shopWidth + 1 + 4, y, str, 1, color);
				}
				void drawRight(string str, uint color = 0xFFAAAAAA)
				{
					if (str == null)
						str = "???";
					int textWidth = Renderer.MeasureUITextBMP(str, str.Length, 1).x;
					Renderer.DrawUITextBMP(x + shopWidth + 1 + sidePanelWidth - textWidth - 1, y, str, 1, color);
				}

				if (item.type == ItemType.Weapon)
				{
					drawLeft("Attack");
					drawRight(item.attackDamage.ToString("0.0"));
					y += Renderer.smallFont.size + 1;

					//drawLeft("Reach");
					//drawRight(item.attackRange.ToString("0.0"));
					//y += Renderer.smallFont.size + 1;

					//drawLeft("Knockback");
					//drawRight(item.knockback.ToString("0.0"));
					//y += Renderer.smallFont.size + 1;
				}
				else if (item.type == ItemType.Passive)
				{
					if (item.armor > 0)
					{
						drawLeft("Defense");
						drawRight(item.armor.ToString());
						y += Renderer.smallFont.size + 1;
					}
				}

				sidePanelHeight = y - (Math.Max(pos.y - height, 2) + headerHeight);
			}
		}
		else if (dialogueOpen)
		{
			VoiceLine voiceLine = voiceLines[0];

			Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			int lineHeight = 12;
			int headerHeight = 12 + 1;
			int width = 120;
			int height = headerHeight + 4 + voiceLine.lines.Length * lineHeight;
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
					closeDialogue();
					if (shopItems.Count > 0)
						openShop();
				}
			}
		}
	}
}
