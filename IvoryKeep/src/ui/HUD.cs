using Rainfall;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;


struct HUDMessage
{
	public string msg;
	public long timeSent;
}

public class HUD
{
	const float MESSAGE_SHOW_DURATION = 10.0f;
	const float LEVEL_UP_PROMPT_DURATION = 3.0f;
	const float HITMARKER_DURATION = 0.2f;


	Player player;
	GraphicsDevice graphics;

	Texture crosshair;
	Texture crosshairHand;

	Texture armor, armorHalf;

	Texture hitmarker;

	Font stackSizeFont, xpFont, notificationFont, crosshairMessageFont;
	Font levelUpFont;

	long lastHit = 0;
	long lastLevelUp = -10000000000;
	long lastProjectileHit = -10000000000;

	public float vignetteFade = 1.0f;

	List<HUDMessage> messages = new List<HUDMessage>();


	public HUD(Player player)
	{
		this.player = player;
		this.graphics = Renderer.graphics;

		crosshair = Resource.GetTexture("res/texture/ui/crosshair.png");
		crosshairHand = Resource.GetTexture("res/texture/ui/crosshair_hand.png");
		armor = Resource.GetTexture("res/texture/ui/armor.png");
		armorHalf = Resource.GetTexture("res/texture/ui/armor_half.png");
		hitmarker = Resource.GetTexture("res/texture/ui/hitmarker.png");

		stackSizeFont = FontManager.GetFont("default", 20.0f, true);
		xpFont = FontManager.GetFont("default", 20.0f, true);
		notificationFont = FontManager.GetFont("default", 18.0f, true);
		crosshairMessageFont = FontManager.GetFont("default", 17, true);
		levelUpFont = FontManager.GetFont("default", 48, true);
	}

	public void onHit()
	{
		lastHit = Time.currentTime;
	}

	/*
	public void onItemCollected(Item item, int amount)
	{
		for (int i = 0; i < collectedItems.Count; i++)
		{
			ItemCollectedNotification notif = collectedItems[i];
			if (notif.item == item)
			{
				notif.amount += amount;
				notif.timeCollected = Time.currentTime;
				collectedItems[i] = notif;
				return;
			}
		}

		collectedItems.Add(new ItemCollectedNotification() { item = item, amount = amount, timeCollected = Time.currentTime });
	}
	*/

	public void showMessage(string msg)
	{
		messages.Add(new HUDMessage { msg = msg, timeSent = Time.currentTime });
	}

	public void onLevelUp()
	{
		lastLevelUp = Time.currentTime;
	}

	public void onProjectileHit()
	{
		lastProjectileHit = Time.currentTime;
	}

	void renderCrosshair()
	{
		/*
		if (player.interactableInFocus != null)
		{
			int width = crosshairHand.width / 2;
			int height = crosshairHand.height / 2;
			GUI.Texture(Display.viewportSize.x / 2 - width / 2, Display.viewportSize.y / 2 - height / 2, width, height, crosshairHand);
		}
		else
		{
			int width = crosshair.width;
			int height = crosshair.height;
			GUI.Texture(Display.viewportSize.x / 2 - width / 2, Display.viewportSize.y / 2 - height / 2, width, height, crosshair);
		}
		*/
	}

	void renderHealthBar()
	{
		int x = 32;
		int y = 32;
		int width = (int)(player.stats.getMaxHealth() * 10);
		int height = 16;
		int padding = 2;

		GUI.Rect(x - padding, y - padding, width + 2 * padding, height + 2 * padding, 0xFF444444);
		//Renderer.DrawUIRect(x - padding / 2, y - padding / 2, width + padding, height + padding, 0xff222222);

		GUI.Rect(x, y, width, height, 0xff331111);
		GUI.Rect(x, y, (int)(player.stats.health / (float)player.stats.getMaxHealth() * width), height, 0xffE04531);
	}

	void renderStaminaBar()
	{
		int x = 32;
		int y = 62;
		int width = (int)(player.stats.getMaxStamina() * 10);
		int height = 8;
		int padding = 2;

		GUI.Rect(x - padding, y - padding, width + 2 * padding, height + 2 * padding, 0xFF444444);
		//Renderer.DrawUIRect(x - padding / 2, y - padding / 2, width + padding, height + padding, 0xff222222);

		GUI.Rect(x, y, width, height, 0xff1c241d);
		GUI.Rect(x, y, (int)(player.stats.stamina / (float)player.stats.getMaxStamina() * width), height, 0xff478749);
	}

