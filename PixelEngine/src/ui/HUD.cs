using Rainfall;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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
	const float LEVEL_PROMPT_DURATION = 4.5f;
	const float ITEM_NAME_DURATION = 3.0f;


	public static SpriteSheet tileset;

	public static Sprite heartFull, heartHalf, heartEmpty;
	public static Sprite armor, armorEmpty;
	public static Sprite mana, manaEmpty;
	public static Sprite gold;
	public static Sprite staffCharge;

	public static Sprite crosshair;
	public static Sprite aimIndicator;

	static Sprite[] cooldownOverlay;

	static HUD()
	{
		tileset = new SpriteSheet(Resource.GetTexture("res/sprites/ui.png", false), 8, 8);

		heartFull = new Sprite(tileset, 0, 1);
		heartHalf = new Sprite(tileset, 1, 0);
		heartEmpty = new Sprite(tileset, 1, 1);

		mana = new Sprite(tileset, 6, 0);
		manaEmpty = new Sprite(tileset, 7, 0);

		armor = new Sprite(tileset, 4, 0);
		armorEmpty = new Sprite(tileset, 5, 0);

		gold = new Sprite(tileset, 3, 0);

		staffCharge = new Sprite(tileset, 6, 1);

		crosshair = new Sprite(tileset, 2, 4, 1, 1);
		aimIndicator = new Sprite(tileset, 3, 4, 2, 2);

		cooldownOverlay = new Sprite[17];
		for (int i = 0; i < 17; i++)
			cooldownOverlay[i] = new Sprite(tileset, i % 4 * 2, 8 + i / 4 * 2, 2, 2);
	}


	const uint frameColor = 0xFF444444;
	const uint frameSelectedColor = 0xFF777777;
	const uint bgColor = 0xFF111111;
	const uint bgSelectedColor = 0xFF222222;
	const uint txtColor = 0xFFBBBBBB;


<<<<<<< HEAD
	public bool enabled = true;

