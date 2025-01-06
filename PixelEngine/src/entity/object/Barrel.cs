using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Barrel : Entity, Hittable
{
	public float health = 1.5f;

	protected Sprite sprite;
	protected FloatRect rect;

	Item[] items;
	public int coins = 0;

	protected Sound[] hitSound;
	protected Sound[] breakSound;


	public Barrel(params Item[] items)
	{
		this.items = items;

		sprite = new Sprite(tileset, 0, 1);
		rect = new FloatRect(-0.5f, 0, 1, 1);

		collider = new FloatRect(-0.4f, 0.0f, 0.8f, 0.75f);
		platformCollider = true;

		hitSound = Item.woodHit;
		breakSound = [Resource.GetSound("sounds/break_wood.ogg")];
	}

	public Barrel()
		: this(null)
	{
	}

	public override void init(Level level)
	{
		//Vector2i tile = (Vector2i)(position + Vector2.Up * 0.5f);
		//level.setTile(tile.x, tile.y, TileType.dummyPlatform);
		level.addCollider(this);
	}

	public override void destroy()
	{
		//Vector2i tile = (Vector2i)(position + Vector2.Up * 0.5f);
		//level.setTile(tile.x, tile.y, null);
		level.removeCollider(this);
	}

	void dropItems()
	{
		for (int i = 0; i < items.Length; i++)
		{
			Vector2 itemVelocity = new Vector2(MathHelper.RandomFloat(-0.2f, 0.2f), 0.5f) * 8;
			Vector2 throwOrigin = position + new Vector2(0, 0.5f);
			ItemEntity obj = new ItemEntity(items[i], null, itemVelocity);
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
		GameState.instance.level.addEntity(ParticleEffects.CreateDestroyWoodEffect(0xFF675051), position);
		Audio.PlayOrganic(breakSound, new Vector3(position, 0));
		remove();
	}

	public bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		health -= damage;
		if (health <= 0)
			breakBarrel();
		else
		{
			GameState.instance.level.addEntity(ParticleEffects.CreateDestroyWoodEffect(0xFF675051), position);
			Audio.PlayOrganic(hitSound, new Vector3(position, 0));
		}
		return true;
	}

	public override void update()
	{
		TileType tile = GameState.instance.level.getTile(position - new Vector2(0, 0.01f));
		if (!(tile != null && (tile.isSolid || tile.isPlatform)))
		{
			velocity.y += -10 * Time.deltaTime;

			float displacement = velocity.y * Time.deltaTime;
			position.y += displacement;
		}
		else
		{
			if (MathF.Abs(velocity.y) > 10)
				breakBarrel();

			velocity.y = 0;
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x + rect.position.x, position.y + rect.position.y, LAYER_BG, rect.size.x, rect.size.y, 0, sprite);
	}
}
