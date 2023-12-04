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


		internal Sound(IntPtr handle)
		{
			this.handle = handle;
		}

		public float duration
		{
			get { return Native.Audio.Audio_SoundGetDuration(handle); }
		}
	}
}
