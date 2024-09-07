using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


struct HUDMessage
{
	public string msg;
	public long timeSent;
}

public class HUD
{
	const float MESSAGE_SHOW_DURATION = 5.0f;
	const float LEVEL_PROMPT_DURATION = 3.0f;
	const float ITEM_NAME_DURATION = 3.0f;


	public static SpriteSheet tileset;

	public static Sprite heartFull, heartHalf, heartEmpty;
	public static Sprite armor, armorEmpty;
	public static Sprite mana, manaEmpty;
	public static Sprite gem;

	public static Sprite crosshair;

	static HUD()
	{
		tileset = new SpriteSheet(Resource.GetTexture("res/sprites/ui.png", false), 8, 8);

		heartFull = new Sprite(tileset, 0, 0);
		heartHalf = new Sprite(tileset, 1, 0);
		heartEmpty = new Sprite(tileset, 2, 0);

		mana = new Sprite(tileset, 6, 0);
		manaEmpty = new Sprite(tileset, 7, 0);

		armor = new Sprite(tileset, 4, 0);
		armorEmpty = new Sprite(tileset, 5, 0);

		gem = new Sprite(tileset, 3, 0);

		crosshair = new Sprite(tileset, 0, 4, 2, 2);
	}


	Player player;

	List<HUDMessage> messages = new List<HUDMessage>();

	long lastLevelSwitch = -1;
	string levelName;

	long lastItemSwitch = -1;


	public HUD(Player player)
	{
		this.player = player;
	}

	public void showMessage(string msg)
	{
		messages.Add(new HUDMessage { msg = msg, timeSent = Time.currentTime });
	}

	public void onLevelSwitch(string name)
	{
		levelName = name;
		lastLevelSwitch = Time.currentTime;
	}

