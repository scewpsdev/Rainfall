using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


public class MultilayerTrack
{
	uint[] sources;
	Sound[] sounds;

	public bool running = false;


	public MultilayerTrack(string name, int count)
	{
		sources = new uint[count];
		sounds = new Sound[count];

		for (int i = 0; i < count; i++)
		{
			sounds[i] = Resource.GetSound(name + (i + 1) + ".ogg");
		}
	}

	public void start()
	{
		for (int i = 0; i < sounds.Length; i++)
		{
			sources[i] = Audio.PlayBackground(sounds[i], 0);
			Audio.SetSourceLooping(sources[i], true);
			Audio.SetInaudibleBehavior(sources[i], true, false);
			Audio.SetProtect(sources[i], true);
		}
		running = true;
	}

	public void stop(float fadeout = 3)
	{
		for (int i = 0; i < sources.Length; i++)
		{
			Audio.SetProtect(sources[i], false);
			Audio.FadeoutSource(sources[i], fadeout);
		}
		running = false;
	}

	public void setLayer(int layer, float volume = 0.25f, float fadein = 3, float fadeout = 3)
	{
		for (int i = 0; i < sources.Length; i++)
		{
			if (i == layer)
				Audio.FadeVolume(sources[i], volume, fadein);
			else
				Audio.FadeoutVolume(sources[i], fadeout);
		}
	}
}
