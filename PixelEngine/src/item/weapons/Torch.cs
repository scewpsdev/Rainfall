using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Torch : Item
{
	public Torch()
		: base("torch", ItemType.Weapon)
	{
		displayName = "Torch";

		baseDamage = 1.5f;
		baseAttackRange = 1.0f;
		baseAttackRate = 2.0f;
		stab = false;
		baseWeight = 1;

		value = 2;

		canDrop = false;
		isSecondaryItem = true;

		sprite = new Sprite(tileset, 8, 0);
		hasParticleEffect = true;
		particlesOffset = new Vector2(0.25f, 0.25f);
		renderOffset.x = 0.3f;

		hitSound = [Resource.GetSound("res/sounds/hit_torch.ogg")];
	}

	public override ParticleEffect createParticleEffect(Entity entity)
	{
		return Effects.CreateTorchEffect(entity);
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}

	public override void render(Entity entity)
	{
		Renderer.DrawLight(entity.position, new Vector3(1.0f, 0.9f, 0.7f) * 2, 9);
	}
}
