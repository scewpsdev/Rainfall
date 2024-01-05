using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	struct SpriteAnimation
	{
		internal string name;
		internal Vector2i start;
		internal Vector2i delta;
		internal int length;
		internal int fps;
		internal bool looping;
	}

	public class SpriteAnimator
	{
		List<SpriteAnimation> animations = new List<SpriteAnimation>();
		string currentAnimation = null;
		long startTime = 0;


		public void addAnimation(string name, int x, int y, int dx, int dy, int length, int fps, bool looping)
		{
			animations.Add(new SpriteAnimation { name = name, start = new Vector2i(x, y), delta = new Vector2i(dx, dy), length = length, fps = fps, looping = looping });
		}

		public void setAnimation(string name)
		{
			if (currentAnimation != name)
			{
				startTime = Time.currentTime;
				currentAnimation = name;
			}
		}

		SpriteAnimation? getAnimation(string name)
		{
			foreach (SpriteAnimation animation in animations)
			{
				if (animation.name == name)
					return animation;
			}
			return null;
		}

		public void update(Sprite sprite)
		{
			if (currentAnimation != null)
			{
				SpriteAnimation? current = getAnimation(currentAnimation);
				if (current != null)
				{
					long now = Time.currentTime;
					float timer = (now - startTime) / 1e9f;
					int frameIdx = (int)(timer * current.Value.fps);
					if (current.Value.looping)
						frameIdx %= current.Value.length;
					else
						frameIdx = Math.Min(frameIdx, current.Value.length - 1);
					sprite.position = current.Value.start + current.Value.delta * frameIdx;
				}
			}
		}
	}
}
