using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Level1
{
	public static void Generate(LevelGenerator generator, Door lastExit, ref Level lastLevel, List<Level> levels, Simplex simplex)
	{
		// Merchant 1
		{
			Level level = new Level(1, "Level 1");
			generator.generateSingleRoomLevel(level, generator.specialSet, 2, TileType.dirt, TileType.stone);
			generator.generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

			BuilderMerchant merchant = new BuilderMerchant();
			merchant.addShopItem(new BrokenSword());
			merchant.addShopItem(new Club());
			merchant.addShopItem(new Dagger());
			merchant.addShopItem(new Handaxe());
			merchant.addShopItem(new Shortbow(), 4);
			merchant.addShopItem(new Arrow() { stackSize = 6 }, 1);
			merchant.addShopItem(new MagicStaff(), 5);
			merchant.addShopItem(new MagicArrowSpell(), 5);
			merchant.addShopItem(new Bomb() { stackSize = 3 }, 3);
			merchant.addShopItem(new ThrowingKnife() { stackSize = 10 }, 1);
			level.addEntity(merchant, (Vector2)level.rooms[0].getMarker(0x1));

			generator.connectDoors(lastExit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Rat
		{
			Level level = new Level(1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 3, TileType.dirt, TileType.stone);
			generator.generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

			level.addEntity(new BossRoom(level.rooms[0], new Rat()));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Snake
		{
			Level level = new Level(1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 3, TileType.dirt, TileType.stone);
			generator.generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

			level.addEntity(new BossRoom(level.rooms[0], new Snake()));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Spider
		{
			Level level = new Level(1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 3, TileType.dirt, TileType.stone);
			generator.generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

			level.addEntity(new BossRoom(level.rooms[0], new Spider()));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Bat
		{
			Level level = new Level(1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 3, TileType.dirt, TileType.stone);
			generator.generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

			level.addEntity(new BossRoom(level.rooms[0], new Bat()));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Merchant 2
		{
			Level level = new Level(1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 2, TileType.dirt, TileType.stone);
			generator.generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

			BrokenWanderer merchant = new BrokenWanderer();
			merchant.addShopItem(new Apple());
			merchant.addShopItem(new Bread());
			merchant.addShopItem(new Cheese());
			merchant.addShopItem(new RoundShield());
			merchant.addShopItem(new WoodenShield());
			merchant.addShopItem(new AdventurersHoodBlue());
			merchant.addShopItem(new TravellingCloak());
			merchant.addShopItem(new LargeWizardHat());
			merchant.addShopItem(new Arrow() { stackSize = 10 }, 1);
			level.addEntity(merchant, (Vector2)level.rooms[0].getMarker(0x1));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Green Spider
		{
			Level level = new Level(1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 3, TileType.dirt, TileType.stone);
			generator.generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

			level.addEntity(new BossRoom(level.rooms[0], new GreenSpider()));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Orange Bat
		{
			Level level = new Level(1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 3, TileType.dirt, TileType.stone);
			generator.generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

			level.addEntity(new BossRoom(level.rooms[0], new OrangeBat()));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		/*
		// Combo 1
		{
			Level level = new Level(1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 3, TileType.dirt, TileType.stone);
			generator.generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

			level.addEntity(new BossRoom(level.rooms[0], [new Bat(), new Bat(), new Spider()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Combo 2
		{
			Level level = new Level(1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 3, TileType.dirt, TileType.stone);
			generator.generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

			level.addEntity(new BossRoom(level.rooms[0], [new Snake(), new Spider(), new Spider(), new Bat()]));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}
		*/

		// Merchant 3
		{
			Level level = new Level(1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 2, TileType.dirt, TileType.stone);
			generator.generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

			Tinkerer merchant = new Tinkerer();
			merchant.addShopItem(new Shortsword());
			merchant.addShopItem(new Scimitar());
			merchant.addShopItem(new Spear());
			merchant.addShopItem(new Quarterstaff());
			merchant.addShopItem(new WoodenMallet());
			merchant.addShopItem(new Boomerang());
			merchant.addShopItem(new Flail());
			merchant.addShopItem(new LeatherArmor());
			merchant.addShopItem(new LeatherCap());
			merchant.addShopItem(new LeatherGauntlets());
			merchant.addShopItem(new LeatherBoots());
			merchant.addShopItem(new TripleShotSpell());
			merchant.addShopItem(new BurstShotSpell());
			merchant.addShopItem(new LightningSpell());
			merchant.addShopItem(new PotionOfHealing());
			merchant.addShopItem(new Bread() { stackSize = 2 });
			merchant.addShopItem(new Arrow() { stackSize = 30 }, 1);
			level.addEntity(merchant, (Vector2)level.rooms[0].getMarker(0x1));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Golem
		{
			Level level = new Level(1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 3, TileType.dirt, TileType.stone);
			generator.generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

			level.addEntity(new BossRoom(level.rooms[0], new GolemBoss()));

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}

		// Exit
		{
			Level level = new Level(1, "");
			generator.generateSingleRoomLevel(level, generator.specialSet, 4, TileType.dirt, TileType.stone, 0, 0x1);
			generator.generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

			generator.connectDoors(lastLevel.exit, level.entrance);
			levels.Add(level);
			lastLevel = level;
		}
	}
}