	void renderManaBar()
	{
		int x = 32;
		int y = 80;
		int width = (int)(player.stats.getMaxMana() * 10);
		int height = 8;
		int padding = 2;

		GUI.Rect(x - padding, y - padding, width + 2 * padding, height + 2 * padding, 0xFF444444);
		//Renderer.DrawUIRect(x - padding / 2, y - padding / 2, width + padding, height + padding, 0xff222222);

		GUI.Rect(x, y, width, height, 0xff1C1D24);
		GUI.Rect(x, y, (int)(player.stats.mana / (float)player.stats.getMaxMana() * width), height, 0xff7780f7);
	}

	void renderProtection()
	{
		int x = 32;
		int y = 98;
		int iconSize = 24;
		int padding = 3;
		int protection = (int)MathF.Ceiling((1 - player.inventory.getArmorProtection()) * 20);
		for (int i = 0; i < protection / 2; i++)
		{
			GUI.Texture(x + i * (iconSize + padding), y, iconSize, iconSize, armor);
		}
		if (protection % 2 == 1)
		{
			GUI.Texture(x + protection / 2 * (iconSize + padding), y, iconSize, iconSize, armorHalf);
		}
	}

	void renderItemSlot(int x, int y, int width, int height, Item item, int stackSize)
	{
		GUI.Rect(x, y, width, height, 0xff111111);

		if (item != null)
		{
			GUI.Texture(x, y, width, height, item.icon);

			if (item.stackable)
			{
				string stackSizeText = stackSize.ToString();
				GUI.Text(x + width - stackSizeFont.measureText(stackSizeText) - 8, y + height - (int)stackSizeFont.size, 1.0f, stackSizeText, stackSizeFont, 0xffaaaaaa);
			}
		}
	}

	void renderHandItem(int x, int y, int width, int height, int handID)
	{
		ItemSlot slot = player.inventory.getSelectedHandSlot(handID);
		Item item = player.inventory.getSelectedHandItem(handID);

		renderItemSlot(x, y, width, height, item, slot != null ? slot.stackSize : 0);

		if (item != null)
		{
			if (item.category == ItemCategory.Weapon && item.weaponType == WeaponType.Bow)
			{
				int numArrows = player.inventory.arrows.stackSize;

				int ww = 64;
				int hh = 64;

				int yy = Display.viewportSize.y - 25 - height - 25 - hh;
				renderItemSlot(x, yy, ww, hh, numArrows > 0 ? player.inventory.arrows.item : null, numArrows);
			}
			else if (item.category == ItemCategory.Weapon && item.weaponType == WeaponType.Staff)
			{
				ItemSlot spellItem = player.inventory.getCurrentSpellSlot();

				int ww = 64;
				int hh = 64;

				int yy = Display.viewportSize.y - 25 - height - 25 - hh;
				renderItemSlot(x, yy, ww, hh, spellItem?.item, 1);
			}
		}
	}

	void renderEquipment()
	{
		// Left item
		{
			int width = 96;
			int height = 96;
			int x = Display.width - 40 - width - 10 - width;
			int y = Display.viewportSize.y - 40 - height;

			renderHandItem(x, y, width, height, 1);
		}

		// Right item
		{
			int width = 96;
			int height = 96;
			int x = Display.width - 40 - width;
			int y = Display.viewportSize.y - 40 - height;

			renderHandItem(x, y, width, height, 0);
		}

		// Quick Slot
		{
			int width = 64;
			int height = 64;
			int x = 40;
			int y = Display.viewportSize.y - 40 - height;

			ItemSlot slot = player.inventory.getCurrentHotbarSlot();
			if (slot != null)
				renderItemSlot(x, y, width, height, slot.item, slot.stackSize);
			else
				renderItemSlot(x, y, width, height, slot.item, 0);
		}
	}

	void renderXP()
	{
		int x = Display.viewportSize.x - 220;
		int y = Display.viewportSize.y - 80;
		int width = 140;
		int height = 28;
		int padding = 2;

		GUI.Rect(x - padding, y - padding, width + 2 * padding, height + 2 * padding, 0xff888888);
		GUI.Rect(x, y, width, height, 0xff222222);

		string text = player.stats.xp.ToString();
		GUI.Text(x + width - xpFont.measureText(text) - 3 * padding, y + (int)((height - xpFont.size) / 2.0f), 1.0f, text, xpFont, 0xffcccccc);
	}

