using Rainfall;
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
		Renderer.DrawUISprite(x + width / 2 - sprite.width / 2, y, sprite.width, sprite.height, sprite, false, Mathf.VectorToARGB(item.spriteColor));
		y += sprite.height + 1;

		string[] nameLines = Renderer.SplitMultilineText(item.fullDisplayNameFormatted, width);
		foreach (string line in nameLines)
		{
			Renderer.DrawUITextBMPFormatted(x + width / 2 - Renderer.MeasureUITextBMP(line).x / 2, y, line, 1, UIColors.TEXT);
			y += Renderer.smallFont.size;
		}
		y++;

		string[] itemInfoLines = Renderer.SplitMultilineText(item.fullItemTypeFormatted, width - 4);
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

		if (item is SpellBook)
		{
			SpellBook spellBook = item as SpellBook;
			string spellName = spellBook.spell.displayName;
			Renderer.DrawUITextBMP(x + width / 2 - Renderer.MeasureUITextBMP(spellName).x / 2, y, spellName, 1, UIColors.TEXT_MANA);
			y += Renderer.smallFont.size + 4;
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
		void drawComparisonStr(string str, string toStr, int comparison)
		{
			int textWidth = Renderer.MeasureUITextBMP(str, str.Length, 1).x;
			uint color = comparison == 1 ? UIColors.TEXT_UPGRADE : comparison == -1 ? UIColors.TEXT_DOWNGRADE : UIColors.TEXT_COMPARABLE;
			Renderer.DrawUITextBMP(x + width - 1 - textWidth, y, str, 1, color);

			toStr = toStr + " > ";
			int toStrWidth = Renderer.MeasureUITextBMP(toStr, toStr.Length, 1).x;
			Renderer.DrawUITextBMP(x + width - 1 - textWidth - toStrWidth, y, toStr, 1, UIColors.TEXT);
		}
		void drawComparison(float value, float to, bool flipComparison = false)
		{
			string str = MathF.Abs(value - MathF.Round(value)) < 0.0001f ? ((int)value).ToString() : value.ToString("0.0");
			string toStr = to.ToString("0.0");
			int comparison = MathF.Sign(value - to) * (flipComparison ? -1 : 1);
			drawComparisonStr(str, toStr, comparison);
		}

		if (item.type == ItemType.Weapon || item.type == ItemType.Staff)
		{
			drawLeft("Attack");
			float infusedDamage = item.getInfusedDamage();
			if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
				drawComparison(infusedDamage * 10, compareItem.getInfusedDamage() * 10);
			else
				drawRight(infusedDamage * 10);
			y += Renderer.smallFont.size + 1;

			drawLeft("Speed");
			if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
				drawComparison(item.attackRate, compareItem.attackRate);
			else
				drawRight(item.attackRate);
			y += Renderer.smallFont.size + 1;

			drawLeft("Range");
			if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
				drawComparison(item.attackRange, compareItem.attackRange);
			else
				drawRight(item.attackRange);
			y += Renderer.smallFont.size + 1;

			drawLeft("Knockback");
			if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
				drawComparison(item.knockback, compareItem.knockback);
			else
				drawRight(item.knockback);
			y += Renderer.smallFont.size + 1;

			drawLeft("Critical");
			if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
				drawComparison((int)MathF.Round(GameState.instance.player.criticalChance * item.criticalChanceModifier * 100), (int)MathF.Round(GameState.instance.player.criticalChance * compareItem.criticalChanceModifier * 100));
			else
				drawRight((int)MathF.Round(GameState.instance.player.criticalChance * item.criticalChanceModifier * 100));
			y += Renderer.smallFont.size + 1;

			drawLeft("Weight");
			if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
				drawComparison(item.weight, compareItem.weight, true);
			else
				drawRight(item.weight);
			y += Renderer.smallFont.size + 1;

			if (item.buff != null)
			{
				if (item.buff.criticalAttackModifier > 1)
				{
					drawLeft("Crit Modifier");
					if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff) && compareItem.buff != null)
						drawComparison(item.buff.criticalAttackModifier, compareItem.buff.criticalAttackModifier, true);
					else
						drawRight(item.buff.criticalAttackModifier);
					y += Renderer.smallFont.size + 1;
				}
			}

			if (item.bleed > 0)
			{
				drawLeft("Bleed");
				if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff) && compareItem.bleed > 0)
					drawComparison(item.bleed * 10, compareItem.bleed * 10);
				else
					drawRight(item.bleed * 10);
				y += Renderer.smallFont.size + 1;
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
				if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
					drawComparisonStr(Item.GetScalingLetter(compareItem.strengthScaling), scalingStr, MathF.Sign(compareItem.strengthScaling - item.strengthScaling));
				else
					drawRightStr(scalingStr);
				y += Renderer.smallFont.size + 1;
			}

			{
				drawLeft("DEX");
				string scalingStr = Item.GetScalingLetter(item.dexterityScaling);
				if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
					drawComparisonStr(Item.GetScalingLetter(compareItem.dexterityScaling), scalingStr, MathF.Sign(compareItem.dexterityScaling - item.dexterityScaling));
				else
					drawRightStr(scalingStr);
				y += Renderer.smallFont.size + 1;
			}

			{
				string scalingStr = Item.GetScalingLetter(item.intelligenceScaling);
				drawRightStr(scalingStr);
				if (compareItem != null && (item.type == ItemType.Weapon || item.type == ItemType.Staff))
					drawComparisonStr(Item.GetScalingLetter(compareItem.intelligenceScaling), scalingStr, MathF.Sign(compareItem.intelligenceScaling - item.intelligenceScaling));
				else
					drawLeft("INT");
				y += Renderer.smallFont.size + 1;
			}
		}
		else if (item.type == ItemType.Armor)
		{
			drawLeft("Armor");
			if (compareItem != null && compareItem.type == ItemType.Armor)
				drawComparison(item.armor, compareItem.armor);
			else
				drawRight(item.armor);
			y += Renderer.smallFont.size + 1;

			drawLeft("Weight");
			if (compareItem != null && compareItem.type == ItemType.Armor)
				drawComparison(item.weight, compareItem.weight, true);
			else
				drawRight(item.weight);
			y += Renderer.smallFont.size + 1;
		}
		else if (item.type == ItemType.Shield)
		{
			drawLeft("Protection");
			if (compareItem != null && compareItem.type == ItemType.Shield)
				drawComparison(item.blockAbsorption, compareItem.blockAbsorption);
			else
				drawRight(item.blockAbsorption);
			y += Renderer.smallFont.size + 1;

			drawLeft("Armor");
			if (compareItem != null && compareItem.type == ItemType.Shield)
				drawComparison(item.armor, compareItem.armor);
			else
				drawRight(item.armor);
			y += Renderer.smallFont.size + 1;

			drawLeft("Weight");
			if (compareItem != null && compareItem.type == ItemType.Shield)
				drawComparison(item.weight, compareItem.weight, true);
			else
				drawRight(item.weight);
			y += Renderer.smallFont.size + 1;
		}

		if (item.type == ItemType.Potion && item is Potion)
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
			if (compareItem != null && item.type == ItemType.Spell)
				drawComparison(infusedDamage, compareItem.getInfusedDamage());
			else
				drawRight(infusedDamage);
			y += Renderer.smallFont.size + 1;

			drawLeft("Speed");
			if (compareItem != null && item.type == ItemType.Spell)
				drawComparison(item.attackRate, compareItem.attackRate);
			else
				drawRight(item.attackRate);
			y += Renderer.smallFont.size + 1;

			drawLeft("Range");
			if (compareItem != null && item.type == ItemType.Spell)
				drawComparison(item.attackRange, compareItem.attackRange);
			else
				drawRight(item.attackRange);
			y += Renderer.smallFont.size + 1;

			drawLeft("Mana Cost");
			if (compareItem != null && item.type == ItemType.Spell)
				drawComparison(item.manaCost * 10, compareItem.manaCost * 10);
			else
				drawRight(item.manaCost * 10);
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
