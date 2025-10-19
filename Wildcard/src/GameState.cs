using Rainfall;
using System;
using System.Drawing;


public class GameState : State
{
	public static GameState instance;


	public Level level;

	public Player player;
	public PlayerCamera camera;

	uint ambientSource;
	public Sound ambience;

	long lastFreezeTime = -1;
	float freezeDuration;


	public GameState()
	{
		instance = this;
	}

	public override void init()
	{
		level = new Level();
		level.resize(40, 22);

		for (int i = 0; i < level.width; i++)
		{
			level.setTile(i, 0, TileType.dirt);
			level.setTile(i, level.height - 1, TileType.dirt);
		}
		for (int i = 0; i < level.height; i++)
		{
			level.setTile(0, i, TileType.dirt);
			level.setTile(level.width - 1, i, TileType.dirt);
		}

		level.addEntity(player = new Player(), new Vector2(4, 4));
		level.addEntity(camera = new PlayerCamera(player));
	}

	public override void destroy()
	{
		if (ambientSource != 0)
			Audio.StopSource(ambientSource);
	}

	public void freeze(float duration)
	{
		lastFreezeTime = Time.timestamp;
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
		{
			ambientSource = Audio.PlayBackground(ambience, 0.6f, 1, true, 2);
			Audio.SetInaudibleBehavior(ambientSource, true, false);
			Audio.SetProtect(ambientSource, true);
		}
		this.ambience = ambience;
	}

	public void moveEntityToLevel(Entity entity, Level newLevel)
	{
		entity.level.removeEntity(entity);
		newLevel.addEntity(entity, false);
	}

	public override void onKeyEvent(KeyCode key, KeyModifier modifiers, bool down)
	{
	}

	public override void onCharEvent(byte length, uint value)
	{
	}

	public override void onMouseButtonEvent(MouseButton button, bool down)
	{
	}

	public override void onGamepadButtonEvent(GamepadButton button, bool down)
	{
	}

	public override void update()
	{
		bool freeze = lastFreezeTime != -1 && (Time.timestamp - lastFreezeTime) / 1e9f < freezeDuration;

		Time.paused = freeze;

		if (!freeze)
		{
			level.update();
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		if (level != null)
			level.render();
	}
}
