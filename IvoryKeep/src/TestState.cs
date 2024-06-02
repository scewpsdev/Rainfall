using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TestState : State
{
	public static TestState instance;


	Level level;

	Entity map;

	public Player player;
	PlayerCamera camera;

	Cubemap skybox;

	long particleUpdateDelta;
	long animationUpdateDelta;
	long aiUpdateDelta;
	long entityUpdateDelta;


	public TestState()
	{
		instance = this;
	}

	public override unsafe void init()
	{
		level = new Level();

		level.addEntity(map = EntityLoader.Load("res/level/test_level/test_level.rfs"));
		level.addEntity(player = new Player());
		level.addEntity(camera = new PlayerCamera(player)); // new FreeCamera();
		level.addEntity(EntityType.Get("boss1").create(), new Vector3(0, 0, -2));

		player.camera = camera;

		skybox = Resource.GetCubemap("res/level/test_level/sky1_cubemap_equirect.png");
	}

	public override void destroy()
	{
	}

	public override void update()
	{
		long beforeParticleUpdate = Time.timestamp;
		ParticleSystem.Update(camera.position);
		long afterParticleUpdate = Time.timestamp;
		particleUpdateDelta = afterParticleUpdate - beforeParticleUpdate;

		long beforeAnimationUpdate = Time.timestamp;
		Animator.Update(camera.getModelMatrix());
		long afterAnimationUpdate = Time.timestamp;
		animationUpdateDelta = afterAnimationUpdate - beforeAnimationUpdate;

		long beforeAIUpdate = Time.timestamp;
		AI.Update();
		long afterAIUpdate = Time.timestamp;
		aiUpdateDelta = afterAIUpdate - beforeAIUpdate;

		long beforeEntityUpdate = Time.timestamp;
		level.update();
		long afterEntityUpdate = Time.timestamp;
		entityUpdateDelta = afterEntityUpdate - beforeEntityUpdate;
	}

	public override unsafe void draw(GraphicsDevice graphics)
	{
		Renderer.SetCamera(camera.position, camera.rotation, camera.getProjectionMatrix(), camera.near, camera.far);

		Renderer.DrawSky(skybox, 0.5f, Quaternion.Identity);
		Renderer.DrawEnvironmentMap(skybox, 0.25f);

		level.draw(graphics);
	}

	public override void drawDebugStats(int y, byte color, GraphicsDevice graphics)
	{
		Span<byte> str = stackalloc byte[64];

		StringUtils.WriteString(str, "Particle Systems: ");
		StringUtils.AppendInteger(str, ParticleSystem.numParticleSystems);
		graphics.drawDebugText(0, y++, color, str);

		StringUtils.WriteString(str, "Particle Update: ");
		StringUtils.AppendFloat(str, (particleUpdateDelta / 1e9f) * 1000, 2);
		StringUtils.AppendString(str, " ms");
		graphics.drawDebugText(0, y++, color, str);

		StringUtils.WriteString(str, "Animators: ");
		StringUtils.AppendInteger(str, Animator.numAnimators);
		graphics.drawDebugText(0, y++, color, str);

		StringUtils.WriteString(str, "Animation Update: ");
		StringUtils.AppendFloat(str, (animationUpdateDelta / 1e9f) * 1000, 2);
		StringUtils.AppendString(str, " ms");
		graphics.drawDebugText(0, y++, color, str);

		StringUtils.WriteString(str, "AI Update: ");
		StringUtils.AppendFloat(str, (aiUpdateDelta / 1e9f) * 1000, 2);
		StringUtils.AppendString(str, " ms");
		graphics.drawDebugText(0, y++, color, str);

		StringUtils.WriteString(str, "Entity Update: ");
		StringUtils.AppendFloat(str, (entityUpdateDelta / 1e9f) * 1000, 2);
		StringUtils.AppendString(str, " ms");
		graphics.drawDebugText(0, y++, color, str);

		y++;

		StringUtils.WriteString(str, "x=");
		StringUtils.AppendInteger(str, (int)(camera.position.x * 100));
		graphics.drawDebugText(0, y++, color, str);

		StringUtils.WriteString(str, "y=");
		StringUtils.AppendInteger(str, (int)(camera.position.y * 100));
		graphics.drawDebugText(0, y++, color, str);

		StringUtils.WriteString(str, "z=");
		StringUtils.AppendInteger(str, (int)(camera.position.z * 100));
		graphics.drawDebugText(0, y++, color, str);
	}
}
