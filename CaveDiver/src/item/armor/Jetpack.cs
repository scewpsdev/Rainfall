using Rainfall;
using Rainfall.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Jetpack : Item
{
	float power = 40;
	float maxVelocity = 10;
	bool active = false;
	bool inputDown = false;
	bool lastInputDown = false;
	float lastVelocity;
	long lastTick;

	ParticleEffect particles;
	float emissionRate;

	Sound sound;
	uint source;
	float soundVolume = 0;


	public unsafe Jetpack()
		: base("jetpack", ItemType.Relic)
	{
		displayName = "Alchemical Thrusters";
		description = "An ingenious alchemical contraption, powered by volatile fluid which ignites with mana, releasing bursts of energy.";

		baseArmor = 1;
		isPassiveItem = true;
		armorSlot = ArmorSlot.Back;
		value = 42;
		rarity = 0.04f;
		manaCost = 1;
		baseWeight = 5;

		sprite = new Sprite(tileset, 6, 9);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/jetpack.png", false), 0, 0, 32, 32);

		particles = new ParticleEffect(null, "effects/jetpack.rfs");
		emissionRate = particles.systems[0].handle->emissionRate;
		particles.systems[0].handle->emissionRate = 0;
		particles.collision = true;
		particles.bounciness = 0.7f;

		sound = Resource.GetSound("sounds/jetpack.ogg");
	}

	public override void onUnequip(Player player)
	{
		player.canWallJump = true;
	}

	unsafe void activate(Player player)
	{
		active = true;
		particles.systems[0].handle->emissionRate = emissionRate;
		source = Audio.Play(sound, new Vector3(player.position, 0), soundVolume, soundVolume);
	}

	unsafe void deactivate()
	{
		active = false;
		particles.systems[0].handle->emissionRate = 0;
		Audio.FadeoutSource(source, 1.5f);
		source = 0;
	}

	public override void update(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;

			particles.update();

			particles.position = player.position + new Vector2(-0.25f * player.direction, 0.25f);

			soundVolume = MathHelper.Lerp(soundVolume, active ? 1 : 0, 5 * Time.deltaTime);
			Audio.SetSourcePosition(source, new Vector3(player.position, 0));
			Audio.SetSourceGain(source, soundVolume);
			Audio.SetSourcePitch(source, soundVolume);

			lastInputDown = inputDown;
			inputDown = InputManager.IsDown("Jump");

			player.canWallJump = !(active || player.velocity.y < 2 || player.mana > 0.5f);

			if (inputDown)
			{
				if (!active)
				{
					if (player.velocity.y < 2 && player.mana > 0.5f)
						activate(player);
				}
				if (active)
				{
					if (!player.isStunned)
					{
						if (player.velocity.y >= 0)
							player.velocity.y += power * Time.deltaTime;
						else
							player.velocity.y += 1.5f * power * Time.deltaTime;
						player.velocity.y = MathF.Min(player.velocity.y, maxVelocity);
					}

					player.consumeMana(manaCost * Time.deltaTime);

					if (player.mana <= 0)
						deactivate();

					float tick = 0.2f;
					if ((Time.currentTime - lastTick) / 1e9f > tick)
					{
						lastTick = Time.currentTime;

						float hitboxDistance = 3.0f;

						HitData[] hits = new HitData[16];
						int numHits = player.level.overlap(player.position - new Vector2(0.5f, hitboxDistance), player.position + new Vector2(0.5f, 0), hits, Entity.FILTER_DEFAULT | Entity.FILTER_MOB | Entity.FILTER_ITEM);
						for (int i = 0; i < numHits; i++)
						{
							if (hits[i].entity != null && hits[i].entity != player)
							{
								float distance = MathF.Abs(hits[i].entity.position.y + hits[i].entity.collider.max.y - player.position.y);
								if (hits[i].entity is Hittable)
								{
									Hittable hittable = hits[i].entity as Hittable;
									float damage = 10 * MathF.Pow(MathHelper.Clamp(MathHelper.Remap(distance, 0, hitboxDistance, 1, 0.1f), 0.1f, 1), 2);
									hittable.hit(damage * tick, player, this, "A stream of hot plasma", true);
								}
								hits[i].entity.velocity += 5 * (hits[i].entity.position + hits[i].entity.collider.center - player.position) * tick;
							}
						}
					}
				}
			}
			else
			{
				if (lastInputDown && player.velocity.y <= 0 && lastVelocity > 0 && active)
					player.velocity.y = lastVelocity;

				deactivate();
			}
			lastVelocity = player.velocity.y;
		}
	}

	public override void render(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;
			particles.render();
			float brightness = 1 - MathF.Exp(-particles.systems[0].numParticles * 0.1f);
			if (brightness > 0)
				Renderer.DrawLight(player.position + Vector2.Down * 2, MathHelper.ARGBToVector(0xFF2273c0).xyz * 20 * brightness, 4);
		}
	}
}
