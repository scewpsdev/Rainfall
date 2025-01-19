using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Torch : Weapon
{
	public Torch()
		: base("torch")
	{
		displayName = "Torch";

		baseDamage = 1.0f;
		baseAttackRange = 1.0f;
		baseAttackRate = 2.0f;
		stab = false;
		baseWeight = 1;
		doubleBladed = false;
		attackAngleOffset = -0.25f * MathF.PI;

		value = 2;

		canDrop = false;
		isSecondaryItem = true;
		canIgnite = true;

		sprite = new Sprite(tileset, 8, 0);
		hasParticleEffect = true;
		particlesOffset = new Vector2(2, 4) / 16.0f;
		renderOffset = new Vector2(0.5f - 2 / 16.0f, 0.2f);
		alwaysRenderHand = true;

		hitSound = [Resource.GetSound("sounds/hit_torch.ogg")];
	}

	public override ParticleEffect createParticleEffect(Entity entity)
	{
		return ParticleEffects.CreateTorchEffect(entity);
	}

	public override void render(Entity entity)
	{
		Renderer.DrawLight(entity.position, new Vector3(1.0f, 0.9f, 0.7f) * 2, 9);
	}
}
