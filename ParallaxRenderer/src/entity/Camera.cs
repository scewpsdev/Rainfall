using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Camera : Entity
{
	public float fov = 90;
	public float near = 0.1f;
	public float far = 100;

	const float height = 180 / 16.0f;


	public override void update()
	{
		Vector2 delta = Vector2.Zero;
		if (Input.IsKeyDown(KeyCode.A)) delta.x--;
		if (Input.IsKeyDown(KeyCode.D)) delta.x++;
		if (Input.IsKeyDown(KeyCode.W)) delta.y++;
		if (Input.IsKeyDown(KeyCode.S)) delta.y--;
		position.xy += delta * Time.deltaTime * 5;

		position.z = 0.5f * height;
	}

	public override void render()
	{
		float width = Display.aspectRatio * height;
		//Matrix projection = Matrix.CreateOrthographic(width, height, near, far);
		Matrix projection = Matrix.CreatePerspective(fov, Display.aspectRatio, near, far);
		Matrix view = Matrix.CreateTranslation(-position);
		Renderer.SetCamera(projection, view, position.x, position.y, width, height, 0, 0);
	}
}
