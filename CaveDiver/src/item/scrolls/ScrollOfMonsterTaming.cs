using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ScrollOfMonsterTaming : Item
{
	public ScrollOfMonsterTaming()
		: base("scroll_of_monster_taming", ItemType.Scroll)
	{
		displayName = "Scroll of Monster Taming";

		value = 15;

		sprite = new Sprite(tileset, 12, 2);
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
		float range = 8;
		HitData[] hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(player.position - range, player.position + range, hits, Entity.FILTER_MOB);
		for (int i = 0; i < numHits; i++)
		{
			Entity entity = hits[i].entity;
			Debug.Assert(entity is Mob);
			Mob mob = entity as Mob;
			if (mob.ai != null)
			{
				mob.ai.aggroRange = 0;
				mob.ai.setTarget(null);
			}
		}
		player.hud.showMessage("The air fizzles.");

		player.level.addEntity(ParticleEffects.CreateScrollUseEffect(player), player.position + player.collider.center);

		return true;
	}
}
