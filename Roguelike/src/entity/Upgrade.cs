using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


public enum UpgradeType
{
	Health,
	Firerate,
	Damage,
	Speed,
	Knockback,
	Luck,
	Penetration,
	Ricochet,

	Count
}

public class Upgrade : Entity, Interactable
{
	static SpriteSheet sheet;

	static Upgrade()
	{
		sheet = new SpriteSheet(Resource.GetTexture("res/sprites/upgrades.png", false), 8, 8);
	}


	UpgradeType type;
	int cost;

	Sprite sprite;

	public bool consumed = false;


	public Upgrade(UpgradeType type, int cost, float x, float y)
	{
		this.type = type;
		this.cost = cost;
		position = new Vector2(x, y);
		hitbox = new FloatRect(-0.25f, -0.25f, 0.5f, 0.5f);

		sprite = new Sprite(sheet, (int)type, 0);
	}

	public override void reset()
	{
		//removed = true;
		consumed = false;
	}

	public bool canInteract(Entity entity)
	{
		return !consumed;
	}

	public void getInteractionPrompt(Entity entity, out string text, out uint color)
	{
		Player player = entity as Player;

		text = "[E] Upgrade " + type + ": " + cost;
		color = player.points >= cost ? 0xff6abe30 : 0xFFFF7777;
	}

	public void interact(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;
			if (player.points >= cost)
			{
				player.points -= cost;
				Roguelike.instance.manager.pointsSpent += cost;

				if (type == UpgradeType.Health)
				{
					player.maxHealth++;
					player.health = player.maxHealth;
				}
				else if (type == UpgradeType.Firerate)
				{
					player.fireRate++;
				}
				else if (type == UpgradeType.Damage)
				{
					player.damage++;
				}
				else if (type == UpgradeType.Speed)
				{
					player.speed++;
				}
				else if (type == UpgradeType.Knockback)
				{
					player.knockback += 7;
				}
				else if (type == UpgradeType.Luck)
				{
					player.luck += 5;
				}
				else if (type == UpgradeType.Penetration)
				{
					player.penetration++;
				}
				else if (type == UpgradeType.Ricochet)
				{
					player.ricochet++;
				}
				else
				{
					Debug.Assert(false);
				}

				player.audio.playSoundOrganic(player.sfxPowerup);

				consumed = true;
			}
		}
	}

	public override void draw()
	{
		float height = 1.0f + MathF.Sin(Hash.hash(position.x) % 4 + Time.currentTime / 1e9f * 1.0f) * 0.25f;
		Renderer.DrawVerticalSprite(position.x - 0.5f, position.y - 0.01f, height, 1, 1, sprite, false);
	}
}
