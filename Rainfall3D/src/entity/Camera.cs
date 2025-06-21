using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Camera : Entity
{
	public float fov = 90.0f;
	public float near = 0.05f;
	public float far = 100.0f;

	public float pitch, yaw, roll;


	public override void draw(GraphicsDevice graphics)
	{
		Audio.UpdateListener(position, rotation);
		Renderer.SetCamera(position, rotation, fov, Display.aspectRatio, near, far);
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
