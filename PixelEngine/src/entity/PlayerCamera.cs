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

	Simplex simplex;
	List<ScreenShakeData> screenShakes = new List<ScreenShakeData>();
	Vector2 currentScreenShake;


	public PlayerCamera(Player player)
	{
		this.player = player;
		position = player.position;

		simplex = new Simplex((uint)Time.currentTime);
	}

	public Vector2 worldToScreen(Vector2 pos)
	{
		float x = MathHelper.Remap(pos.x, position.x - 0.5f * width, position.x + 0.5f * width, 0, 1) * Renderer.UIWidth;
		float y = MathHelper.Remap(pos.y, position.y - 0.5f * height, position.y + 0.5f * height, 1, 0) * Renderer.UIHeight;
		return new Vector2(x, y);
	}

	public Vector2 screenToWorld(Vector2i pos)
	{
		float x = MathHelper.Remap((pos.x + 0.5f) / Renderer.UIWidth, 0, 1, position.x - 0.5f * width, position.x + 0.5f * width);
		float y = MathHelper.Remap((pos.y + 0.5f) / Renderer.UIHeight, 1, 0, position.y - 0.5f * height, position.y + 0.5f * height);
		return new Vector2(x, y);
	}

	public void addScreenShake(Vector2 position, float intensity, float falloff)
	{
		screenShakes.Add(new ScreenShakeData { position = position, intensity = intensity, falloff = falloff, startTime = Time.currentTime });
	}

	public override void update()
	{
		width = PixelEngine.instance.width / 16.0f; // Display.width / (float)scale / 16.0f;
		height = PixelEngine.instance.height / 16.0f; // Display.height / (float)scale / 16.0f;

		//height = 1080 / 4.0f / 16.0f;

		// pixel perfect correction
		//scale = (int)MathF.Ceiling(Display.height / height / 16.0f);
		//height = Display.height / (float)scale / 16.0f;

		//float aspect = Display.aspectRatio;
		//width = aspect * height;

		float x0 = 0.0f + 0.5f * width;
		float x1 = GameState.instance.level.width - 0.5f * width;
		float y0 = 0.0f; // + 0.5f * height;
		float y1 = GameState.instance.level.height - 0.5f * height;

		HitData currentCameraFrame = GameState.instance.level.sample(position, FILTER_CAMERA_FRAME);
		//if (currentCameraFrame != null)
		//{
		//	target = currentCameraFrame.entity.position;
		//}
		//else
		{
			target = player.position + player.collider.center;
		}

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

		if (player.numOverlaysOpen == 0 || player.numOverlaysOpen == 1 && player.inventoryOpen)
		{
			//Vector2 aimDirection = new Vector2(player.lookDirection.x, screenToWorld(Renderer.cursorPosition).y - position.y);
			Vector2 aimDirection = screenToWorld(Renderer.cursorPosition) - position;
			if (Settings.game.aimMode == AimMode.Directional)
				target += aimDirection * 0.1f * player.aimDistance;
			else if (Settings.game.aimMode == AimMode.Crosshair)
				target += aimDirection * 0.1f * player.aimDistance;
		}

		position.x = MathHelper.Lerp(position.x, target.x, 4 * Time.deltaTime);
		position.y = MathHelper.Lerp(position.y, target.y, 8 * Time.deltaTime);
		//velocity = Vector2.Lerp(velocity, player.velocity, 10 * Time.deltaTime);
		//position += velocity * Time.deltaTime;

		if (width < level.width)
			position.x = MathHelper.Clamp(position.x, x0, x1);
		else
			position.x = 0.5f * level.width;

		if (height < level.height)
			position.y = MathHelper.Clamp(position.y, y0, y1);
		else
			position.y = MathF.Min(position.y, 0.5f * level.height);

		currentScreenShake = Vector2.Zero;
		for (int i = 0; i < screenShakes.Count; i++)
		{
			float elapsed = (Time.currentTime - screenShakes[i].startTime) / 1e9f;
			float amplitude = MathF.Exp(-elapsed * 3) * screenShakes[i].intensity;

			float distance = (screenShakes[i].position - position).length;
			float attenuation = MathF.Exp(-distance * 0.1f * screenShakes[i].falloff);
			amplitude *= attenuation;

			Vector2 value = new Vector2(simplex.sample1f(elapsed * 20), simplex.sample1f(-elapsed * 20)) * amplitude;
			currentScreenShake += value;

			if (amplitude < 0.01f)
				screenShakes.RemoveAt(i--);
		}
	}

	public override void render()
	{
		Renderer.SetCamera(position + currentScreenShake, width, height);
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