	void renderMessages()
	{
		for (int i = 0; i < messages.Count; i++)
		{
			HUDMessage notif = messages[i];

			float elapsed = (Time.currentTime - notif.timeSent) / 1e9f;
			if (elapsed >= MESSAGE_SHOW_DURATION)
			{
				messages.RemoveAt(i);
				i--;
			}
		}

		for (int i = 0; i < messages.Count; i++)
		{
			HUDMessage notif = messages[i];

			string msg = notif.msg;

			int lineSpacing = 5;

			int height = (int)notificationFont.size + lineSpacing;
			int x = 10;
			int y = Display.viewportSize.y - 160 + (-messages.Count + i) * height;

			float elapsed = (Time.currentTime - notif.timeSent) / 1e9f;
			float alpha = elapsed < MESSAGE_SHOW_DURATION - 1 ? 1 : MathHelper.Lerp(1, 0, (elapsed - MESSAGE_SHOW_DURATION + 1) / 1);
			uint color = MathHelper.ColorAlpha(0xFFAAAAAA, alpha);

			GUI.Text(x, y, 1, msg, notificationFont, color);
		}
	}

	void renderLevelUp()
	{
		if ((Time.currentTime - lastLevelUp) / 1e9f < LEVEL_UP_PROMPT_DURATION)
		{
			string text = "Level Up";
			int width = levelUpFont.measureText(text);
			float elapsed = (Time.currentTime - lastLevelUp) / 1e9f;
			float progress = elapsed / LEVEL_UP_PROMPT_DURATION;
			float yanim = MathHelper.Lerp(0, -Display.height / 8, progress);
			float alpha = elapsed < 1 ? elapsed : elapsed > LEVEL_UP_PROMPT_DURATION - 1 ? (1 - (elapsed - (LEVEL_UP_PROMPT_DURATION - 1))) : 1;
			uint color = MathHelper.ColorAlpha(0xFFAAAAAA, alpha);
			GUI.Text(Display.width / 2 - width / 2, Display.height / 4 + (int)yanim, 1, text, levelUpFont, color);
		}
	}

	void renderHitmarker()
	{
		if ((Time.currentTime - lastProjectileHit) / 1e9f < HITMARKER_DURATION)
		{
			int size = hitmarker.height / 2;
			int x = Display.width / 2 - size / 2;
			int y = Display.height / 2 - size / 2;
			float elapsed = (Time.currentTime - lastProjectileHit) / 1e9f;
			float progress = elapsed / HITMARKER_DURATION;
			int numFrames = hitmarker.width / hitmarker.height;
			int frame = (int)(progress * numFrames);
			int u = frame * hitmarker.height;
			int v = 0;
			GUI.Texture(x, y, size, size, hitmarker, u, v, u + hitmarker.height, v + hitmarker.height, 0xFFFFFFFF);
		}
	}

	/*
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

			GUI.Rect(x, y, width, height, 0xff111111);
			{
				GUI.Texture(x + padding, y + padding, iconSize, iconSize, item.icon);

				if (notif.amount > 1 || notif.item.stackable)
					GUI.Text(x + padding + iconSize - padding * 3, y + padding + iconSize - (int)stackSizeFont.size, 1.0f, notif.amount.ToString(), stackSizeFont, 0xffaaaaaa);

				GUI.Text(x + padding + iconSize + padding * 5, y + padding * 2, 1.0f, item.displayName, notificationFont, 0xffaaaaaa);
				GUI.Text(x + padding + iconSize + padding * 5, y + padding + iconSize - padding - (int)notificationFont.size, 1.0f, item.typeSpecifier, notificationFont, 0xff777777);
			}
		}
	}
	*/

	public void draw()
	{
		//renderPrompt();
		renderCrosshair();
		renderHealthBar();
		renderStaminaBar();
		renderManaBar();
		renderProtection();
		renderEquipment();
		//renderXP();
		//renderCollectedItems();
		renderMessages();
		renderLevelUp();
		renderHitmarker();

		float timeSinceHit = (Time.currentTime - lastHit) / 1e9f;
		float vignetteHitEffect = MathF.Exp(-timeSinceHit * 6.0f);
		Renderer.vignetteColor = Vector3.Lerp(Vector3.Zero, new Vector3(1.0f, 0.0f, 0.0f), vignetteHitEffect);

		Renderer.vignetteFalloff = MathHelper.Lerp(50.0f, 0.37f, MathF.Pow(vignetteFade, 0.5f));
	}
}
