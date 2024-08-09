using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PlayerCamera : Entity
{
	Player player;
	Vector2 target;

	float width, height;
	int scale;


	public PlayerCamera(Player player)
	{
		this.player = player;
	}

	public Vector2i worldToScreen(Vector2 pos)
	{
		int x = (int)(MathHelper.Remap(pos.x, position.x - 0.5f * width, position.x + 0.5f * width, 0, 1) * Renderer.UIWidth);
		int y = (int)(MathHelper.Remap(pos.y, position.y - 0.5f * height, position.y + 0.5f * height, 1, 0) * Renderer.UIHeight);
		return new Vector2i(x, y);
	}

	public override void update()
	{
		height = 1080 / 4.0f / 16.0f;

		// pixel perfect correction
		scale = (int)MathF.Ceiling(Display.height / height / 16.0f);
		height = Display.height / (float)scale / 16.0f;

		float aspect = Display.aspectRatio;
		width = aspect * height;

		float x0 = 0.0f + 0.5f * width;
		float x1 = GameState.instance.level.width - 0.5f * width;
		float y0 = 0.0f + 0.5f * height;
		float y1 = GameState.instance.level.height - 0.5f * height;

		target = player.position + new Vector2(0, 2);
		if (player.inventoryOpen)
			target += new Vector2(-width / 4, 0);

		target.x = MathHelper.Clamp(target.x, x0, x1);
		target.y = MathHelper.Clamp(target.y, y0, y1);

		position = Vector2.Lerp(position, target, 5 * Time.deltaTime);

		if (x1 > x0)
			position.x = MathHelper.Clamp(position.x, x0, x1);
		else
			position.x = (x0 + x1) * 0.5f;
		if (y1 > y0)
			position.y = MathHelper.Clamp(position.y, y0, y1);
		else
			position.y = (y0 + y1) * 0.5f;
	}

	public override void render()
	{
		Matrix projection = Matrix.CreateOrthographic(width, height, 1, -1);
		Matrix view = getTransform().inverted;

		Renderer.SetCamera(projection, view, -0.5f * width, 0.5f * width, -0.5f * height, 0.5f * height);
	}
}
