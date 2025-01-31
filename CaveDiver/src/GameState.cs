using Rainfall;
using System;
using System.Xml;


public class RunStats
{
	public static readonly uint[] recordColors = [0xFF5f81cf, 0xFFd0be69, 0xFF6cb859, 0xFFb15848];

	public string seed;
	public float duration = 0.0f;
	public int floor = -1;
	public string areaName;
	public int kills = 0;
	public int chestsOpened = 0;
	public int stepsWalked = 0;
	public int hitsTaken = 0;

	public bool active = true;
	public bool isCustomRun = false;

	public Entity killedBy;
	public string killedByName;
	public long endedTime = -1;
	public bool hasWon = false;
	public bool scoreRecord, floorRecord, timeRecord, killRecord;


	public RunStats(string seed, bool isCustomRun)
	{
		this.seed = seed;
		this.isCustomRun = isCustomRun;
	}

	public void update(bool paused)
	{
		if (active && !paused)
		{
			duration += Time.deltaTime;
		}
	}

	public int score
	{
		get
		{
			int result = 0;

			result += hasWon ? 5000 : 0;
			result += floor * 1000;
			result += kills * 200;
			result += chestsOpened * 17;
			result += stepsWalked * 5;
			result += (int)(MathF.Exp(-hitsTaken * 0.2f) * duration);

			return result;
		}
	}
}

public class GameState : State
{
	const float AREA_TEXT_DURATION = 7.0f;
	const float AREA_TEXT_FADE = 2.0f;

	public const float GAME_OVER_SCREEN_DELAY = 2.0f;

	const float LEVEL_FADE = 0.5f;


	public static GameState instance;


	public SaveFile save;
	public RunStats run;
	string seed = null;
	public LevelGenerator generator;

	public Level tutorial;
	List<Level> levels = new List<Level>();

	public Level level;

	public List<WorldEventListener> worldEventListeners = new List<WorldEventListener>();

	Level newLevel = null;
	Vector2 newLevelSpawnPosition;
	long levelSwitchTime = -1;

	public Player player;
	public PlayerCamera camera;

	uint ambientSource;
	public Sound ambience;

	public Mob[] currentBoss { get; private set; }
	public float currentBossMaxHealth;
	public long bossFightStarted = -1;

	public bool isPaused = false;
	public bool consoleOpen = false;
	public bool onscreenPrompt = false;

	long entityUpdateDelta;

	long lastFreezeTime = -1;
	float freezeDuration;


	public GameState(int saveID, string seed, bool customRun = false, bool dailyRun = false)
	{
		this.seed = seed;
		instance = this;

		save = customRun ? SaveFile.customRun : dailyRun ? SaveFile.dailyRun : SaveFile.Load(saveID);
		QuestManager.Init();
		NPCManager.Init();

		reset();
	}

