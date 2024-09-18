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

	Count
}

enum FountainState
{
	None = 0,

	Menu,
	ThrowItem,
}

public class Fountain : Entity, Interactable
{
	FountainEffect effect = FountainEffect.None;
	bool consumed = false;

	Sprite sprite;
	uint outline = 0;

	ParticleEffect particles;

	FountainState state = FountainState.None;
	int selectedOption = 0;


	public Fountain(FountainEffect effect)
	{
		displayName = "Fountain";

		this.effect = effect;
		sprite = new Sprite(TileType.tileset, 2, 6);
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

	public void interact(Player player)
	{
		openScreen();
	}

	public float getRange()
	{
		return 1;
	}

	void drink()
	{
		Player player = GameState.instance.player;

		switch (effect)
		{
			case FountainEffect.None:
				player.hud.showMessage("It tastes bland.");
				break;
			case FountainEffect.Heal:
				player.health = MathF.Min(player.health + Random.Shared.NextSingle() * 2, player.maxHealth);
				player.hud.showMessage("You feel refreshed.");
				break;
			case FountainEffect.Regenerate:
				player.addStatusEffect(new HealEffect(1, 5));
				player.hud.showMessage("You feel your strength returning.");
				break;
			case FountainEffect.Damage:
				player.hit(Random.Shared.NextSingle() * 2, this, null, "Boiling Water");
				player.hud.showMessage("The water is scalding hot.");
				break;
			case FountainEffect.Poison:
				player.addStatusEffect(new PoisonEffect(1, 16));
				player.hud.showMessage("The water burns on your tongue.");
				break;
			case FountainEffect.Mana:
				player.addStatusEffect(new ManaRechargeEffect(player.maxMana, 4));
				player.hud.showMessage("You feel energy flow through you.");
				break;
		}
		consumed = true;

		unsafe
		{
			particles.systems[0].handle->emissionRate = 0;
		}
	}

	void fillBottle()
	{
		Player player = GameState.instance.player;
		player.removeItem(player.getItem("glass_bottle"));

		switch (effect)
		{
			case FountainEffect.None:
				GameState.instance.level.addEntity(new ItemEntity(new BottleOfWater()), position + Vector2.Up);
				break;
			case FountainEffect.Heal:
				GameState.instance.level.addEntity(new ItemEntity(new PotionOfHealing() { healAmount = 2 * Random.Shared.NextSingle() }), position + Vector2.Up);
				break;
			case FountainEffect.Regenerate:
				GameState.instance.level.addEntity(new ItemEntity(new PotionOfHealing() { healAmount = 1 }), position + Vector2.Up);
				break;
			case FountainEffect.Damage:
				GameState.instance.level.addEntity(new ItemEntity(new BottleOfWater() { boiling = true, displayName = "Boiling Water", stackable = false }), position + Vector2.Up);
				break;
			case FountainEffect.Poison:
				GameState.instance.level.addEntity(new ItemEntity(new PoisonVial()), position + Vector2.Up);
				break;
			case FountainEffect.Mana:
				GameState.instance.level.addEntity(new ItemEntity(new PotionOfEnergy() { amount = player.maxMana }), position + Vector2.Up);
				break;
		}
		consumed = true;

		unsafe
		{
			particles.systems[0].handle->emissionRate = 0;
		}
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

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, 1, 1, sprite);

		if (outline != 0)
			Renderer.DrawOutline(position.x - 0.5f, position.y, 1, 1, sprite, false, outline);

		if (state == FountainState.Menu)
		{
			Player player = GameState.instance.player;

			List<string> options = new List<string>();
			options.Add("Drink");
			if (player.getItem("glass_bottle") != null)
				options.Add("Fill Bottle");
			if (player.items.Count > 0)
				options.Add("Throw Item");
			options.Add("Walk Away");

			Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));

			int option = InteractableMenu.Render(pos, displayName, options, out bool closed, ref selectedOption);

			if (option != -1)
			{
				if (options[option] == "Drink")
				{
					drink();
					closeScreen();
				}
				else if (options[option] == "Fill Bottle")
				{
					fillBottle();
					closeScreen();
				}
				else if (options[option] == "Throw Item")
				{
					state = FountainState.ThrowItem;
				}
				else if (options[option] == "Walk Away")
					closeScreen();
			}

			if (closed)
				closeScreen();
		}
		else if (state == FountainState.ThrowItem)
		{
			//
		}
	}
}
