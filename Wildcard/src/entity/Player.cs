using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class Player : Entity
{
	const float JUMP_BUFFER = 0.2f;
	const float COYOTE_TIME = 0.15f;
#if DEBUG
	const float SPRINT_MULTIPLIER = 1.5f;
#else
	const float SPRINT_MULTIPLIER = 1.5f;
#endif
	const float DUCKED_MULTIPLIER = 0.6f;
	const float MAX_FALL_SPEED = -18;
	const float HIT_COOLDOWN = 1.0f;
	const float STUN_DURATION = 1.0f;
	const float FALL_STUN_DISTANCE = 8;
	const float FALL_DAMAGE_DISTANCE = 10;
	const float MANA_KILL_REWARD = 0.2f; //0.5f;
#if DEBUG
	const float SPRINT_MANA_COST = 0.5f;
#else
	const float SPRINT_MANA_COST = 0.5f;
#endif


	public const float defaultSpeed = 6;
	public float speed = defaultSpeed;
	public float acceleration = 16;
	public float climbingSpeed = 5;
	public float jumpPower = 11; //12; //10.5f;
	public float gravity = -22;
	public bool canWallJump = true;
	public float wallJumpPower = /*6; //*/10;
	public float wallControl = 2;
	public int airJumps = 0;
	public int airJumpsLeft = 0;
	public const float defaultManaRecovery = 0.0f; //0.01f;
	public float manaHitReward = 0.2f;
	public float coinCollectDistance = 1.0f;
	public float aimDistance = 1.0f;
	public float criticalChance = 0.05f;

	//public float maxHealth = 3;
	public float health = 3;
	public float maxHealth => hp * 0.5f;

	//public float maxMana = 2;
	public float mana = 2;
	public float maxMana => magic * 0.5f;

	public int hp = 8;
	public int magic = 4;
	public int strength = 1;
	public int dexterity = 1;
	public int intelligence = 1;
	public int swiftness = 1;

	public int money = 0;
	public int playerLevel = 1;
	public int xp = 0;

	public int nextLevelXP => (int)MathF.Round(30 * (MathF.Exp((playerLevel - 1) * 0.1f) + (playerLevel - 1) * 0.6f));
	public int availableStatUpgrades = 0;

	public int direction = 1;
	public Vector2 lookDirection = Vector2.Right;
	public float impulseVelocity;
	float wallJumpVelocity;
	float wallJumpFactor;
	public bool isGrounded = false;
	bool isMoving = false;
	bool isSprinting = false;
	public bool isDucked = false;
	public bool isClimbing = false;
	public bool isLookingUp = false;
	float fallDistance = 0;
	public Vector2 lastStableGround = Vector2.Zero;

	// Status effects
	public bool isStunned = false;
	public bool isVisible = true;

	public float visibility { get => (isVisible ? 1 : 0.25f) * Mathf.Lerp(0.5f, 1.0f, level.lightLevel) * (isDucked ? 0.5f : 1.0f); }

	public InputBinding currentAttackInput = null;

	long lastJumpInput = -10000000000;
	long lastDodgeInput = -10000000000;
	long lastGrounded = -10000000000;
	long lastWallTouchRight = -10000000000;
	long lastWallTouchLeft = -10000000000;

	long lastHit = -1;
	long stunTime = -1;

	public ActionQueue actions;

	//public Interactable interactableInFocus = null;
	//public Climbable currentLadder = null;
	//Climbable lastLadderJumpedFrom = null;

	//public HUD hud;
	//InventoryUI inventoryUI;
	//MapUI mapUI;
	public int numOverlaysOpen = 0;
	public bool inventoryOpen = false;
	public bool mapOpen = false;

	Sound[] stepSound;
	public Sound jumpSound;
	public Sound landSound;
	Sound[] ladderSound;
	Sound[] hitSound;
	Sound wallTouchSound;


	public Player()
	{
		actions = new ActionQueue(this);

		collider = new FloatRect(-0.15f, 0, 0.3f, 0.9f);
		filterGroup = FILTER_PLAYER;

		stepSound = Resource.GetSounds("sounds/step", 6);
		jumpSound = Resource.GetSound("sounds/jump_bare.ogg");
		landSound = Resource.GetSound("sounds/land.ogg");
		ladderSound = Resource.GetSounds("sounds/step_wood", 3);
		hitSound = Resource.GetSounds("sounds/flesh", 2);
		wallTouchSound = Resource.GetSound("sounds/wall_touch.ogg");

		health = maxHealth;
		mana = maxMana;
	}

	public override void init(Level level)
	{
	}

	public override void onLevelSwitch(Level newLevel)
	{
	}

	void updateMovement()
	{
		Vector2 delta = Input.GamepadAxis;
		if (delta.lengthSquared < 0.25f)
			delta = Vector2.Zero;

		if (isStunned)
		{
			if ((Time.currentTime - stunTime) / 1e9f > STUN_DURATION)
			{
				isStunned = false;
				stunTime = -1;
			}
		}

		if (isAlive && !isStunned && (actions.currentAction == null || actions.currentAction.canMove))
		{
			if (InputManager.IsDown("Left"))
				delta.x--;
			if (InputManager.IsDown("Right"))
				delta.x++;
			if (isClimbing)
			{
				if (InputManager.IsDown("Up"))
				{
					//if (GameState.instance.level.getClimbable(position + new Vector2(0, 0.2f)) != null)
					//	delta.y++;
					//else
					{
						TileType tile = GameState.instance.level.getTile(position);
						TileType up = GameState.instance.level.getTile(position + new Vector2(0, 0.2f));
						if (tile != null && tile.isPlatform && up == null)
						{
							//lastLadderJumpedFrom = currentLadder;
							//currentLadder = null;
							isClimbing = false;
							position.y = MathF.Floor(position.y + 0.2f);
						}
					}
				}
				if (InputManager.IsDown("Down"))
				{
					delta.y--;
				}
			}
			else
			{
				if (isDucked)
				{
					TileType tile = GameState.instance.level.getTile(position);
					if (position.x - MathF.Floor(position.x) > -collider.position.x && MathF.Ceiling(position.x) - position.x > collider.position.x + collider.size.x &&
						tile != null && tile.isPlatform && Mathf.Fract(position.y) > tile.platformHeight - 0.25f)
						position.y = MathF.Floor(position.y) + tile.platformHeight - 0.25f;
				}
			}

			if (canWallJump)
			{
				if (/*InputManager.IsDown("Right") &&*/ GameState.instance.level.overlapTiles(position + new Vector2(0, 0.1f), position + new Vector2(collider.max.x + 0.2f, collider.max.y - 0.05f)))
				{
					//if ((Time.currentTime - lastWallTouchRight) / 1e9f > COYOTE_TIME && velocity.y < -0.5f)
					//	Audio.PlayOrganic(wallTouchSound, new Vector3(position, 0), 1.0f);
					lastWallTouchRight = Time.currentTime;
				}
				if (/*InputManager.IsDown("Left") &&*/ GameState.instance.level.overlapTiles(position + new Vector2(collider.min.x - 0.2f, 0.1f), position + new Vector2(0.0f, collider.max.y - 0.05f)))
				{
					//if ((Time.currentTime - lastWallTouchLeft) / 1e9f > COYOTE_TIME && velocity.y < -0.5f)
					//	Audio.PlayOrganic(wallTouchSound, new Vector3(position, 0), 1.0f);
					lastWallTouchLeft = Time.currentTime;
				}
			}

			//isSprinting = InputManager.IsDown("Sprint") && (isSprinting ? mana > 0 : mana > 0.2f) && delta.lengthSquared > 0;
			if (InputManager.IsPressed("Sprint"))
				lastDodgeInput = Time.currentTime;
			if (InputManager.IsPressed("Sprint") || (Time.currentTime - lastDodgeInput) / 1e9f < JUMP_BUFFER)
			{
				//if (isGrounded && numOverlaysOpen == 0 && mana >= DodgeAction.manaCost)
				{
					//if (InputManager.IsDown("Left") || InputManager.IsDown("Right"))
					//	actions.queueAction(new DodgeAction());
					//else
					//	actions.queueAction(new BackhopAction());
					lastDodgeInput = -1;
				}
			}

			isDucked = InputManager.IsDown("Down") && numOverlaysOpen == 0;
			collider.size.y = isDucked ? 0.4f : 0.8f;
			if (!isDucked)
			{
				if (Mathf.Fract(position.y) > 1 - collider.max.y)
				{
					TileType topTile = level.getTile(position + new Vector2(0, collider.max.y));
					if (topTile != null && topTile.isSolid)
						position.y = MathF.Min(position.y, MathF.Floor(position.y + collider.max.y) - collider.max.y);
				}
			}

			isLookingUp = isGrounded && InputManager.IsDown("Up");

			if (isGrounded)
			{
				lastGrounded = Time.currentTime;
				airJumpsLeft = airJumps;
				lastStableGround = position;
			}

			if (InputManager.IsPressed("Jump") && (actions.currentAction == null || actions.currentAction.canJump))
			{
				if (isClimbing)
				{
					velocity.y = InputManager.IsDown("Down") ? -0.5f * jumpPower : jumpPower;
					lastJumpInput = 0;
					lastGrounded = 0;
					//lastLadderJumpedFrom = currentLadder;
					//currentLadder = null;
					isClimbing = false;
				}
				else
				{
					lastJumpInput = Time.currentTime;
					if (isGrounded || (Time.currentTime - lastGrounded) / 1e9f < COYOTE_TIME)
					{
						velocity.y = jumpPower;
						Audio.Play(jumpSound, new Vector3(position, 0));
						lastJumpInput = 0;
						lastGrounded = 0;
					}
					else if (airJumpsLeft > 0)
					{
						velocity.y = jumpPower;
						lastJumpInput = 0;
						airJumpsLeft--;
						//level.addEntity(ParticleEffects.CreateAirJumpEffect(this), position);
					}
					else if (!isGrounded)
					{
						if ((Time.currentTime - lastWallTouchRight) / 1e9f < COYOTE_TIME)
						{
							velocity.y = jumpPower * 0.75f;
							wallJumpVelocity = -wallJumpPower;
							wallJumpFactor = 1.0f;
							lastWallTouchRight = 0;
						}

						if ((Time.currentTime - lastWallTouchLeft) / 1e9f < COYOTE_TIME)
						{
							velocity.y = jumpPower * 0.75f;
							wallJumpVelocity = wallJumpPower;
							wallJumpFactor = 1.0f;
							lastWallTouchLeft = 0;
						}
					}
				}
			}
			else if ((Time.currentTime - lastJumpInput) / 1e9f < JUMP_BUFFER)
			{
				if (isGrounded || (Time.currentTime - lastGrounded) / 1e9f < COYOTE_TIME)
				{
					velocity.y = jumpPower;
					Audio.Play(jumpSound, new Vector3(position, 0));
					lastJumpInput = 0;
					lastGrounded = 0;
				}
			}
		}
		else
		{
			isSprinting = false;
			isDucked = false;
			//currentLadder = null;
			isClimbing = false;
		}

		isMoving = delta.x != 0 || actions.currentAction != null && actions.currentAction.actionMovement != 0;

		velocity.x = isMoving ? delta.x * speed /** currentSpeedModifier*/ : 0;

		float maxCursorDistance = 5; // (handItem != null ? MathF.Min(handItem.attackRange * 2, 5) : 1.8f) * 0.2f;
		if (isAlive)
		{
			Vector2 controllerAim = Input.GamepadAxisRight;
			if (controllerAim.lengthSquared > 0.25f)
				lookDirection = controllerAim * maxCursorDistance;

			if (delta.x != 0)
				direction = MathF.Sign(delta.x);
		}
		if (isAlive && numOverlaysOpen == 0)
		{
			if (Settings.game.aimMode == AimMode.Simple)
			{
				if (InputManager.IsDown("Up"))
					lookDirection = Vector2.Up;
				else if (/*!isGrounded &&*/ InputManager.IsDown("Down"))
					lookDirection = Vector2.Down;
				else
					lookDirection = new Vector2(direction, 0);
			}
			else if (Settings.game.aimMode == AimMode.Directional)
			{
				if (Input.cursorHasMoved)
				{
					/*
					if ((Renderer.cursorPosition - playerScreenPos).length > maxCursorDistance * 16)
					{
						Vector2i newCursorPos = playerScreenPos + (Vector2i)Vector2.Round((Renderer.cursorPosition - playerScreenPos).normalized * maxCursorDistance * 16);
						Input.cursorPosition = newCursorPos * Display.viewportSize / new Vector2i(Renderer.UIWidth, Renderer.UIHeight);
					}
					*/
					lookDirection = GameState.instance.camera.screenToWorld(Renderer.cursorPosition) - GameState.instance.camera.screenToWorld(Renderer.size / 2); // (position + collider.center);
					if (lookDirection.length > maxCursorDistance)
						lookDirection = lookDirection.normalized * maxCursorDistance;

					/*
					if (MathF.Abs(lookDirection.x) > maxCursorDistance)
						lookDirection.x = MathF.Sign(lookDirection.x) * maxCursorDistance;
					if (MathF.Abs(lookDirection.y) > maxCursorDistance)
						lookDirection.y = MathF.Sign(lookDirection.y) * maxCursorDistance;
					*/

					Vector2i playerScreenPos = Display.viewportSize / 2; // new Vector2i(Renderer.UIWidth, Renderer.UIHeight) / 2; // GameState.instance.camera.worldToScreen(position + collider.center);
					if ((Input.cursorPosition - playerScreenPos).length > maxCursorDistance * 16 * Wildcard.instance.scale)
					//if (MathF.Abs(Input.cursorPosition.x - playerScreenPos.x) > maxCursorDistance * 16 * IvoryKeep.instance.scale ||
					//	MathF.Abs(Input.cursorPosition.y - playerScreenPos.y) > maxCursorDistance * 16 * IvoryKeep.instance.scale)
					{
						Vector2 dir = (Vector2)(Input.cursorPosition - playerScreenPos);
						dir = dir.normalized * maxCursorDistance * 16 * Wildcard.instance.scale;
						dir += playerScreenPos;

						int x = (int)dir.x; //Math.Clamp(Input.cursorPosition.x, (int)MathF.Round(playerScreenPos.x - maxCursorDistance * 16 * IvoryKeep.instance.scale), (int)MathF.Round(playerScreenPos.x + maxCursorDistance * 16 * IvoryKeep.instance.scale));
						int y = (int)dir.y; //Math.Clamp(Input.cursorPosition.y, (int)MathF.Round(playerScreenPos.y - maxCursorDistance * 16 * IvoryKeep.instance.scale), (int)MathF.Round(playerScreenPos.y + maxCursorDistance * 16 * IvoryKeep.instance.scale));
						Vector2i newCursorPos = new Vector2i(x, y);
						Input.cursorPosition = newCursorPos; // * Display.viewportSize / new Vector2i(Renderer.UIWidth, Renderer.UIHeight);
					}
				}

				if (actions.currentAction != null && actions.currentAction.turnToCrosshair)
					direction = Math.Sign(lookDirection.x);
				else if (delta.x != 0)
					direction = MathF.Sign(delta.x);
			}
			else if (Settings.game.aimMode == AimMode.Crosshair)
			{
				if (Input.cursorHasMoved)
				{
					lookDirection = GameState.instance.camera.screenToWorld(Renderer.cursorPosition) - (position + collider.center);
				}

				if (actions.currentAction != null && actions.currentAction.turnToCrosshair)
					direction = Math.Sign(lookDirection.x);
				else if (delta.x != 0)
					direction = MathF.Sign(delta.x);
			}

			//lookDirection = Vector2.Rotate(Vector2.Right, MathF.Floor((lookDirection.angle + MathF.PI * 0.125f) / (MathF.PI * 0.25f)) * MathF.PI * 0.25f);
			if (MathF.Abs(lookDirection.x) < 0.001f)
				lookDirection.x = 0;
			if (MathF.Abs(lookDirection.y) < 0.001f)
				lookDirection.y = 0;
		}

		if (!isClimbing)
		{
			float gravityMultiplier = 1;
			if (!isAlive || !InputManager.IsDown("Jump"))
			{
				gravityMultiplier = 2;
				if (InputManager.IsReleased("Jump"))
					velocity.y = MathF.Min(velocity.y, 0);
			}
			//if (InputManager.IsDown("Down") && actions.currentAction == null)
			//	gravityMultiplier *= 1.5f;
			velocity.y += gravityMultiplier * gravity * Time.deltaTime;
			velocity.y = MathF.Max(velocity.y, MAX_FALL_SPEED);

			if (lastWallTouchLeft == Time.currentTime && InputManager.IsDown("Left") || lastWallTouchRight == Time.currentTime && InputManager.IsDown("Right"))
				velocity.y = MathF.Max(velocity.y, -16 / wallControl);

			wallJumpFactor = Mathf.MoveTowards(wallJumpFactor, 0, wallControl /* * getWallControlModifier()*/ * Time.deltaTime);
			velocity.x = Mathf.Lerp(velocity.x, wallJumpVelocity, wallJumpFactor);

			if (!isStunned || isGrounded)
				impulseVelocity = Mathf.Lerp(impulseVelocity, 0, 8 * Time.deltaTime);
			if (MathF.Sign(impulseVelocity) == MathF.Sign(velocity.x))
				impulseVelocity = 0;
			else if (velocity.x == 0)
				impulseVelocity = MathF.Sign(impulseVelocity) * MathF.Min(MathF.Abs(impulseVelocity), speed);
			//impulseVelocity.x = impulseVelocity.x - velocity.x;
			velocity.x += impulseVelocity;

			//if (isGrounded && lastLadderJumpedFrom != null)
			//	lastLadderJumpedFrom = null;
		}
		else
		{
			velocity.y = delta.y * climbingSpeed /* * equipLoadModifier*/;
		}

		Vector2 displacement = velocity * Time.deltaTime;
		if (actions.currentAction != null && actions.currentAction.actionMovement != 0)
			displacement.x += actions.currentAction.actionMovement * Time.deltaTime;

		if (!isGrounded && !isClimbing && displacement.y < 0)
			fallDistance += -displacement.y;
		else
			fallDistance = 0;

		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, isDucked, true);

		isGrounded = GameState.instance.level.overlapTiles(position + collider.min + new Vector2(0, -0.1f), position + new Vector2(collider.max.x, collider.min.y + 0.1f));

		if ((collisionFlags & Level.COLLISION_Y) != 0)
		{
			if (fallDistance >= FALL_STUN_DISTANCE && velocity.y <= MAX_FALL_SPEED)
			{
				//stun(null);
			}
			if (fallDistance >= FALL_DAMAGE_DISTANCE && velocity.y <= MAX_FALL_SPEED)
			{
				float fallDmg = (fallDistance - FALL_DAMAGE_DISTANCE) * 0.5f /*/ equipLoadModifier*/;
				//hit(fallDmg, null, null, "A high fall", false);
			}
			//if (velocity.y < -2)
			//	onLand();

			if (velocity.y < 0)
				isGrounded = true;

			velocity.y = 0;
			//impulseVelocity.x *= 0.5f;
		}
		if ((collisionFlags & Level.COLLISION_X) != 0)
		{
			impulseVelocity = 0;
			wallJumpFactor = 0;
		}

		position += displacement;
	}

	void updateAnimation()
	{
	}

	public override void update()
	{
		updateMovement();
		updateAnimation();

		//Audio.UpdateListener(new Vector3(position, 5), Quaternion.Identity);
		Audio.UpdateListener(new Vector3(GameState.instance.camera.position, 20), Quaternion.Identity);
		Audio.Set3DVolume(20.0f);

		if (numOverlaysOpen == 0)
			Input.cursorMode = CursorMode.Hidden;
	}

	public override void render()
	{
		if (!isAlive)
			return;

		bool hitCooldown = isAlive && lastHit != -1 && (Time.currentTime - lastHit) / 1e9f < HIT_COOLDOWN;
		bool show = !hitCooldown || (lastHit != -1 && (int)((Time.currentTime - lastHit) / 1e9f * 20) % 2 == 0);

		if (isVisible)
		{
			Vector2 snappedPosition = position;
			snappedPosition.x = MathF.Round(snappedPosition.x * 16) / 16;
			snappedPosition.y = MathF.Round(snappedPosition.y * 16) / 16;

			if (show)
			{
				Renderer.DrawSprite(snappedPosition.x - 0.25f, snappedPosition.y, 0.5f, 1, null, false, 0xFF7F7F7F);
			}
		}

		if (actions.currentAction != null)
			actions.currentAction.render(this);

		Renderer.DrawLight(position + new Vector2(0, 0.5f), new Vector3(1.0f) * 1.5f, 7);

		{
			//hud.render();
			//inventoryUI.render();
			//mapUI.render();
		}
	}

	public bool isAlive
	{
		get => health > 0;
	}

	public Vector2 center => position + collider.center;
}
