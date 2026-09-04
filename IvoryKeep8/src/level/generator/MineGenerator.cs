using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MineGenerator : LevelGenerator
{
	public MineGenerator(string seed)
		: base("mines", "Mines", minesSet, TileType.rock, TileType.stone, seed)
	{
	}

	public override string getAreaName()
	{
		return "Crystal Mines";
	}

	public override int getAreaFirstFloor()
	{
		return 5;
	}

	public override int getNumFloors()
	{
		return 4;
	}

	public override string getLevelName(int floor)
	{
		return floor == 0 ? "Crystal Mines" : "";
	}

	public override int getLootValue(int floor)
	{
		return 15 + (floor + 1) * 3;
	}

	public override void getLevelSize(int floor, out int width, out int height)
	{
		width = 70;
		height = 30;

		float wideLevelChance = 0.2f;
		if (random.NextSingle() < wideLevelChance)
			Mathf.Swap(ref width, ref height);

		float squareLevelChance = 0.2f;
		if (random.NextSingle() < squareLevelChance)
		{
			width = 50;
			height = 50;
		}
	}

	public override int getAmbientLight()
	{
		float darkLevelChance = 0.1f;
		float brightLevelChance = 0.1f;
		if (random.NextSingle() < darkLevelChance)
			return 1;
		else if (random.NextSingle() < brightLevelChance)
			return 5;
		else
			return 2;
	}

	public override Sound getAmbientSound()
	{
		return Resource.GetSound("sounds/ambience.ogg");
	}

	public override MultilayerTrack getAmbientTrack()
	{
		return minesAmbientTrack;
	}

	public override Door createEntranceDoor(Level lastLevel, Door lastExit)
	{
		return new CaveEntranceDoor(lastLevel, lastExit);
	}

	public override BossRoom createBossRoom(Room room)
	{
		return new MinesBossRoom(room);
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
		return DropRates.minesDroprates;
	}

	public override int getNumItems(int floor)
	{
		return Mathf.RollDice(2, 2, random) - 1;
	}

	public override Container createContainer(Item[] items)
	{
		return new Barrel(items);
	}

	public override ExplosiveObject createExplosiveObject()
	{
		return new DynamiteObject();
	}

	public override List<Mob> createEnemy(Level level)
	{
		List<Mob> mobs = new List<Mob>();
		if (level.lightLevel <= 3)
			mobs.Add(new Spider());
		else
			mobs.Add(new Snake());
		mobs.Add(new Snake());
		mobs.Add(new Bat());
		//mobs.Add(new Slime());
		if (level.lightLevel <= 3)
			mobs.Add(new GreenSpider());
		else
			mobs.Add(new SkeletonArcher());
		if (getLocalFloor() >= 1)
			mobs.Add(new OrangeBat());
		if (getLocalFloor() >= 2)
			mobs.Add(new BlueSlime());
		if (getLocalFloor() >= 3)
			mobs.Add(new SkeletonArcher());
		return mobs;
	}

	public override List<NPC> getNPCList()
	{
		List<NPC> npcs = new List<NPC>();
		npcs.Add(new BuilderMerchant());
		npcs.Add(new TravellingMerchant());
		if (!QuestManager.tryGetQuest(GameState.instance.save, "logan", "logan_quest", out Quest loganQuest) || loganQuest.state != QuestState.InProgress)
			npcs.Add(new Logan() /*NPCManager.logan*/);
		//npcs.Add(new Tinkerer() /*NPCManager.tinkerer*/);

		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) || GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_COMPLETED))
			npcs.Add(new RatNPC() /*NPCManager.rat*/);

		return npcs;
	}
}
