using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class FollowCamera : Camera
{
	Entity follow;

	public FollowCamera(Entity follow)
	{
		this.follow = follow;
	}

	public override void update()
	{
		base.update();

		Matrix transform = follow.getModelMatrix() * Matrix.CreateTranslation(0, 3, 4);
		position = transform.translation;
		rotation = Quaternion.LookAt((follow.position - position) * new Vector3(1, 0, 1));
	}
}
