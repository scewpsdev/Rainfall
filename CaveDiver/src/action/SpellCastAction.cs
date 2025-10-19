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

	bool cancelled = false;

	public List<Entity> hitEntities = new List<Entity>();


	public SpellCastAction(Item weapon, bool mainHand, Spell spell, float manaCost)
		: base("spell_cast", mainHand)
	{
		duration = 1000;

		this.weapon = weapon;
		this.spell = spell;
		this.manaCost = manaCost;

		setRenderWeapon(mainHand, weapon);
	}

	public override void onQueued(Player player)
	{
		duration = 1.0f / spell.attackRate / weapon.attackRate / player.getAttackSpeedModifier();

		direction = player.lookDirection.normalized;
		charDirection = MathF.Abs(player.lookDirection.x) > 0.001f ? MathF.Sign(player.lookDirection.x) : player.direction;
	}

	public override void onStarted(Player player)
	{
		//if (player.mana < manaCost)
		//	duration *= 2;

		spell.charge(player, weapon, manaCost, duration);
	}

	public override void onFinished(Player player)
	{
		if (!cancelled && spell.cast(player, weapon, manaCost, duration))
		{
			player.consumeMana(manaCost);
			if (weapon.staffCharges > 0)
				weapon.staffCharges--;

			if (spell.castSound != null)
				Audio.PlayOrganic(spell.castSound, new Vector3(player.position, 0));
		}
	}

	public override void update(Player player)
	{
		base.update(player);

		direction = player.lookDirection.normalized;
		charDirection = MathF.Abs(player.lookDirection.x) > 0.001f ? MathF.Sign(player.lookDirection.x) : player.direction;

		if (player.currentAttackInput == null || !player.currentAttackInput.isDown())
		{
			cancel();
			cancelled = true;
		}

		spell.update(player);
	}

	public override void render(Player player)
	{
		Vector2 position = player.getWeaponOrigin(mainHand);
		float progress = MathF.Min(elapsedTime / duration * 1.5f, 1);
		Renderer.DrawLight(player.position + position + new Vector2(0, progress * 0.3f), new Vector3(0.7f, 1.0f, 0.9f) * progress, 5);
	}

	public override Matrix getItemTransform(Player player, bool mainHand)
	{
		Vector2 position = player.getWeaponOrigin(mainHand);
		Item item = mainHand ? player.handItem : player.offhandItem;
		if (item != null)
			position += item.renderOffset;
		float progress = MathF.Min(elapsedTime / duration * 1.5f, 1);
		return Matrix.CreateRotation(Vector3.UnitY, charDirection == -1 ? MathF.PI : 0)
			* Matrix.CreateTranslation(new Vector3(position + new Vector2(MathF.Sin(elapsedTime * 71), MathF.Cos(elapsedTime * 53 + 143.34f)) * 0.04f * progress, 0))
			* Matrix.CreateRotation(Vector3.UnitZ, (direction * new Vector2(charDirection, 1)).angle + progress * 0.5f)
			;
	}
}
