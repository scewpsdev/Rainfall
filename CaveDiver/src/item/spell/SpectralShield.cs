using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class SpectralShieldEntity : Entity, Hittable
{
	SpectralShield spell;
	Player player;
	float distance = 1.0f;
	float rotationSpeed = 2;
	float cooldown = 1;

	float angle = 0;
	Sprite sprite;

	long lastBlock;

	public SpectralShieldEntity(SpectralShield spell, Player player, float angleOffset = 0)
	{
		this.spell = spell;
		this.player = player;

		angle = angleOffset;

		sprite = new Sprite(tileset, 7, 1);

		collider = new FloatRect(-0.5f, -0.5f, 1, 1);
		//filterGroup = FILTER_PROJECTILE;
	}

	public override void onLevelSwitch(Level newLevel)
	{
		GameState.instance.moveEntityToLevel(this, newLevel);
	}

	public override void update()
	{
		angle += rotationSpeed * Time.deltaTime;
		position = player.position + player.collider.center + Vector2.Rotate(Vector2.Right * distance, angle);

		Span<HitData> hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(position + collider.min, position + collider.max, hits, FILTER_MOB);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity != null && hits[i].entity is Mob)
			{
				Mob mob = hits[i].entity as Mob;
				if (hit(mob.damage, mob, mob.handItem))
				{
					if (mob.ai != null)
						mob.ai.onAttacked(this);
				}
			}
		}
	}

	public override void render()
	{
		bool visible = (Time.currentTime - lastBlock) / 1e9f < cooldown ? Time.gameTime * 10 % 1 < 0.5f : true;
		if (visible)
			Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, LAYER_BG, 1, 1, angle, sprite, false, MathF.Cos(angle) < 0, Vector4.One, true);
	}

	public bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		if ((Time.currentTime - lastBlock) / 1e9f > cooldown)
		{
			if (by is Projectile && (by as Projectile).shooter == player)
				return false;

			player.consumeMana(spell.manaCost);
			if (player.mana == 0)
				spell.deactivate(player);
			if (by != null && by is Mob)
			{
				Mob mob = by as Mob;
				mob.stun();
			}
			level.addEntity(new ParryEffect(this), position);
			lastBlock = Time.currentTime;
			return true;
		}
		return false;
	}
}

public class SpectralShield : Spell
{
	SpectralShieldEntity shield1, shield2;


	public SpectralShield()
		: base("spectral_shield")
	{
		displayName = "Spectral Shield";

		value = 27;

		baseAttackRate = 1;
		manaCost = 0.5f;

		spellIcon = new Sprite(tileset, 7, 9);

		useSound = Resource.GetSounds("sounds/cast", 3);
	}

	void activate(Player player)
	{
		player.level.addEntity(shield1 = new SpectralShieldEntity(this, player), player.position);
		player.level.addEntity(shield2 = new SpectralShieldEntity(this, player, MathF.PI), player.position);
	}

	public void deactivate(Player player)
	{
		shield1.remove();
		shield1 = null;
		shield2.remove();
		shield2 = null;
	}

	public override bool cast(Player player, Item staff, float manaCost, float duration)
	{
		if (shield1 == null && shield2 == null)
		{
			activate(player);
			return true;
		}
		else
		{
			deactivate(player);
			return false;
		}
	}
}
