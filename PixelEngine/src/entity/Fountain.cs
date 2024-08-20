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

	Count
}

public class Fountain : Entity, Interactable
{
	FountainEffect effect = FountainEffect.None;
	bool consumed = false;

	Sprite sprite;
	uint outline = 0;


	public Fountain(FountainEffect effect)
	{
		this.effect = effect;
		sprite = new Sprite(TileType.tileset, 0, 6, 2, 2);
	}

	public Fountain(Random random)
		: this((FountainEffect)(random.Next() % (int)FountainEffect.Count))
	{
	}

	public bool canInteract(Player player)
	{
		return !consumed;
	}

	public float getRange()
	{
		return 1.5f;
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
				player.health += Random.Shared.NextSingle() * 2;
				player.hud.showMessage("You feel refreshed.");
				break;
			case FountainEffect.Regenerate:
				player.addStatusEffect(new HealEffect(2, 5));
				player.hud.showMessage("You feel your strength returning.");
				break;
			case FountainEffect.Damage:
				player.hit(Random.Shared.NextSingle() * 2, this, null);
				player.hud.showMessage("The water is scalding hot.");
				break;
			case FountainEffect.Poison:
				player.addStatusEffect(new PoisonEffect(2, 15));
				player.hud.showMessage("The water burns on your tongue.");
				break;
		}
		consumed = true;
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 1, position.y, 2, 2, sprite);

		if (outline != 0)
			Renderer.DrawOutline(position.x - 1, position.y, 2, 2, sprite, false, outline);
	}
}
