using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum UpgradeType
{
	Health,
	Firerate,
	Damage,
	Speed,

	Count
}

public class Upgrade : Entity, Interactable
{
	UpgradeType type;
	int cost;

	bool consumed = false;


	public Upgrade(UpgradeType type, int cost, float x, float y)
	{
		this.type = type;
		this.cost = cost;
		position = new Vector2(x, y);
		hitbox = new FloatRect(-0.5f, -0.5f, 1, 1);
	}

	public override void reset()
	{
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
				Gaem.instance.manager.pointsSpent += cost;

				if (type == UpgradeType.Health)
				{
					player.maxHealth += 2;
					player.health = player.maxHealth;
				}
				else if (type == UpgradeType.Firerate)
				{
					player.fireRate += 5;
				}
				else if (type == UpgradeType.Damage)
				{
					player.damage += player.damage / 2;
				}
				else if (type == UpgradeType.Speed)
				{
					player.speed += 2;
				}

				consumed = true;
			}
		}
	}

	public override void draw()
	{
		Renderer.DrawSprite(position.x - 0.25f, position.y - 0.25f, 0.5f, 0.5f, null, false, 0xff6abe30);
	}
}
