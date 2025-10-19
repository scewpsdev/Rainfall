using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicMissileProjectile : Projectile
{
	const float speed = 16;
	const float radius = 2;
	float explosionDamage;

	public MagicMissileProjectile(Vector2 direction, Vector2 startVelocity, Vector2 offset, Player player, Item staff, Item spell)
		: base(direction * speed, startVelocity, offset, player, spell, 0)
	{
		gravity = -30;
		explosionDamage = spell.baseDamage * staff.getAttackDamage(player) * player.getMagicDamageModifier();

		sprite = new Sprite(Item.tileset, 9, 1);
		spriteColor = new Vector4(1.5f);
		additive = true;
	}

	public override void onHit(Vector2 normal)
	{
		GameState.instance.level.addEntity(ParticleEffects.CreateImpactEffect(normal, velocity.length, MathHelper.ARGBToVector(0xFF99eeee).xyz), position - velocity * Time.deltaTime);
		SpellEffects.Explode(position, radius, explosionDamage, this, null);
	}

	public override void render()
	{
		base.render();
		Renderer.DrawLight(position, MathHelper.ARGBToVector(0xFF99eeee).xyz * 4, 5);
	}
}