	void reset(StartingClass startingClass = null, bool quickRestart = false)
	{
		destroy();

		currentBoss = null;

		run = new RunStats(seed != null ? seed : Hash.hash(Time.timestamp).ToString(), seed != null);

		QuestManager.Init();
		NPCManager.Init();

		generator = new LevelGenerator();

		tutorial = new Level(-1, "");

		player = new Player();
		camera = new PlayerCamera(player);

		Simplex simplex = new Simplex(12345, 3);

		Level lastLevel;

		Door door1, door2, door3;
		generator.generateSingleRoomLevel(tutorial, generator.specialSet, 1, TileType.dirt, TileType.stone, 0x21, 0x0a, new TutorialExitDoor(null, null));
		generator.generateCaveBackground(tutorial, simplex, TileType.dirt, TileType.stone);
		tutorial.addEntity(door1 = new Door(null), tutorial.rooms[0].getMarker(0x0a) + new Vector2(0.5f, 0));
		tutorial.addEntity(door2 = new Door(null), tutorial.rooms[0].getMarker(0x0b) + new Vector2(0.5f, 0));
		tutorial.addEntity(door3 = new Door(null), tutorial.rooms[0].getMarker(0x0c) + new Vector2(0.5f, 0));
		tutorial.addEntity(new TutorialText("1", 0xFF7F7F7F), tutorial.rooms[0].getMarker(0x0a) + new Vector2(0.5f, 1.5f));
		tutorial.addEntity(new TutorialText("2", 0xFF7F7F7F), tutorial.rooms[0].getMarker(0x0b) + new Vector2(0.5f, 1.5f));
		tutorial.addEntity(new TutorialText("3", 0xFF7F7F7F), tutorial.rooms[0].getMarker(0x0c) + new Vector2(0.5f, 1.5f));
		tutorial.addEntity(new Tutorial(tutorial.rooms[0]));
		levels.Add(tutorial);

		lastLevel = tutorial;
		Level1.Generate(generator, door1, ref lastLevel, levels, simplex);
		lastLevel.exit.destination = tutorial;
		lastLevel.exit.otherDoor = door1;

		lastLevel = tutorial;
		Level2.Generate(generator, door2, ref lastLevel, levels, simplex);
		lastLevel.exit.destination = tutorial;
		lastLevel.exit.otherDoor = door2;

		lastLevel = tutorial;
		Level3.Generate(generator, door3, ref lastLevel, levels, simplex);
		lastLevel.exit.finalExit = true;
		//lastLevel.exit.destination = tutorial;
		//lastLevel.exit.otherDoor = door3;


		if (save.hasFlag(SaveFile.FLAG_TUTORIAL_FINISHED))
		{
			level = null;
			switchLevel(tutorial, door1.getSpawnPoint() + new Vector2(-1.5f, 0));
			player.money = 10;
			levelSwitchTime = -1;
		}
		else
		{
			level = null;
			switchLevel(tutorial, tutorial.entrance.getSpawnPoint());
			player.money = 10;
			levelSwitchTime = -1;
		}
	}

	unsafe void loadScene(string path, Level level)
	{
		Model scene = Resource.GetModel(path, false);
		foreach (Node node in scene.skeleton.nodes)
		{
			if (node.name == "tilemap")
				continue;

			if (node.name == "background")
			{
				if (node.meshes.Length == 0)
					continue;

				int meshID = node.meshes[0];
				MeshData* mesh = &scene.scene->meshes[meshID];
				if (mesh->materialID == -1)
					continue;

				MaterialData* material = &scene.scene->materials[mesh->materialID];
				if (material->diffuse == null)
					continue;

				string texturePath = new string((sbyte*)material->diffuse->path);
				texturePath = StringUtils.AbsolutePath(texturePath, path);
				Texture texture = Resource.GetTexture(texturePath, false);
				level.bg = texture;

				continue;
			}

			for (int i = 0; i < node.meshes.Length; i++)
			{
				int meshID = node.meshes[i];
				MeshData* mesh = &scene.scene->meshes[meshID];

				if (mesh->materialID != -1)
				{
					MaterialData* material = &scene.scene->materials[mesh->materialID];
					if (material->diffuse != null)
					{
						string texturePath = new string((sbyte*)material->diffuse->path);
						texturePath = StringUtils.AbsolutePath(texturePath, path);
						Texture texture = Resource.GetTexture(texturePath, false);

						int numSubMeshes = mesh->vertexCount / 4;
						for (int k = 0; k < numSubMeshes; k++)
						{
							Vector2 min = new Vector2(float.MaxValue);
							Vector2 max = new Vector2(float.MinValue);
							float z = 0;
							Vector2 uv0 = new Vector2(float.MaxValue);
							Vector2 uv1 = new Vector2(float.MinValue);
							for (int j = k * 4; j < k * 4 + 4; j++)
							{
								PositionNormalTangent* vertex = &mesh->vertices[j];
								Vector3 position = node.transform * vertex->position;
								min = Vector2.Min(min, position.xy);
								max = Vector2.Max(max, position.xy);
								z = position.z;

								Vector2 uv = mesh->texcoords[j];
								uv0 = Vector2.Min(uv0, uv);
								uv1 = Vector2.Max(uv1, uv);
							}

							Vector2 center = (min + max) * 0.5f;
							Vector2 size = max - min;

							ParallaxObject entity = new ParallaxObject();

							int u0 = (int)MathF.Round(uv0.x * texture.width);
							int v0 = (int)MathF.Round(uv0.y * texture.height);
							int w = (int)MathF.Round((uv1.x - uv0.x) * texture.width);
							int h = (int)MathF.Round((uv1.y - uv0.y) * texture.height);
							entity.sprite = new Sprite(texture, u0, v0, w, h);
							entity.rect = new FloatRect(-0.5f * size, size);
							entity.z = z;

							//entity.rect = new FloatRect(-0.5f * size, size);
							level.addEntity(entity, center);
						}
					}
				}
			}
		}
	}

