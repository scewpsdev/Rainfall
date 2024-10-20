using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class Cliffside : Entity
{
	Room room;
	Texture waves;
	Sprite caveBg;

	Sound caveAmbience;


	public Cliffside(Room room)
	{
		this.room = room;

		waves = Resource.GetTexture("res/level/hub/waves.png", (uint)SamplerFlags.Point | (uint)SamplerFlags.VClamp);
		caveBg = new Sprite(Resource.GetTexture("res/level/hub/bg2.png", false));

		caveAmbience = Resource.GetSound("res/sounds/ambience.ogg");
	}

	public override void init(Level level)
	{
		level.addEntity(new EventTrigger(new Vector2(1, 3), null, (Player player) =>
		{
			int direction = MathF.Sign(player.velocity.x);
			if (direction == 1)
				onTutorialEnter();
			else if (direction == -1)
				onBeachEnter();
		}), new Vector2(59, 38));
	}

	void onTutorialEnter()
	{
		GameState.instance.setAmbience(caveAmbience);
	}

	void onBeachEnter()
	{
		GameState.instance.setAmbience(level.ambientSound);
	}

	public override void render()
	{
		// Caves bg

		Renderer.DrawSprite(room.width - caveBg.width / 16, 0, 0.5f, caveBg.width / 16, caveBg.height / 16, 0, caveBg);


		// Waves

		Vector2i wavesPosition = new Vector2i(25, 37);

		int waveLayers = 10;
		for (int i = -5; i < waveLayers; i++)
		{
			float zoffset = ParallaxObject.LayerToZ(i <= 0 ? i * 0.4f : i * 0.2f + i * i * 0.1f); // i < 10 ? ParallaxObject.LayerToZ((i - 10) * -0.1f) : (i - 10) * -0.1f;
			float yoffset = i <= 0 ? zoffset * 3 : zoffset * 3; // (i - 10) * 0.5f;
			float brightness = (1 - MathF.Exp(-(i + 5) * 0.2f)); // MathF.Min(0.5f + i * 0.1f, 2);

			//Vector4 color = new Vector4(MathHelper.SRGBToLinear(MathHelper.ARGBToVector(0xFF36b3be).xyz * brightness * 1.2f) + MathHelper.ARGBToVector(0xFF6eafeb).xyz * 0.3f, 1);
			Vector4 color = MathHelper.SRGBToLinear(MathHelper.ARGBToVector(0xFF36b3be) * brightness * 1.2f);

			float xanimation = Time.currentTime / 1e9f * 16 * ((int)Hash.hash(i) % 2 * 2 - 1) * MathF.Pow(0.5f, 0.2f * i);
			float yanimation = 0.5f * MathF.Sin(Hash.hash(i) % 10 + Time.currentTime / 1e9f);

			Vector3 vertex = ParallaxObject.ParallaxEffect(new Vector3((Vector2)wavesPosition + new Vector2(0, yoffset + yanimation), zoffset));
			float width = i <= 0 ? 60 * MathF.Pow(0.5f, ParallaxObject.ZToLayer(zoffset)) : 60;
			int height = 35;
			int u0 = (int)xanimation;
			int v0 = 0;
			int w = (int)MathF.Round(width * 16);
			int h = (int)MathF.Round(height * 16) * 2;
			Renderer.DrawSprite(vertex.x - 0.5f * width, vertex.y - height, vertex.z, width, height, waves, u0, v0, w, h, color);
		}
	}
}
