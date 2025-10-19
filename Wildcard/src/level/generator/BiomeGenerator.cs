using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public abstract class BiomeGenerator
{
	protected Level level;
	protected byte[] biomes;
	protected int biome;
	protected float[] lootValues;

	protected bool[] objectFlags;

	protected string seed;
	public Random random;
	protected Simplex simplex;


	public virtual void setup(Level level, byte[] biomes, int biome, float[] lootValues, string seed)
	{
		this.level = level;
		this.biomes = biomes;
		this.biome = biome;
		this.lootValues = lootValues;
		this.seed = seed;

		objectFlags = new bool[level.width * level.height];

		random = new Random((int)Hash.combine(Hash.hash(seed), Hash.hash(biome)));
		simplex = new Simplex(Hash.hash(seed), 3);
	}

	protected bool getMask(int x, int y) => biomes[x + y * level.width] == biome;

	public void setObjectFlag(int x, int y, bool flag = true)
	{
		objectFlags[x + y * level.width] = flag;
	}

	public bool getObjectFlag(int x, int y) => objectFlags[x + y * level.width];

	public float getLootValue(Vector2i tile) => lootValues.Length > 1 ? lootValues[tile.x + tile.y * level.width] : lootValues[0];

	public void generateSimplexBackground()
	{
		for (int y = 0; y < level.height; y++)
		{
			for (int x = 0; x < level.width; x++)
			{
				if (getMask(x, y))
				{
					float type = simplex.sample2f(-x * 0.05f, -y * 0.05f);
					TileType tile = type > 0 ? getBackgroundTile(-x, -y) : null;
					level.setBGTile(x, y, tile);
				}
			}
		}
	}

	public void placeEntity(Entity entity, Vector2i tile, Vector2 offset = default)
	{
		offset += new Vector2(0.5f, 0);
		if (entity.collider != null && entity.collider.min.y != 0)
			offset.y += 0.5f;
		level.addEntity(entity, tile + offset);
		objectFlags[tile.x + tile.y * level.width] = true;
	}

	public void spawnChest(Vector2i tile, float lootValueMultiplier = 1)
	{
		float scamChestChance = 0.02f;
		bool scam = random.NextSingle() < scamChestChance;

		TileType left = level.getTile(tile.x - 1, tile.y);
		TileType right = level.getTile(tile.x + 1, tile.y);
		Item[] items = scam ? [new Bomb().cook()] : Item.CreateRandom(random, DropRates.chest, WorldManager.biomeLootValues[biome] * lootValueMultiplier);
		Chest chest = new Chest(items, left != null && right == null);
		placeEntity(chest, tile);

		float chestCoinsChance = 0.1f;
		if (random.NextSingle() < chestCoinsChance)
		{
			int amount = MathHelper.RandomInt(0, WorldManager.biomeLootValues[biome], random);
			chest.coins = amount;
		}
	}

	public bool spawnEnemy(Vector2i tile, Mob enemy)
	{
		int x = tile.x;
		int y = tile.y;

		TileType up = level.getTile(x, y + 1);
		TileType down = level.getTile(x, y - 1);
		TileType left = level.getTile(x - 1, y);
		TileType right = level.getTile(x + 1, y);

		TileType downLeft = level.getTile(x - 1, y - 1);
		TileType downRight = level.getTile(x + 1, y - 1);

		if (!enemy.canFly && enemy.gravity != 0 && left == null && right == null && up == null && down != null && (downLeft != null || downRight != null)
			|| enemy.canFly && left == null && right == null
			|| enemy.gravity == 0 && (down != null || up != null))
		{
			enemy.direction = random.NextSingle() < 0.5f ? 1 : -1;

			float itemDropChance = 0.075f;
			while (itemDropChance > 0 && random.NextSingle() < itemDropChance)
			{
				Item[] drops = Item.CreateRandom(random, DropRates.mob, WorldManager.biomeLootValues[biome] * enemy.itemDropValueMultiplier);
				foreach (Item drop in drops)
					enemy.itemDrops.Add(drop);
				itemDropChance--;
			}

			placeEntity(enemy, tile);
			return true;
		}

		return false;
	}

	protected void spawnItem(Vector2i tile, Item[] items)
	{
		int x = tile.x;
		int y = tile.y;

		float chestChance = 0.1f;
		float barrelChance = 0.4f;

		float f = random.NextSingle();
		if (f < chestChance)
		{
			float scamChestChance = 0.05f;
			bool scam = random.NextSingle() < scamChestChance;
			if (scam)
				items = [new Bomb().cook()];

			TileType left = level.getTile(x - 1, y);
			TileType right = level.getTile(x + 1, y);
			Chest chest = new Chest(items, left != null && right == null);
			placeEntity(chest, tile);

			float chestCoinsChance = 0.03f;
			if (random.NextSingle() < chestCoinsChance)
			{
				int amount = MathHelper.RandomInt(10, 20, random);
				chest.coins = amount;
			}
		}
		else if (f < chestChance + barrelChance)
		{
			Container container = createContainer(items);
			placeEntity(container, tile);

			float coinsChance = 0.08f;
			if (random.NextSingle() < coinsChance)
			{
				int amount = MathHelper.RandomInt(1, 6, random);
				container.coins = amount;
			}
		}
		else
		{
			foreach (Item item in items)
			{
				ItemEntity itemEntity = new ItemEntity(item);
				placeEntity(itemEntity, tile, new Vector2(0, 0.5f));
			}
		}
	}

	public abstract TileType getBackgroundTile(int x, int y);
	public abstract void generateBaseLevel();
	public abstract void spawnNPC(Vector2i tile);
	public abstract Container createContainer(Item[] items);
	public abstract ExplosiveObject createExplosiveObject();
}
