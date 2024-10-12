using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class CharacterInfoPanel
{
	static void RenderPlayer(int x, int y, int width, int height)
	{
		Player player = GameState.instance.player;

		int size = 16;
		int xx = x + width / 2 - size / 2;
		int yy = y + size * 3 / 4;
		Renderer.DrawUISprite(xx - size / 2, yy - size / 2, size * 2, size * 2, null, false, 0xFF050505);

		if (player.offhandItem != null)
		{
			int w = (int)MathF.Round(player.offhandItem.size.x * size);
			int h = (int)MathF.Round(player.offhandItem.size.y * size);
			Renderer.DrawUISprite(xx - (w - size) / 2 + (int)(player.getWeaponOrigin(false).x * size + player.offhandItem.renderOffset.x * size), yy + size / 2 - (h - size) - (int)(player.getWeaponOrigin(false).y * size + player.offhandItem.renderOffset.y * size), w, h, player.offhandItem.sprite);
		}

		//player.animator.update(player.sprite);
		Renderer.DrawUISprite(xx, yy, size, size, player.sprite);

		for (int i = 0; i < player.passiveItems.Count; i++)
		{
			if (player.passiveItems[i] != null && player.passiveItems[i].ingameSprite != null)
			{
				int ss = size * player.passiveItems[i].ingameSpriteSize;
				//player.animator.update(player.passiveItems[i].ingameSprite);
				Renderer.DrawUISprite(xx - (ss - size) / 2, yy - (ss - size), ss, ss, player.passiveItems[i].ingameSprite, false, MathHelper.VectorToARGB(player.passiveItems[i].ingameSpriteColor));
			}
		}

		if (player.handItem != null)
		{
			int w = (int)MathF.Round(player.handItem.size.x * size);
			int h = (int)MathF.Round(player.handItem.size.y * size);
			Renderer.DrawUISprite(xx - (w - size) / 2 + (int)(player.getWeaponOrigin(true).x * size + player.handItem.renderOffset.x * size), yy + size / 2 - (h - size) - (int)(player.getWeaponOrigin(true).y * size + player.handItem.renderOffset.y * size), w, h, player.handItem.sprite);
		}
	}

	public static void Render(int x, int y, int width, int height, Player player)
	{
		Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);
		Renderer.DrawUISprite(x, y, width, height, null, false, 0xFF111111);

		y += 8;

		Renderer.DrawUITextBMP(x + width / 2 - Renderer.MeasureUITextBMP("Character").x / 2, y, "Character", 1, UIColors.TEXT);

		y += Renderer.smallFont.size + 4;

		RenderPlayer(x + 4, y, 32, 32);

		string className = player.startingClass != null ? player.startingClass.name : "No Class";
		uint classColor = player.startingClass != null ? player.startingClass.color : UIColors.TEXT_SUBTLE;
		Renderer.DrawUITextBMP(x + 4 + 32 + 4, y + 8 + 4, className, 1, classColor);
		Renderer.DrawUITextBMP(x + 4 + 32 + 4, y + 24 - 4, "Level " + player.playerLevel.ToString(), 1, UIColors.TEXT);

		y += 32 + 8;

		void drawLeft(string str, uint color = 0xFFAAAAAA)
		{
			if (str == null)
				str = "???";
			Renderer.DrawUITextBMP(x + 4, y, str, 1, color);
		}
		void drawRight(string str, uint color = 0xFFAAAAAA)
		{
			if (str == null)
				str = "???";
			int textWidth = Renderer.MeasureUITextBMP(str, str.Length, 1).x;
			Renderer.DrawUITextBMP(x + width - textWidth - 4, y, str, 1, color);
		}
		string formatValue(float value)
		{
			return MathF.Abs(value - MathF.Round(value)) < 0.0001f ? ((int)value).ToString() : value.ToString("0.0");
		}
		void drawRightValue(float value, uint color = 0xFFAAAAAA)
		{
			drawRight(formatValue(value), color);
		}
		void drawRightValueCompare(float value, float defaultValue, uint color = 0xFFAAAAAA, uint positiveColor = 0, uint negativeColor = 0)
		{
			if (positiveColor == 0) positiveColor = color;
			if (negativeColor == 0) negativeColor = color;
			string str = formatValue(value / defaultValue * 100) + "%";
			uint c = value > defaultValue ? positiveColor : value < defaultValue ? negativeColor : color;
			drawRight(str, c);
		}
		void drawRightValueRelative(float value, float defaultValue, uint color = 0xFFAAAAAA, uint positiveColor = 0, uint negativeColor = 0)
		{
			if (positiveColor == 0) positiveColor = color;
			if (negativeColor == 0) negativeColor = color;
			string str = (value >= defaultValue ? "+" : "") + formatValue((value - defaultValue) / defaultValue * 100) + "%";
			drawRight(str, value > defaultValue ? positiveColor : value < defaultValue ? negativeColor : color);
		}

		drawLeft("Health", UIColors.TEXT_HEALTH);
		drawRight(formatValue(player.health * 10) + "/" + formatValue(player.maxHealth * 10), UIColors.TEXT);

		y += Renderer.smallFont.size + 1;

		drawLeft("Mana", UIColors.TEXT_MANA);
		drawRight(formatValue(player.mana * 10) + "/" + formatValue(player.maxMana * 10), UIColors.TEXT);
		y += Renderer.smallFont.size + 1;
		drawLeft("Mana Recovery Rate", UIColors.TEXT_MANA);
		drawRightValueRelative(player.getManaRecoveryModifier(), 1, UIColors.TEXT, UIColors.TEXT_UPGRADE, UIColors.TEXT_DOWNGRADE);
		y += Renderer.smallFont.size + 1;

		drawLeft("Speed", UIColors.TEXT_SPEED);
		drawRightValueCompare(player.speed, Player.defaultSpeed, UIColors.TEXT, UIColors.TEXT_UPGRADE, UIColors.TEXT_DOWNGRADE);

		y += Renderer.smallFont.size + 1;

		drawLeft("Equip Load", UIColors.TEXT_SPEED);
		drawRightValueCompare(player.getTotalEquipLoad(), 10, UIColors.TEXT);

		y += Renderer.smallFont.size + 1;

		drawLeft("Armor", UIColors.TEXT);
		drawRightValue(player.getTotalArmor(), UIColors.TEXT);

		y += Renderer.smallFont.size + 1;

		drawLeft("Attack Damage", UIColors.TEXT);
		drawRightValueRelative(player.getAttackDamageModifier(), 1);
		y += Renderer.smallFont.size + 1;
		drawLeft("Attack Speed", UIColors.TEXT);
		drawRightValueRelative(player.getAttackSpeedModifier(), 1);
		y += Renderer.smallFont.size + 1;

		drawLeft("Critical Hit Chance", UIColors.TEXT);
		drawRightValueCompare(player.criticalChance * player.getCriticalChanceModifier(), 1, UIColors.TEXT);
		y += Renderer.smallFont.size + 1;
		drawLeft("Critical Hit Damage", UIColors.TEXT);
		drawRightValueRelative(player.getCriticalAttackModifier(), 1, UIColors.TEXT);
		y += Renderer.smallFont.size + 1;
	}
}
