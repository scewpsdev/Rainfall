using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ItemBuff
{
	public static Sprite movementSpeedModifierIcon = new Sprite(StatusEffect.tileset, 2, 1);
	public static Sprite attackDamageModifierIcon = new Sprite(StatusEffect.tileset, 3, 0);
	public static Sprite attackSpeedModifierIcon = new Sprite(StatusEffect.tileset, 3, 1);
	public static Sprite manaCostModifierIcon = new Sprite(StatusEffect.tileset, 2, 2);
	public static Sprite stealthAttackModifierIcon = new Sprite(StatusEffect.tileset, 3, 2);
	public static Sprite defenseModifierIcon = new Sprite(StatusEffect.tileset, 0, 3);
	public static Sprite accuracyModifierIcon = new Sprite(StatusEffect.tileset, 1, 3);
	public static Sprite criticalAttackModifierIcon = new Sprite(StatusEffect.tileset, 2, 3);


	public Item item;

	public float movementSpeedModifier = 1.0f;
	public float wallControlModifier = 1.0f;
	public float meleeDamageModifier = 1.0f;
	public float rangedDamageModifier = 1.0f;
	public float magicDamageModifier = 1.0f;
	public float attackSpeedModifier = 1.0f;
	public float manaCostModifier = 1.0f;
	public float manaRecoveryModifier = 1.0f;
	public float stealthAttackModifier = 1.0f;
	public float defenseModifier = 1.0f;
	public float accuracyModifier = 1.0f;
	public float criticalChanceModifier = 1.0f;
	public float criticalAttackModifier = 1.0f;

	public bool renderAura = true;
	public float auraStrength = 1.0f;
	Sprite aura;


	public ItemBuff(Item item)
	{
		this.item = item;

		aura = new Sprite(Entity.effectsTileset, 2, 2, 2, 2);
	}

	public ItemBuff copy() => (ItemBuff)MemberwiseClone();

	public void render(Entity entity)
	{
		if (renderAura)
		{
			Vector2 center = entity.position + entity.collider.center;
			float animation = 1.5f + MathF.Sin(Time.currentTime / 1e9f * 50) * 0.2f;
			float size = MathF.Ceiling(entity.collider.size.x) * animation;
			float alpha = 1 - MathF.Exp(-(auraStrength - 1) * 1.0f);
			uint color = 0xFFFFFFFF; // 0xFFd5a58f;
			Renderer.DrawSprite(center.x - 0.5f * size, center.y - 0.5f * size, Entity.LAYER_PLAYER_BG, size, size, 0, aura, false, MathHelper.ARGBToVector(color) * new Vector4(alpha, alpha, alpha, 1), true);
		}
	}
}
