using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class CaveEntranceDoor : Door
{
	Sprite stairs;
	Sprite bg;

	public CaveEntranceDoor(Level destination, Door otherDoor)
		: base(destination, otherDoor, true, 0)
	{
		sprite = new Sprite(tileset, 4, 6, 3, 3);
		bg = new Sprite(tileset, 7, 6, 3, 3);
		stairs = new Sprite(tileset, 10, 7, 2, 2);
		rect = new FloatRect(-1.5f, 0, 3, 3);

		collider = new Hitbox(-1.5f, 0.0f, 3, 2);
	}

	public override void render()
	{
		base.render();

		Renderer.DrawSprite(position.x + rect.position.x, position.y + rect.position.y, LAYER_BG + 0.00001f, rect.size.x, rect.size.y, 0, bg, false, 0xFF3F3F3F);

		float width = 2;
		float depth = 0.2f;
		Vector3 vertex0 = ParallaxObject.ParallaxEffect(new Vector3(position + new Vector2(-0.5f * width, 0), 0));
		Vector3 vertex1 = ParallaxObject.ParallaxEffect(new Vector3(position + new Vector2(0.5f * width, 0), 0));
		Vector3 vertex2 = ParallaxObject.ParallaxEffect(new Vector3(position + new Vector2(0.5f * width, 2), depth));
		Vector3 vertex3 = ParallaxObject.ParallaxEffect(new Vector3(position + new Vector2(-0.5f * width, 2), depth));
		Renderer.DrawSpriteEx(vertex0, vertex1, vertex2, vertex3, stairs, false, Vector4.One);
	}
}

public class DungeonEntrance : Door
{
	public DungeonEntrance(Level destination, Door otherDoor)
		: base(destination, otherDoor)
	{
	}

	public override bool canInteract(Player player)
	{
		return player.getItem("lost_sigil") != null || !locked;
	}

	public override void interact(Player player)
	{
		base.interact(player);
		if (player.getItem("lost_sigil") != null)
			player.removeItem(player.getItem("lost_sigil"));
	}
}

public class CaveGenerator : LevelGenerator
{
	public CaveGenerator(string seed)
		: base("caves", "Caves", cavesSet, TileType.dirt, TileType.stone, seed)
	{
		spawnStartingWeapon = true;
	}

	public override string getAreaName()
	{
		return "Forgotten Cavern";
	}

	public override int getAreaFirstFloor()
	{
		return 0;
	}

	public override int getNumFloors()
	{
		return 5;
	}

	public override string getLevelName(int floor)
	{
		return displayName + " " + StringUtils.ToRoman(floor + 1);
	}

	public override int getLootValue(int floor)
	{
		return (floor + 1) * 3;
	}

	public override void getLevelSize(int floor, out int width, out int height)
	{
		width = 40;
		height = 40;

		float wideLevelChance = 0.2f;
		if (random.NextSingle() < wideLevelChance)
			width = 60;
		float highLevelChance = 0.2f;
		if (random.NextSingle() < highLevelChance)
			height = 60;
	}

	public override int getAmbientLight()
	{
		float darkLevelChance = 0.1f;
		float brightLevelChance = 0.1f;
		if (random.NextSingle() < darkLevelChance)
			return 0;
		else if (random.NextSingle() < brightLevelChance)
			return 5;
		else
			return 3;
	}

	public override Sound getAmbientSound()
	{
		return Resource.GetSound("sounds/ambience.ogg");
	}

	public override MultilayerTrack getAmbientTrack()
	{
		return cavesAmbientTrack;
	}

	public override Door createEntranceDoor(Level lastLevel, Door lastExit)
	{
		return new CaveEntranceDoor(lastLevel, lastExit);
	}

	public override BossRoom createBossRoom(Room room)
	{
		return new CavesBossRoom(room);
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
		RoomDef[] secretRooms = new RoomDef[7];
		secretRooms[0] = specialSet.roomDefs[6];
		secretRooms[1] = specialSet.roomDefs[7];
		secretRooms[2] = specialSet.roomDefs[9];
		secretRooms[3] = specialSet.roomDefs[11];
		secretRooms[4] = !GameState.instance.save.areAllStartingClassesUnlocked() ? cavesSpecialSet.roomDefs[0] : specialSet.roomDefs[6];
		secretRooms[5] = specialSet.roomDefs[20];
		secretRooms[6] = cavesSpecialSet.roomDefs[1];

		return secretRooms;
	}

	public override Entity createSecretRoomEntity(int type, Room room)
	{
		if (type == 0) return new CavesSpecialRoom1(room, this);
		if (type == 1) return new CavesSpecialRoom2(room, this);
		if (type == 2) return new CavesSpecialRoom3(room, this);
		if (type == 3) return new CavesPlatformingRoom1(room, this);
		if (type == 4) return !GameState.instance.save.areAllStartingClassesUnlocked() ? new PrisonCellRoom(room, this) : new CavesSpecialRoom1(room, this);
		if (type == 6) return new CavesSurvivalRoom(room, this);
		return null;
	}

	public override float[] getDroprates()
	{
		return DropRates.cavesDroprates;
	}

	public override int getNumItems(int floor)
	{
		return Mathf.RollDice(2, 2, random);
	}

	public override Container createContainer(Item[] items)
	{
		return new Barrel(items);
	}

	public override ExplosiveObject createExplosiveObject()
	{
		return new ExplosiveBarrel();
	}

	public override List<Mob> createEnemy(Level level)
	{
		List<Mob> mobs = new List<Mob>();
		mobs.Add(new Rat());
		if (level.lightLevel <= 3)
			mobs.Add(new Spider());
		mobs.Add(new Snake());
		mobs.Add(new Bat());
		if (getLocalFloor() >= 3)
			mobs.Add(new Slime());
		mobs.Add(new Beetle());
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
