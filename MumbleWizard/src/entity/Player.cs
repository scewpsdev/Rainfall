using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Player : Entity
{
	const float INTERACT_RANGE = 3.0f;

	const float HIT_COOLDOWN = 2.0f;


	Camera camera;

	SpriteSheet spriteSheet;
	Sprite sprite;
	SpriteAnimator animator;

	SpriteSheet bookSheet;
	Sprite bookSprite;

	Texture gui;

	Texture shadow;
	bool direction = true;
	bool running = false;
	int lastStep = 0;

	public AudioSource audio;
	Sound[] sfxStep;
	Sound sfxDing;
	Sound[] sfxHit;
	Sound[] sfxMumble;
	public Sound sfxPowerup;
	public int pointsCollected = 0;

	long shootBegin;
	long lastHitTime;

	public float speed;
	public int maxHealth;
	public int health;
	public int fireRate;
	public int damage;
	public int knockback;
	public int luck;
	public int penetration;
	public int ricochet;

	public int points;

	Interactable interactableInFocus = null;


	public Player(Vector2 position, Camera camera)
	{
		this.position = position;
		this.camera = camera;

		size = new Vector2(2, 2);
		collider = new FloatRect(-0.5f, 0.0f, 1.0f, 1.0f);
		hitbox = new FloatRect(-0.5f, 0, 1, 2);

		spriteSheet = new SpriteSheet(Resource.GetTexture("res/sprites/player.png", false), 16, 16);
		sprite = new Sprite(spriteSheet, 0, 0);

		bookSheet = new SpriteSheet(Resource.GetTexture("res/sprites/book.png", false), 24, 24);
		bookSprite = new Sprite(bookSheet, 0, 0);

		gui = Resource.GetTexture("res/sprites/ui.png", false);

		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 1, 0, 4, 6, true);
		animator.addAnimation("run", 4, 0, 1, 0, 8, 10, true);
		animator.setAnimation("idle");

		shadow = Resource.GetTexture("res/sprites/shadow.png", true);

		audio = new AudioSource(new Vector3(position, 0.0f));

		sfxStep = [
			Resource.GetSound("res/sounds/step1.ogg"),
			Resource.GetSound("res/sounds/step2.ogg"),
		];
		sfxDing = Resource.GetSound("res/sounds/ding.ogg");
		sfxHit = [
			Resource.GetSound("res/sounds/hit1.ogg"),
			Resource.GetSound("res/sounds/hit2.ogg"),
		];
		sfxPowerup = Resource.GetSound("res/sounds/powerup.ogg");
		sfxMumble = [
			Resource.GetSound("res/sounds/mumble1.ogg"),
			Resource.GetSound("res/sounds/mumble2.ogg"),
			Resource.GetSound("res/sounds/mumble3.ogg"),
			Resource.GetSound("res/sounds/mumble4.ogg"),
			Resource.GetSound("res/sounds/mumble5.ogg"),
			Resource.GetSound("res/sounds/mumble6.ogg"),
			Resource.GetSound("res/sounds/mumble7.ogg"),
			Resource.GetSound("res/sounds/mumble8.ogg"),
		];

		reset();
	}

	public override void reset()
	{
		shootBegin = 0;
		lastHitTime = 0;

		speed = 5;
		maxHealth = 3;
		health = 3;
		fireRate = 2;
		damage = 3;
		knockback = 5;
		luck = 10;
		penetration = 0;
		ricochet = 0;

		points = 0;
	}

	public override void destroy()
	{
		audio.destroy();
	}

	public void hit(Entity from)
	{
		if ((Time.currentTime - lastHitTime) / 1e9f < HIT_COOLDOWN)
			return;

		health--;
		if (health == 0)
			onDeath();

		audio.playSoundOrganic(sfxHit, 2);

		Gaem.instance.manager.hitsTaken++;

		lastHitTime = Time.currentTime;
	}

	void onDeath()
	{
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
			velocity += ((Vector2)input).normalized * speed;
		}
		running = input.x != 0 || input.y != 0;

		Vector2 delta = velocity * Time.deltaTime;

		CollisionDetection.DoWallCollision(position, collider, ref delta, level, out bool collidesX, out bool collidesY);

		position += delta;
	}

	void updateActions()
	{
		if (Input.IsMouseButtonDown(MouseButton.Left))
		{
			if (shootBegin == 0)
			{
				shootBegin = Time.currentTime;
				//if (!audio.isPlaying)
				//audio.playSoundOrganic(sfxMumble, 0.4f, 1.0f, 1.0f, 0.1f);
			}
			if (shootBegin != 0 && (Time.currentTime - shootBegin) / 1e9f >= 1.0f / fireRate)
			{
				shoot();
				shootBegin = 0;
			}
		}
		else
		{
			shootBegin = 0;
		}

		List<Entity> nearbyEntities = CollisionDetection.OverlapEntities(position - new Vector2(INTERACT_RANGE), new Vector2(2 * INTERACT_RANGE), level);

		interactableInFocus = null;
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

		foreach (Entity nearbyEntity in nearbyEntities)
		{
			if (nearbyEntity is Toucheable)
			{
				if (nearbyEntity.position.x + nearbyEntity.collider.max.x > position.x + collider.min.x &&
					nearbyEntity.position.x + nearbyEntity.collider.min.x < position.x + collider.max.x &&
					nearbyEntity.position.y + nearbyEntity.collider.max.y > position.y + collider.min.y &&
					nearbyEntity.position.y + nearbyEntity.collider.min.y < position.y + collider.max.y)
				{
					Toucheable toucheable = nearbyEntity as Toucheable;
					toucheable.touch(this);
				}
			}
		}
	}

	void shoot()
	{
		Vector2 target = camera.pixelToPosition(Input.cursorPosition);
		Vector2 direction = (target - position).normalized;
		Vector2 shootOrigin = position + direction * 0.5f;
		level.addEntity(new Bullet(this, damage, shootOrigin, direction));
		Gaem.instance.manager.bulletsFired++;
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
		if (running)
		{
			animator.getAnimation("run").fps = (int)speed * 2;
			animator.setAnimation("run");

			if (animator.currentFrame == 3 || animator.currentFrame == 7)
			{
				if (lastStep != animator.currentFrame)
				{
					//if (!audio.isPlaying)
					audio.playSoundOrganic(sfxStep, 3);
					lastStep = animator.currentFrame;
				}
			}
		}
		else
			animator.setAnimation("idle");

		animator.update(sprite);
	}

	public override void update()
	{
		if (health == 0)
			return;

		updateMovement();
		updateActions();
		updateCamera();
		updateAnimations();

		audio.updateTransform(new Vector3(position, 0.0f));
		if (pointsCollected > 0)
		{
			pointsCollected = Math.Min(pointsCollected, 50);
			if (audio.timePlaying > 0.1f && audio.currentSound == sfxDing || audio.currentSound != sfxDing)
			{
				//audio.stop();
				audio.playSoundOrganic(sfxDing, 0.5f, 1.0f, 0.2f, 0.2f);
			}
			pointsCollected--;
		}
	}

	public override void draw()
	{
		Vector2 cursorWorldPos = camera.pixelToPosition(Input.cursorPosition);
		Vector2 cursorDirection = (cursorWorldPos - position).normalized;
		float cursorRotation = MathF.Atan2(cursorDirection.y, cursorDirection.x) - MathF.PI * 0.5f;
		direction = cursorDirection.x > 0;

		Renderer.DrawVerticalSprite(position.x - 0.5f * size.x, position.y, size.x, size.y, sprite, !direction, 0xFFFFFFFF);
		Renderer.DrawSprite(position.x - 1.5f, position.y - 1.5f, 0.75f, 3, 3, cursorRotation, bookSprite, false);
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 0.01f, 1, 1, shadow, 0, 0, 8, 8, 0xFFFFFFFF);

		if (shootBegin != 0)
		{
			float progress = (Time.currentTime - shootBegin) / 1e9f * fireRate;
			Renderer.DrawVerticalSprite(position.x - 0.5f, position.y, 2.5f, 1.0f, 0.125f, null, false, 0xFF111111);
			Renderer.DrawVerticalSprite(position.x - 0.5f, position.y - 0.0001f, 2.5f, progress, 0.125f, null, false, 0xFF9999FF);
		}

		Renderer.DrawLight(position + new Vector2(0.0f, 0.5f), new Vector3(1.0f, 0.9f, 0.7f) * 4, 12.0f);

		// Health
		{
			for (int i = 0; i < maxHealth; i++)
			{
				Renderer.DrawUISprite(20 + i * 54, 20, 9 * 6, 9 * 6, gui, i < health ? 0 : 9, 0, 9, 9);
			}
		}

		// Currency
		{
			int width = 240;
			int height = 54;
			Renderer.DrawUISprite(20, 100, width, height, null, false, 0xFFAAAAAA);
			Renderer.DrawUISprite(26, 106, width - 12, height - 12, null, false, 0xFF222222);
			Renderer.DrawUIText(26 + 6, 106 + 6, points.ToString(), 0xFFFFFFFF);
		}

		// Wave
		{
			Renderer.DrawUIText(26 + 6, 156 + 6, "Wave " + Gaem.instance.manager.wave.ToString(), 0xFFFFFFFF);
		}

		// Interaction
		{
			if (interactableInFocus != null)
			{
				interactableInFocus.getInteractionPrompt(this, out string prompt, out uint color);
				int width = prompt.Length * 32;
				Renderer.DrawUIText(Display.width / 2 - width / 2, Display.height - 100, prompt, color);
			}
		}
	}
}
