using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ArmorStand : Entity, Interactable
{
	StartingClass startingClass;

	Sprite sprite;
	uint outline;

	int direction;


	public ArmorStand(StartingClass startingClass, int direction = 1)
	{
		this.startingClass = startingClass;
		this.direction = direction;

		sprite = new Sprite(tileset, 2, 10);
	}

	public ArmorStand()
		: this(null)
	{
	}

	public bool canInteract(Player player)
	{
		return startingClass != null && player.startingClass != startingClass; // && player.money >= cost;
	}

	public void interact(Player player)
	{
		player.setStartingClass(startingClass);
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
		return new Vector2((!mainHand ? 2 / 16.0f : -3 / 16.0f) * direction, (!mainHand ? 5 / 16.0f : 4 / 16.0f));
	}

	public override void render()
	{
		bool renderClass = startingClass != null && GameState.instance.player.startingClass != startingClass;

		if (outline != 0)
		{
			Renderer.DrawOutline(position.x - 0.5f, position.y, LAYER_BG + 0.002f, 1, 1, 0, sprite, direction == -1, outline);

			if (renderClass)
			{
				for (int i = 0; i < startingClass.items.Length; i++)
				{
					if (startingClass.items[i].ingameSprite != null)
					{
						Renderer.DrawOutline(position.x - 0.5f * startingClass.items[i].ingameSpriteSize, position.y + 1.0f / 16 + 0.5f - 0.5f * startingClass.items[i].ingameSpriteSize, LAYER_BG + 0.002f, startingClass.items[i].ingameSpriteSize, startingClass.items[i].ingameSpriteSize, 0, startingClass.items[i].ingameSprite, direction == -1, outline);
					}
					else if (startingClass.items[i].isSecondaryItem)
					{
						Renderer.DrawOutline(position.x - 0.5f * startingClass.items[i].size.x + startingClass.items[i].renderOffset.x * direction + getWeaponOrigin(false).x, position.y + 1.0f / 16 - 0.5f * startingClass.items[i].size.y + getWeaponOrigin(false).y, LAYER_BG + 0.002f, startingClass.items[i].size.x, startingClass.items[i].size.y, 0, startingClass.items[i].sprite, direction == -1, outline);
					}
					else if (startingClass.items[i].isHandItem)
					{
						Renderer.DrawOutline(position.x - 0.5f * startingClass.items[i].size.x + startingClass.items[i].renderOffset.x * direction + getWeaponOrigin(true).x, position.y + 1.0f / 16 - 0.5f * startingClass.items[i].size.y + getWeaponOrigin(true).y, LAYER_BG + 0.002f, startingClass.items[i].size.x, startingClass.items[i].size.y, 0, startingClass.items[i].sprite, direction == -1, outline);
					}
				}
			}
		}

		Renderer.DrawSprite(position.x - 0.5f, position.y, LAYER_BG, 1, 1, 0, sprite, direction == -1);

		if (renderClass)
		{
			for (int i = 0; i < startingClass.items.Length; i++)
			{
				if (startingClass.items[i].ingameSprite != null)
				{
					Renderer.DrawSprite(position.x - 0.5f * startingClass.items[i].ingameSpriteSize, position.y + 1.0f / 16 + 0.5f - 0.5f * startingClass.items[i].ingameSpriteSize, LAYER_BG - 0.001f, startingClass.items[i].ingameSpriteSize, startingClass.items[i].ingameSpriteSize, 0, startingClass.items[i].ingameSprite, direction == -1, startingClass.items[i].ingameSpriteColor);
				}
				else if (startingClass.items[i].isSecondaryItem)
				{
					Renderer.DrawSprite(position.x - 0.5f * startingClass.items[i].size.x + startingClass.items[i].renderOffset.x * direction + getWeaponOrigin(false).x, position.y + 1.0f / 16 - 0.5f * startingClass.items[i].size.y + getWeaponOrigin(false).y, LAYER_BG + 0.001f, startingClass.items[i].size.x, startingClass.items[i].size.y, 0, startingClass.items[i].sprite, direction == -1, startingClass.items[i].spriteColor);
				}
				else if (startingClass.items[i].isHandItem)
				{
					Renderer.DrawSprite(position.x - 0.5f * startingClass.items[i].size.x + startingClass.items[i].renderOffset.x * direction + getWeaponOrigin(true).x, position.y + 1.0f / 16 - 0.5f * startingClass.items[i].size.y + getWeaponOrigin(true).y, LAYER_BG - 0.002f, startingClass.items[i].size.x, startingClass.items[i].size.y, 0, startingClass.items[i].sprite, direction == -1, startingClass.items[i].spriteColor);
				}
			}

			if (outline != 0)
			{
				Renderer.DrawWorldTextBMP(position.x - Renderer.MeasureWorldTextBMP(startingClass.name).x / 2 / 16, position.y + 2.5f, 0, startingClass.name, 1.0f / 16, startingClass.color);
			}
		}
	}
}
