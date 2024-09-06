using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


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

public class Fountain : Entity, Interactable
{
	FountainEffect effect = FountainEffect.None;
	bool consumed = false;

	Sprite sprite;
	uint outline = 0;

	ParticleEffect particles;


	public Fountain(FountainEffect effect)
	{
		this.effect = effect;
		sprite = new Sprite(TileType.tileset, 2, 6);
	}

	public Fountain(Random random)
		: this((FountainEffect)(random.Next() % (int)FountainEffect.Count))
	{
	}

	public override void init()
	{
		GameState.instance.level.addEntity(particles = Effects.CreateFountainEffect(), position);
	}

	public override void destroy()
	{
		particles.remove();
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

	public void interact(Player player)
	{
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
				player.hit(Random.Shared.NextSingle() * 2, this, null);
				player.hud.showMessage("The water is scalding hot.");
				break;
			case FountainEffect.Poison:
				player.addStatusEffect(new PoisonEffect(1, 16));
				player.hud.showMessage("The water burns on your tongue.");
				break;
			case FountainEffect.Mana:
				player.mana = MathF.Min(player.mana + Random.Shared.NextSingle() * 2, player.maxMana);
				player.hud.showMessage("You feel energy flow through you.");
				break;
		}
		consumed = true;

		unsafe
		{
			particles.system.handle->emissionRate = 0;
			particles = null;
		}
	}

	public override void update()
	{
		TileType tile = GameState.instance.level.getTile(position + new Vector2(0.5f, -0.5f));
		if (tile == null)
			remove();
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, 1, 1, sprite);

		if (outline != 0)
			Renderer.DrawOutline(position.x - 0.5f, position.y, 1, 1, sprite, false, outline);
	}
}
