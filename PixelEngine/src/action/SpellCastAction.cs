using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SpellCastAction : EntityAction
{
	Item weapon;
	public Spell spell;
	float manaCost;

	Vector2 direction;
	int charDirection;

	public List<Entity> hitEntities = new List<Entity>();


	public SpellCastAction(Item weapon, bool mainHand, Spell spell, float manaCost)
		: base("spell_cast", mainHand)
	{
		duration = 1000;

		this.weapon = weapon;
		this.spell = spell;
		this.manaCost = manaCost;

		renderWeapon = true;
	}

	public override void onQueued(Player player)
	{
		duration = 1.0f / spell.attackRate / weapon.attackRate / player.getAttackSpeedModifier();

		direction = player.lookDirection.normalized;
		charDirection = MathF.Abs(player.lookDirection.x) > 0.001f ? MathF.Sign(player.lookDirection.x) : player.direction;
	}

	public override void onStarted(Player player)
	{
		if (player.mana < manaCost)
			duration *= 2;

		if (spell.cast(player, weapon, manaCost, duration))
		{
			player.consumeMana(manaCost);

			if (spell.castSound != null)
				Audio.PlayOrganic(spell.castSound, new Vector3(player.position, 0));
		}
	}

	public override void update(Player player)
	{
		base.update(player);

		spell.update(player);
	}

	public override Matrix getItemTransform(Player player)
	{
		Vector2 position = player.getWeaponOrigin(mainHand);
		Item item = mainHand ? player.handItem : player.offhandItem;
		if (item != null)
			position += item.renderOffset;
		float progress = MathF.Min(elapsedTime / duration * 1.5f, 1);
		return Matrix.CreateRotation(Vector3.UnitY, charDirection == -1 ? MathF.PI : 0)
			* Matrix.CreateTranslation(position.x, position.y, 0)
			* Matrix.CreateRotation(Vector3.UnitZ, (direction * new Vector2(charDirection, 1)).angle + MathHelper.Lerp(MathF.PI * 0.75f, -0.25f * MathF.PI, progress));
	}
}
