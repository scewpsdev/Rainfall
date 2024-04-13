using Rainfall;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;


struct ItemCollectedNotification
{
	public Item item;
	public int amount;
	public long timeCollected;
}

internal class HUD
{
	Player player;

	FontData fontData;
	Font promptFont, xpFont, notificationFont, stackSizeFont;

	Texture crosshair;
	Texture crosshairHand;

	public List<ItemCollectedNotification> collectedItems = new List<ItemCollectedNotification>();


	public HUD(Player player)
	{
		this.player = player;

		fontData = Resource.GetFontData("res/fonts/libre-baskerville.regular.ttf");
		promptFont = fontData.createFont(28.0f);
		xpFont = fontData.createFont(20.0f);
		notificationFont = fontData.createFont(18.0f);
		stackSizeFont = fontData.createFont(20.0f);

		crosshair = Resource.GetTexture("res/texture/ui/crosshair.png");
		crosshairHand = Resource.GetTexture("res/texture/ui/hand_right.png");
	}

	void renderCrosshair()
	{
		if (player.interactableInFocus != null)
		{
			int width = crosshairHand.info.width / 2;
			int height = crosshairHand.info.height / 2;
			Renderer.DrawUITexture(Display.viewportSize.x / 2 - width / 2, Display.viewportSize.y / 2 - height / 2, width, height, crosshairHand);
		}
		else
		{
			int width = crosshair.info.width / 2;
			int height = crosshair.info.height / 2;
			Renderer.DrawUITexture(Display.viewportSize.x / 2 - width / 2, Display.viewportSize.y / 2 - height / 2, width, height, crosshair);
		}
	}

	void renderHealthBar()
	{
		int x = 90;
		int y = 40;
		int width = player.stats.maxHealth * 2;
		int height = 10;
		int padding = 2;

		Renderer.DrawUIRect(x - padding, y - padding, width + 2 * padding, height + 2 * padding, 0xffaaaaaa);
		Renderer.DrawUIRect(x - padding / 2, y - padding / 2, width + padding, height + padding, 0xff222222);

		Renderer.DrawUIRect(x, y, width, height, 0xff331111);
		Renderer.DrawUIRect(x, y, (int)((float)player.stats.health / player.stats.maxHealth * width), height, 0xffcc3333);
	}

	void renderStaminaBar()
	{
		int x = 90;
		int y = 60;
		int width = (int)(player.stats.maxStamina * 10.0f);
		int height = 10;
		int padding = 2;

		Renderer.DrawUIRect(x - padding, y - padding, width + 2 * padding, height + 2 * padding, 0xffaaaaaa);
		Renderer.DrawUIRect(x - padding / 2, y - padding / 2, width + padding, height + padding, 0xff222222);

		Renderer.DrawUIRect(x, y, width, height, 0xff1c241d);
		Renderer.DrawUIRect(x, y, (int)(player.stats.stamina / player.stats.maxStamina * width), height, 0xff478749);
	}

	void renderManaBar()
	{
		int x = 90;
		int y = 80;
		int width = player.stats.maxMana * 2;
		int height = 10;
		int padding = 2;

		Renderer.DrawUIRect(x - padding, y - padding, width + 2 * padding, height + 2 * padding, 0xffaaaaaa);
		Renderer.DrawUIRect(x - padding / 2, y - padding / 2, width + padding, height + padding, 0xff222222);

		Renderer.DrawUIRect(x, y, width, height, 0xff1C1D24);
		Renderer.DrawUIRect(x, y, (int)((float)player.stats.mana / player.stats.maxMana * width), height, 0xff374087);
	}

	void renderItemSlot(int x, int y, int width, int height, Item item, int stackSize)
	{
		Renderer.DrawUIRect(x, y, width, height, 0xff111111);

		if (item != null)
		{
			Renderer.DrawUITexture(x, y, width, height, item.icon);

			if (item.stackable)
			{
				string stackSizeText = stackSize.ToString();
				Renderer.DrawText(x + width - stackSizeFont.measureText(stackSizeText) - 8, y + height - (int)stackSizeFont.size, 1.0f, stackSizeText, stackSizeFont, 0xffaaaaaa);
			}
		}
	}

