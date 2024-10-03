using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ArmorStand : Entity, Interactable
{
	Sprite sprite;
	uint outline;
	string label;
	int cost;

	public List<Item> items = new List<Item>();


	public ArmorStand(string label = null, int cost = 0, params Item[] items)
	{
		this.label = label;
		this.cost = cost;

		sprite = new Sprite(TileType.tileset, 2, 10);

		this.items.AddRange(items);
	}

	public ArmorStand()
		: this(null, 0, [])
	{
	}

	public bool canInteract(Player player)
	{
		return items.Count > 0; // && player.money >= cost;
	}

	public void interact(Player player)
	{
		player.clearInventory();
		for (int i = 0; i < items.Count; i++)
			player.giveItem(items[i]);
		items.Clear();
		player.money = Math.Max(player.money - cost, 0);
	}

	public void onFocusEnter(Player player)
	{
		outline = OUTLINE_COLOR;
	}

	public void onFocusLeft(Player player)
	{
		outline = 0;
	}

	Vector2 getWeaponOrigin(bool mainHand)
	{
		return new Vector2((!mainHand ? 4 / 16.0f : -3 / 16.0f), (!mainHand ? 5 / 16.0f : 4 / 16.0f));
	}

	public override void render()
	{
		if (outline != 0)
		{
			Renderer.DrawOutline(position.x - 0.5f, position.y, LAYER_BG + 0.001f, 1, 1, 0, sprite, false, outline);

			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].ingameSprite != null)
				{
					Renderer.DrawOutline(position.x - 0.5f * items[i].ingameSpriteSize, position.y + 1.0f / 16 + 0.5f - 0.5f * items[i].ingameSpriteSize, LAYER_BG + 0.001f, items[i].ingameSpriteSize, items[i].ingameSpriteSize, 0, items[i].ingameSprite, false, outline);
				}
				else if (items[i].isHandItem)
				{
					Renderer.DrawOutline(position.x - 0.5f * items[i].size.x + items[i].renderOffset.x + getWeaponOrigin(true).x, position.y + 1.0f / 16 - 0.5f * items[i].size.y + getWeaponOrigin(true).y, LAYER_BG + 0.001f, items[i].size.x, items[i].size.y, 0, items[i].sprite, false, outline);
				}
			}
		}

		Renderer.DrawSprite(position.x - 0.5f, position.y, LAYER_BG, 1, 1, 0, sprite);

		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].ingameSprite != null)
			{
				Renderer.DrawSprite(position.x - 0.5f * items[i].ingameSpriteSize, position.y + 1.0f / 16 + 0.5f - 0.5f * items[i].ingameSpriteSize, LAYER_BG - 0.001f, items[i].ingameSpriteSize, items[i].ingameSpriteSize, 0, items[i].ingameSprite, false, items[i].ingameSpriteColor);
			}
			else if (items[i].isHandItem)
			{
				Renderer.DrawSprite(position.x - 0.5f * items[i].size.x + items[i].renderOffset.x + getWeaponOrigin(true).x, position.y + 1.0f / 16 - 0.5f * items[i].size.y + getWeaponOrigin(true).y, LAYER_BG - 0.002f, items[i].size.x, items[i].size.y, 0, items[i].sprite, false, items[i].spriteColor);
			}
		}

		if (label != null && outline != 0)
		{
			Renderer.DrawWorldTextBMP(position.x - Renderer.MeasureWorldTextBMP(label).x / 2 / 16, position.y + 2.5f, 0, label, 1.0f / 16, 0xFFAAAAAA);
		}
	}
}
