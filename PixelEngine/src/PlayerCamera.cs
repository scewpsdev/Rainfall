using Rainfall;


struct ScreenShakeData
{
	public long startTime;
	public Vector2 position;
	public float intensity;
	public float falloff;
}

public class PlayerCamera : Entity
{
	const float LOOK_DOWN_DELAY = 0.5f;


	Player player;
	Vector2 target;

	public float width, height;
	int scale;

	long lastDownInput = -1;

	Simplex simplex;
	List<ScreenShakeData> screenShakes = new List<ScreenShakeData>();
	Vector2 currentScreenShake;


	public PlayerCamera(Player player)
	{
		this.player = player;
		position = player.position;

		simplex = new Simplex((uint)Time.currentTime);
	}

	public Vector2i worldToScreen(Vector2 pos)
	{
		int x = (int)MathF.Round(MathHelper.Remap(pos.x, position.x - 0.5f * width, position.x + 0.5f * width, 0, 1) * Renderer.UIWidth);
		int y = (int)MathF.Round(MathHelper.Remap(pos.y, position.y - 0.5f * height, position.y + 0.5f * height, 1, 0) * Renderer.UIHeight);
		return new Vector2i(x, y);
	}

	public Vector2 screenToWorld(Vector2i pos)
	{
		float x = MathHelper.Remap((pos.x + 0.5f) / Display.width, 0, 1, position.x - 0.5f * width, position.x + 0.5f * width);
		float y = MathHelper.Remap((pos.y + 0.5f) / Display.height, 1, 0, position.y - 0.5f * height, position.y + 0.5f * height);
		return new Vector2(x, y);
	}

	public void addScreenShake(Vector2 position, float intensity, float falloff)
	{
		screenShakes.Add(new ScreenShakeData { position = position, intensity = intensity, falloff = falloff, startTime = Time.currentTime });
	}

	public override void update()
	{
		width = 1920 / 5.0f / 16.0f;
		scale = (int)MathF.Round(Display.width / width / 16.0f);
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

		target = player.position + player.collider.center;

		//if (player.inventoryOpen)
		//	target += new Vector2(-width / 4, 0);

		target.x = MathHelper.Clamp(target.x, x0, x1);
		target.y = MathHelper.Clamp(target.y, y0, y1);

		/*
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
		*/

		if (player.numOverlaysOpen == 0)
		{
			//Vector2 aimDirection = new Vector2(player.lookDirection.x, screenToWorld(Renderer.cursorPosition).y - position.y);
			Vector2 aimDirection = screenToWorld(Input.cursorPosition) - position;
			target += aimDirection * 0.2f;
		}

		position = Vector2.Lerp(position, target, 8 * Time.deltaTime);

		if (x1 > x0)
			position.x = MathHelper.Clamp(position.x, x0, x1);
		else
			position.x = (x0 + x1) * 0.5f;
		if (y1 > y0)
			position.y = MathHelper.Clamp(position.y, y0, y1);
		else
			position.y = (y0 + y1) * 0.5f;

		currentScreenShake = Vector2.Zero;
		for (int i = 0; i < screenShakes.Count; i++)
		{
			float elapsed = (Time.currentTime - screenShakes[i].startTime) / 1e9f;
			float amplitude = MathF.Exp(-elapsed * screenShakes[i].falloff) * screenShakes[i].intensity;

			float distance = (screenShakes[i].position - position).length;
			float attenuation = MathF.Exp(-distance * 0.3f);
			amplitude *= attenuation;

			Vector2 value = new Vector2(simplex.sample1f(elapsed * 20), simplex.sample1f(-elapsed * 20)) * amplitude;
			currentScreenShake += value;

			if (amplitude < 0.01f)
				screenShakes.RemoveAt(i--);
		}
	}

	public override void render()
	{
		Matrix projection = Matrix.CreateOrthographic(width, height, 1, -1);
		Matrix transform = getTransform(currentScreenShake);
		Matrix view = transform.inverted;

		Renderer.SetCamera(projection, view, position.x, position.y, width, height);
	}

	public float left
	{
		get => position.x - 0.5f * width;
	}

	public float right
	{
		get => position.x + 0.5f * width;
	}

	public float bottom
	{
		get => position.y - 0.5f * height;
	}

	public float top
	{
		get => position.y + 0.5f * height;
	}
}
