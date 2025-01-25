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
		public float fps;
		public bool looping;

		public float duration
		{
			get => length / fps;
			set { fps = length / value; }
		}
	}

	public class SpriteAnimationEvent
	{
		public string name;
		public int frame;
		public Action action;
	}

	public class SpriteAnimator
	{
		List<SpriteAnimation> animations = new List<SpriteAnimation>();
		public string currentAnimation = null;
		public long startTime = 0;
		public int lastFrameIdx = -1;

		List<SpriteAnimationEvent> events = new List<SpriteAnimationEvent>();


		/*
		public void addAnimation_(string name, int x, int y, int dx, int dy, int length, float fps, bool looping)
		{
			animations.Add(new SpriteAnimation { name = name, start = new Vector2i(x, y), delta = new Vector2i(dx, dy), length = length, fps = fps, looping = looping });
		}
		*/

		public void addAnimation(string name, int start, int length, float duration, bool looping = false)
		{
			animations.Add(new SpriteAnimation { name = name, start = new Vector2i(start, 0), delta = new Vector2i(1, 0), length = length, fps = length / duration, looping = looping });
		}

		public void addAnimation(string name, int length, float duration, bool looping = false)
		{
			int start = 0;
			if (animations.Count > 0)
			{
				SpriteAnimation lastAnim = animations[animations.Count - 1];
				start = lastAnim.start.x + lastAnim.length * lastAnim.delta.x;
			}
			addAnimation(name, start, length, duration, looping);
		}

		public void setAnimation(string name)
		{
			if (currentAnimation != name && getAnimation(name) != null)
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

		public void addAnimationEvent(string name, int frame, Action action)
		{
			events.Add(new SpriteAnimationEvent() { name = name, frame = frame, action = action });
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
					sprite.position = current.start * sprite.size + current.delta * sprite.size * frameIdx;

					for (int i = 0; i < events.Count; i++)
					{
						if (events[i].name == currentAnimation)
						{
							if (frameIdx == events[i].frame && lastFrameIdx != frameIdx)
								events[i].action();
						}
					}

					lastFrameIdx = frameIdx;
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
			set
			{
				if (currentAnimation != null)
				{
					SpriteAnimation current = getAnimation(currentAnimation);
					int frameIdx = value;
					float timer = frameIdx / current.fps;
					long now = Time.currentTime;
					startTime = now - (long)(timer * 1e9f);
				}
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
