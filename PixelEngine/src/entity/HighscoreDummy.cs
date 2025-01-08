using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HighscoreDummy : Entity
{
	RunData data;
	Sprite sprite;

	string[] label;
	uint color;


	public HighscoreDummy(RunData data, string[] label, uint color)
	{
		this.data = data;
		this.label = label;
		this.color = color;
		sprite = new Sprite(Resource.GetTexture("sprites/player.png", false), 0, 0, 16, 16);
	}

	public Vector2 getWeaponOrigin(bool mainHand)
	{
		return new Vector2((!mainHand ? 4 / 16.0f : -3 / 16.0f), (!mainHand ? 5 / 16.0f : 4 / 16.0f));
	}

	void renderHandItem(float layer, bool mainHand, Item item)
	{
		uint color = mainHand ? 0xFFFFFFFF : 0xFF7F7F7F;

		if (item != null && item.sprite != null)
		{
			Vector2 weaponPosition = new Vector2(position.x + (item.renderOffset.x + getWeaponOrigin(mainHand).x), position.y + getWeaponOrigin(mainHand).y);
			Renderer.DrawSprite(weaponPosition.x - 0.5f * item.size.x, weaponPosition.y - 0.5f * item.size.y, layer, item.size.x, item.size.y, 0, item.sprite, false, color);
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, 1, 1, sprite, false, 0xFFFFFFFF);

		for (int i = 0; i < data.passiveItems.Length; i++)
		{
			if (data.passiveItems[i] != null)
			{
				if (data.passiveItems[i].ingameSprite != null)
					Renderer.DrawSprite(position.x - 0.5f * data.passiveItems[i].ingameSpriteSize, position.y + 0.5f - 0.5f * data.passiveItems[i].ingameSpriteSize, LAYER_PLAYER_ARMOR, data.passiveItems[i].ingameSpriteSize, data.passiveItems[i].ingameSpriteSize, 0, data.passiveItems[i].ingameSprite, false, data.passiveItems[i].ingameSpriteColor);
			}
		}

		renderHandItem(LAYER_PLAYER_ITEM_MAIN, true, data.handItems[0]);
		renderHandItem(LAYER_PLAYER_ITEM_SECONDARY, false, data.handItems[1]);

		Renderer.DrawLight(position + new Vector2(0, 2), new Vector3(1.0f) * 1.5f, 5);

		for (int i = 0; i < label.Length; i++)
		{
			float width = Renderer.MeasureWorldTextBMP(label[i], -1, 1.0f / 16).x;
			Renderer.DrawWorldTextBMP(position.x - 0.5f * width + 0.01f, position.y + 3 + (label.Length - i - 1) * 0.5f, 0, label[i], 1.0f / 16, i < label.Length - 1 ? 0xFFAAAAAA : color);
		}
	}
}