=======
>>>>>>> d79438fbcb8d9274022f6ae886162149e731fa4a
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
		if (messages.Count > 0 && messages[messages.Count - 1].msg == msg)
		{
			HUDMessage hmsg = messages[messages.Count - 1];
			hmsg.count++;
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
		float elapsed = (Time.currentTime - lastLevelSwitch) / 1e9f;
		if (lastLevelSwitch != -1 && elapsed < LEVEL_PROMPT_DURATION && levelName != null)
		{
			int width = Renderer.MeasureUIText(levelName, levelName.Length, 1).x;
			float progress = elapsed / LEVEL_PROMPT_DURATION;
			float yanim = MathHelper.Lerp(0, -Renderer.UIHeight / 8, progress);
			float alpha = elapsed < 1 ? elapsed : elapsed > LEVEL_PROMPT_DURATION - 2 ? (1 - 0.5f * (elapsed - (LEVEL_PROMPT_DURATION - 2))) : 1;
			uint color = MathHelper.ColorAlpha(0xFFAAAAAA, alpha);
			Renderer.DrawUIText(Renderer.UIWidth / 2 - width / 2, Renderer.UIHeight / 4 + (int)yanim + 1, levelName, 1, MathHelper.ColorAlpha(0xFF000000, alpha));
			Renderer.DrawUIText(Renderer.UIWidth / 2 - width / 2, Renderer.UIHeight / 4 + (int)yanim, levelName, 1, color);
		}
	}

	void renderHealth()
	{
		for (int i = 0; i < player.maxHealth; i++)
		{
			int size = 8;
			int padding = 2;
			int x = Renderer.UIWidth / 2 - 8 - size - i * (size + padding);
			int y = Renderer.UIHeight - 4 - 16 - 7 - size;

			Renderer.DrawUIOutline(x, y, size, size, heartEmpty, false, 0xFF000000);
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
	}

	void ___renderHealth()
	{
		int width = (int)MathF.Round(player.maxHealth * 20);
		int height = 7;
		int x = Renderer.UIWidth / 2 - 8 - 2 - width;
		int y = Renderer.UIHeight - 4 - 16 - 7 - height;

		Renderer.DrawUISprite(x - 1, y, width + 2, height, null, false, frameColor);
		Renderer.DrawUISprite(x, y - 1, width, height + 2, null, false, frameColor);

		Renderer.DrawUISprite(x, y, width, height, null, false, bgColor);

		int barWidth = (int)MathF.Ceiling(player.health * 20);
		Renderer.DrawUISprite(x + (width - barWidth), y, barWidth, height, null, false, 0xFF841e1e);
		Renderer.DrawUISprite(x + (width - barWidth), y, barWidth, height / 2, null, false, 0xFFd84343);

		string countTxt = ((int)MathF.Floor(player.health * 10 + 0.001f)).ToString() + "/" + ((int)(player.maxHealth * 10)).ToString();
		Renderer.DrawUITextBMP(x + width / 2 - Renderer.MeasureUITextBMP(countTxt).x / 2, y, countTxt, 1, 0xFFAAAAAA);
	}

	void renderMana()
	{
		for (int i = 0; i < player.maxMana; i++)
		{
			int size = 8;
			int padding = 2;
			int x = Renderer.UIWidth / 2 + 8 + i * (size + padding);
			int y = Renderer.UIHeight - 4 - 16 - 7 - size;

			Renderer.DrawUIOutline(x, y, size, size, manaEmpty, false, 0xFF000000);
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
	}

	void ___renderMana()
	{
		int width = (int)MathF.Round(player.maxMana * 20);
		int height = 7;
		int x = Renderer.UIWidth / 2 + 8 + 1;
		int y = Renderer.UIHeight - 4 - 16 - 7 - height;

		Renderer.DrawUISprite(x - 1, y, width + 2, height, null, false, frameColor);
		Renderer.DrawUISprite(x, y - 1, width, height + 2, null, false, frameColor);

		Renderer.DrawUISprite(x, y, width, height, null, false, bgColor);

		int barWidth = (int)MathF.Ceiling(player.mana * 20);
		Renderer.DrawUISprite(x, y, barWidth, height, null, false, 0xFF4d4195);
		Renderer.DrawUISprite(x, y, barWidth, height / 2, null, false, 0xFF6555c8);

		string countTxt = ((int)MathF.Floor(player.mana * 10 + 0.001f)).ToString() + "/" + ((int)(player.maxMana * 10)).ToString();
		Renderer.DrawUITextBMP(x + width / 2 - Renderer.MeasureUITextBMP(countTxt).x / 2, y, countTxt, 1, 0xFFAAAAAA);
	}

	void renderXP()
	{
		int width = 80;
		int height = 2;
		int x = Renderer.UIWidth / 2 - width / 2;
		int y = Renderer.UIHeight - 4 - 16 - 4;

		float progress = player.xp / (float)player.nextLevelXP;

		Renderer.DrawUISprite(x, y, width, height, null, false, 0xFF222222);
		Renderer.DrawUISprite(x, y, (int)MathF.Round(width * progress), height, null, false, UIColors.TEXT_SPEED);

		string lvlStr = player.playerLevel.ToString();
		Renderer.DrawUITextBMP(Renderer.UIWidth / 2 - Renderer.MeasureUITextBMP(lvlStr).x / 2, y - 8, lvlStr, 1, UIColors.TEXT_SPEED);
	}

	void ___renderMoney()
	{
		int size = 8;
		int x = 6;
		int y = 6 + 8 + 3 + 8 + 3;

		Renderer.DrawUIOutline(x, y, size, size, gold, false, 0x5F000000);
		Renderer.DrawUISprite(x, y, size, size, gold, false);
		Renderer.DrawUITextBMP(x + size + 3, y, player.money.ToString(), 1);
	}

	void renderMoney()
	{
		int size = 8;
		int x = Renderer.UIWidth / 2 - 8 - 2 * (16 + 1) - 4 - 8;
		int y = Renderer.UIHeight - 4 - 16 + 5;

		Renderer.DrawUISprite(x, y, size, size, gold, false);

		string moneyStr = player.money.ToString();
		Renderer.DrawUITextBMP(x - 3 - Renderer.MeasureUITextBMP(moneyStr).x, y, moneyStr, 1, 0xFFd2b459);
	}

	void ___renderArmor()
	{
		int totalArmor = player.getTotalArmor();
		if (totalArmor > 0)
		{
			int size = 8;
			int x = 6;
			int y = 6 + 8 + 3 + 8 + 3 + 8 + 3;

			Renderer.DrawUIOutline(x, y, size, size, armor, false, 0x5F000000);
			Renderer.DrawUISprite(x, y, size, size, armor, false);
			Renderer.DrawUITextBMP(x + size + 3, y, totalArmor.ToString(), 1);
		}
	}

	void renderArmor()
	{
		int totalArmor = player.getTotalArmor();
		if (totalArmor > 0)
		{
			int size = 8;
			int x = Renderer.UIWidth / 2 - 8 - 8 - 1;
			int y = Renderer.UIHeight - 4 - 16 - 12 - 12;

			Renderer.DrawUISprite(x, y, size, size, armor, false);

			string armorStr = totalArmor.ToString();
			Renderer.DrawUITextBMP(x - 3 - Renderer.MeasureUITextBMP(armorStr).x, y, armorStr, 1, 0xFF5481da);
		}
	}

	void ___renderStatusEffects()
	{
		int totalArmor = player.getTotalArmor();

		int size = 8;
		int x = 6 + (totalArmor > 0 ? 8 + 3 + 8 + 3 : 0);
		int y = 6 + 8 + 3 + 8 + 3 + 8 + 3;

		for (int i = 0; i < player.statusEffects.Count; i++)
		{
			StatusEffect effect = player.statusEffects[i];
			uint color = effect.positiveEffect ? 0xFF777777 : 0xFF886666;

			Renderer.DrawUISprite(x, y, size + 2, size + 2, null, false, color);
			Renderer.DrawUISprite(x + 1, y + 1, size, size, null, false, 0xFF222222);
			Renderer.DrawUISprite(x + 1, y + 1, size, size, 0, effect.icon, effect.iconColor);

			x += effect.icon.width + 3;
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

			if (modifier.movementSpeedModifier != 1)
				renderIcon(ItemBuff.movementSpeedModifierIcon, 0xFFFFFFFF, modifier.movementSpeedModifier > 1);
			if (modifier.meleeDamageModifier != 1)
				renderIcon(ItemBuff.attackDamageModifierIcon, 0xFFFFFFFF, modifier.meleeDamageModifier > 1);
			if (modifier.attackSpeedModifier != 1)
				renderIcon(ItemBuff.attackSpeedModifierIcon, 0xFFFFFFFF, modifier.attackSpeedModifier > 1);
			if (modifier.manaCostModifier != 1)
				renderIcon(ItemBuff.manaCostModifierIcon, 0xFFFFFFFF, modifier.manaCostModifier < 1);
			if (modifier.stealthAttackModifier != 1)
				renderIcon(ItemBuff.stealthAttackModifierIcon, 0xFFFFFFFF, modifier.stealthAttackModifier > 1);
			if (modifier.defenseModifier != 1)
				renderIcon(ItemBuff.defenseModifierIcon, 0xFFFFFFFF, modifier.defenseModifier > 1);
			if (modifier.accuracyModifier != 1)
				renderIcon(ItemBuff.accuracyModifierIcon, 0xFFFFFFFF, modifier.accuracyModifier > 1);
			if (modifier.criticalAttackModifier != 1)
				renderIcon(ItemBuff.criticalAttackModifierIcon, 0xFFFFFFFF, modifier.criticalAttackModifier > 1);
		}

		for (int i = 0; i < player.statusEffects.Count; i++)
		{
			StatusEffect effect = player.statusEffects[i];
			renderIcon(effect.icon, effect.iconColor, effect.positiveEffect, effect.getProgress());
		}
	}

	void ___renderHandItems(bool flipItems)
	{
		int size = 16;
		int x = Renderer.UIWidth / 2 - 80; // 4;
		int y = flipItems ? 4 : Renderer.UIHeight - 4 - size;
		int padding = 1;

		float alpha = player.position.y < GameState.instance.camera.bottom + 0.1f * GameState.instance.camera.height &&
			player.position.x < GameState.instance.camera.left + 0.2f * GameState.instance.camera.width ||
			Input.cursorPosition.x < 0.45f * Display.width && Input.cursorPosition.y > 0.9f * Display.height
			? 0.2f : 1.0f;
		alpha = 1;
		uint frameColor = MathHelper.ColorAlpha(0xFF555555, alpha);
		uint bgColor = MathHelper.ColorAlpha(0xFF222222, alpha);
		uint txtColor = MathHelper.ColorAlpha(0xFFBBBBBB, alpha);

		Renderer.DrawUISprite(x - padding, y - padding, 2 * size + 3 * padding, size + 2 * padding, null, false, frameColor);

		Renderer.DrawUISprite(x, y, size, size, null, false, bgColor);
		if (player.offhandItem != null)
		{
			Renderer.DrawUISprite(x, y, size, size, player.offhandItem.sprite);

			if (player.offhandItem.stackable && player.offhandItem.stackSize > 1)
				Renderer.DrawUITextBMP(x + size + padding + size - size / 4, y + size - Renderer.smallFont.size + 2, player.offhandItem.stackSize.ToString(), 1, txtColor);

			if (player.offhandItem.requiredAmmo != null)
			{
				Renderer.DrawUISprite(x - 1, y + (flipItems ? -1 : 1) * (-padding - size) - 1, size + 2, size + 2, null, false, frameColor);
				Renderer.DrawUISprite(x, y + (flipItems ? -1 : 1) * (-padding - size), size, size, null, false, bgColor);

				Item ammo = player.getItem(player.offhandItem.requiredAmmo);
				if (ammo != null)
				{
					Renderer.DrawUISprite(x, y + (flipItems ? -1 : 1) * (-padding - size), size, size, ammo.sprite);
					if (ammo.stackable && ammo.stackSize > 1)
						Renderer.DrawUITextBMP(x + size - size / 4, y + (flipItems ? -1 : 1) * (-padding - size) + size - Renderer.smallFont.size + 2, ammo.stackSize.ToString(), 1, txtColor);
				}
			}
			else if (player.offhandItem.type == ItemType.Staff && player.offhandItem.staffCharges > 0)
			{
				Renderer.DrawUISprite(x - 1, y + (flipItems ? -1 : 1) * (-padding - size) + 4, 8, 8, staffCharge);
				Renderer.DrawUITextBMP(x + 8, y + (flipItems ? -1 : 1) * (-padding - size) + size / 2 - Renderer.smallFont.size / 2, player.offhandItem.staffCharges.ToString(), 1, txtColor);
			}
		}

		Renderer.DrawUISprite(x + size + padding, y, size, size, null, false, bgColor);
		if (player.handItem != null)
		{
			Renderer.DrawUISprite(x + size + padding, y, size, size, player.handItem.getIcon());
			if (player.handItem.stackable && player.handItem.stackSize > 1)
				Renderer.DrawUITextBMP(x + size + padding + size - size / 4, y + size - Renderer.smallFont.size + 2, player.handItem.stackSize.ToString(), 1, txtColor);

			if (player.handItem.requiredAmmo != null)
			{
				Renderer.DrawUISprite(x + size + padding - 1, y + (flipItems ? -1 : 1) * (-padding - size) - 1, size + 2, size + 2, null, false, frameColor);
				Renderer.DrawUISprite(x + size + padding, y + (flipItems ? -1 : 1) * (-padding - size), size, size, null, false, bgColor);

				Item ammo = player.getItem(player.handItem.requiredAmmo);
				if (ammo != null)
				{
					Renderer.DrawUISprite(x + size + padding, y + (flipItems ? -1 : 1) * (-padding - size), size, size, ammo.sprite);
					if (ammo.stackable && ammo.stackSize > 1)
						Renderer.DrawUITextBMP(x + size + padding + size - size / 4, y + (flipItems ? -1 : 1) * (-padding - size) + size - Renderer.smallFont.size + 2, ammo.stackSize.ToString(), 1, txtColor);
				}
			}
			else if (player.handItem.type == ItemType.Staff && player.handItem.staffCharges > 0)
			{
				Renderer.DrawUISprite(x + size + padding - 1, y + (flipItems ? -1 : 1) * (-padding - size) + 4, 8, 8, staffCharge);
				Renderer.DrawUITextBMP(x + size + padding + 8, y + (flipItems ? -1 : 1) * (-padding - size) + size / 2 - Renderer.smallFont.size / 2, player.handItem.staffCharges.ToString(), 1, txtColor);
			}
		}
	}

	void renderHandItems()
	{
		int size = 16;
		int x = Renderer.UIWidth / 2 - 2 * (16 + 1) - 8; // 80; // 4;
		int y = Renderer.UIHeight - 4 - size;

		Renderer.DrawUISprite(x, y + 1, size, size - 2, null, false, frameColor);
		Renderer.DrawUISprite(x + 1, y, size - 2, size, null, false, frameColor);
		Renderer.DrawUISprite(x + 1, y + 1, size - 2, size - 2, null, false, bgColor);

		Renderer.DrawUISprite(x + size + 1, y + 1, size, size - 2, null, false, frameColor);
		Renderer.DrawUISprite(x + size + 1 + 1, y, size - 2, size, null, false, frameColor);
		Renderer.DrawUISprite(x + size + 1 + 1, y + 1, size - 2, size - 2, null, false, bgColor);

		if (player.offhandItem != null)
		{
			Renderer.DrawUISprite(x, y, size, size, player.offhandItem.sprite);

			if (player.offhandItem.stackable && player.offhandItem.stackSize > 1)
				Renderer.DrawUITextBMP(x + size - size / 4, y + size - Renderer.smallFont.size + 2, player.offhandItem.stackSize.ToString(), 1, txtColor);

			if (player.offhandItem.requiredAmmo != null)
			{
				Renderer.DrawUISprite(Renderer.UIWidth / 2 - 8 - 2 * (16 + 1) - 4 - 8 - 32 - 32, Renderer.UIHeight - 4 - 16, size, size, Item.GetItemPrototype(player.offhandItem.requiredAmmo).sprite);
				Item ammo = player.getItem(player.offhandItem.requiredAmmo);
				int count = ammo != null ? ammo.stackSize : 0;
				string countStr = count.ToString();
				Renderer.DrawUITextBMP(Renderer.UIWidth / 2 - 8 - 2 * (16 + 1) - 4 - 8 - 32 - 32 - 2 - Renderer.MeasureUITextBMP(countStr).x, Renderer.UIHeight - 4 - 16 + 5, countStr, 1, txtColor);
			}
			else if (player.offhandItem.type == ItemType.Staff && player.offhandItem.maxStaffCharges > 0)
			{
				Renderer.DrawUISprite(Renderer.UIWidth / 2 - 8 - 2 * (16 + 1) - 4 - 8 - 32 - 32, Renderer.UIHeight - 4 - 16 + 4, 8, 8, staffCharge);
				string countStr = player.offhandItem.staffCharges.ToString();
				Renderer.DrawUITextBMP(Renderer.UIWidth / 2 - 8 - 2 * (16 + 1) - 4 - 8 - 32 - 32 - 2 - Renderer.MeasureUITextBMP(countStr).x, Renderer.UIHeight - 4 - 16 + 5, countStr, 1, txtColor);
			}
		}

		if (player.handItem != null)
		{
			Renderer.DrawUISprite(x + size + 1, y, size, size, player.handItem.getIcon());
			if (player.handItem.stackable && player.handItem.stackSize > 1)
				Renderer.DrawUITextBMP(x + size + 1 + size - size / 4, y + size - Renderer.smallFont.size + 2, player.handItem.stackSize.ToString(), 1, txtColor);

			if (player.handItem.requiredAmmo != null)
			{
				Renderer.DrawUISprite(Renderer.UIWidth / 2 - 8 - 2 * (16 + 1) - 4 - 8 - 32, Renderer.UIHeight - 4 - 16, size, size, Item.GetItemPrototype(player.handItem.requiredAmmo).sprite);
				Item ammo = player.getItem(player.handItem.requiredAmmo);
				int count = ammo != null ? ammo.stackSize : 0;
				string countStr = count.ToString();
				Renderer.DrawUITextBMP(Renderer.UIWidth / 2 - 8 - 2 * (16 + 1) - 4 - 8 - 32 - 2 - Renderer.MeasureUITextBMP(countStr).x, Renderer.UIHeight - 4 - 16 + 5, countStr, 1, txtColor);
			}
			else if (player.handItem.type == ItemType.Staff && player.handItem.maxStaffCharges > 0)
			{
				Renderer.DrawUISprite(Renderer.UIWidth / 2 - 8 - 2 * (16 + 1) - 4 - 8 - 32, Renderer.UIHeight - 4 - 16 + 4, 8, 8, staffCharge);
				string countStr = player.handItem.staffCharges.ToString();
				Renderer.DrawUITextBMP(Renderer.UIWidth / 2 - 8 - 2 * (16 + 1) - 4 - 8 - 32 - 2 - Renderer.MeasureUITextBMP(countStr).x, Renderer.UIHeight - 4 - 16 + 5, countStr, 1, txtColor);
			}
		}
	}

	void ___renderQuickItems(bool flipItems)
	{
		int size = 16;
		int width = player.activeItems.Length * (size + 1) - 1;
		int height = size;
		int x = Renderer.UIWidth / 2 - (4 * (size + 1)) / 2; // 4 + 32 + 16;
		int y = flipItems ? 4 : Renderer.UIHeight - 4 - size;

		float alpha = player.position.y < GameState.instance.camera.bottom + 0.25f * GameState.instance.camera.height &&
			player.position.x < GameState.instance.camera.left + 0.45f * GameState.instance.camera.width ||
			Input.cursorPosition.x < 0.45f * Display.width && Input.cursorPosition.y > 0.75f * Display.height
			? 0.2f : 1.0f;
		alpha = 1;
		uint frameColor = MathHelper.ColorAlpha(0xFF555555, alpha);
		uint bgColor = MathHelper.ColorAlpha(0xFF222222, alpha);
		uint selectionColor = MathHelper.ColorAlpha(0xFF777777, alpha);
		uint txtColor = MathHelper.ColorAlpha(0xFFBBBBBB, alpha);

		Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, frameColor);

		for (int i = 0; i < player.activeItems.Length; i++)
		{
			int xx = x + i * (size + 1);
			int yy = y;

			Renderer.DrawUISprite(xx, yy, size, size, null, false, bgColor);
		}

		for (int i = 0; i < player.activeItems.Length; i++)
		{
			int xx = x + i * (size + 1);
			int yy = y;

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

	void renderQuickItems()
	{
		int size = 16;
		int width = player.activeItems.Length * (size + 1) - 1;
		int height = size;
		int x = Renderer.UIWidth / 2 + 8; // - (4 * (size + 1)) / 2; // 4 + 32 + 16;
		int y = Renderer.UIHeight - 4 - size;

		for (int i = 0; i < player.activeItems.Length; i++)
		{
			int xx = x + i * (size + 1);
			int yy = y;

			Renderer.DrawUISprite(xx + 1, yy, size - 2, size, null, false, frameColor);
			Renderer.DrawUISprite(xx, yy + 1, size, size - 2, null, false, frameColor);
			Renderer.DrawUISprite(xx + 1, yy + 1, size - 2, size - 2, null, false, player.selectedActiveItem == i ? bgSelectedColor : bgColor);
		}

		for (int i = 0; i < player.activeItems.Length; i++)
		{
			int xx = x + i * (size + 1);
			int yy = y;

			if (player.activeItems[i] != null)
			{
				Renderer.DrawUISprite(xx, yy, size, size, player.activeItems[i].sprite);

				if (player.activeItems[i].type == ItemType.Spell && player.actions.currentAction is SpellCastAction && (player.actions.currentAction as SpellCastAction).spell == player.activeItems[i])
				{
					SpellCastAction spellCast = player.actions.currentAction as SpellCastAction;
					float progress = MathF.Min(spellCast.elapsedTime / spellCast.duration, 1);
					int overlayIdx = (int)(progress * 16);
					Renderer.DrawUISprite(xx, yy, size, size, cooldownOverlay[overlayIdx], false, 0xAF000000);
				}
			}

			if (player.selectedActiveItem == i)
			{
				Renderer.DrawUISprite(xx, yy, size, 1, null, false, frameSelectedColor);
				Renderer.DrawUISprite(xx, yy + size - 1, size, 1, null, false, frameSelectedColor);
				Renderer.DrawUISprite(xx, yy + 1, 1, size - 2, null, false, frameSelectedColor);
				Renderer.DrawUISprite(xx + size - 1, yy + 1, 1, size - 2, null, false, frameSelectedColor);
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

	public void render()
	{
		if (player.numOverlaysOpen > 0)
		{
			Input.cursorMode = CursorMode.Normal;
			return;
		}

<<<<<<< HEAD
		if (!enabled)
			return;

=======
>>>>>>> d79438fbcb8d9274022f6ae886162149e731fa4a
		renderHealth();
		renderMana();
		renderXP();
		renderMoney();
		renderArmor();
		renderStatusEffects();
		renderHandItems();
		renderQuickItems();

		/*
		if (player.position.y < GameState.instance.camera.bottom + 0.2f * GameState.instance.camera.height ||
			player.position.y < GameState.instance.camera.bottom + 0.4f * GameState.instance.camera.height && player.lookDirection.normalized.y < -0.5f)
			flipItems = true;
		else if (player.position.y > GameState.instance.camera.top - 0.2f * GameState.instance.camera.height ||
			player.position.y > GameState.instance.camera.top - 0.4f * GameState.instance.camera.height && player.lookDirection.normalized.y > 0.5f)
			flipItems = false;
		*/

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
				Vector2i pos = GameState.instance.camera.worldToScreen(player.position + player.collider.center + player.lookDirection);
				Renderer.DrawUISprite(pos.x - aimIndicator.width / 2, pos.y - aimIndicator.height / 2, aimIndicator.width, aimIndicator.height, player.lookDirection.angle, aimIndicator);
			}
			// Crosshair
			else if (Settings.game.aimMode == AimMode.Crosshair)
			{
				Renderer.DrawUISprite(Renderer.cursorPosition.x - crosshair.width / 2, Renderer.cursorPosition.y - crosshair.height / 2, crosshair.width, crosshair.height, crosshair);
			}
		}

		if (GameState.instance.currentBoss != null)
		{
			int width = Renderer.UIWidth / 2;
			int height = 4;
			int x = Renderer.UIWidth / 2 - width / 2;
			int y = 10;

			//Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, 0, null, 0xFFAAAAAA);
			Renderer.DrawUISprite(x, y, width, height, 0, null, 0xFF3F0000);
			Renderer.DrawUISprite(x, y, (int)MathF.Ceiling(GameState.instance.currentBoss.health / GameState.instance.currentBossMaxHealth * width), height, 0, null, 0xFF983a2e);
			Renderer.DrawUITextBMP(x, y + height + 3, GameState.instance.currentBoss.displayName, 1, 0xFF111111);
			Renderer.DrawUITextBMP(x, y + height + 2, GameState.instance.currentBoss.displayName, 1, 0xFFAAAAAA);
		}
	}
}
