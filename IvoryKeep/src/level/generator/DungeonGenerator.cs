using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


public class DungeonGenerator : LevelGenerator
{
	public DungeonGenerator(string seed)
		: base("dungeon", "Dungeon", dungeonsSet, TileType.bricks, TileType.stone, seed)
	{
	}

	public override string getAreaName()
	{
		return "The Weeping Catacombs";
	}

	public override int getAreaFirstFloor()
	{
		return 12;
	}

	public override int getNumFloors()
	{
		return 4;
	}

	public override string getLevelName(int floor)
	{
		return floor == 0 ? "The Weeping Catacombs" : "";
	}

	public override int getLootValue(int floor)
	{
		return 45 + (floor + 1) * 4;
	}

	public override void getLevelSize(int floor, out int width, out int height)
	{
		width = 40;
		height = 60;

		float wideLevelChance = 0.2f;
		if (random.NextSingle() < wideLevelChance)
			width = 60;
	}

	public override TileType getDefaultTile(int floor)
	{
		return null;
	}

	public override int getAmbientLight()
	{
		return 2; // Mathf.ARGBToVector(0xFF3b3159).xyz;
	}

	public override Sound getAmbientSound()
	{
		return Resource.GetSound("sounds/ambience.ogg");
	}

	public override MultilayerTrack getAmbientTrack()
	{
		return dungeonsAmbientTrack;
	}

	public override Door createEntranceDoor(Level lastLevel, Door lastExit)
	{
		return new CaveEntranceDoor(lastLevel, lastExit);
	}

	public override BossRoom createBossRoom(Room room)
	{
		return new DungeonsBossRoom(room);
	}

	public override RoomDef[] getDeadEndRoomDefs()
	{
		RoomDef[] secretRooms = new RoomDef[6];
		secretRooms[0] = specialSet.roomDefs[6];
		secretRooms[1] = specialSet.roomDefs[random.Next() % 2 == 0 ? 7 : 8];
		secretRooms[2] = specialSet.roomDefs[random.Next() % 2 == 0 ? 9 : 10];
		secretRooms[3] = specialSet.roomDefs[11];
		secretRooms[4] = !GameState.instance.save.areAllStartingClassesUnlocked() ? cavesSpecialSet.roomDefs[0] : specialSet.roomDefs[6];
		secretRooms[5] = specialSet.roomDefs[20];

		return secretRooms;
	}

	public override Entity createDeadEndRoomEntity(int type, Room room)
	{
		if (type == 0) return new CavesSpecialRoom1(room, this);
		if (type == 1) return new CavesSpecialRoom2(room, this);
		if (type == 2) return new CavesSpecialRoom3(room, this);
		if (type == 3) return new CavesPlatformingRoom1(room, this);
		if (type == 4) return !GameState.instance.save.areAllStartingClassesUnlocked() ? new PrisonCellRoom(room, this) : new CavesSpecialRoom1(room, this);
		return null;
	}

	public override RoomDef[] getSecretRoomDefs()
	{
		RoomDef[] secretRooms = new RoomDef[6];
		secretRooms[0] = specialSet.roomDefs[6];
		secretRooms[1] = specialSet.roomDefs[7];
		secretRooms[2] = specialSet.roomDefs[9];
		secretRooms[3] = specialSet.roomDefs[11];
		secretRooms[4] = !GameState.instance.save.areAllStartingClassesUnlocked() ? cavesSpecialSet.roomDefs[0] : specialSet.roomDefs[6];
		secretRooms[5] = specialSet.roomDefs[20];

		return secretRooms;
	}

	public override Entity createSecretRoomEntity(int type, Room room)
	{
		if (type == 0) return new CavesSpecialRoom1(room, this);
		if (type == 1) return new CavesSpecialRoom2(room, this);
		if (type == 2) return new CavesSpecialRoom3(room, this);
		if (type == 3) return new CavesPlatformingRoom1(room, this);
		if (type == 4) return !GameState.instance.save.areAllStartingClassesUnlocked() ? new PrisonCellRoom(room, this) : new CavesSpecialRoom1(room, this);
		return null;
	}

	public override float[] getDroprates()
	{
		return DropRates.cavesDroprates;
	}

	public override int getNumItems(int floor)
	{
		return Mathf.RollDice(2, 2, random) - 1;
	}

	public override Container createContainer(Item[] items)
	{
		return new Pot(items);
	}

	public override ExplosiveObject createExplosiveObject()
	{
		return new ExplosivePot();
	}

	public override List<Mob> createEnemy(Level level)
	{
		List<Mob> mobs = new List<Mob>();
		if (level.lightLevel <= 3)
			mobs.Add(new GreenSpider());
		else
			mobs.Add(new SkeletonArcher());
		mobs.Add(new OrangeBat());
		mobs.Add(new BlueSlime());
		mobs.Add(new SkeletonArcher());
		if (getLocalFloor() >= 1)
			mobs.Add(new Leprechaun());
		if (getLocalFloor() >= 2)
			mobs.Add(new Stalker());
		if (getLocalFloor() >= 3)
			mobs.Add(new Gandalf());
		return mobs;
	}


	public override NPC createNPC(int type, Level level)
	{
		List<NPC> npcs = new List<NPC>();
		npcs.Add(new TravellingMerchant(random, level));
		if (!QuestManager.tryGetQuest(GameState.instance.save, "logan", "logan_quest", out Quest loganQuest) || loganQuest.state != QuestState.InProgress)
			npcs.Add(new Logan() /*NPCManager.logan*/);
		//npcs.Add(new Tinkerer() /*NPCManager.tinkerer*/);

		return npcs[type % npcs.Count];
	}

	public override void onFloorFinish(Level level)
	{
		// Sconces
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down != null && up == null)
			{
				float sconceChance = 0.02f;
				if (random.NextSingle() < sconceChance)
				{
					level.addEntity(new FireSconce(), new Vector2(x + 0.5f, y));
					setObjectFlag(x, y);
				}
			}
		});
	}
}
