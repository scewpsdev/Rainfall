using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Player : Entity
{
	const float SPEED = 10;
	const float SHOOT_COOLDOWN = 0.1f;

	const float INTERACT_RANGE = 3.0f;


	Camera camera;

	SpriteSheet spriteSheet;
	Sprite sprite;
	SpriteAnimator animator;

	long lastShootTime = 0;

	public int points = 0;

	Interactable interactableInFocus = null;


	public Player(Vector2 position, Camera camera)
	{
		this.position = position;
		this.camera = camera;

		collider = new FloatRect(-0.5f, 0.0f, 1.0f, 1.0f);

		spriteSheet = new SpriteSheet(Resource.GetTexture("res/sprites/player.png", (uint)SamplerFlags.Point), 16, 16);
		sprite = new Sprite(spriteSheet, 0, 0);

		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 1, 0, 8, 8, true);
		animator.setAnimation("idle");
	}

	public void hit(Entity from)
	{
		// take damage
	}

	void updateMovement()
	{
		Vector2i input = Vector2i.Zero;

		if (Input.IsKeyDown(KeyCode.KeyA))
			input.x--;
		if (Input.IsKeyDown(KeyCode.KeyD))
			input.x++;
		if (Input.IsKeyDown(KeyCode.KeyS))
			input.y--;
		if (Input.IsKeyDown(KeyCode.KeyW))
			input.y++;

		Vector2 velocity = Vector2.Zero;

		if (input.x != 0 || input.y != 0)
		{
			velocity += ((Vector2)input).normalized * SPEED;
		}

		Vector2 delta = velocity * Time.deltaTime;

		CollisionDetection.DoWallCollision(position, collider, ref delta, level, out bool collidesX, out bool collidesY);

		position += delta;
	}

	void updateActions()
	{
		if (Input.IsMouseButtonDown(MouseButton.Left))
		{
			if ((Time.currentTime - lastShootTime) / 1e9f >= SHOOT_COOLDOWN)
			{
				shoot();
				lastShootTime = Time.currentTime;
			}
		}

		interactableInFocus = null;
		List<Entity> nearbyEntities = CollisionDetection.OverlapEntities(position - new Vector2(INTERACT_RANGE), new Vector2(2 * INTERACT_RANGE), level);
		foreach (Entity nearbyEntity in nearbyEntities)
		{
			if (nearbyEntity is Interactable)
			{
				Interactable interactable = nearbyEntity as Interactable;
				if (interactable.canInteract(this))
				{
					interactableInFocus = interactable;
					break;
				}
			}
		}

		if (interactableInFocus != null)
		{
			if (Input.IsKeyPressed(KeyCode.KeyE))
			{
				interactableInFocus.interact(this);
			}
		}
	}

	void shoot()
	{
		Vector2 target = camera.pixelToPosition(Input.cursorPosition);
		Vector2 shootOrigin = position + new Vector2(0.0f, 0.5f);
		Vector2 direction = (target - shootOrigin).normalized;
		level.addEntity(new Bullet(this, shootOrigin, direction));
	}

	void updateCamera()
	{
		float x0 = 0.5f * camera.width;
		float x1 = level.width - 0.5f * camera.width;
		float y0 = 0.5f * camera.height;
		float y1 = level.height - 0.5f * camera.height;

		float targetX = MathHelper.Clamp(position.x, x0, x1);
		float targetY = MathHelper.Clamp(position.y + 0.5f, y0, y1);

		camera.position.x = MathHelper.Lerp(camera.position.x, targetX, 5 * Time.deltaTime);
		camera.position.y = MathHelper.Lerp(camera.position.y, targetY, 5 * Time.deltaTime);

		camera.position.x = MathHelper.Clamp(camera.position.x, x0, x1);
		camera.position.y = MathHelper.Clamp(camera.position.y, y0, y1);
	}

	void updateAnimations()
	{
		animator.update(sprite);
	}

	public override void update()
	{
		updateMovement();
		updateActions();
		updateCamera();
		updateAnimations();
	}

	public override void draw()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, 1, 2, null, 0xFF7777FF);

		// Currency
		{
			int width = 150;
			int height = 40;
			Renderer.DrawUISprite(Display.width - width - 20, 20, width, height, null, 0xFFAAAAAA);
			Renderer.DrawUISprite(Display.width - width - 20, 20, width, height, null, 0xFF222222);
			Renderer.DrawUIText(Display.width - width - 20 + 4, 20 + 4, points.ToString(), 0xFFFFFFFF);
		}

		// Interaction
		{
			if (interactableInFocus != null)
			{
				string prompt = interactableInFocus.getInteractionPrompt(this);
				int width = prompt.Length * 32;
				Renderer.DrawUIText(Display.width / 2 - width / 2, Display.height - 100, prompt, 0xFFFFFFFF);
			}
		}
	}
}
