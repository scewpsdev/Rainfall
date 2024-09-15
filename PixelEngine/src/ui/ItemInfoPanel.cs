using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class ItemInfoPanel
{
	public static int Render(Item item, int x, int y, int width, int height)
	{
		int top = y;

		Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);
		Renderer.DrawUISprite(x, y, width, height, null, false, 0xFF222222);

		y += 4;

		Renderer.DrawUISprite(x + width / 2 - item.sprite.width / 2, y, item.sprite.width, item.sprite.height, item.sprite, false, MathHelper.VectorToARGB(item.spriteColor));
		y += item.sprite.height + 1;

		string[] nameLines = Renderer.SplitMultilineText(item.displayName, width);
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
		void drawRight(string str, uint color = 0xFFAAAAAA)
		{
			if (str == null)
				str = "???";
			int textWidth = Renderer.MeasureUITextBMP(str, str.Length, 1).x;
			Renderer.DrawUITextBMP(x + width - textWidth - 1, y, str, 1, color);
		}

		if (item.type == ItemType.Weapon)
		{
			drawLeft("Attack");
			drawRight(item.attackDamage.ToString("0.0"));
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
			drawLeft("Defense");
			drawRight(item.armor.ToString());
			y += Renderer.smallFont.size + 1;
		}

		return y - top;
	}
}
