using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AttackModifier : StatusEffect
{
	public float strength;
	bool renderAura;

	Sprite buffSprite;

	public AttackModifier(float strength, bool renderAura = true)
		: base("attack_modifier", new Sprite(tileset, 3, 0))
	{
		this.strength = strength;
		this.renderAura = renderAura;
		buffSprite = new Sprite(Entity.effectsTileset, 2, 2, 2, 2);
	}

	public override void render(Entity entity)
	{
		if (renderAura)
		{
			Vector2 center = entity.position + entity.collider.center;
			float animation = 1.5f + MathF.Sin(Time.currentTime / 1e9f * 50) * 0.2f;
			float size = MathF.Ceiling(entity.collider.size.x) * animation;
			float alpha = 1 - MathF.Exp(-(strength - 1) * 1.0f);
			uint color = 0xFFFFFFFF; // 0xFFd5a58f;
			Renderer.DrawSprite(center.x - 0.5f * size, center.y - 0.5f * size, Entity.LAYER_BG, size, size, 0, buffSprite, false, MathHelper.ARGBToVector(color) * new Vector4(alpha, alpha, alpha, 1), true);
		}
	}
}
