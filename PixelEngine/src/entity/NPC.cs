using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class NPC : Mob, Interactable
{
	bool shopOpen = false;

	List<Tuple<Item, int>> shopItems = new List<Tuple<Item, int>>();
	int selectedItem = 0;

	Sprite gemSprite;


	public NPC(string name)
		: base(name)
	{
		displayName = "Cool NPC";

		sprite = new Sprite(Resource.GetTexture("res/sprites/merchant.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 1, 0, 2, 2, true);
		animator.setAnimation("idle");

		gemSprite = HUD.gems;
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

	public void addShopItem(Item item, Random random)
	{
		int price = (int)(item.value * MathHelper.RandomFloat(0.95f, 1.5f, random));
		shopItems.Add(new Tuple<Item, int>(item, price));
	}

	public bool canInteract(Player player)
	{
		return !shopOpen && shopItems.Count > 0;
	}

	public float getRange()
	{
		return 2;
	}

	public void interact(Player player)
	{
		if (!shopOpen)
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
		outline = 0x9FFFFFFF;
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

	public override void update()
	{
		Player player = GameState.instance.player;

		if (shopOpen)
		{
			float maxDistance = 2;
			if (InputManager.IsPressed("UIQuit") || (player.position - position).lengthSquared > maxDistance * maxDistance)
			{
				closeShop();
			}
		}

		const float lookRange = 3;
		if (player.position.y >= position.y - 1.0f && (player.position - position).lengthSquared < lookRange * lookRange)
		{
			direction = MathF.Sign(player.position.x - position.x);
		}
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
			int width = 120;
			int height = headerHeight + shopItems.Count * lineHeight;
			int x = Math.Min(pos.x, Renderer.UIWidth - width);
			int y = pos.y - height;

			Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);

			Renderer.DrawUISprite(x, y, width, headerHeight - 1, null, false, 0xFF222222);
			Renderer.DrawUITextBMP(x + 2, y + 2, displayName, 1, 0xFFAAAAAA);
			Renderer.DrawUISprite(x + width - 1 - gemSprite.width, y + 2, gemSprite.width, gemSprite.height, gemSprite);
			string moneyStr = GameState.instance.player.money.ToString();
			Renderer.DrawUITextBMP(x + width - 1 - gemSprite.width - Renderer.MeasureUITextBMP(moneyStr, moneyStr.Length, 1).x - 2, y + 2, moneyStr, 1, 0xFFAAAAAA);
			y += headerHeight;

			for (int i = 0; i < shopItems.Count; i++)
			{
				bool selected = selectedItem == i;

				Item item = shopItems[i].Item1;
				int price = shopItems[i].Item2;

				Renderer.DrawUISprite(x, y, width, lineHeight, null, false, selected ? 0xFF333333 : 0xFF222222);
				Renderer.DrawUISprite(x + 1, y + 1, lineHeight, lineHeight, item.sprite);
				Renderer.DrawUITextBMP(x + 1 + lineHeight + 5, y + 4, item.displayName, 1, 0xFFAAAAAA);

				string quantity = price.ToString();
				bool canAfford = GameState.instance.player.money >= price;
				Renderer.DrawUITextBMP(x + width - 1 - Renderer.MeasureUITextBMP(quantity, quantity.Length, 1).x, y + 4, quantity, 1, canAfford ? 0xFFAAAAAA : 0xFFAA3333);

				if (i == selectedItem && InputManager.IsPressed("Interact") && canAfford)
				{
					InputManager.ConsumeEvent("Interact");
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

				y += lineHeight;
			}
		}
	}
}
