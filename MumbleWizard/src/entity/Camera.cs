using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Camera : Entity
{
	public float width, height;


	public Camera(float width, float height)
	{
		this.width = width;
		this.height = height;
	}

	public override void update()
	{
		int pixelScale = Display.width / 320;
		float pixelsX = Display.width / (float)pixelScale;
		float pixelsY = Display.height / (float)pixelScale;
		int pixelsPerUnit = 8;
		float unitsX = pixelsX / pixelsPerUnit;
		float unitsY = pixelsY / pixelsPerUnit;
		width = unitsX;
		height = unitsY;

		Audio.UpdateListener(new Vector3(position, 2.0f), Quaternion.Identity);
	}

	public float left { get => position.x - 0.5f * width; }
	public float right { get => position.x + 0.5f * width; }
	public float bottom { get => position.y - 0.5f * height; }
	public float top { get => position.y + 0.5f * height; }

	public Vector2 pixelToPosition(Vector2i pixel)
	{
		Vector2 viewSpace = new Vector2((pixel.x + 0.5f) / Display.width * 2 - 1, -(2 * (pixel.y + 0.5f) / Display.height - 1)) * 0.5f * new Vector2(width, height);
		Vector2 worldSpace = position + viewSpace;
		return worldSpace;
	}

	public Matrix getProjectionMatrix()
	{
		return Matrix.CreateOrthographic(width, height, -10.0f, 10.0f);
	}

	public Matrix getViewMatrix()
	{
		return Matrix.CreateTranslation(-position.x, -position.y, 0.0f);
	}
}