	public override void init()
	{
	}

	public override void destroy()
	{
		if (save != null && save.id != -1)
			SaveFile.Save(save);

		if (ambientSource != 0)
			Audio.StopSource(ambientSource);

		foreach (Level level in levels)
			level.destroy();
		levels.Clear();

		worldEventListeners.Clear();
	}

	public void freeze(float duration)
	{
		lastFreezeTime = Time.currentTime;
		freezeDuration = duration;
	}

	public void setAmbience(Sound ambience)
	{
		if (ambientSource != 0)
		{
			Audio.FadeoutSource(ambientSource, 2);
			ambientSource = 0;
		}
		if (ambience != null)
			ambientSource = Audio.PlayBackground(ambience, 0.6f, 1, true, 2);
		this.ambience = ambience;
	}

	public void switchLevel(Level newLevel, Vector2 spawnPosition)
	{
		if (level != null && newLevel != null)
		{
			if (!save.hasFlag(SaveFile.FLAG_TUTORIAL_FINISHED))
				save.setFlag(SaveFile.FLAG_TUTORIAL_FINISHED);
		}

		this.newLevel = newLevel;
		this.newLevelSpawnPosition = spawnPosition;
		levelSwitchTime = Time.currentTime;

		if (currentBoss != null)
			currentBoss = null;
	}

	public void moveEntityToLevel(Entity entity, Level newLevel)
	{
		entity.level.removeEntity(entity);
		newLevel.addEntity(entity, false);
	}

	public void stopRun(bool hasWon, Entity killedBy = null, string killedByName = null)
	{
		run.active = false;
		run.endedTime = Time.currentTime;
		run.hasWon = hasWon;
		run.killedBy = killedBy;
		run.killedByName = killedByName;

		SaveFile.OnRunFinished(run, save);
		GameOverScreen.Init();
	}

	public void setBoss(Mob[] boss)
	{
		currentBoss = boss;
		if (boss != null)
		{
			//currentBossMaxHealth = boss.health;
			bossFightStarted = Time.currentTime;
		}
		else
		{
			bossFightStarted = -1;
		}
	}

	public override void onKeyEvent(KeyCode key, KeyModifier modifiers, bool down)
	{
#if DEBUG
		if (key == KeyCode.Semicolon && modifiers == KeyModifier.None && down)
		{
			consoleOpen = !consoleOpen;
			if (consoleOpen)
				DebugConsole.OnOpen();
			else
				DebugConsole.OnClose();
			Input.ConsumeKeyEvent(key);
		}
#endif
		if (consoleOpen)
		{
			DebugConsole.OnKeyEvent(key, modifiers, down);
			Input.ConsumeKeyEvent(key);
		}
		else if (isPaused)
			PauseMenu.OnKeyEvent(key, modifiers, down);
	}

	public override void onCharEvent(byte length, uint value)
	{
		char c = (char)value;
		if (c == 'ö')
			return;
		if (consoleOpen)
			DebugConsole.OnCharEvent(c);
	}

	public override void onMouseButtonEvent(MouseButton button, bool down)
	{
		if (isPaused)
			PauseMenu.OnMouseButtonEvent(button, down);
	}

	public override void onGamepadButtonEvent(GamepadButton button, bool down)
	{
		if (isPaused)
			PauseMenu.OnGamepadButtonEvent(button, down);
	}

