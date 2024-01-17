using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public class Sound
	{
		internal IntPtr handle;

		public readonly float duration;


		internal Sound(IntPtr handle, float duration)
		{
			this.handle = handle;
			this.duration = duration;
		}
	}
}
