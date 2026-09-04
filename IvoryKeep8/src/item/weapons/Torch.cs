using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Torch : Item
{
	ParticleEffect particles;


	public Torch()
		: base("torch", ItemType.Utility)
	{
		displayName = "Torch";

		baseDamage = 1.0f;
		baseAttackRange = 1.0f;
		baseAttackRate = 2.0f;
		baseWeight = 1;
		doubleBladed = false;

		baseValue = 2;

		canDrop = false;
		isSecondaryItem = true;
		canIgnite = true;

		sprite = new Sprite(tileset, 8, 0);
		hasParticleEffect = true;
		particlesOffset = new Vector2(2, 4) / 16.0f;
		renderOffset = new Vector2(0.1f, 0.2f);

		hitSound = [Resource.GetSound("sounds/hit_torch.ogg")];
	}

	public override ParticleEffect createParticleEffect(Entity entity)
	{
		return particles = ParticleEffects.CreateTorchEffect(entity);
	}

	public override void onLevelSwitch(Level to)
	{
		GameState.instance.moveEntityToLevel(particles, to);
	}

	public override bool use(Player player)
	{
		player.throwItem(this);
		return true;
	}

	public override void render(Entity entity)
	{
		Renderer.DrawLight(entity.position, new Vector3(1.0f, 0.9f, 0.7f) * 2, 9);
	}
}