	public override void update()
	{
		if (!isPaused && InputManager.IsPressed("UIQuit", true) && player.numOverlaysOpen == 0)
		{
			isPaused = true;
			PauseMenu.OnPause();
		}
		else if (isPaused && InputManager.IsPressed("UIQuit", true))
		{
			isPaused = false;
			PauseMenu.OnUnpause();
		}

		Time.paused = isPaused || onscreenPrompt;

		run.update(isPaused || onscreenPrompt);
		QuestManager.Update();

		if (newLevel != null && (Time.currentTime - levelSwitchTime) / 1e9f >= LEVEL_FADE)
		{
			if (level != null)
			{
				for (int i = 0; i < level.entities.Count; i++)
					level.entities[i].onLevelSwitch(newLevel);

				level.removeEntity(player);
				level.removeEntity(camera);
			}

			for (int i = 0; i < newLevel.entities.Count; i++)
				newLevel.entities[i].onLevelSwitch(newLevel);

			newLevel.addEntity(player, newLevelSpawnPosition, level == null);
			newLevel.addEntity(camera, level == null);

			camera.position = player.position;

			if (newLevel.floor > run.floor)
			{
				run.floor = newLevel.floor;
				if (newLevel.name != null && newLevel.name != "")
					run.areaName = newLevel.name;
			}

			level = newLevel;
			newLevel = null;

			player.hud.onLevelSwitch(level.name);

			setAmbience(level.ambientSound);
		}

		if (!isPaused && !onscreenPrompt && newLevel == null && !(run.endedTime != -1 && (Time.currentTime - run.endedTime) / 1e9f >= GAME_OVER_SCREEN_DELAY))
		{
			bool freeze = lastFreezeTime != -1 && (Time.currentTime - lastFreezeTime) / 1e9f < freezeDuration;

			if (!freeze)
			{
				long beforeEntityUpdate = Time.timestamp;
				level.update();
				long afterEntityUpdate = Time.timestamp;
				entityUpdateDelta = afterEntityUpdate - beforeEntityUpdate;
			}
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		level.render();

		if ((Time.currentTime - levelSwitchTime) / 1e9f < 2 * LEVEL_FADE)
		{
			float fade = 1 - MathF.Abs(1 - (Time.currentTime - levelSwitchTime) / 1e9f / LEVEL_FADE);
			uint color = MathHelper.ColorAlpha(0xFF000000, fade);
			Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, null, false, color);
		}

		if (!run.active)
		{
			if (run.endedTime != -1 && (Time.currentTime - run.endedTime) / 1e9f >= GAME_OVER_SCREEN_DELAY)
			{
				GameOverScreen.Render();

				if (InputManager.IsPressed("UIConfirm"))
				{
					Audio.PlayBackground(UISound.uiConfirm2);
					GameOverScreen.Destroy();
					reset(player.startingClass, true);
				}
				if (InputManager.IsPressed("UIConfirm2"))
				{
					Audio.PlayBackground(UISound.uiConfirm2);
					GameOverScreen.Destroy();
					reset();
				}
			}
			else
			{
				//Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, 0, null, MathHelper.ColorAlpha(0xFF000000, MathF.Pow((Time.currentTime - run.endedTime) / 1e9f / GAME_OVER_SCREEN_DELAY, 3)));
			}
		}

		if (isPaused)
		{
			if (!PauseMenu.Render(this))
			{
				isPaused = false;
				PauseMenu.OnUnpause();
			}
		}

		if (consoleOpen)
			DebugConsole.Render();

#if DEBUG
		Renderer.DrawUITextBMP(0, Renderer.UIHeight - 8, "x: " + player.position.x.ToString("0.0") + ", y: " + player.position.y.ToString("0.0"));
#endif
	}

	public override void drawDebugStats(int y, byte color, GraphicsDevice graphics)
	{
		Span<byte> str = stackalloc byte[64];

		StringUtils.WriteString(str, "Entity Update: ");
		StringUtils.AppendFloat(str, (entityUpdateDelta / 1e9f) * 1000, 2);
		StringUtils.AppendString(str, " ms");
		graphics.drawDebugText(0, y++, color, str);

		y++;

		StringUtils.WriteString(str, "Grounded = ");
		StringUtils.AppendBool(str, player.isGrounded);
		graphics.drawDebugText(0, y++, color, str);
	}
}
