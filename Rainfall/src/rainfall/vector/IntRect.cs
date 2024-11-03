using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public class IntRect
	{
		public Vector2i position;
		public Vector2i size;


		public IntRect(Vector2i position, Vector2i size)
		{
			this.position = position;
			this.size = size;
		}

		public IntRect(int x, int y, int width, int height)
		{
			position = new Vector2i(x, y);
			size = new Vector2i(width, height);
		}

		public Vector2i min
		{
			get => position;
		}

		public Vector2i max
		{
			get => position + size;
		}
	}
}
