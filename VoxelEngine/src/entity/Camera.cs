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
	public float far = 500.0f;


	public float pitch, yaw;

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
		rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw) * Quaternion.FromAxisAngle(Vector3.Right, pitch);
		Matrix model = Matrix.CreateTranslation(position) * Matrix.CreateRotation(rotation);
		return model.inverted;
	}
}
