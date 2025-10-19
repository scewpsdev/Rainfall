using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Shop : Entity, Interactable
{
	int x, y;

	SpriteSheet sheet;
	Sprite sprite;

	Upgrade[] upgrades = new Upgrade[3];


	public Shop(int x, int y)
	{
		this.x = x;
		this.y = y;
		position = new Vector2(x, y);

		sheet = new SpriteSheet(Resource.GetTexture("res/sprites/shop.png", false), 64, 32);
		sprite = new Sprite(sheet, 0, 0);

		staticCollider = true;
		collider = new FloatRect(-4, 0, 8, 4);
		hitbox = new FloatRect(-4, 0, 8, 4);

		TopDownTest.instance.manager.addShop(this);
	}

	public void refill()
	{
		for (int i = 0; i < 3; i++)
		{
			if (upgrades[i] != null)
				level.removeEntity(upgrades[i]);
			UpgradeType type = (UpgradeType)(Random.Shared.Next() % (int)UpgradeType.Count);
			while (type == UpgradeType.Ricochet)
			{
				float keepChance = 0.1f;
				if (Random.Shared.NextSingle() < keepChance)
					break;
				type = (UpgradeType)(Random.Shared.Next() % (int)UpgradeType.Count);
			}
			int cost = TopDownTest.instance.manager.upgradeCost;
			upgrades[i] = new Upgrade(type, cost, x - 2 + i * 2, y - 0.01f);
			level.addEntity(upgrades[i]);
		}
	}

	public void clear()
	{
		for (int i = 0; i < 3; i++)
		{
			if (upgrades[i] != null)
			{
				level.removeEntity(upgrades[i]);
				upgrades[i] = null;
			}
		}
	}

	public bool canInteract(Entity entity)
	{
		bool isEmpty = true;
		for (int i = 0; i < 3; i++)
		{
			if (upgrades[i] != null)
				isEmpty = false;
		}
		return isEmpty;
	}

	public void getInteractionPrompt(Entity entity, out string text, out uint color)
	{
		Player player = entity as Player;

		int cost = TopDownTest.instance.manager.upgradeCost / 4 * 3;
		text = "[E] Refill " + cost;
		color = player.points >= cost ? 0xFF5555FF : 0xFFFF7777;
	}

	public void interact(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;

			int cost = TopDownTest.instance.manager.upgradeCost / 4 * 3;
			if (player.points >= cost)
			{
				player.points -= cost;
				TopDownTest.instance.manager.pointsSpent += cost;
				refill();

				player.audio.playSoundOrganic(player.sfxPowerup);
			}
		}
	}

	public override void update()
	{
		for (int i = 0; i < 3; i++)
		{
			if (upgrades[i] != null && upgrades[i].consumed)
			{
				TopDownTest.instance.manager.upgradeCost += TopDownTest.instance.manager.upgradeCost / 4;
				TopDownTest.instance.manager.clearShops();
				break;
			}
		}
	}

	public override void draw()
	{
		Renderer.DrawVerticalSprite(position.x - 4, position.y, 8, 4, sprite, false);
	}
}
