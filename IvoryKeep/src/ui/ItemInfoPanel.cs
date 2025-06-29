﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class ItemInfoPanel
{
	public static float Render(Item item, float x, float y, int width, int height, Item compareItem = null)
	{
		float top = y;

		Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, item.rarityColor /*UIColors.WINDOW_FRAME*/);
		Renderer.DrawUISprite(x, y, width, height, null, false, UIColors.WINDOW_BACKGROUND);

		y += 4;

		Sprite sprite = item.spellIcon != null ? item.spellIcon : item.sprite;
		Renderer.DrawUIOutline(x + width / 2 - sprite.width / 2, y, sprite.width, sprite.height, sprite, false, 0xFF000000);
		Renderer.DrawUISprite(x + width / 2 - sprite.width / 2, y, sprite.width, sprite.height, sprite, false, MathHelper.VectorToARGB(item.spriteColor));
		y += sprite.height + 1;

		string[] nameLines = Renderer.SplitMultilineText(item.fullDisplayName, width);
		foreach (string line in nameLines)
		{
			Renderer.DrawUITextBMP(x + width / 2 - Renderer.MeasureUITextBMP(line).x / 2, y, line, 1, UIColors.TEXT);
			y += Renderer.smallFont.size;
		}
		y++;

		string rarityString = "\\x0x" + item.rarityColor.ToString("X") + "\\" + item.rarityString + "\\x0\\";
		string itemTypeStr = item.type.ToString();
		string itemInfo = rarityString + " " + (item.twoHanded ? "Two Handed " : "") + (item.isSecondaryItem ? "Secondary " : "") + itemTypeStr;
		string[] itemInfoLines = Renderer.SplitMultilineText(itemInfo, width - 4);
		foreach (string line in itemInfoLines)
		{
			Renderer.DrawUITextBMPFormatted(x + width / 2 - Renderer.MeasureUITextBMP(line).x / 2, y, line, 1, UIColors.TEXT_SUBTLE);
			y += Renderer.smallFont.size;
		}
		y += 4;

		if (item.description != null)
		{
			string[] descriptionLines = Renderer.SplitMultilineText(item.description, width);
			foreach (string line in descriptionLines)
			{
				Renderer.DrawUITextBMP(x + width / 2 - Renderer.MeasureUITextBMP(line).x / 2, y, line, 1, UIColors.TEXT);
				y += Renderer.smallFont.size;
			}
			y += 4;
		}

		void drawLeft(string str, uint color = UIColors.TEXT)
		{
			if (str == null)
				str = "???";
			Renderer.DrawUITextBMP(x + 4, y, str, 1, color);
		}
		void drawRightStr(string str, uint color = UIColors.TEXT)
		{
			int textWidth = Renderer.MeasureUITextBMP(str, str.Length, 1).x;
			Renderer.DrawUITextBMP(x + width - textWidth - 1, y, str, 1, color);
		}
		void drawRight(float value, uint color = UIColors.TEXT)
		{
			string str = MathF.Abs(value - MathF.Round(value)) < 0.0001f ? ((int)value).ToString() : value.ToString("0.0");
			drawRightStr(str, color);
		}
		void drawComparisonStr(string str, int comparison)
		{
			int textWidth = Renderer.MeasureUITextBMP(str, str.Length, 1).x;
			uint color = comparison == 1 ? UIColors.TEXT_UPGRADE : comparison == -1 ? UIColors.TEXT_DOWNGRADE : UIColors.TEXT_COMPARABLE;
			Renderer.DrawUITextBMP(x + width - 1 - textWidth, y, str, 1, color);
		}
		void drawComparison(float value, float to, bool flipComparison = false)
		{
			string str = MathF.Abs(value - MathF.Round(value)) < 0.0001f ? ((int)value).ToString() : value.ToString("0.0");
			int comparison = MathF.Sign(value - to) * (flipComparison ? -1 : 1);
			drawComparisonStr(str, comparison);
		}

		if (item.type == ItemType.Weapon || item.type == ItemType.Staff)
		{
			drawLeft("Attack");
			float infusedDamage = item.getInfusedDamage();
			drawRight(infusedDamage);
			if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
				drawComparison(infusedDamage, compareItem.getInfusedDamage());
			y += Renderer.smallFont.size + 1;

			drawLeft("Speed");
			drawRight(item.attackRate);
			if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
				drawComparison(item.attackRate, compareItem.attackRate);
			y += Renderer.smallFont.size + 1;

			drawLeft("Range");
			drawRight(item.attackRange);
			if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
				drawComparison(item.attackRange, compareItem.attackRange);
			y += Renderer.smallFont.size + 1;

			drawLeft("Knockback");
			drawRight(item.knockback);
			if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
				drawComparison(item.knockback, compareItem.knockback);
			y += Renderer.smallFont.size + 1;

			drawLeft("Weight");
			drawRight(item.weight);
			if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
				drawComparison(item.weight, compareItem.weight, true);
			y += Renderer.smallFont.size + 1;

			if (item.buff != null)
			{
				if (item.buff.criticalAttackModifier > 1)
				{
					drawLeft("Critical");
					drawRight(item.buff.criticalAttackModifier);
					if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff) && compareItem.buff != null)
						drawComparison(item.buff.criticalAttackModifier, compareItem.buff.criticalAttackModifier, true);
					y += Renderer.smallFont.size + 1;
				}
			}

			if (item.type == ItemType.Staff)
			{
				Staff staff = item as Staff;

				if (staff.staffCharges >= 0)
				{
					drawLeft("Charges");
					drawRight(staff.staffCharges);
					y += Renderer.smallFont.size + 1;
				}

				/*
				y += 4;
				drawLeft("Attuned spells:" + (staff.attunedSpells.Count > 0 ? "" : " ---"));
				//y += Renderer.smallFont.size + 1;
				y++;

				if (staff.attunedSpells.Count > 0)
				{
					x += 4;
					for (int i = 0; i < staff.attunedSpells.Count; i++)
					{
						if (staff.attunedSpells[i] != null)
						{
							y += Renderer.smallFont.size;
							drawLeft(staff.attunedSpells[i].fullDisplayName, UIColors.TEXT_SUBTLE);
						}
					}
					x -= 4;
				}
				y += Renderer.smallFont.size + 1;
				*/
			}

			y += 4;


			// Scaling values

			{
				drawLeft("STR");
				string scalingStr = Item.GetScalingLetter(item.strengthScaling);
				drawRightStr(scalingStr);
				if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
					drawComparisonStr(scalingStr, MathF.Sign(compareItem.strengthScaling - item.strengthScaling));
				y += Renderer.smallFont.size + 1;
			}

			{
				drawLeft("DEX");
				string scalingStr = Item.GetScalingLetter(item.dexterityScaling);
				drawRightStr(scalingStr);
				if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
					drawComparisonStr(scalingStr, MathF.Sign(compareItem.dexterityScaling - item.dexterityScaling));
				y += Renderer.smallFont.size + 1;
			}

			{
				drawLeft("INT");
				string scalingStr = Item.GetScalingLetter(item.intelligenceScaling);
				drawRightStr(scalingStr);
				if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
					drawComparisonStr(scalingStr, MathF.Sign(compareItem.intelligenceScaling - item.intelligenceScaling));
				y += Renderer.smallFont.size + 1;
			}
		}
		else if (item.type == ItemType.Armor)
		{
			drawLeft("Armor");
			drawRight(item.armor);
			if (compareItem != null && compareItem.type == ItemType.Armor)
				drawComparison(item.armor, compareItem.armor);
			y += Renderer.smallFont.size + 1;

			drawLeft("Weight");
			drawRight(item.weight);
			if (compareItem != null && compareItem.type == ItemType.Armor)
				drawComparison(item.weight, compareItem.weight, true);
			y += Renderer.smallFont.size + 1;
		}
		else if (item.type == ItemType.Shield)
		{
			drawLeft("Protection");
			drawRight(item.blockAbsorption);
			if (compareItem != null && compareItem.type == ItemType.Shield)
				drawComparison(item.blockAbsorption, compareItem.blockAbsorption);
			y += Renderer.smallFont.size + 1;

			drawLeft("Armor");
			drawRight(item.armor);
			if (compareItem != null && compareItem.type == ItemType.Shield)
				drawComparison(item.armor, compareItem.armor);
			y += Renderer.smallFont.size + 1;

			drawLeft("Weight");
			drawRight(item.weight);
			if (compareItem != null && compareItem.type == ItemType.Shield)
				drawComparison(item.weight, compareItem.weight, true);
			y += Renderer.smallFont.size + 1;
		}

		if (item.type == ItemType.Potion)
		{
			Potion potion = item as Potion;
			for (int i = 0; i < potion.effects.Count; i++)
			{
				drawLeft(potion.effects[i].name + " effect");
				y += Renderer.smallFont.size;
			}
		}

		if (item.type == ItemType.Spell)
		{
			drawLeft("Attack");
			float infusedDamage = item.getInfusedDamage();
			drawRight(infusedDamage);
			if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
				drawComparison(infusedDamage, compareItem.getInfusedDamage());
			y += Renderer.smallFont.size + 1;

			drawLeft("Speed");
			drawRight(item.attackRate);
			if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
				drawComparison(item.attackRate, compareItem.attackRate);
			y += Renderer.smallFont.size + 1;
		}

		Item itemInInv = GameState.instance.player.getItem(item.name);
		if (itemInInv != null && item.stackable)
		{
			y += 4;

			string str = itemInInv.stackSize + "x stored";
			Renderer.DrawUITextBMP(x + width / 2 - Renderer.MeasureUITextBMP(str).x / 2, y, str, 1, UIColors.TEXT);
			y += Renderer.smallFont.size + 1;
		}

		return y - top;
	}
}
