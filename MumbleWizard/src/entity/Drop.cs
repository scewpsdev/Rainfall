using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


public enum DropType
{
	Health,
	Points,

	Count
}

public class Drop : Entity, Toucheable
{
	static SpriteSheet sheet;
	static Texture shadow;

	static Drop()
	{
		sheet = new SpriteSheet(Resource.GetTexture("res/sprites/items.png", false), 8, 8);
		shadow = Resource.GetTexture("res/sprites/shadow.png", true);
	}


	DropType type;

	Sprite sprite;


	public Drop(DropType type, Vector2 position)
	{
		this.type = type;
		this.position = position;

		sprite = new Sprite(sheet, 0, (int)type);

		collider = new FloatRect(-0.25f, 0.0f, 0.5f, 1.0f);
		hitbox = new FloatRect(-0.25f, 0.0f, 0.5f, 1.0f);
	}

	public override void reset()
	{
		removed = true;
	}

	public void touch(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;

			bool consumed = true;
			if (type == DropType.Health)
			{
				if (player.health < player.maxHealth)
				{
					player.health++;
				}
				else
				{
					consumed = false;
				}
			}
			else if (type == DropType.Points)
			{
				int numOrbs = (Gaem.instance.manager.doorCost + Gaem.instance.manager.upgradeCost) / 20;
				int amount = 10;
				while (numOrbs > 500)
				{
					numOrbs /= 10;
					amount *= 10;
				}
				for (int i = 0; i < numOrbs; i++)
				{
					float angle = MathHelper.RandomFloat(0.0f, MathF.PI * 2);
					Vector2 offset = Vector2.Rotate(Vector2.UnitX, angle);
					XPOrb orb = new XPOrb(position + offset, player, amount);
					float speed = MathHelper.Lerp(4, 8, angle / MathF.PI * 0.5f);
					orb.velocity = offset * speed;
					level.addEntity(orb);
				}
			}
			if (consumed)
			{
				player.audio.playSoundOrganic(player.sfxPowerup);
				removed = true;
			}
		}
	}

	public override void draw()
	{
		float height = 1.0f + MathF.Sin(Hash.hash(position.x) % 4 + Time.currentTime / 1e9f * 2) * 0.5f;
		Renderer.DrawVerticalSprite(position.x - 0.5f, position.y, height, 1.0f, 1.0f, sprite, false);
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 0.01f, 1, 1, shadow, 0, 0, 8, 8, 0xFFFFFFFF);
		Renderer.DrawLight(position + new Vector2(0.0f, height), new Vector3(1.0f, 0.8f, 0.6f) * 3, 4.0f);
	}
}
