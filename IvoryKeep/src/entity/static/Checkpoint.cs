using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


enum CheckpointScreen
{
	None = 0,

	Menu,
	UseItem,
}

public class Checkpoint : Entity, Interactable
{
	bool activated = false;

	Sprite sprite;
	uint outline = 0;

	ParticleEffect particles;

	Sound idleSound;
	uint source;

	CheckpointScreen state = CheckpointScreen.None;
	int selectedOption = 0;
	int selectedItem = 0;


	public Checkpoint()
	{
		displayName = "Fountain";

		sprite = new Sprite(tileset, 2, 6);

		collider = new FloatRect(-0.4f, 0.0f, 0.8f, 14 / 16.0f);
		platformCollider = true;

		idleSound = Resource.GetSound("sounds/fountain.ogg");
	}

	public override unsafe void init(Level level)
	{
		level.addEntityCollider(this);
	}

	public override void destroy()
	{
		level.removeEntityCollider(this);

		if (particles != null)
		{
			particles.remove();
			particles = null;
		}
		if (source != 0)
		{
			Audio.FadeoutSource(source, 1);
			source = 0;
		}

		if (state != CheckpointScreen.None)
			closeScreen();
	}

	public bool canInteract(Player player)
	{
		//return !consumed;
		return true;
	}

	public void onFocusEnter(Player player)
	{
		outline = OUTLINE_COLOR;
	}

	public void onFocusLeft(Player player)
	{
		outline = 0;
	}

	void openScreen()
	{
		state = CheckpointScreen.Menu;
		GameState.instance.player.numOverlaysOpen++;
		selectedOption = 0;
	}

	void closeScreen()
	{
		state = CheckpointScreen.None;
		GameState.instance.player.numOverlaysOpen--;
	}

	void initMenu()
	{
		state = CheckpointScreen.Menu;
	}

	void initUseItem()
	{
		state = CheckpointScreen.UseItem;
		selectedItem = 0;
	}

	public void interact(Player player)
	{
		if (!activated)
		{
			activated = true;

			level.addEntity(particles = ParticleEffects.CreateFountainEffect(), position + new Vector2(0, 1.0f - 2.0f / 16));

			source = Audio.Play(idleSound, new Vector3(position, 0));
			Audio.SetPaused(source, true);
			Audio.SetSourceLooping(source, true);

			GameState.instance.currentCheckpointLevel = level.name;
			GameState.instance.currentCheckpoint = player.lastStableGround != Vector2.Zero ? player.lastStableGround : player.position;
		}
		else
		{
			openScreen();
		}
	}

	public float getRange()
	{
		return 2;
	}

	public override void onLevelSwitch(Level newLevel)
	{
		Audio.SetPaused(source, newLevel != level);
	}

	public override void update()
	{
		Player player = GameState.instance.player;

		float maxDistance = getRange();
		if (state != CheckpointScreen.None && (InputManager.IsPressed("UIQuit") || (player.position - position).lengthSquared > maxDistance * maxDistance))
		{
			closeScreen();
		}

		TileType tile = GameState.instance.level.getTile(position + new Vector2(0.5f, -0.5f));
		if (tile == null)
			remove();
	}

	bool rest(Player player)
	{
		player.addStatusEffect(new HealStatusEffect(player.maxHealth - player.health, 3));

		Item glassBottle = player.getItem("glass_bottle");
		if (glassBottle != null)
		{
			player.removeItem(glassBottle);
			player.giveItem(new PotionOfHealing(1));
		}

		return true;
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, 1, 1, sprite);

		if (outline != 0)
			Renderer.DrawOutline(position.x - 0.5f, position.y, 1, 1, sprite, false, outline);

		Player player = GameState.instance.player;

		if (state == CheckpointScreen.Menu)
		{
			List<string> options = new List<string>();
			options.Add("Rest");
			options.Add("Walk away");

			Vector2 pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			int option = InteractableMenu.Render(pos, displayName, options, out bool closed, ref selectedOption);

			if (option != -1)
			{
				if (options[option] == "Rest")
				{
					rest(player);
					closeScreen();
				}
				else if (options[option] == "Walk away")
				{
					closeScreen();
				}
			}

			if (closed)
				closeScreen();
		}
		else if (state == CheckpointScreen.UseItem)
		{
			Vector2 pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			List<Item> items = new List<Item>(player.items.Count);
			for (int i = 0; i < player.items.Count; i++)
				items.Add(player.items[i]);

			int choice = ItemSelector.Render(pos, "Use item", items, null, -1, player, true, null, false, out bool secondary, out bool closed, ref selectedItem);
			if (choice != -1)
			{
				Item item = items[choice];
				//useItem(item, player);
				closeScreen();
			}

			if (closed)
				initMenu();
		}
	}
}
