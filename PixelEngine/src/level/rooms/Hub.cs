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

		collider = new FloatRect(-1.5f, 0.0f, 3, 2);
	}
}

public class CastleGate : Door
{
	public CastleGate(Level destination, Door otherDoor = null)
		: base(destination, otherDoor, false, 0.0f)
	{
		sprite = new Sprite(tileset, 0, 11, 8, 8);
		rect = new FloatRect(-4, 0, 8, 8);

		collider = new FloatRect(-4, 0, 8, 2);
	}
}


public class Hub : Entity
{
	Room room;

	Texture stairs;


	public Hub(Room room)
	{
		this.room = room;

		stairs = Resource.GetTexture("res/level/hub/stairs.png", false);
	}

	public override void init(Level level)
	{
		level.addEntity(level.entrance = new LevelTransition(GameState.instance.cliffside, GameState.instance.cliffside.exit, new Vector2(1.0f, 2)), new Vector2(-1 + 0.1f, 29));
		GameState.instance.cliffside.exit.otherDoor = level.entrance;

		//level.addEntity(new ParallaxObject(Resource.GetTexture("res/level/hub/parallax1.png", false), 1.0f), new Vector2(level.width, level.height) * 0.5f + new Vector2(-17, 0));
		//level.addEntity(new ParallaxObject(Resource.GetTexture("res/level/hub/parallax2.png", false), 0.01f), new Vector2(level.width, level.height) * 0.5f + new Vector2(4, 0));

		//level.addEntity(tutorialExitDoor, hub.rooms[0].getMarker(01) + new Vector2(0.5f, 0));

		level.addEntity(new Fountain(FountainEffect.None), level.rooms[0].getMarker(11) + new Vector2(7, 0));


		SaveFile save = GameState.instance.save;

		for (int i = 0; i < StartingClass.startingClasses.Length; i++)
		{
			StartingClass startingClass = StartingClass.startingClasses[i];
			Vector2 position = new Vector2(-StartingClass.startingClasses.Length / 2 * 1.5f - 0.5f + i * 1.5f + i * 2 / StartingClass.startingClasses.Length * 2.5f, 0);
			level.addEntity(new ArmorStand(save.isStartingClassUnlocked(startingClass) ? startingClass : null), level.rooms[0].getMarker(10) + position);
		}

#if DEBUG
		level.addEntity(new ArmorStand(StartingClass.dev, -1), level.rooms[0].getMarker(10) + new Vector2(6.5f, 0));
#endif

		BrokenWanderer npc = new BrokenWanderer(Random.Shared, level);
		npc.clearShop();
		npc.addShopItem(new Rock());
		npc.addShopItem(new Torch());
		npc.addShopItem(new Bomb(), 7);
		npc.addShopItem(new IronKey(), 8);
		npc.addShopItem(new ThrowingKnife() { stackSize = 8 }, 1);
		npc.direction = 1;
		npc.buysItems = false;
		level.addEntity(npc, new Vector2(54, 23));

		//level.addEntity(new IronDoor(save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) ? null : "dummy_key"), new Vector2(38.5f, 23));
		if (save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) && !save.hasFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_COMPLETED))
		{
			RatNPC rat = new RatNPC(null);
			rat.clearShop();
			rat.direction = 1;
			level.addEntity(rat, (Vector2)level.rooms[0].getMarker(0x0e));

			level.addEntity(new RopeEntity(13), new Vector2(46, 23));
		}

		if (GameState.instance.save.hasFlag(SaveFile.FLAG_CAVES_FOUND) && !GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_GATEKEEPER_MET))
		{
			TravellingMerchant gatekeeper = new TravellingMerchant(null, level);
			level.addEntity(gatekeeper, (Vector2)room.getMarker(17));
		}

		if (GameState.instance.save.tryGetQuest("logan", "logan_quest", out Quest loganQuest) && !loganQuest.isCompleted)
		{
			level.addEntity(new Logan(), new Vector2(56, 23));
		}

		for (int i = 0; i < save.highscores.Length; i++)
		{
			Vector2 position = room.getMarker(15) + new Vector2(i * 5, 0);
			level.addEntity(new Pedestal(), position);

			if (save.highscores[i].score > 0)
			{
				string[] label = i == 0 ? ["Highest Score:", save.highscores[i].score.ToString()] :
					i == 1 ? ["Highest Floor:", save.highscores[i].floor != -1 ? (save.highscores[i].floor + 1).ToString() : "???"] :
					i == 2 ? ["Fastest Time:", save.highscores[i].time != -1 ? StringUtils.TimeToString(save.highscores[i].time) : "???"] :
					i == 3 ? ["Most kills:", save.highscores[i].kills.ToString()] : ["???"];
				uint color = RunStats.recordColors[i];
				level.addEntity(new HighscoreDummy(save.highscores[i], label, color), position + Vector2.Up);
			}
		}
	}

	public override void render()
	{
		Vector2 dungeonEntrancePosition = (Vector2)room.getMarker(0x0b);
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
