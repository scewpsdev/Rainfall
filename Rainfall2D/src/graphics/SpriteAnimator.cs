using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public class SpriteAnimation
	{
		public string name;
		public Vector2i start;
		public Vector2i delta;
		public int length;
		public int fps;
		public bool looping;
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

		public SpriteAnimation getAnimation(string name)
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
				SpriteAnimation current = getAnimation(currentAnimation);
				if (current != null)
				{
					long now = Time.currentTime;
					float timer = (now - startTime) / 1e9f;
					int frameIdx = (int)(timer * current.fps);
					if (current.looping)
						frameIdx %= current.length;
					else
						frameIdx = Math.Min(frameIdx, current.length - 1);
					sprite.position = current.start + current.delta * frameIdx;
				}
			}
		}

		public int currentFrame
		{
			get
			{
				if (currentAnimation != null)
				{
					SpriteAnimation current = getAnimation(currentAnimation);
					long now = Time.currentTime;
					float timer = (now - startTime) / 1e9f;
					int frameIdx = (int)(timer * current.fps);
					if (current.looping)
						frameIdx %= current.length;
					else
						frameIdx = Math.Min(frameIdx, current.length - 1);
					return frameIdx;
				}
				return -1;
			}
		}

		public bool finished
		{
			get
			{
				if (currentAnimation != null)
				{
					SpriteAnimation current = getAnimation(currentAnimation);
					long now = Time.currentTime;
					float timer = (now - startTime) / 1e9f;
					int frameIdx = (int)(timer * current.fps);
					if (current.looping)
						return false;
					else
						return frameIdx >= current.length;
				}
				return false;
			}
		}
	}
}
