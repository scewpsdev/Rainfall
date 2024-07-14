using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class RunStats
{
	public uint seed;
	public float duration = 0.0f;
	public int floor = 0;

	public bool active = true;


	public RunStats(uint seed)
	{
		this.seed = seed;
	}

	public void update()
	{
		if (active)
		{
			duration += Time.deltaTime;
		}
	}
}

public class GameState : State
{
	const float AREA_TEXT_DURATION = 7.0f;
	const float AREA_TEXT_FADE = 2.0f;

	const float GAME_OVER_SCREEN_DELAY = 2.0f;


	public static GameState instance;

	long particleUpdateDelta;
	long animationUpdateDelta;
	long entityUpdateDelta;

	public RunStats run;

	List<Level> cachedLevels = new List<Level>();
	public Level level;

	Level newLevel = null;
	Door newLevelDoor = null;

	public Player player;
	PlayerCamera camera;


	public unsafe GameState()
	{
		reset();
	}

	void reset()
	{
		level?.destroy();
		level = null;

		for (int i = 0; i < cachedLevels.Count; i++)
		{
			cachedLevels[i].destroy();
		}
		cachedLevels.Clear();

		run = new RunStats(12345678); // Hash.hash(Time.timestamp)

		level = new Level(-1);
		level.addEntity(new ItemEntity(new Sword()), new Vector2(13, 10));
		level.addEntity(new Snake(), new Vector2(5, 1));
		level.addEntity(player = new Player(), new Vector2(10, 1));
		level.addEntity(camera = new PlayerCamera(player));

		Level secondLevel = new Level(-1);
		secondLevel.setTile(secondLevel.width - 2, 2, 3);
		secondLevel.setTile(secondLevel.width - 3, 2, 3);
		secondLevel.setTile(secondLevel.width - 4, 2, 3);
		secondLevel.addEntity(new Ladder(5), new Vector2(8, 1));
		secondLevel.addEntity(new ArrowTrap(new Vector2(-1, 0)), new Vector2(12, 1));
		secondLevel.setTile(12, 1, 1);
		cachedLevels.Add(secondLevel);

		Level[] levels = [new Level(1), new Level(2), new Level(3)];

		level.exit = new Door(secondLevel);
		level.addEntity(level.exit, new Vector2(15, 1));

		secondLevel.entrance = new Door(level, level.exit);
		secondLevel.addEntity(secondLevel.entrance, new Vector2(4, 1));
		level.exit.otherDoor = secondLevel.entrance;

		secondLevel.exit = new Door(levels[0]);
		secondLevel.addEntity(secondLevel.exit, new Vector2(15, 1));

		Level lastLevel = secondLevel;
		for (int i = 0; i < levels.Length; i++)
		{
			LevelGenerator generator = new LevelGenerator(run.seed, i);
			generator.run(levels[i], i < levels.Length - 1 ? levels[i + 1] : null, lastLevel);
			lastLevel = levels[i];
		}

		switchLevel(levels[0], levels[0].entrance);
	}

	public override void init()
	{
		instance = this;
	}

	public override void destroy()
	{
		level.destroy();
		for (int i = 0; i < cachedLevels.Count; i++)
		{
			cachedLevels[i].destroy();
		}
	}

	public void switchLevel(Level newLevel, Door door)
	{
		this.newLevel = newLevel;
		this.newLevelDoor = door;
	}

	public override void update()
	{
		run.update();

		if (newLevel != null)
		{
			cachedLevels.Add(level);
			level.removeEntity(player);
			level.removeEntity(camera);

			if (cachedLevels.Contains(newLevel))
				cachedLevels.Remove(newLevel);
			newLevel.addEntity(player, newLevelDoor.position, false);
			newLevel.addEntity(camera, false);

			if (newLevel.floor > run.floor)
				run.floor = newLevel.floor;

			level = newLevel;
			newLevel = null;
		}

		long beforeParticleUpdate = Time.timestamp;
		ParticleSystem.Update(Vector3.Zero);
		long afterParticleUpdate = Time.timestamp;
		particleUpdateDelta = afterParticleUpdate - beforeParticleUpdate;

		long beforeAnimationUpdate = Time.timestamp;
		Animator.Update(Matrix.Identity);
		long afterAnimationUpdate = Time.timestamp;
		animationUpdateDelta = afterAnimationUpdate - beforeAnimationUpdate;

		long beforeEntityUpdate = Time.timestamp;
		level.update();
		long afterEntityUpdate = Time.timestamp;
		entityUpdateDelta = afterEntityUpdate - beforeEntityUpdate;
	}

	public override void draw(GraphicsDevice graphics)
	{
		level.render();

		if (!player.isAlive && (Time.currentTime - player.deathTime) / 1e9f >= GAME_OVER_SCREEN_DELAY)
		{
			GameOverScreen.Render();

			if (Input.IsKeyPressed(KeyCode.X))
				reset();
		}
	}

	public override void drawDebugStats(int y, byte color, GraphicsDevice graphics)
	{
		Span<byte> str = stackalloc byte[64];

		StringUtils.WriteString(str, "Particle Systems: ");
		StringUtils.AppendInteger(str, ParticleSystem.numParticleSystems);
		graphics.drawDebugText(0, y++, color, str);

		StringUtils.WriteString(str, "Animators: ");
		StringUtils.AppendInteger(str, Animator.numAnimators);
		graphics.drawDebugText(0, y++, color, str);

		StringUtils.WriteString(str, "Particle Update: ");
		StringUtils.AppendFloat(str, (particleUpdateDelta / 1e9f) * 1000, 2);
		StringUtils.AppendString(str, " ms");
		graphics.drawDebugText(0, y++, color, str);

		StringUtils.WriteString(str, "Animation Update: ");
		StringUtils.AppendFloat(str, (animationUpdateDelta / 1e9f) * 1000, 2);
		StringUtils.AppendString(str, " ms");
		graphics.drawDebugText(0, y++, color, str);

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
