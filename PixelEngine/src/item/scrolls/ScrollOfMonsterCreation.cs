using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ScrollOfMonsterCreation : Item
{
	public ScrollOfMonsterCreation()
		: base("scroll_create_monster", ItemType.Scroll)
	{
		displayName = "Scroll of Monster Creation";

		value = 29;

		sprite = new Sprite(tileset, 4, 10);
		spellIcon = new Sprite(tileset, 12, 2);
	}

	Vector2i getSpawnTile(Vector2 position)
	{
		int x0 = Math.Max((int)position.x - 4, 0);
		int x1 = Math.Min((int)position.x + 4, GameState.instance.level.width - 1);
		int y0 = Math.Max((int)position.y - 4, 0);
		int y1 = Math.Min((int)position.y + 4, GameState.instance.level.height - 1);

		for (int i = 0; i < 1000; i++)
		{
			int x = MathHelper.RandomInt(x0, x1);
			int y = MathHelper.RandomInt(y0, y1);
			TileType tile = GameState.instance.level.getTile(x, y);
			if (tile == null || !tile.isSolid)
			{
				return new Vector2i(x, y);
			}
		}
		Debug.Assert(false);
		return Vector2i.Zero;
	}

	Mob getMonster(Random random)
	{
		float enemyType = random.NextSingle();

		Mob enemy;

		if (enemyType > 0.75f)
			enemy = new Snake();
		else if (enemyType > 0.5f)
		{
			float spiderType = random.NextSingle();
			if (spiderType < 0.9f)
				enemy = new Spider();
			else
				enemy = new GreenSpider();
		}
		else if (enemyType > 0.25f)
		{
			enemy = new Bat();
		}
		else
		{
			enemy = new Rat();
		}

		return enemy;
	}

	public override bool use(Player player)
	{
		int numMonsters = MathHelper.RandomInt(1, 4);
		for (int i = 0; i < numMonsters; i++)
		{
			Vector2i tile = getSpawnTile(player.position);
			Mob monster = getMonster(Random.Shared);
			GameState.instance.level.addEntity(monster, new Vector2(tile.x + 0.5f, tile.y + 0.5f) - monster.collider.center);
		}
		player.hud.showMessage("The air fizzles.");

		player.level.addEntity(ParticleEffects.CreateScrollUseEffect(player), player.position + player.collider.center);

		return true;
	}
}
