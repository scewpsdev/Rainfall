using Rainfall;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


struct HUDMessage
{
	public string msg;
	public long timeSent;
	public int count;
}

public class HUD
{
	const float MESSAGE_SHOW_DURATION = 5.0f;
	const float POPUP_DURATION = 4.5f;
	const float ITEM_NAME_DURATION = 3.0f;


	public static SpriteSheet tileset;

	public static Sprite heartFull, heartEmpty, heartHalf, heartHalfEmpty;
	public static Sprite armor, armorEmpty;
	public static Sprite mana, manaEmpty;
	public static Sprite gold;
	public static Sprite staffCharge;

	public static Sprite crosshair;
	public static Sprite aimIndicator;

	static Sprite[] cooldownOverlay;

	static Sprite[] xpChargeLevels;

	static HUD()
	{
		tileset = new SpriteSheet(Resource.GetTexture("sprites/ui.png", false), 8, 8);

		heartFull = new Sprite(tileset, 0, 0);
		heartEmpty = new Sprite(tileset, 1, 0);
		heartHalf = new Sprite(tileset, 0, 1);
		heartHalfEmpty = new Sprite(tileset, 1, 1);

		mana = new Sprite(tileset, 6, 0);
		manaEmpty = new Sprite(tileset, 7, 0);

		armor = new Sprite(tileset, 4, 0);
		armorEmpty = new Sprite(tileset, 5, 0);

		gold = new Sprite(tileset, 3, 1);

		staffCharge = new Sprite(tileset, 6, 1);

		crosshair = new Sprite(tileset, 2, 4, 1, 1);
		aimIndicator = new Sprite(tileset, 3, 4, 2, 2);

		cooldownOverlay = new Sprite[17];
		for (int i = 0; i < 16; i++)
			cooldownOverlay[i] = new Sprite(tileset, i % 4 * 2, 8 + i / 4 * 2, 2, 2);
		cooldownOverlay[16] = null;

		xpChargeLevels = new Sprite[16];
		for (int i = 0; i < 16; i++)
			xpChargeLevels[i] = new Sprite(tileset, i % 4 * 2, 16 + i / 4 * 2, 2, 2);
	}


	const uint frameColor = 0xFF444444;
	const uint frameSelectedColor = 0xFF777777;
	const uint bgColor = 0xFF111111;
	const uint bgSelectedColor = 0xFF222222;
	const uint txtColor = 0xFFBBBBBB;


	public bool enabled = true;

	public float screenFade = 1.0f;

	Player player;

	List<HUDMessage> messages = new List<HUDMessage>();

	long lastLevelSwitch = -1;
	string levelName;

	long lastItemSwitch = -1;
	long lastSpellSwitch = -1;


	public HUD(Player player)
	{
		this.player = player;
	}

