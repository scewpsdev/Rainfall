using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Level2
{
	public static void Generate(LevelGenerator generator, Door lastExit, ref Level lastLevel, List<Level> levels, Simplex simplex)
	{
		// Merchant 1
		{
			Level level = new Level(2, "Level 2");
			generator.generateSingleRoomLevel(level, generator.specialSet, 2, TileType.stone, TileType.rock);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.rock);

			BuilderMerchant merchant = new BuilderMerchant();
			merchant.buysItems = true;
			merchant.addShopItem(new Longbow());
			merchant.addShopItem(new Arrow() { stackSize = 20 }, 1);
			merchant.addShopItem(new ElderwoodStaff());
			merchant.addShopItem(new HealingSpell());
			merchant.addShopItem(new AmethystRing());
			merchant.addShopItem(new BlademastersRing());
			merchant.addShopItem(new Shortsword());
			merchant.addShopItem(new Scimitar());
			merchant.addShopItem(new Spear());
			merchant.addShopItem(new Cheese() { stackSize = 3 });
			level.addEntity(merchant, (Vector2)level.rooms[0].getMarker(0x1));

			generator.connectDoors(lastExit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Slime
		{
			Level level = new Level(2, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 5, TileType.stone, TileType.rock);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.rock);

			level.addEntity(new BossRoom(level.rooms[0], [new Slime(), new Slime(), new Slime()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Blue Slime
		{
			Level level = new Level(2, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 5, TileType.stone, TileType.rock);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.rock);

			level.addEntity(new BossRoom(level.rooms[0], [new BlueSlime(), new Slime()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Skeleton
		{
			Level level = new Level(2, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 5, TileType.stone, TileType.rock);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.rock);

			level.addEntity(new BossRoom(level.rooms[0], [new SkeletonArcher(), new SkeletonArcher()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Merchant 4
		{
			Level level = new Level(2, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 2, TileType.stone, TileType.rock);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.rock);

			Tinkerer merchant = new Tinkerer();
			merchant.addShopItem(new ChainmailArmor());
			merchant.addShopItem(new ChainmailHood());
			merchant.addShopItem(new ChainmailGauntlets());
			merchant.addShopItem(new ChainmailBoots());
			merchant.addShopItem(new BerserkersChain());
			merchant.addShopItem(new RingOfDexterity());
			merchant.addShopItem(new RingOfSwiftness());
			merchant.addShopItem(new RingOfTears());
			merchant.addShopItem(new RingOfThorns());
			merchant.addShopItem(new ThrowingKnife() { stackSize = 30 }, 3);
			merchant.addShopItem(new Cheese() { stackSize = 3 });
			merchant.addShopItem(new PoisonVial() { stackSize = 3 });
			merchant.addShopItem(new Arrow() { stackSize = 50 }, 1);
			level.addEntity(merchant, (Vector2)level.rooms[0].getMarker(0x1));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Stalker
		{
			Level level = new Level(2, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 5, TileType.stone, TileType.rock);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.rock);

			level.addEntity(new BossRoom(level.rooms[0], [
				new Stalker() { direction = Random.Shared.Next() % 2 * 2 - 1 },
				new Stalker() { direction = Random.Shared.Next() % 2 * 2 - 1 },
				new Stalker() { direction = Random.Shared.Next() % 2 * 2 - 1 },
				new Stalker() { direction = Random.Shared.Next() % 2 * 2 - 1 },
				new Stalker() { direction = Random.Shared.Next() % 2 * 2 - 1 },
				new Stalker() { direction = Random.Shared.Next() % 2 * 2 - 1 },
				new Stalker() { direction = Random.Shared.Next() % 2 * 2 - 1 },
			]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Combo 3
		{
			Level level = new Level(2, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 5, TileType.stone, TileType.rock);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.rock);

			level.addEntity(new BossRoom(level.rooms[0], [new GreenSpider(), new SkeletonArcher(), new OrangeBat(), new BlueSlime(), new Stalker(), new Stalker()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Merchant 4
		{
			Level level = new Level(2, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 2, TileType.stone, TileType.rock);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.rock);

			Tinkerer merchant = new Tinkerer();
			merchant.addShopItem(new BattleAxe());
			merchant.addShopItem(new Halberd());
			merchant.addShopItem(new Longsword());
			merchant.addShopItem(new Rapier());
			merchant.addShopItem(new MoonbladeAxe());
			merchant.addShopItem(new Crossbow());
			//merchant.addShopItem(new Flail());
			merchant.addShopItem(new Greataxe());
			merchant.addShopItem(new Greathammer());
			merchant.addShopItem(new Zweihander());
			merchant.addShopItem(new RoyalGreatsword());
			merchant.addShopItem(new Twinblades());
			merchant.addShopItem(new AstralScepter());
			merchant.addShopItem(new BattleAxe());
			merchant.addShopItem(new Halberd());
			merchant.addShopItem(new Longsword());
			merchant.addShopItem(new Rapier());
			merchant.addShopItem(new MoonbladeAxe());
			merchant.addShopItem(new IronShield());
			merchant.addShopItem(new SpectralShield());
			merchant.addShopItem(new MissileSpell());
			merchant.addShopItem(new GoldenApple());
			level.addEntity(merchant, (Vector2)level.rooms[0].getMarker(0x1));

			Blacksmith blacksmith = NPCManager.blacksmith;
			level.addEntity(blacksmith, (Vector2)level.rooms[0].getMarker(0x2));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Gandalf
		{
			Level level = new Level(2, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 5, TileType.stone, TileType.rock);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.rock);

			level.addEntity(new BossRoom(level.rooms[0], [new Gandalf()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Combo
		{
			Level level = new Level(2, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 5, TileType.stone, TileType.rock);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.rock);

			level.addEntity(new BossRoom(level.rooms[0], [new Gandalf(), new SkeletonArcher(), new OrangeBat()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Combo
		{
			Level level = new Level(2, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 5, TileType.stone, TileType.rock);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.rock);

			level.addEntity(new BossRoom(level.rooms[0], [new GolemBoss(), new Stalker() { direction = Random.Shared.Next() % 2 * 2 - 1 }, new Stalker() { direction = Random.Shared.Next() % 2 * 2 - 1 }]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Merchant 4
		{
			Level level = new Level(2, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 2, TileType.stone, TileType.rock);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.rock);

			TravellingMerchant merchant = new TravellingMerchant();
			merchant.addShopItem(new NimbleGloves());
			merchant.addShopItem(new PotionOfHealing() { stackSize = 2 });
			merchant.addShopItem(new Bread() { stackSize = 5 });
			merchant.addShopItem(new Arrow() { stackSize = 50 }, 1);
			level.addEntity(merchant, (Vector2)level.rooms[0].getMarker(0x1));

			Blacksmith blacksmith = new Blacksmith();
			level.addEntity(blacksmith, (Vector2)level.rooms[0].getMarker(0x2));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Raya
		{
			Level level = new Level(2, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 5, TileType.stone, TileType.rock);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.rock);

			level.addEntity(new BossRoom(level.rooms[0], [new Raya()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Exit
		{
			Level level = new Level(2, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 4, TileType.stone, TileType.rock, 0, 0x1);
			generator.generateCaveBackground(level, simplex, TileType.stone, TileType.rock);

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}
	}
}
