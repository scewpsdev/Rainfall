using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Player : Entity
{
	Camera camera;

	Tileset tileset;
	Sprite sprite;
	SpriteAnimator animator;

	bool direction = true;


	public Player(Camera camera)
	{
		this.camera = camera;

		tileset = new Tileset(Resource.GetTexture("res/entity/player/player.png"), 16, 16);
		sprite = new Sprite(tileset, 0, 0);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 1, 0, 8, 6, true);
		animator.setAnimation("idle");
	}

	public override void update()
	{
		Vector2 delta = Vector2.Zero;
		if (Input.IsKeyDown(KeyCode.KeyA)) delta.x -= 5;
		if (Input.IsKeyDown(KeyCode.KeyD)) delta.x += 5;
		if (Input.IsKeyDown(KeyCode.KeyW)) delta.y -= 5;
		if (Input.IsKeyDown(KeyCode.KeyS)) delta.y += 5;
		if (delta.x > 0)
			direction = true;
		else if (delta.x < 0)
			direction = false;

		position.x += delta.x * Time.deltaTime;
		position.z += delta.y * Time.deltaTime;

		//if (Input.IsKeyDown(KeyCode.Left)) camera.position.x -= 5 * Time.deltaTime;
		//if (Input.IsKeyDown(KeyCode.Right)) camera.position.x += 5 * Time.deltaTime;
		//if (Input.IsKeyDown(KeyCode.Up)) camera.position.z -= 5 * Time.deltaTime;
		//if (Input.IsKeyDown(KeyCode.Down)) camera.position.z += 5 * Time.deltaTime;

		camera.position = position + new Vector3(0.0f, 0.0f, -0.5f);

		animator.update(sprite);

		if (Input.IsMouseButtonPressed(MouseButton.Left) || Input.IsKeyPressed(KeyCode.KeyJ))
		{
			Vector2i cursorOffset = Input.cursorPosition - Display.viewportSize / 2;
			Vector3 direction = new Vector3(cursorOffset.x, 0, cursorOffset.y).normalized;
			Application.instance.level.addEntity(new MagicBullet(direction), position + 0.5f * direction + new Vector3(0.0f, 0.5f, 0.0f));
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawEntitySprite(position, new Vector2(1.0f), sprite, direction);
	}
}
