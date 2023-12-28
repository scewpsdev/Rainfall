using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Camera : Entity
{
	public float fov = 90.0f;
	public float near = 0.01f;
	public float far = 200.0f;


	public override void update()
	{
		Audio.UpdateListener(position, rotation);
	}

	public Matrix getProjectionMatrix()
	{
		return Matrix.CreatePerspective(MathHelper.ToRadians(fov), Display.aspectRatio, near, far);
	}

	public Matrix getViewMatrix()
	{
		Matrix model = Matrix.CreateTranslation(position) * Matrix.CreateRotation(rotation);
		return model.inverted;
	}
}
