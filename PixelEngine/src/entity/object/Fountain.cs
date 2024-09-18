using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum FountainEffect
{
	None,
	Heal,
	Regenerate,
	Damage,
	Poison,
	Mana,
	Teleport,

	Count
}

enum FountainState
{
	None = 0,

	Menu,
	UseItem,
}

public class Fountain : Entity, Interactable
{
	FountainEffect effect = FountainEffect.None;
	List<PotionEffect> potionEffects = new List<PotionEffect>();
	bool consumed = false;

	Sprite sprite;
	uint outline = 0;

	ParticleEffect particles;

	FountainState state = FountainState.None;
	int selectedOption = 0;
	int selectedItem = 0;


	public Fountain(FountainEffect effect)
	{
		this.effect = effect;
		displayName = "Fountain";

		sprite = new Sprite(TileType.tileset, 2, 6);

		switch (effect)
		{
			case FountainEffect.None:
				potionEffects.Add(new WaterEffect());
				break;
			case FountainEffect.Heal:
				potionEffects.Add(new HealEffect(Random.Shared.NextSingle() * 2, 2));
				break;
			case FountainEffect.Regenerate:
				potionEffects.Add(new HealEffect(1, 5));
				break;
			case FountainEffect.Damage:
				potionEffects.Add(new WaterEffect(true));
				break;
			case FountainEffect.Poison:
				potionEffects.Add(new PoisonEffect(1, 16));
				break;
			case FountainEffect.Mana:
				potionEffects.Add(new ManaEffect(2, 4));
				break;
			case FountainEffect.Teleport:
				potionEffects.Add(new TeleportEffect());
				break;
			default:
				Debug.Assert(false);
				break;
		}
	}

	public Fountain(Random random)
		: this((FountainEffect)(random.Next() % (int)FountainEffect.Count))
	{
	}

	public Fountain()
		: this(Random.Shared)
	{
	}

	public override unsafe void init(Level level)
	{
		level.addEntity(particles = Effects.CreateFountainEffect(), position + new Vector2(0, 1.0f - 2.0f / 16));
	}

	public override void destroy()
	{
		particles.remove();

		if (state != FountainState.None)
			closeScreen();
	}

	public bool canInteract(Player player)
	{
		return !consumed;
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
		state = FountainState.Menu;
		GameState.instance.player.numOverlaysOpen++;
		selectedOption = 0;
	}

	void closeScreen()
	{
		state = FountainState.None;
		GameState.instance.player.numOverlaysOpen--;
	}

	void initMenu()
	{
		state = FountainState.Menu;
	}

	void initUseItem()
	{
		state = FountainState.UseItem;
		selectedItem = 0;
	}

	public void interact(Player player)
	{
		openScreen();
	}

	public float getRange()
	{
		return 1;
	}

	public override void update()
	{
		Player player = GameState.instance.player;

		float maxDistance = getRange();
		if (state != FountainState.None && (InputManager.IsPressed("UIQuit") || (player.position - position).lengthSquared > maxDistance * maxDistance))
		{
			closeScreen();
		}

		TileType tile = GameState.instance.level.getTile(position + new Vector2(0.5f, -0.5f));
		if (tile == null)
			remove();
	}

	unsafe void drink()
	{
		Player player = GameState.instance.player;

		for (int i = 0; i < potionEffects.Count; i++)
		{
			potionEffects[i].apply(player, null);
		}

		consumed = true;
		particles.systems[0].handle->emissionRate = 0;
	}

	unsafe bool fillBottle()
	{
		Player player = GameState.instance.player;

		Potion potion = new Potion();
		foreach (PotionEffect effect in potionEffects)
			potion.addEffect(effect);
		GameState.instance.level.addEntity(new ItemEntity(potion), position + Vector2.Up);

		consumed = true;
		particles.systems[0].handle->emissionRate = 0;
		return true;
	}

	unsafe bool dipScroll()
	{
		Player player = GameState.instance.player;

		switch (effect)
		{
			case FountainEffect.Mana:
				GameState.instance.level.addEntity(new ItemEntity(new ScrollOfWeaponEnchantment()), position + Vector2.Up);
				break;
			case FountainEffect.Teleport:
				GameState.instance.level.addEntity(new ItemEntity(new ScrollOfTeleportation()), position + Vector2.Up);
				break;
			default:
				return false;
		}

		consumed = true;
		particles.systems[0].handle->emissionRate = 0;

		return true;
	}

	void pourPotion(Item potion)
	{
		Potion p = potion as Potion;
		potionEffects.AddRange(p.effects);

		// alchemy?
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, 1, 1, sprite);

		if (outline != 0)
			Renderer.DrawOutline(position.x - 0.5f, position.y, 1, 1, sprite, false, outline);

		Player player = GameState.instance.player;

		if (state == FountainState.Menu)
		{
			List<string> options = new List<string>();
			options.Add("Drink");
			if (player.items.Count > 0)
				options.Add("Use item");
			options.Add("Walk away");

			Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			int option = InteractableMenu.Render(pos, displayName, options, out bool closed, ref selectedOption);

			if (option != -1)
			{
				if (options[option] == "Drink")
				{
					drink();
					closeScreen();
				}
				else if (options[option] == "Use item")
				{
					initUseItem();
				}
				else if (options[option] == "Walk away")
				{
					closeScreen();
				}
			}

			if (closed)
				closeScreen();
		}
		else if (state == FountainState.UseItem)
		{
			Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			List<Item> items = new List<Item>(player.items.Count);
			for (int i = 0; i < player.items.Count; i++)
				items.Add(player.items[i].Item2);

			int choice = ItemSelector.Render(pos, "Use item", items, null, -1, player, out bool closed, ref selectedItem);
			if (choice != -1)
			{
				Item item = items[choice];

				if (item.name == "glass_bottle")
				{
					if (fillBottle())
					{
						player.removeItemSingle(item);
						player.hud.showMessage("You fill up the glass bottle.");
					}
				}
				else if (item.name == "blank_paper")
				{
					if (dipScroll())
						player.hud.showMessage("The paper fully soaks up the liquid.");
					else
						player.hud.showMessage("The paper turns to mush.");
					player.removeItemSingle(item);
				}
				else if (item.type == ItemType.Potion)
				{
					pourPotion(item);
					player.removeItemSingle(item);
					player.hud.showMessage("You pour the potion into the fountain.");
				}
				else
				{
					player.hud.showMessage("Nothing happens.");
					/*
					item = player.removeItemSingle(item);
					sunkenItems.Add(item);
					player.hud.showMessage("You threw " + item.displayName + " into the fountain.");
					*/
				}

				if (consumed)
					closeScreen();
			}

			if (closed)
				initMenu();
		}
	}
}