	void renderHandItem(int x, int y, int width, int height, int handID)
	{
		//Renderer.DrawUIRect(x, y, width, height, 0xff111111);

		ItemSlot slot = player.inventory.getSelectedHandSlot(handID);
		Item item = player.inventory.getSelectedHandItem(handID);

		renderItemSlot(x, y, width, height, item, slot != null ? slot.stackSize : 0);

		/*
			Texture icon = item.icon;
			Renderer.DrawUITexture(x, y, width, height, icon);
		*/

		if (item != null && item.category == ItemCategory.Weapon && item.weaponType == WeaponType.Bow)
		{
			int numArrows = player.inventory.totalArrowCount;

			int yy = Display.viewportSize.y - 25 - height - 25 - height;
			renderItemSlot(x, yy, width, height, numArrows > 0 ? player.inventory.arrows[0].item : null, numArrows);


			/*
			Renderer.DrawUIRect(x, yy, width, height, 0xff111111);

			if (numArrows > 0)
			{
				Texture arrowIcon = player.inventory.arrows[0].item.icon;
				Renderer.DrawUITexture(x, yy, width, height, arrowIcon);

				string stackSizeText = numArrows.ToString();
				Renderer.DrawText(x + width - stackSizeFont.measureText(stackSizeText) - 8, yy + height - (int)stackSizeFont.size, 1.0f, stackSizeText, stackSizeFont, 0xffaaaaaa);
			}
			*/
		}
	}

	void renderEquipment()
	{
		// Left item
		{
			int width = 64;
			int height = 64;
			int x = 25;
			int y = Display.viewportSize.y - 25 - height;

			renderHandItem(x, y, width, height, 1);
		}

		// Right item
		{
			int width = 64;
			int height = 64;
			int x = 25 + width + 10;
			int y = Display.viewportSize.y - 25 - height;

			renderHandItem(x, y, width, height, 0);
		}

		// Quick Slot
		{
			int width = 64;
			int height = 64;
			int x = 25 + width + 10 + width + 40;
			int y = Display.viewportSize.y - 25 - height;

			ItemSlot slot = player.inventory.getQuickSlot(0);
			renderItemSlot(x, y, width, height, slot != null ? slot.item : null, slot != null ? slot.stackSize : 0);

			/*
			Renderer.DrawUIRect(x, y, width, height, 0xff111111);

			if (player.inventory.getQuickSlot(0) != null)
			{
				Texture icon = player.inventory.getQuickSlot(0).item.icon;
				Renderer.DrawUITexture(x, y, width, height, icon);
			}
			*/
		}
	}

	void renderXP()
	{
		int x = Display.viewportSize.x - 220;
		int y = Display.viewportSize.y - 80;
		int width = 140;
		int height = 28;
		int padding = 2;

		Renderer.DrawUIRect(x - padding, y - padding, width + 2 * padding, height + 2 * padding, 0xff888888);
		Renderer.DrawUIRect(x, y, width, height, 0xff222222);

		string text = player.stats.xp.ToString();
		Renderer.DrawText(x + width - xpFont.measureText(text) - 3 * padding, y + (int)((height - xpFont.size) / 2.0f), 1.0f, text, xpFont, 0xffcccccc);
	}

	void renderCollectedItems()
	{
		float windowDuration = 4.0f;

		for (int i = 0; i < collectedItems.Count; i++)
		{
			ItemCollectedNotification notif = collectedItems[i];

			float elapsed = (Time.currentTime - notif.timeCollected) / 1e9f;
			if (elapsed >= windowDuration)
			{
				collectedItems.RemoveAt(i);
				i--;
			}
		}

		for (int i = 0; i < collectedItems.Count; i++)
		{
			ItemCollectedNotification notif = collectedItems[i];

			Item item = notif.item;

			int padding = 3;
			int iconSize = 48;

			int windowSpacing = 20;
			int width = 240;
			int height = iconSize + 2 * padding;
			int x = 10;
			int y = Display.viewportSize.y - 160 + (-collectedItems.Count + i) * (height + windowSpacing);

			Renderer.DrawUIRect(x, y, width, height, 0xff111111);
			{
				Renderer.DrawUITexture(x + padding, y + padding, iconSize, iconSize, item.icon);

				if (notif.amount > 1 || notif.item.stackable)
					Renderer.DrawText(x + padding + iconSize - padding * 2, y + padding + iconSize - (int)stackSizeFont.size, 1.0f, notif.amount.ToString(), stackSizeFont, 0xffaaaaaa);

				Renderer.DrawText(x + padding + iconSize + padding * 3, y + padding * 2, 1.0f, item.displayName, notificationFont, 0xffaaaaaa);
				Renderer.DrawText(x + padding + iconSize + padding * 3, y + padding + iconSize - padding - (int)notificationFont.size, 1.0f, item.typeSpecifier, notificationFont, 0xff777777);
			}
		}
	}

	public void draw(GraphicsDevice graphics)
	{
		//renderPrompt();
		//renderCrosshair();
		renderHealthBar();
		renderStaminaBar();
		renderManaBar();
		renderEquipment();
		renderXP();
		renderCollectedItems();
	}
}
