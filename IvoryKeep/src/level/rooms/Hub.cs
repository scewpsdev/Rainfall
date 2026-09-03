using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DungeonGate : Door
{
	public DungeonGate(Level destination, Door otherDoor = null, float layer = 0)
		: base(destination, otherDoor, false, layer)
	{
		sprite = new Sprite(tileset, 6, 9, 3, 2);
		rect = new FloatRect(-1.5f, 0.0f, 3.0f, 2.0f);

		collider = new Hitbox(-1.5f, -2, 3, 2);
	}

	public DungeonGate()
		: this(null)
	{
	}

	public override void render()
	{
		base.render();

		Vector2 dungeonEntrancePosition = position;
		int numSteps = 20;
		float width = 1.2f;
		float z = 0.15f;
		for (int i = 0; i < numSteps; i++)
		{
			// vertical
			{
				Vector3 vertex0 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(-width, 0.5f / 16 - 2 + i / (float)numSteps * 2), i / (float)numSteps * z));
				Vector3 vertex1 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(width, 0.5f / 16 - 2 + i / (float)numSteps * 2), i / (float)numSteps * z));
				Vector3 vertex2 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), i / (float)numSteps * z));
				Vector3 vertex3 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(-width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), i / (float)numSteps * z));
				Renderer.DrawSpriteEx(vertex0, vertex1, vertex2, vertex3, null, 0, 0, 0, 0, 0xFF6e6e6e);
			}
			// horizontal
			{
				Vector3 vertex0 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(-width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), i / (float)numSteps * z));
				Vector3 vertex1 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), i / (float)numSteps * z));
				Vector3 vertex2 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), (i + 1) / (float)numSteps * z));
				Vector3 vertex3 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(-width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), (i + 1) / (float)numSteps * z));
				Renderer.DrawSpriteEx(vertex0, vertex1, vertex2, vertex3, null, 0, 0, 0, 0, 0xFF767676);
			}
		}
	}
}

public class CastleGate : Door
{
	public CastleGate(Level destination, Door otherDoor = null)
		: base(destination, otherDoor, false, 0.0f)
	{
		sprite = new Sprite(tileset, 0, 11, 8, 8);
		rect = new FloatRect(-4, 0, 8, 8);

		collider = new Hitbox(-4, 0, 8, 2);

		locked = true;
		interactRange = 2;
	}

	public CastleGate()
		: this(null)
	{
	}
}

public class HubSpawn : Door
{
	public HubSpawn()
		: base(null, null, false, 0)
	{
	}

	public override bool canInteract(Player player)
	{
		return false;
	}

	public override void render()
	{
	}
}


public class Hub : Entity
{
	Room room;

	Texture stairs;

	Blacksmith blacksmith;

	public Elevator[] elevators = new Elevator[3];

	public Door pedestalRoomEntrance;


	public Hub(Room room)
	{
		this.room = room;

		stairs = Resource.GetTexture("level/hub/stairs.png", false);
	}

