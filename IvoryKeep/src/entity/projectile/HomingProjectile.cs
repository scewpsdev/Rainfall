using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HomingProjectile : Projectile
{
	Player player;

	ParticleEffect trailEffect;


	public HomingProjectile(Vector2 direction, Vector2 startVelocity, Vector2 offset, Player player, Item spell, Item staff)
		: base(direction * 1, startVelocity, offset, player, spell, spell.baseDamage * staff.getAttackDamage(player) * player.getMagicDamageModifier())
	{
		this.player = player;

		//maxSpeed = 40;
		//acceleration = 50;
		maxRicochets = 0;
		maxRange = spell.attackRange;

		sprite = new Sprite(Item.tileset, 14, 12);
		//spriteColor = new Vector4(1.5f);
		additive = true;

		trailColor = 0xFF99eeee;
	}

	public override unsafe void init(Level level)
	{
		base.init(level);

		level.addEntity(trailEffect = ParticleEffects.CreateMagicTrailEffect(this), position);
		trailEffect.systems[0].handle->emissionRate *= 0.1f;
	}

	public override unsafe void destroy()
	{
		base.destroy();

		trailEffect.systems[0].handle->emissionRate = 0;
	}

	public override void onHit(Vector2 normal)
	{
		GameState.instance.level.addEntity(ParticleEffects.CreateImpactEffect(normal, velocity.length, Mathf.ARGBToVector(0xFF99eeee).xyz), position - velocity * Time.deltaTime);
	}

	public override void update()
	{
		float t = (Time.gameTime - shootTime) * 4;
		float f = 0.25f * MathF.Exp(-t * 3) + 1.0f / (1 + MathF.Exp(-t + 3));
		//speed = MathF.Pow(speed, 2);
		float speed = f * 30;
		Vector2 targetPosition = player.center + player.lookDirection * 5;
		Vector2 initialDirection = player.lookDirection;
		Vector2 projectedPosition = player.center + initialDirection * Vector2.Dot(initialDirection, position - player.center);
		Vector2 toTarget = (projectedPosition + initialDirection * 5 - position).normalized; // (target.center - position).normalized;
		Vector2 direction = velocity.normalized;
		//float lerpStrength = (1 - MathF.Exp(-t)) * 10;
		float lerpStrength = Math.Min((MathF.Exp(t * 0.5f) - 1) * 30, 30);
		direction = Vector2.Lerp(direction, (direction + toTarget).normalized, lerpStrength * Time.deltaTime).normalized;
		velocity = direction * speed;

		base.update();

		rotation = Time.gameTime * 40;
	}

	public override void render()
	{
		base.render();
		Renderer.DrawLight(position, Mathf.ARGBToVector(0xFF99eeee).xyz * 3, 4);
	}
}