	public void showMessage(string msg)
	{
		if (messages.Count > 0 && messages[messages.Count - 1].msg == msg)
		{
			HUDMessage hmsg = messages[messages.Count - 1];
			hmsg.count++;
			hmsg.timeSent = Time.currentTime;
			messages[messages.Count - 1] = hmsg;
		}
		else
		{
			messages.Add(new HUDMessage { msg = msg, timeSent = Time.currentTime, count = 1 });
		}
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

	public void onSpellSwitch()
	{
		lastSpellSwitch = Time.currentTime;
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

			string msg = notif.msg + (notif.count > 1 ? " x " + notif.count : "");

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
		string[] text = null;
		float elapsed = 0;

		if (GameState.instance.currentBoss != null)
		{
			if (GameState.instance.currentBoss.nameSubtitle != null)
				text = [GameState.instance.currentBoss.displayName, GameState.instance.currentBoss.nameSubtitle];
			else
				text = [GameState.instance.currentBoss.displayName];
			elapsed = (Time.currentTime - GameState.instance.bossFightStarted) / 1e9f;
		}
		else if (lastLevelSwitch != -1 && levelName != null)
		{
			text = [levelName];
			elapsed = (Time.currentTime - lastLevelSwitch) / 1e9f;
		}

		if (text != null && elapsed < POPUP_DURATION)
		{
			for (int i = 0; i < text.Length; i++)
			{
				Vector2i size = Renderer.MeasureUIText(text[i]);
				float progress = elapsed / POPUP_DURATION;
				float yanim = MathHelper.Lerp(0, -Renderer.UIHeight / 8, progress);
				yanim = 0;
				float alpha = elapsed < 1 ? elapsed : elapsed > POPUP_DURATION - 2 ? (1 - 0.5f * (elapsed - (POPUP_DURATION - 2))) : 1;
				uint color = MathHelper.ColorAlpha(0xFFAAAAAA, alpha);
				Renderer.DrawUIText(Renderer.UIWidth / 2 - size.x / 2, Renderer.UIHeight / 4 + (-text.Length + i) * (size.y + 1) + (int)yanim + 1, text[i], 1, MathHelper.ColorAlpha(0xFF000000, alpha));
				Renderer.DrawUIText(Renderer.UIWidth / 2 - size.x / 2, Renderer.UIHeight / 4 + (-text.Length + i) * (size.y + 1) + (int)yanim, text[i], 1, color);
			}
		}
	}

	void renderStatusBar(int x, int y, int progress, int size, uint color)
	{
		int thickness = 2;

		Renderer.DrawUISprite(x, y, size + 1, thickness, null, false, 0xFF555555);
		Renderer.DrawUISprite(x, y, size, thickness, null, false, 0xFF222222);
		Renderer.DrawUISprite(x - 1, y, progress + 2, thickness, null, false, UIColors.WINDOW_FRAME);
		Renderer.DrawUISprite(x, y, progress, thickness, null, false, color);
	}

	void renderHealth()
	{
		int x = 28;
		int y = 12;
		int width = 10;

		renderStatusBar(x, y, (int)(player.health * width), (int)(player.maxHealth * width), UIColors.TEXT_HEALTH);
	}

	void renderMana()
	{
		int x = 28;
		int y = 18;
		int width = 10;

		renderStatusBar(x, y, (int)(player.mana * width), (int)(player.maxMana * width), UIColors.TEXT_MANA);
	}

	void renderXP()
	{
		int x = 8;
		int y = 8;
		int size = 16;

		float progress = player.xp / (float)player.nextLevelXP;
		int spriteIdx = (int)MathF.Round(progress * 15);
		Sprite sprite = xpChargeLevels[spriteIdx];

		Renderer.DrawUISprite(x, y, size, size, sprite, false);

		string lvlTxt = player.playerLevel.ToString();
		Vector2i txtSize = Renderer.MeasureUITextBMP(lvlTxt);
		Renderer.DrawUITextBMP(x + size / 2 - txtSize.x / 2, y + size / 2 - txtSize.y / 2 + 1, lvlTxt, 1, 0xFF8cb877);
	}

	void renderMoney()
	{
		int size = 8;
		int x = 12;
		int y = 28;

		Renderer.DrawUISprite(x, y, size, size, gold, false);

		string moneyStr = player.money.ToString();
		uint moneyColor = 0xFF926c5c; // 0xFFd2b459;
		Renderer.DrawUITextBMP(x + size + 2, y, moneyStr, 1, moneyColor);
	}

	void renderArmor()
	{
		int totalArmor = (int)MathF.Round(player.getTotalArmor());
		if (totalArmor > 0)
		{
			int size = 8;
			int x = 12 + 20;
			int y = 28;

			Renderer.DrawUISprite(x, y, size, size, armor, false);

			string armorStr = totalArmor.ToString();
			Renderer.DrawUITextBMP(x + size + 3, y, armorStr, 1, 0xFF5481da);
		}
	}

	void renderStatusEffects()
	{
		int size = 8;
		int x = Renderer.UIWidth / 2 + 8;
		int y = Renderer.UIHeight - 4 - 16 - 12 - 12;

		void renderIcon(Sprite sprite, uint spriteColor, bool positive, float progress = -1)
		{
			uint color = positive ? 0xFF777777 : 0xFF886666;

			Renderer.DrawUISprite(x, y, size + 2, size + 2, null, false, color);
			Renderer.DrawUISprite(x + 1, y + 1, size, size, null, false, 0xFF222222);
			Renderer.DrawUISprite(x + 1, y + 1, size, size, 0, sprite, spriteColor);

			if (progress != -1)
			{
				int overlayIdx = Math.Clamp(16 - (int)(progress * 16), 0, 16);
				Renderer.DrawUISprite(x, y, size + 2, size + 2, cooldownOverlay[overlayIdx], true, 0xAF000000);
			}

			x += sprite.width + 3;
		}

		for (int i = 0; i < player.itemBuffs.Count; i++)
		{
			ItemBuff modifier = player.itemBuffs[i];
			if (modifier.active)
			{
				if (modifier.movementSpeedModifier != 1)
					renderIcon(ItemBuff.movementSpeedModifierIcon, 0xFFFFFFFF, modifier.movementSpeedModifier > 1);
				if (modifier.wallControlModifier != 1)
					renderIcon(ItemBuff.movementSpeedModifierIcon, 0xFFFFFFFF, modifier.movementSpeedModifier > 1);
				if (modifier.meleeDamageModifier != 1)
					renderIcon(ItemBuff.attackDamageModifierIcon, 0xFFFFFFFF, modifier.meleeDamageModifier > 1);
				if (modifier.rangedDamageModifier != 1)
					renderIcon(ItemBuff.attackDamageModifierIcon, 0xFFFFFFFF, modifier.meleeDamageModifier > 1);
				if (modifier.magicDamageModifier != 1)
					renderIcon(ItemBuff.attackDamageModifierIcon, 0xFFFFFFFF, modifier.meleeDamageModifier > 1);
				if (modifier.attackSpeedModifier != 1)
					renderIcon(ItemBuff.attackSpeedModifierIcon, 0xFFFFFFFF, modifier.attackSpeedModifier > 1);
				if (modifier.manaCostModifier != 1)
					renderIcon(ItemBuff.manaCostModifierIcon, 0xFFFFFFFF, modifier.manaCostModifier < 1);
				if (modifier.manaRecoveryModifier != 1)
					renderIcon(ItemBuff.manaCostModifierIcon, 0xFFFFFFFF, modifier.manaCostModifier < 1);
				if (modifier.stealthAttackModifier != 1)
					renderIcon(ItemBuff.stealthAttackModifierIcon, 0xFFFFFFFF, modifier.stealthAttackModifier > 1);
				if (modifier.defenseModifier != 1)
					renderIcon(ItemBuff.defenseModifierIcon, 0xFFFFFFFF, modifier.defenseModifier > 1);
				if (modifier.accuracyModifier != 1)
					renderIcon(ItemBuff.accuracyModifierIcon, 0xFFFFFFFF, modifier.accuracyModifier > 1);
				if (modifier.criticalAttackModifier != 1)
					renderIcon(ItemBuff.criticalAttackModifierIcon, 0xFFFFFFFF, modifier.criticalAttackModifier > 1);
				if (modifier.criticalChanceModifier != 1)
					renderIcon(ItemBuff.criticalAttackModifierIcon, 0xFFFFFFFF, modifier.criticalAttackModifier > 1);
			}
		}

		for (int i = 0; i < player.statusEffects.Count; i++)
		{
			StatusEffect effect = player.statusEffects[i];
			renderIcon(effect.icon, effect.iconColor, effect.positiveEffect, effect.getProgress());
		}
	}

	void renderItemSlot(int x, int y, Item item)
	{
		Renderer.DrawUISprite(x - 6, y - 5, 12, 10, null, false, bgColor);
		Renderer.DrawUISprite(x - 5, y - 6, 10, 12, null, false, bgColor);

		if (item != null)
		{
			if (item.spellIcon != null)
			{
				Renderer.DrawUIOutline(x - 8, y - 8, 16, 16, item.spellIcon, false, 0xFF000000);
				Renderer.DrawUISprite(x - 8, y - 8, 16, 16, item.spellIcon);
			}
			else
			{
				Renderer.DrawUIOutline(x - 8, y - 8, 16, 16, item.icon, false, 0xFF000000);
				Renderer.DrawUISprite(x - 8, y - 8, 16, 16, item.icon);
			}

			if (item.stackable && item.stackSize > 1)
				Renderer.DrawUITextBMP(x - 8 + 12, y - 8 + 16 - Renderer.smallFont.size + 2, item.stackSize.ToString(), 1, txtColor);
		}
	}

	void renderItems()
	{
		Vector2i crossCenter = new Vector2i(30, Renderer.UIHeight - 30);
		int crossDistance = 12;

		{
			float elapsed = (Time.currentTime - lastItemSwitch) / 1e9f;
			if (elapsed < ITEM_NAME_DURATION)
			{
				int leftItem = player.getPreviousActiveItem();
				int rightItem = player.getNextActiveItem();

				float alpha = elapsed < ITEM_NAME_DURATION - 1 ? 1 : MathHelper.Lerp(1, 0, (elapsed - ITEM_NAME_DURATION + 1) / 1);
				if (leftItem != -1)
					Renderer.DrawUISpriteSolid(crossCenter.x - 10 - 8, crossCenter.y + crossDistance - 8, 16, 16, player.activeItems[leftItem].spellIcon != null ? player.activeItems[leftItem].spellIcon : player.activeItems[leftItem].icon, false, MathHelper.ColorAlpha(0x3FFFFFFF, alpha));
				if (rightItem != -1)
					Renderer.DrawUISpriteSolid(crossCenter.x + 10 - 8, crossCenter.y + crossDistance - 8, 16, 16, player.activeItems[leftItem].spellIcon != null ? player.activeItems[leftItem].spellIcon : player.activeItems[leftItem].icon, false, MathHelper.ColorAlpha(0x3FFFFFFF, alpha));
			}
		}

		if (player.getSelectedSpell() != null)
		{
			float elapsed = (Time.currentTime - lastSpellSwitch) / 1e9f;
			if (elapsed < ITEM_NAME_DURATION)
			{
				int leftItem = player.getPreviousSpellItem();
				int rightItem = player.getNextSpellItem();

				float alpha = elapsed < ITEM_NAME_DURATION - 1 ? 1 : MathHelper.Lerp(1, 0, (elapsed - ITEM_NAME_DURATION + 1) / 1);
				if (leftItem != -1)
					Renderer.DrawUISpriteSolid(crossCenter.x - 10 - 8, crossCenter.y - crossDistance - 8, 16, 16, player.spellItems[leftItem].spellIcon != null ? player.spellItems[leftItem].spellIcon : player.spellItems[leftItem].icon, false, MathHelper.ColorAlpha(0x3FFFFFFF, alpha));
				if (rightItem != -1)
					Renderer.DrawUISpriteSolid(crossCenter.x + 10 - 8, crossCenter.y - crossDistance - 8, 16, 16, player.spellItems[leftItem].spellIcon != null ? player.spellItems[leftItem].spellIcon : player.spellItems[leftItem].icon, false, MathHelper.ColorAlpha(0x3FFFFFFF, alpha));
			}
		}

		renderItemSlot(crossCenter.x + crossDistance, crossCenter.y, player.handItem);
		renderItemSlot(crossCenter.x - crossDistance, crossCenter.y, player.offhandItem);
		renderItemSlot(crossCenter.x, crossCenter.y + crossDistance, player.activeItems[player.selectedActiveItem]);
		renderItemSlot(crossCenter.x, crossCenter.y - crossDistance, player.getSelectedSpell());
	}

	public void render()
	{
		if (player.numOverlaysOpen > 0)
		{
			Input.cursorMode = CursorMode.Normal;
			return;
		}

		if (enabled)
		{
			renderHealth();
			renderMana();
			renderXP();
			renderMoney();
			renderArmor();
			renderStatusEffects();
			renderItems();

			renderMessages();
			renderPopup();

			// Aim Direction
			if (player.isAlive)
			{
				if (Settings.game.aimMode == AimMode.Simple)
				{
				}
				// Aim indicator
				else if (Settings.game.aimMode == AimMode.Directional)
				{
					Vector2 pos = GameState.instance.camera.worldToScreen(player.position + player.collider.center + player.lookDirection);
					Renderer.DrawUISprite(pos.x - aimIndicator.width / 2, pos.y - aimIndicator.height / 2, aimIndicator.width, aimIndicator.height, player.lookDirection.angle, aimIndicator);
				}
				// Crosshair
				else if (Settings.game.aimMode == AimMode.Crosshair)
				{
					Renderer.DrawUISprite(Renderer.cursorPosition.x - crosshair.width / 2, Renderer.cursorPosition.y - crosshair.height / 2, crosshair.width, crosshair.height, crosshair);
				}
			}
		}

		if (screenFade != 1)
		{
			Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, null, false, MathHelper.ColorAlpha(0xFF000000, 1 - screenFade));
		}
	}
}
