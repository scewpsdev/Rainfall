using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CameraFrame : Entity
{
	public Vector2 size;


	public CameraFrame(Vector2 size)
	{
		this.size = size;

		collider = new FloatRect(-0.5f * size.x, -0.5f * size.y, size.x, size.y);
		filterGroup = FILTER_CAMERA_FRAME;
	}
}
