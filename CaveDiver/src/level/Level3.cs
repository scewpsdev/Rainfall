using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Level3
{
	public static void Generate(LevelGenerator generator, Door lastExit, ref Level lastLevel, List<Level> levels, Simplex simplex)
	{
		// Merchant
		{
			Level level = new Level(-1, "Level 3");
			generator.generateSingleRoomLevel(level, generator.specialSet, 2, TileType.stone, TileType.bricks);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.bricks);

			Tinkerer merchant = new Tinkerer();
			merchant.addShopItem(new AutomaticCrossbow());
			merchant.addShopItem(new Arrow() { stackSize = 3000 });
			merchant.addShopItem(new IronArmor());
			merchant.addShopItem(new BarbarianHelmet());
			merchant.addShopItem(new IronGauntlets());
			merchant.addShopItem(new IronSabatons());
			merchant.addShopItem(new Bloodfang());
			merchant.addShopItem(new KeenEdge());
			merchant.addShopItem(new RingOfRetaliation());
			merchant.addShopItem(new Cheese() { stackSize = 5 });
			level.addEntity(merchant, (Vector2)level.rooms[0].getMarker(0x1));

			Blacksmith blacksmith = new Blacksmith();
			level.addEntity(blacksmith, (Vector2)level.rooms[0].getMarker(0x2));

			generator.connectDoors(lastExit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Combo
		{
			Level level = new Level(-1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 6, TileType.stone, TileType.bricks);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.bricks);

			level.addEntity(new BossRoom(level.rooms[0], [new Gandalf(), new Gandalf()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Combo
		{
			Level level = new Level(-1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 6, TileType.stone, TileType.bricks);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.bricks);

			level.addEntity(new BossRoom(level.rooms[0], [
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
				new Stalker() {direction = Random.Shared.Next() % 2 * 2 - 1},
			]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Combo
		{
			Level level = new Level(-1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 6, TileType.stone, TileType.bricks);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.bricks);

			level.addEntity(new BossRoom(level.rooms[0], [new GolemBoss(), new GolemBoss()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Combo
		{
			Level level = new Level(-1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 6, TileType.stone, TileType.bricks);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.bricks);

			level.addEntity(new BossRoom(level.rooms[0], [new Leprechaun(), new Leprechaun(), new Leprechaun(), new Leprechaun(), new Leprechaun()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Combo
		{
			Level level = new Level(-1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 6, TileType.stone, TileType.bricks);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.bricks);

			level.addEntity(new BossRoom(level.rooms[0], [new Leprechaun(), new Gandalf(), new SkeletonArcher(), new BlueSlime()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Combo
		{
			Level level = new Level(-1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 6, TileType.stone, TileType.bricks);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.bricks);

			level.addEntity(new BossRoom(level.rooms[0], [new BlueSlime(), new SkeletonArcher(), new SkeletonArcher(), new SkeletonArcher()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Combo
		{
			Level level = new Level(-1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 6, TileType.stone, TileType.bricks);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.bricks);

			level.addEntity(new BossRoom(level.rooms[0], [new BlueSlime(), new SkeletonArcher(), new SkeletonArcher(), new SkeletonArcher()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Combo
		{
			Level level = new Level(-1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 6, TileType.stone, TileType.bricks);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.bricks);

			level.addEntity(new BossRoom(level.rooms[0], [new GolemBoss(), new Raya()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Merchant
		{
			Level level = new Level(-1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 2, TileType.stone, TileType.bricks);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.bricks);

			TravellingMerchant merchant = new TravellingMerchant();
			merchant.addShopItem(new PotionOfEnergy() { stackSize = 3 });
			merchant.addShopItem(new PotionOfHealing() { stackSize = 3 });
			merchant.addShopItem(new PotionOfGreaterHealing());
			merchant.addShopItem(new PoisonVial() { stackSize = 1 });
			level.addEntity(merchant, (Vector2)level.rooms[0].getMarker(0x1));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Combo
		{
			Level level = new Level(-1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 6, TileType.stone, TileType.bricks);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.bricks);

			level.addEntity(new BossRoom(level.rooms[0], new Garran()));
			//level.addEntity(new BossRoom(level.rooms[0], new Raya()));
			//level.addEntity(new BossRoom(level.rooms[0], new GolemBoss()));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Exit
		{
			Level level = new Level(-1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 7, TileType.stone, TileType.bricks, 0, 0x1);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.bricks);

			level.addEntity(new Pedestal(), level.rooms[0].getMarker(0x2) + new Vector2(0.5f, 0));
			level.addEntity(new ItemEntity(new LostSigil()), level.rooms[0].getMarker(0x2) + new Vector2(0.5f, 1));
			level.addEntity(new TutorialText("Thanks for playing!", 0xFFCCCCCC), level.rooms[0].getMarker(0x2) + new Vector2(0.5f, 5));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}
	}
}
