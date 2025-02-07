using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public class Sound
	{
		internal IntPtr resource;


		internal Sound(IntPtr resource)
		{
			this.resource = resource;
		}

		public IntPtr handle => Resource.Resource_SoundGetHandle(resource);
		public float duration => Resource.Resource_SoundGetDuration(resource);

		public bool singleInstance
		{
			set
			{
				Audio.Audio_SoundSetSingleInstance(handle, (byte)(value ? 1 : 0));
			}
		}
	}
}
