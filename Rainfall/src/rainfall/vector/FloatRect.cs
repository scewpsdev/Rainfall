using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public struct FloatRect
	{
		public Vector2 position;
		public Vector2 size;


		public FloatRect(Vector2 position, Vector2 size)
		{
			this.position = position;
			this.size = size;
		}

		public FloatRect(float x, float y, float width, float height)
		{
			position = new Vector2(x, y);
			size = new Vector2(width, height);
		}

		public Vector2 min
		{
			get => position;
		}

		public Vector2 max
		{
			get => position + size;
		}

		public Vector2 center
		{
			get => new Vector2(position.x + 0.5f * size.x, position.y + 0.5f * size.y);
		}

		public static bool operator ==(FloatRect a, FloatRect b)
		{
			return a.position == b.position && a.size == b.size;
		}

		public static bool operator !=(FloatRect a, FloatRect b)
		{
			return a.position != b.position || a.size != b.size;
		}

		public override bool Equals(object obj)
		{
			if (obj is FloatRect)
			{
				return this == (FloatRect)obj;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
