using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public class MeshCollider
	{
		internal IntPtr handle;
		public Matrix transform { get; internal set; }


		internal MeshCollider(IntPtr handle, Matrix transform)
		{
			this.handle = handle;
			this.transform = transform;
		}

		public void destroy()
		{
			Physics.Physics_DestroyMeshCollider(handle);
		}
	}

	public class ConvexMeshCollider
	{
		internal IntPtr handle;
		public Matrix transform { get; internal set; }


		internal ConvexMeshCollider(IntPtr handle, Matrix transform)
		{
			this.handle = handle;
			this.transform = transform;
		}

		public void destroy()
		{
			Physics.Physics_DestroyConvexMeshCollider(handle);
		}
	}
}
