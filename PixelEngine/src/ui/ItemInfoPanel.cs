using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class ItemInfoPanel
{
	public static int Render(Item item, int x, int y, int width, int height, Item compareItem = null)
	{
		int top = y;

		Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);
		Renderer.DrawUISprite(x, y, width, height, null, false, 0xFF222222);

		y += 4;

		Renderer.DrawUISprite(x + width / 2 - item.sprite.width / 2, y, item.sprite.width, item.sprite.height, item.sprite, false, MathHelper.VectorToARGB(item.spriteColor));
		y += item.sprite.height + 1;

		string[] nameLines = Renderer.SplitMultilineText(item.fullDisplayName, width);
		foreach (string line in nameLines)
		{
			Renderer.DrawUITextBMP(x + width / 2 - Renderer.MeasureUITextBMP(line).x / 2, y, line, 1, 0xFFAAAAAA);
			y += Renderer.smallFont.size;
		}
		y++;

		string rarityString = item.rarityString;
		string itemTypeStr = item.type.ToString();
		string itemInfo = rarityString + " " + (item.twoHanded ? "Two Handed " : "") + itemTypeStr;
		string[] itemInfoLines = Renderer.SplitMultilineText(itemInfo, width);
		foreach (string line in itemInfoLines)
		{
			Renderer.DrawUITextBMP(x + width / 2 - Renderer.MeasureUITextBMP(line).x / 2, y, line, 1, 0xFF666666);
			y += Renderer.smallFont.size;
		}
		y += 4;

		if (item.description != null)
		{
			string[] descriptionLines = Renderer.SplitMultilineText(item.description, width);
			foreach (string line in descriptionLines)
			{
				Renderer.DrawUITextBMP(x + width / 2 - Renderer.MeasureUITextBMP(line).x / 2, y, line, 1, 0xFFAAAAAA);
				y += Renderer.smallFont.size;
			}
			y += 4;
		}

		void drawLeft(string str, uint color = 0xFFAAAAAA)
		{
			if (str == null)
				str = "???";
			Renderer.DrawUITextBMP(x + 4, y, str, 1, color);
		}
		void drawRight(float value, uint color = 0xFFAAAAAA)
		{
			string str = MathF.Abs(value - MathF.Round(value)) < 0.0001f ? ((int)value).ToString() : value.ToString("0.0");
			int textWidth = Renderer.MeasureUITextBMP(str, str.Length, 1).x;
			Renderer.DrawUITextBMP(x + width - textWidth - 1, y, str, 1, color);
		}
		void drawComparison(float value, float to)
		{
			string str = MathF.Abs(value - MathF.Round(value)) < 0.0001f ? ((int)value).ToString() : value.ToString("0.0");
			int textWidth = Renderer.MeasureUITextBMP(str, str.Length, 1).x;
			uint color = value > to ? 0xFF5ecb57 : value < to ? 0xFFbd4242 : 0xFFAAAAAA;
			Renderer.DrawUITextBMP(x + width - 1 - textWidth, y, str, 1, color);
		}

		if (item.type == ItemType.Weapon)
		{
			drawLeft("DMG");
			drawRight(item.attackDamage);
			if (compareItem != null && compareItem.type == ItemType.Weapon)
				drawComparison(item.attackDamage, compareItem.attackDamage);
			y += Renderer.smallFont.size + 1;

			drawLeft("DEX");
			drawRight(item.attackRate);
			if (compareItem != null && compareItem.type == ItemType.Weapon)
				drawComparison(item.attackRate, compareItem.attackRate);
			y += Renderer.smallFont.size + 1;

			drawLeft("CRIT");
			drawRight(item.criticalChance * 100);
			if (compareItem != null && compareItem.type == ItemType.Weapon)
				drawComparison(item.criticalChance * 100, compareItem.criticalChance * 100);
			y += Renderer.smallFont.size + 1;

			drawLeft("RANGE");
			drawRight(item.attackRange);
			if (compareItem != null && compareItem.type == ItemType.Weapon)
				drawComparison(item.attackRange, compareItem.attackRange);
			y += Renderer.smallFont.size + 1;

			drawLeft("KNOCK");
			drawRight(item.knockback);
			if (compareItem != null && compareItem.type == ItemType.Weapon)
				drawComparison(item.knockback, compareItem.knockback);
			y += Renderer.smallFont.size + 1;

			

			//drawLeft("Reach");
			//drawRight(item.attackRange.ToString("0.0"));
			//y += Renderer.smallFont.size + 1;

			//drawLeft("Knockback");
			//drawRight(item.knockback.ToString("0.0"));
			//y += Renderer.smallFont.size + 1;
		}
		else if (item.type == ItemType.Armor)
		{
			drawLeft("ARM");
			drawRight(item.armor);
			if (compareItem != null && compareItem.type == ItemType.Armor)
				drawComparison(item.armor, compareItem.armor);
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

		return y - top;
	}
}
