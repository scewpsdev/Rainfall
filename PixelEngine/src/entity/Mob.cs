using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class Mob : Entity, Hittable
{
	const float SPRINT_MULTIPLIER = 1.8f;
	const float DUCKED_MULTIPLIER = 0.8f;
	const float STUN_DURATION = 0.4f;


	public float speed = 4;
	public float climbingSpeed = 4;
	public float jumpPower = 12;
	public float gravity = -30;

	public bool inputLeft, inputRight, inputUp, inputDown;
	public bool inputSprint, inputDuck, inputJump;

	public int direction = 1;
	float currentSpeed;
	public bool isGrounded = false;
	bool isSprinting = false;
	bool isDucked = false;
	bool isClimbing = false;
	float distanceWalked = 0;
	bool isStunned = false;
	long stunTime = -1;

	protected Sprite sprite;

	public int health = 1;

	public int damage = 1;

	protected AI ai;

	Interactable interactableInFocus = null;
	Climbable currentLadder = null;

	public Item handItem = null;


	public Mob()
	{
		filterGroup = FILTER_MOB;
	}

	public override void destroy()
	{
	}

	public void hit(int damage, Entity by)
	{
		health -= damage;

		if (health > 0)
			stun();
		else
			onDeath();
	}

	void onDeath()
	{
		remove();
	}

	public void stun()
	{
		stunTime = Time.currentTime;
	}

	void updateMovement()
	{
		Vector2 delta = Vector2.Zero;

		isStunned = stunTime != -1 && (Time.currentTime - stunTime) / 1e9f < STUN_DURATION;

		if (!isStunned)
		{
			if (inputLeft)
				delta.x--;
			if (inputRight)
				delta.x++;
			if (isClimbing)
			{
				if (inputUp)
				{
					if (GameState.instance.level.getClimbable(position + new Vector2(0, 0.2f)) != null)
						delta.y++;
				}
				if (inputDown)
					delta.y--;
			}

			isSprinting = inputSprint;

			isDucked = inputDuck;
			collider.size.y = isDucked ? 0.5f : 1;

			if (inputJump)
			{
				if (isGrounded)
				{
					velocity.y = jumpPower;
				}
				else if (isClimbing)
				{
					velocity.y = jumpPower;
					currentLadder = null;
					isClimbing = false;
				}
			}
		}

		if (delta.x != 0)
		{
			//if (isGrounded)
			{
				if (delta.x > 0)
					direction = 1;
				else if (delta.x < 0)
					direction = -1;

				currentSpeed = isSprinting ? SPRINT_MULTIPLIER * speed : isDucked ? DUCKED_MULTIPLIER * speed : speed;
				velocity.x = delta.x * currentSpeed;
			}
		}
		else
		{
			//if (isGrounded)
			velocity.x = 0.0f;
		}

		if (!isClimbing)
		{
			velocity.y += gravity * Time.deltaTime;
		}
		else
		{
			velocity.y = delta.y * climbingSpeed;
		}

		Vector2 displacement = velocity * Time.deltaTime;
		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, inputDown);

		isGrounded = false;
		if ((collisionFlags & Level.COLLISION_Y) != 0)
		{
			if (velocity.y < 0)
				isGrounded = true;

			velocity.y = 0;
		}
		position += displacement;
		distanceWalked += MathF.Abs(displacement.x);

		float rotationDst = direction == 1 ? 0 : MathF.PI;
		rotation = MathHelper.Lerp(rotation, rotationDst, 5 * Time.deltaTime);
	}

	void updateActions()
	{
		Climbable hoveredLadder = GameState.instance.level.getClimbable(position + new Vector2(0, 0.1f));
		if (currentLadder == null)
		{
			if (hoveredLadder != null && inputUp)
			{
				currentLadder = hoveredLadder;
				isClimbing = true;
				velocity = Vector2.Zero;
			}
		}
		else
		{
			if (hoveredLadder == null)
			{
				currentLadder = null;
				isClimbing = false;
			}
		}

		if (handItem != null)
		{
			/*
			if (Input.IsKeyPressed(KeyCode.X))
			{
				Input.ConsumeKeyEvent(KeyCode.X);
				handItem.type.use(handItem, this);
			}
			*/
		}

		//actions.update();
	}

	void updateAnimation()
	{
	}

	public override void update()
	{
		if (ai != null)
			ai.update(this);

		updateMovement();
		updateActions();
		updateAnimation();
	}

	public override void render()
	{
		if (sprite != null)
			Renderer.DrawSprite(position.x - 0.5f, position.y, 0, 1, isDucked ? 0.5f : 1, 0, sprite, direction == -1, 0xFFFFFFFF);

		if (handItem != null)
		{
			Renderer.DrawSprite(position.x - 0.25f, position.y + (isDucked ? 0.5f : 1) + 0.5f - 0.25f, 0, 0.5f, 0.5f, null, 0, 0, 0, 0, 0xFF444444);
		}
	}
}