	public override void init(Level level)
	{
		//level.addEntity(level.entrance = new LevelTransition(GameState.instance.cliffside, GameState.instance.cliffside.exit, new Vector2i(1, 2), Vector2i.Left), new Vector2(-1, 29));

		//if (GameState.instance.cliffside.exit != null)
		//	GameState.instance.cliffside.exit.otherDoor = level.entrance;

		//level.addEntity(new ParallaxObject(Resource.GetTexture("level/hub/parallax1.png", false), 1.0f), new Vector2(level.width, level.height) * 0.5f + new Vector2(-17, 0));
		//level.addEntity(new ParallaxObject(Resource.GetTexture("level/hub/parallax2.png", false), 0.01f), new Vector2(level.width, level.height) * 0.5f + new Vector2(4, 0));

		//level.addEntity(tutorialExitDoor, hub.rooms[0].getMarker(01) + new Vector2(0.5f, 0));

		level.addEntity(new Fountain(FountainEffect.None), level.getMarker(0x2) + new Vector2(-2, 0));

		level.addEntity(new HubSpawn(), level.getMarker(0xA));


		SaveFile save = GameState.instance.save;

		for (int i = 0; i < StartingClass.startingClasses.Length; i++)
		{
			StartingClass startingClass = StartingClass.startingClasses[i];
			//Vector2 position = new Vector2(-StartingClass.startingClasses.Length / 2 * 1.5f - 0.5f + i * 1.5f + i * 2 / StartingClass.startingClasses.Length * 2.5f, 0);
			float x = i - StartingClass.startingClasses.Length / 2;
			x += x >= 0 ? 1 : 0;
			x = level.getMarker(0xA).x + x * 1.5f;
			Vector2 position = new Vector2(x, 2);
			level.addEntity(new ArmorStand(save.isStartingClassUnlocked(startingClass) ? startingClass : null), position);
			//level.addEntity(new ArmorStand(startingClass), position);
		}

#if DEBUG
		level.addEntity(new ArmorStand(StartingClass.dev, -1), new Vector2(level.getMarker(0xA).x + 2 + StartingClass.startingClasses.Length * 1.5f, 2));
#endif

		//level.addEntity(new StashChest(StashChestMode.Retrieve) { flipped = true }, new Vector2(2, 2));


		if (QuestManager.tryGetQuest(GameState.instance.save, "logan", "logan_quest", out Quest loganQuest) && (loganQuest.state == QuestState.InProgress || loganQuest.state == QuestState.Completed))
		{
			spawnLogan();
		}

		level.addEntity(pedestalRoomEntrance = new Door(GameState.instance.hub2, null), level.getMarker(0x2));

		/*
		for (int i = 0; i < elevators.Length; i++)
		{
			elevators[i] = new Elevator();
			elevators[i].locked = true;
			level.addEntity(elevators[i], (Vector2)room.getMarker(0x14 + (uint)i));
		}
		*/

		{
			blacksmith = level.getEntity<Blacksmith>(); // NPCManager.blacksmith;
			blacksmith.addShopItem(new Dagger());
			blacksmith.addShopItem(new Torch());
			blacksmith.addShopItem(new Bomb(), 7);
			blacksmith.addShopItem(new Lockpick(), 8);
			blacksmith.addShopItem(new ThrowingKnife() { stackSize = 12 }, 1);
			blacksmith.addShopItem(new AdventurersHoodBlue());
			blacksmith.addShopItem(new Bread());
		}

		if (GameState.instance.save.hasFlag(SaveFile.FLAG_CAVES_FOUND) && GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_GATEKEEPER_MET))
		{
			TravellingMerchant gatekeeper = new TravellingMerchant();
			level.addEntity(gatekeeper, level.getEntity<CastleGate>().position + new Vector2(3, 0));
		}
	}

	public void spawnLogan()
	{
		level.addEntity(new Logan() /*NPCManager.logan*/, level.getMarker(0xd));
	}

	public override void render()
	{
		base.render();

		/*
		Vector2 dungeonEntrancePosition = (Vector2)room.getMarker(0xa);
		int numSteps = 20;
		float width = 1.2f;
		float z = 0.15f;
		for (int i = 0; i < numSteps; i++)
		{
			// vertical
			{
				Vector3 vertex0 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(-width, 0.5f / 16 - 2 + i / (float)numSteps * 2), i / (float)numSteps * z));
				Vector3 vertex1 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(width, 0.5f / 16 - 2 + i / (float)numSteps * 2), i / (float)numSteps * z));
				Vector3 vertex2 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), i / (float)numSteps * z));
				Vector3 vertex3 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(-width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), i / (float)numSteps * z));
				Renderer.DrawSpriteEx(vertex0, vertex1, vertex2, vertex3, null, 0, 0, 0, 0, 0xFF6e6e6e);
			}
			// horizontal
			{
				Vector3 vertex0 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(-width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), i / (float)numSteps * z));
				Vector3 vertex1 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), i / (float)numSteps * z));
				Vector3 vertex2 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), (i + 1) / (float)numSteps * z));
				Vector3 vertex3 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(-width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), (i + 1) / (float)numSteps * z));
				Renderer.DrawSpriteEx(vertex0, vertex1, vertex2, vertex3, null, 0, 0, 0, 0, 0xFF767676);
			}
		}
		*/
	}
}
