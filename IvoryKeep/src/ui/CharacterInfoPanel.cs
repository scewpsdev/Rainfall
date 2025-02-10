using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class CharacterInfoPanel
{
	static int selectedLevelStat = 0;

	public static void OnOpen()
	{
		selectedLevelStat = 0;
	}

	public static int Render(int x, int y, int width, int height, Player player)
	{
		int top = y;

		Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);
		Renderer.DrawUISprite(x, y, width, height, null, false, 0xFF111111);

		//y += 4;

		//Renderer.DrawUITextBMP(x + width / 2 - Renderer.MeasureUITextBMP("Character").x / 2, y, "Character", 1, UIColors.TEXT);

		//y += Renderer.smallFont.size + 4;

		PlayerThumbnail.Render(x + 4, y, 32, 32);

		string className = player.startingClass != null ? player.startingClass.name : "No Class";
		uint classColor = player.startingClass != null ? player.startingClass.color : UIColors.TEXT_SUBTLE;
		Renderer.DrawUITextBMP(x + 4 + 32 + 4, y + 8 + 4, className, 1, classColor);
		Renderer.DrawUITextBMP(x + 4 + 32 + 4, y + 24 - 4, "Level " + player.playerLevel.ToString(), 1, UIColors.TEXT);

		y += 32 + 4;

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
			return MathF.Abs(value - MathF.Round(value)) < 0.0001f ? ((int)MathF.Round(value)).ToString() : value.ToString("0.0");
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
		void drawLevel(int idx, string name, ref int value, uint color)
		{
			if (player.availableStatUpgrades > 0)
			{
				bool hovered = Renderer.IsHovered(x, y, width, Renderer.smallFont.size);
				if (hovered)
					selectedLevelStat = idx;
				if (selectedLevelStat == idx)
				{
					Renderer.DrawUISprite(x, y, width, Renderer.smallFont.size - 1, null, false, 0xFF222222);
					float alpha = MathF.Sin(Time.currentTime / 1e9f * 5) * 0.5f + 0.5f;
					Renderer.DrawUISprite(x, y, width, Renderer.smallFont.size - 1, null, false, MathHelper.ColorAlpha(UIColors.WINDOW_FRAME, alpha));
					if (hovered && (Input.IsKeyPressed(KeyCode.Return) || Input.IsMouseButtonPressed(MouseButton.Left, true)))
					{
						value++;
						player.availableStatUpgrades--;

						Audio.PlayBackground(UISound.uiSwitch);
					}
				}
			}

			drawLeft(name, color);
			drawRightValue(value, color);
		}

		y += 4;

		drawLeft("Health", UIColors.TEXT_HEALTH);
		drawRight(formatValue(player.health) + "/" + formatValue(player.maxHealth), player.availableStatUpgrades > 0 && selectedLevelStat == 0 ? UIColors.TEXT_UPGRADE : UIColors.TEXT);
		y += Renderer.smallFont.size;

		drawLeft("Mana", UIColors.TEXT_MANA);
		drawRight(formatValue(player.mana) + "/" + formatValue(player.maxMana), player.availableStatUpgrades > 0 && selectedLevelStat == 1 ? UIColors.TEXT_UPGRADE : UIColors.TEXT);
		y += Renderer.smallFont.size;

		drawLeft("Speed", UIColors.TEXT_SPEED);
		drawRightValueCompare(player.speed * player.currentSpeedModifier, Player.defaultSpeed, UIColors.TEXT, UIColors.TEXT_UPGRADE, UIColors.TEXT_DOWNGRADE);
		y += Renderer.smallFont.size;

		drawLeft("Equip Load", UIColors.TEXT_SPEED);
		drawRightValueCompare(player.getTotalEquipLoad(), 10, UIColors.TEXT);
		y += Renderer.smallFont.size;

		drawLeft("Armor", UIColors.TEXT_ARMOR);
		drawRightValue(player.getTotalArmor(), UIColors.TEXT);
		y += Renderer.smallFont.size;

		y += 4;

		/*
		drawLevel(0, "HP", ref player.hp, UIColors.TEXT);
		y += Renderer.smallFont.size;

		drawLevel(1, "MP", ref player.magic, UIColors.TEXT);
		y += Renderer.smallFont.size;
		*/

		drawLevel(0, "STR", ref player.strength, UIColors.TEXT);
		y += Renderer.smallFont.size;

		drawLevel(1, "DEX", ref player.dexterity, UIColors.TEXT);
		y += Renderer.smallFont.size;

		drawLevel(2, "INT", ref player.intelligence, UIColors.TEXT);
		y += Renderer.smallFont.size;

		drawLevel(3, "SWF", ref player.swiftness, UIColors.TEXT);
		y += Renderer.smallFont.size;

		if (InputManager.IsPressed("Down", true))
		{
			selectedLevelStat = (selectedLevelStat + 1) % 6;
			Audio.PlayBackground(UISound.uiClick);
		}
		if (InputManager.IsPressed("Up", true))
		{
			selectedLevelStat = (selectedLevelStat + 6 - 1) % 6;
			Audio.PlayBackground(UISound.uiClick);
		}

		y += 4;

		drawLeft("Attack Damage", UIColors.TEXT);
		drawRightValueRelative(player.getMeleeDamageModifier(), 1, UIColors.TEXT);
		y += Renderer.smallFont.size;
		drawLeft("Attack Speed", UIColors.TEXT);
		drawRightValueRelative(player.getAttackSpeedModifier(), 1, UIColors.TEXT);
		y += Renderer.smallFont.size;
		drawLeft("Magic Damage", UIColors.TEXT);
		drawRightValueRelative(player.getMagicDamageModifier(), 1, UIColors.TEXT);
		y += Renderer.smallFont.size;
		drawLeft("Movement Speed", UIColors.TEXT);
		drawRightValueRelative(player.getMovementSpeedModifier(), 1, player.availableStatUpgrades > 0 && selectedLevelStat == 3 ? UIColors.TEXT_UPGRADE : UIColors.TEXT);
		y += Renderer.smallFont.size;

		drawLeft("Mana Recovery Rate", UIColors.TEXT);
		drawRightValueRelative(player.getManaRecoveryModifier(), 1, UIColors.TEXT, UIColors.TEXT_UPGRADE, UIColors.TEXT_DOWNGRADE);
		y += Renderer.smallFont.size;

		drawLeft("Critical Hit Chance", UIColors.TEXT);
		drawRightValueCompare(player.criticalChance * player.getCriticalChanceModifier(), 1, UIColors.TEXT);
		y += Renderer.smallFont.size;
		drawLeft("Critical Hit Damage", UIColors.TEXT);
		drawRightValueRelative(player.getCriticalAttackModifier(), 1, UIColors.TEXT);
		y += Renderer.smallFont.size;

		drawLeft("Attack Rating", UIColors.TEXT);
		drawRightValue(player.handItem != null ? player.handItem.getAttackDamage(player) : DefaultWeapon.instance.getAttackDamage(player), player.availableStatUpgrades > 0 && (selectedLevelStat == 0 || selectedLevelStat == 1 || selectedLevelStat == 2) ? UIColors.TEXT_UPGRADE : UIColors.TEXT);
		y += Renderer.smallFont.size;

		y += -2 + 4;

		return y - top;
	}
}
