using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Camera : Entity
{
	public const int pixelScale = 6;
	public const int tileRes = 16;
	public const float near = -100.0f;
	public const float far = 100.0f;


	Quaternion rotation;

	public override void init()
	{
		position.y = 10.0f;
		rotation = Quaternion.FromAxisAngle(Vector3.Right, MathF.PI * -0.5f);
	}

	public override void update()
	{
		Audio.UpdateListener(position, rotation);
	}

	public Matrix getProjectionMatrix()
	{
		float height = Display.viewportSize.y / (float)pixelScale / tileRes;
		float width = Display.aspectRatio * height;
		return Matrix.CreateOrthographic(width, height, near, far);
	}

	public Matrix getViewMatrix()
	{
		Matrix model = Matrix.CreateTranslation(position) * Matrix.CreateRotation(rotation);
		return model.inverted;
	}
}
