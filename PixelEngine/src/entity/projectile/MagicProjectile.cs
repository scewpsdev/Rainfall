using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicProjectile : Projectile
{
	const float speed = 50; //2;

	public MagicProjectile(Vector2 direction, Vector2 startVelocity, Vector2 offset, Player player, Item spell, Item staff)
		: base(direction * speed, startVelocity, offset, player, spell, spell.baseDamage * staff.getAttackDamage(player) * player.getMagicDamageModifier())
	{
		//maxSpeed = 40;
		//acceleration = 50;
		maxRicochets = 0;
		maxRange = 8;

		sprite = new Sprite(Item.tileset, 9, 1);
		spriteColor = new Vector4(1.5f);
		additive = true;

		trailColor = 0xFF99eeee;
	}

	public override void onHit(Vector2 normal)
	{
		GameState.instance.level.addEntity(ParticleEffects.CreateImpactEffect(normal, velocity.length, MathHelper.ARGBToVector(0xFF99eeee).xyz), position - velocity * Time.deltaTime);
	}

	public override void render()
	{
		base.render();
		Renderer.DrawLight(position, MathHelper.ARGBToVector(0xFF99eeee).xyz * 3, 4);
	}
}
