using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public class Cubemap
	{
		internal IntPtr resource;
		public readonly ushort handle;
		public readonly TextureInfo info;


		internal unsafe Cubemap(IntPtr resource)
		{
			this.resource = resource;
			handle = Resource.Resource_TextureGetHandle(resource);
			info = *Resource.Resource_TextureGetInfo(resource);
		}

		public Cubemap(ushort handle, TextureInfo info)
		{
			this.handle = handle;
			this.info = info;
		}
	}
}
