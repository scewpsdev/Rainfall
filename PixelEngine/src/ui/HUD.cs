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


	public static SpriteSheet tileset;

	public static Sprite heartFull, heartHalf, heartEmpty;
	public static Sprite armor, armorEmpty;
	public static Sprite gem;

	static HUD()
	{
		tileset = new SpriteSheet(Resource.GetTexture("res/sprites/ui.png", false), 8, 8);

		heartFull = new Sprite(tileset, 0, 0);
		heartHalf = new Sprite(tileset, 1, 0);
		heartEmpty = new Sprite(tileset, 2, 0);

		armor = new Sprite(tileset, 4, 0);
		armorEmpty = new Sprite(tileset, 5, 0);

		gem = new Sprite(tileset, 3, 0);
	}


	Player player;

	List<HUDMessage> messages = new List<HUDMessage>();

	long lastLevelSwitch = -1;
	string levelName;


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
			int y = Renderer.UIHeight - 30 + (-messages.Count + i) * height;

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
			return;

		// Health
		for (int i = 0; i < player.maxHealth; i++)
		{
			int size = 8;
			int padding = 3;
			int x = 6 + i * (size + padding);
			int y = 6;

			Renderer.DrawUISprite(x, y, size, size, heartEmpty);
			if (i < player.health)
			{
				float fraction = MathF.Min(player.health - i, 1);
				fraction = MathF.Floor(fraction * 7) / 8.0f + 0.125f;
				//Renderer.DrawUISprite(x, y, size, size, heartFull);
				Renderer.DrawUISprite(x, y + (int)((1 - fraction) * size), size, (int)(fraction * size), heartFull.spriteSheet.texture, heartFull.position.x, heartFull.position.y + (int)(heartFull.size.y * (1 - fraction)), heartFull.size.x, (int)(heartFull.size.y * fraction));
			}
			//else if (i == player.health / 2 && player.health % 2 == 1)
			//	Renderer.DrawUISprite(x, y, size, size, heartHalf);
			else
				Renderer.DrawUISprite(x, y, size, size, heartEmpty);
		}

		// Armor
		int totalArmor = player.getTotalArmor();
		for (int i = 0; i < (int)MathF.Ceiling(totalArmor / 10.0f); i++)
		{
			int size = 8;
			int padding = 3;
			int x = 6 + player.maxHealth * (size + padding) + 4 + i * (size + padding);
			int y = 6;

			Renderer.DrawUISprite(x, y, size, size, armorEmpty);
			float fraction = MathF.Min(totalArmor / 10.0f - i, 1);
			fraction = MathF.Floor(fraction * 7) / 8.0f + 0.125f;
			Renderer.DrawUISprite(x, y + (int)((1 - fraction) * size), size, (int)(fraction * size), armor.spriteSheet.texture, armor.position.x, armor.position.y + (int)(armor.size.y * (1 - fraction)), armor.size.x, (int)(armor.size.y * fraction));
		}

		{ // Gems
			int size = 8;
			int x = 6;
			int y = 6 + 8 + 3;

			Renderer.DrawUISprite(x, y, size, size, gem, false);
			Renderer.DrawUITextBMP(x + size + 3, y, player.money.ToString(), 1);
		}

		{ // Hand item
			int size = 16;
			int x = 12;
			int y = Renderer.UIHeight - 12 - size;

			Renderer.DrawUISprite(x, y, size, size, null, 0, 0, 0, 0, 0xFF111111);
			if (player.handItem != null)
				Renderer.DrawUISprite(x, y, size, size, player.handItem.sprite);
		}

		{ // Quick item
			int size = 16;
			int x = 16 + 16 + 4;
			int y = Renderer.UIHeight - 12 - size;

			Renderer.DrawUISprite(x, y, size, size, null, 0, 0, 0, 0, 0xFF111111);
			if (player.quickItems[player.currentQuickItem] != null)
			{
				Renderer.DrawUISprite(x, y, size, size, player.quickItems[player.currentQuickItem].sprite);
				if (player.quickItems[player.currentQuickItem].stackable && player.quickItems[player.currentQuickItem].stackSize > 1)
					Renderer.DrawUITextBMP(x + size - size / 4, y + size - Renderer.smallFont.size + 2, player.quickItems[player.currentQuickItem].stackSize.ToString(), 1, 0xFFBBBBBB);
			}
		}

		renderMessages();
		renderPopup();
	}
}
