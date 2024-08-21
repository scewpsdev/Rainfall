using Rainfall;


public class PlayerCamera : Entity
{
	const float LOOK_DOWN_DELAY = 0.5f;


	Player player;
	Vector2 target;

	float width, height;
	int scale;

	long lastDownInput = -1;


	public PlayerCamera(Player player)
	{
		this.player = player;
		position = player.position;
	}

	public Vector2i worldToScreen(Vector2 pos)
	{
		int x = (int)MathF.Round(MathHelper.Remap(pos.x, position.x - 0.5f * width, position.x + 0.5f * width, 0, 1) * Renderer.UIWidth);
		int y = (int)MathF.Round(MathHelper.Remap(pos.y, position.y - 0.5f * height, position.y + 0.5f * height, 1, 0) * Renderer.UIHeight);
		return new Vector2i(x, y);
	}

	public override void update()
	{
		width = 320 / 16.0f;
		scale = (int)MathF.Ceiling(Display.width / width / 16.0f);
		width = Display.width / (float)scale / 16.0f;
		height = width / Display.aspectRatio;

		//height = 1080 / 4.0f / 16.0f;

		// pixel perfect correction
		//scale = (int)MathF.Ceiling(Display.height / height / 16.0f);
		//height = Display.height / (float)scale / 16.0f;

		//float aspect = Display.aspectRatio;
		//width = aspect * height;

		float x0 = 0.0f + 0.5f * width;
		float x1 = GameState.instance.level.width - 0.5f * width;
		float y0 = 0.0f + 0.5f * height;
		float y1 = GameState.instance.level.height - 0.5f * height;

		target = player.position + new Vector2(0, 2);
		//if (player.inventoryOpen)
		//	target += new Vector2(-width / 4, 0);
		if (InputManager.IsDown("Down") && player.currentLadder == null)
		{
			if (lastDownInput == -1)
				lastDownInput = Time.currentTime;
			if ((Time.currentTime - lastDownInput) / 1e9f > LOOK_DOWN_DELAY)
				target += new Vector2(0, -height / 8 * 3 * (1 - MathF.Exp(-((Time.currentTime - lastDownInput) / 1e9f - LOOK_DOWN_DELAY) * 2.0f)));
		}
		else
		{
			lastDownInput = -1;
		}

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
