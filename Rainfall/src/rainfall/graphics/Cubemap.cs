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
		internal ushort handle;

		public readonly TextureInfo info;


		internal Cubemap(ushort handle, TextureInfo info)
		{
			this.handle = handle;
			this.info = info;
		}
	}
}