	public void onItemSwitch()
	{
		lastItemSwitch = Time.currentTime;
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

			int height = (int)Renderer.smallFont.size;
			int x = 12;
			int y = Renderer.UIHeight - 34 + (-messages.Count + i) * height;

			float elapsed = (Time.currentTime - notif.timeSent) / 1e9f;
			float alpha = elapsed < MESSAGE_SHOW_DURATION - 1 ? 1 : MathHelper.Lerp(1, 0, (elapsed - MESSAGE_SHOW_DURATION + 1) / 1);
			uint color = MathHelper.ColorAlpha(0xFFAAAAAA, alpha);

			Renderer.DrawUITextBMP(x, y, msg, 1, color);
		}
	}

	void renderPopup()
	{
		float elapsed = (Time.currentTime - lastLevelSwitch) / 1e9f;
		if (lastLevelSwitch != -1 && elapsed < LEVEL_PROMPT_DURATION)
		{
			int width = Renderer.MeasureUIText(levelName, levelName.Length, 1).x;
			float progress = elapsed / LEVEL_PROMPT_DURATION;
			float yanim = MathHelper.Lerp(0, -Renderer.UIHeight / 8, progress);
			float alpha = elapsed < 1 ? elapsed : elapsed > LEVEL_PROMPT_DURATION - 1 ? (1 - (elapsed - (LEVEL_PROMPT_DURATION - 1))) : 1;
			uint color = MathHelper.ColorAlpha(0xFFAAAAAA, alpha);
			Renderer.DrawUIText(Renderer.UIWidth / 2 - width / 2, Renderer.UIHeight / 4 + (int)yanim, levelName, 1, color);
		}
	}

	public void render()
	{
		if (player.numOverlaysOpen > 0)
		{
			Input.cursorMode = CursorMode.Normal;
			return;
		}

		// Health
		for (int i = 0; i < player.maxHealth; i++)
		{
			int size = 8;
			int padding = 3;
			int x = 6 + i * (size + padding);
			int y = 6;

			Renderer.DrawUIOutline(x, y, size, size, heartEmpty, false, 0x5F000000);
			Renderer.DrawUISprite(x, y, size, size, heartEmpty);
			if (i < player.health)
			{
				float fraction = MathF.Min(player.health - i, 1);
				fraction = MathF.Floor(fraction * 8) / 8.0f;
				Renderer.DrawUISprite(x, y + (int)((1 - fraction) * size), size, (int)(fraction * size), heartFull.spriteSheet.texture, heartFull.position.x, heartFull.position.y + (int)(heartFull.size.y * (1 - fraction)), heartFull.size.x, (int)(heartFull.size.y * fraction));
			}
			else
			{
				Renderer.DrawUISprite(x, y, size, size, heartEmpty);
			}
		}

		// Armor
		int totalArmor = player.getTotalArmor();
		for (int i = 0; i < (int)MathF.Ceiling(totalArmor / 4.0f); i++)
		{
			int size = 8;
			int padding = 3;
			int x = 6 + (int)MathF.Round(player.maxHealth) * (size + padding) + 4 + i * (size + padding);
			int y = 6;

			Renderer.DrawUIOutline(x, y, size, size, armorEmpty, false, 0x5F000000);
			Renderer.DrawUISprite(x, y, size, size, armorEmpty);
			float fraction = MathF.Min(totalArmor / 4.0f - i, 1);
			fraction = MathF.Floor(fraction * 7) / 8.0f + 0.125f;
			Renderer.DrawUISprite(x, y + (int)((1 - fraction) * size), size, (int)(fraction * size), armor.spriteSheet.texture, armor.position.x, armor.position.y + (int)(armor.size.y * (1 - fraction)), armor.size.x, (int)(armor.size.y * fraction));
		}

		// Mana
		for (int i = 0; i < player.maxMana; i++)
		{
			int size = 8;
			int padding = 3;
			int x = 6 + i * (size + padding);
			int y = 6 + 8 + 3;

			Renderer.DrawUIOutline(x, y, size, size, manaEmpty, false, 0x5F000000);
			Renderer.DrawUISprite(x, y, size, size, manaEmpty);
			if (i < player.mana)
			{
				float fraction = MathF.Min(player.mana - i, 1);
				fraction = MathF.Floor(fraction * 8) / 8.0f;
				Renderer.DrawUISprite(x, y + (int)((1 - fraction) * size), size, (int)(fraction * size), mana.spriteSheet.texture, mana.position.x, mana.position.y + (int)(heartFull.size.y * (1 - fraction)), heartFull.size.x, (int)(mana.size.y * fraction));
			}
			else
			{
				Renderer.DrawUISprite(x, y, size, size, manaEmpty);
			}
		}

		{ // Gems
			int size = 8;
			int x = 6;
			int y = 6 + 8 + 3 + 8 + 3;

			Renderer.DrawUIOutline(x, y, size, size, gem, false, 0x5F000000);
			Renderer.DrawUISprite(x, y, size, size, gem, false);
			Renderer.DrawUITextBMP(x + size + 3, y, player.money.ToString(), 1);
		}

		{ // Hand items
			int size = 16;
			int x = 12;
			int y = Renderer.UIHeight - 12 - size;
			int padding = 1;

			float alpha = player.position.y < GameState.instance.camera.bottom + 0.1f * GameState.instance.camera.height &&
				player.position.x < GameState.instance.camera.left + 0.2f * GameState.instance.camera.width ||
				Input.cursorPosition.x < 0.5f * Display.width && Input.cursorPosition.y > 0.9f * Display.height
				? 0.2f : 1.0f;
			uint frameColor = MathHelper.ColorAlpha(0xFF555555, alpha);
			uint bgColor = MathHelper.ColorAlpha(0xFF222222, alpha);

			Renderer.DrawUISprite(x - padding, y - padding, 2 * size + 3 * padding, size + 2 * padding, null, false, frameColor);

			Renderer.DrawUISprite(x, y, size, size, null, false, bgColor);
			if (player.offhandItem != null)
				Renderer.DrawUISprite(x, y, size, size, player.offhandItem.sprite);

			Renderer.DrawUISprite(x + size + padding, y, size, size, null, false, bgColor);
			if (player.handItem != null)
				Renderer.DrawUISprite(x + size + padding, y, size, size, player.handItem.sprite);
		}

		{ // Quick item
			int size = 16;
			int width = player.activeItems.Length * (size + 1) + 1;
			int height = size + 2;
			int x = 12 + 32 + 16;
			int y = Renderer.UIHeight - 12 - size;

			float alpha = player.position.y < GameState.instance.camera.bottom + 0.25f * GameState.instance.camera.height &&
				player.position.x < GameState.instance.camera.left + 0.5f * GameState.instance.camera.width ||
				Input.cursorPosition.x < 0.5f * Display.width && Input.cursorPosition.y > 0.75f * Display.height
				? 0.2f : 1.0f;
			uint frameColor = MathHelper.ColorAlpha(0xFF555555, alpha);
			uint bgColor = MathHelper.ColorAlpha(0xFF222222, alpha);
			uint selectionColor = MathHelper.ColorAlpha(0xFF777777, alpha);
			uint txtColor = MathHelper.ColorAlpha(0xFFBBBBBB, alpha);

			Renderer.DrawUISprite(x, y, width, height, null, false, frameColor);

			for (int i = 0; i < player.activeItems.Length; i++)
			{
				int xx = x + 1 + i * (size + 1);
				int yy = y + 1;

				Renderer.DrawUISprite(xx, yy, size, size, null, false, bgColor);
			}

			for (int i = 0; i < player.activeItems.Length; i++)
			{
				int xx = x + 1 + i * (size + 1);
				int yy = y + 1;

				if (player.activeItems[i] != null)
				{
					Renderer.DrawUISprite(xx, yy, size, size, player.activeItems[i].sprite);
				}

				if (player.selectedActiveItem == i)
				{
					Renderer.DrawUISprite(xx - 2, yy - 2, size + 4, 2, null, false, selectionColor);
					Renderer.DrawUISprite(xx - 2, yy + size, size + 4, 2, null, false, selectionColor);
					Renderer.DrawUISprite(xx - 2, yy, 2, size, null, false, selectionColor);
					Renderer.DrawUISprite(xx + size, yy, 2, size, null, false, selectionColor);
				}

				if (player.activeItems[i] != null)
				{
					if (player.activeItems[i].stackable && player.activeItems[i].stackSize > 1)
						Renderer.DrawUITextBMP(xx + size - size / 4, yy + size - Renderer.smallFont.size + 2, player.activeItems[i].stackSize.ToString(), 1, txtColor);
				}
			}

			if (player.activeItems[player.selectedActiveItem] != null)
			{
				float elapsed = (Time.currentTime - lastItemSwitch) / 1e9f;
				if (elapsed < ITEM_NAME_DURATION)
				{
					float txtAlpha = elapsed < ITEM_NAME_DURATION - 1 ? 1 : MathHelper.Lerp(1, 0, (elapsed - ITEM_NAME_DURATION + 1) / 1);
					uint color = MathHelper.ColorAlpha(txtColor, txtAlpha);
					string txt = player.activeItems[player.selectedActiveItem].displayName;
					Vector2i txtSize = Renderer.MeasureUITextBMP(txt);
					Renderer.DrawUITextBMP(x + width + 6, y + size / 2 - txtSize.y / 2, txt, 1, color);
				}
			}
		}

		renderMessages();
		renderPopup();

		// Crosshair
		{
			Renderer.DrawUISprite(Renderer.cursorPosition.x - crosshair.width / 2, Renderer.cursorPosition.y - crosshair.height / 2, crosshair.width, crosshair.height, crosshair);
			Input.cursorMode = CursorMode.Hidden;
		}
	}
}
