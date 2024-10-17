using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Barrel : Entity, Hittable
{
	Sprite sprite;

	Item[] items;
	public int coins = 0;

	Sound breakSound;


	public Barrel(params Item[] items)
	{
		this.items = items;

		sprite = new Sprite(TileType.tileset, 0, 1);

		collider = new FloatRect(-0.4f, 0.0f, 0.8f, 0.75f);

		breakSound = Resource.GetSound("res/sounds/break_wood.ogg");
	}

	public Barrel()
		: this(null)
	{
	}

	void dropItems()
	{
		for (int i = 0; i < items.Length; i++)
		{
			Vector2 itemVelocity = new Vector2(MathHelper.RandomFloat(-0.2f, 0.2f), MathHelper.RandomFloat(-0.2f, 0.2f)) * 8;
			Vector2 throwOrigin = position + new Vector2(0, 0.5f);
			ItemEntity obj = new ItemEntity(items[i], null, itemVelocity) { zVelocity = 0.5f };
			GameState.instance.level.addEntity(obj, throwOrigin);
		}
		items = null;

		for (int i = 0; i < coins; i++)
		{
			Coin coin = new Coin();
			Vector2 spawnPosition = position + new Vector2(0, 0.5f) + Vector2.Rotate(Vector2.UnitX, i / (float)coins * 2 * MathF.PI) * 0.2f;
			coin.velocity = (spawnPosition - position - new Vector2(0, 0.5f)).normalized * 4;
			GameState.instance.level.addEntity(coin, spawnPosition);
		}
	}

	void breakBarrel()
	{
		if (items != null)
			dropItems();
		GameState.instance.level.addEntity(Effects.CreateDestroyWoodEffect(0xFF675051), position);
		Audio.PlayOrganic(breakSound, new Vector3(position, 0));
		remove();
	}

	public bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		breakBarrel();
		return true;
	}

	public override void render()
	{
		Renderer.DrawVerticalSprite(position.x - 0.5f, position.y, 1, 1, sprite);
	}
}
