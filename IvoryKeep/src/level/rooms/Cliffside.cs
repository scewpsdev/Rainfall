﻿using Rainfall;
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


	public Cliffside(Room room)
	{
		this.room = room;

		waves = Resource.GetTexture("level/cliffside/waves.png", (uint)SamplerFlags.Point | (uint)SamplerFlags.VClamp);
	}

	public override void init(Level level)
	{
		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_TUTORIAL_FINISHED))
		{
			level.addEntity(new ExplosiveBarrel() { health = 1000 }, (Vector2)room.getMarker(40) + new Vector2(0.5f, 0.0f));
			level.addEntity(new ExplosiveBarrel() { health = 1000 }, (Vector2)room.getMarker(40) + new Vector2(-0.5f, 0.0f));
			for (int i = 0; i < 3; i++)
				level.setTile(room.getMarker(40).x + 1, room.getMarker(40).y + i, TileType.dirt);

			level.addEntity(new EventTrigger(new Vector2(1, 2), (Player player) =>
			{
				player.clearInventory();
			}, null), new Vector2(room.width - 1, 38));
		}

		//level.addEntity(new ParallaxObject(Resource.GetTexture("level/cliffside/parallax0.png", false), 10), new Vector2(33, 200));
		//level.addEntity(new ParallaxObject(Resource.GetTexture("level/cliffside/parallax1.png", false), 9), new Vector2(33, 200));
		//level.addEntity(new ParallaxObject(Resource.GetTexture("level/cliffside/parallax2.png", false), 8), new Vector2(33, 200));

		level.addEntity(new CameraFrame(new Vector2(20, 40)), new Vector2(33, 70));

		level.addEntity(level.exit = new LevelTransition(GameState.instance.hub, GameState.instance.hub.entrance, new Vector2i(2, 2), Vector2i.Right), new Vector2(room.width, 38));

		/*
		level.addEntity(new EventTrigger(new Vector2(1, 3), null, (Player player) =>
		{
			int direction = MathF.Sign(player.velocity.x);
			if (direction == 1)
				onTutorialEnter();
			else if (direction == -1)
				onBeachEnter();
		}), new Vector2(59, 38));
		*/
	}

	void onTutorialEnter()
	{
		//GameState.instance.setAmbience(caveAmbience);
	}

	void onBeachEnter()
	{
		GameState.instance.setAmbience(level.ambientSound);
	}

	public override void render()
	{
		// Caves bg

		//Renderer.DrawSprite(room.width - caveBg.width / 16, 0, 0.5f, caveBg.width / 16, caveBg.height / 16, 0, caveBg);


		// Waves

		Vector2i wavesPosition = new Vector2i(33, 39);

		int waveLayers = 10;
		for (int i = 0/*-5*/; i < waveLayers; i++)
		{
			float zoffset = ParallaxObject.LayerToZ(i <= 0 ? i * 0.4f : i * 0.2f + i * i * 0.1f); // i < 10 ? ParallaxObject.LayerToZ((i - 10) * -0.1f) : (i - 10) * -0.1f;
			float yoffset = i <= 0 ? zoffset * 3 : zoffset * 3; // (i - 10) * 0.5f;
			float brightness = (1 - MathF.Exp(-(i + 5) * 0.2f)); // MathF.Min(0.5f + i * 0.1f, 2);

			//Vector4 color = new Vector4(MathHelper.SRGBToLinear(MathHelper.ARGBToVector(0xFF36b3be).xyz * brightness * 1.2f) + MathHelper.ARGBToVector(0xFF6eafeb).xyz * 0.3f, 1);
			Vector4 color = MathHelper.SRGBToLinear(MathHelper.ARGBToVector(0xFF36b3be) * brightness * 1.2f);

			float xanimation = Time.currentTime / 1e9f * 16 * ((int)Hash.hash(i) % 2 * 2 - 1) * MathF.Pow(0.5f, 0.2f * i);
			float yanimation = 0.5f * MathF.Sin(Hash.hash(i) % 10 + Time.currentTime / 1e9f) * (i + 1);

			Vector3 vertex = ParallaxObject.ParallaxEffect(new Vector3((Vector2)wavesPosition + new Vector2(0, yoffset + yanimation), zoffset));
			float width = i <= 0 ? 60 * MathF.Pow(0.5f, ParallaxObject.ZToLayer(zoffset)) : 60;
			float height = 35;
			int u0 = (int)xanimation;
			int v0 = 0;
			int w = (int)MathF.Round(width * 16);
			int h = (int)MathF.Round(height * 16) * 2;
			//Renderer.DrawSprite(vertex.x - 0.5f * width, vertex.y - height, vertex.z, width, height, waves, u0, v0, w, h, color);
			float z = -i;
			float scale = (10 - z) * 0.1f;
			width *= scale;
			height *= scale;
			Renderer.DrawParallaxSprite(wavesPosition.x - 0.5f * width, wavesPosition.y - height + yanimation, z, width, height, 0, waves, u0, v0, w, h, color);
		}
	}
}
