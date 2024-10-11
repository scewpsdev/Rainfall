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

	public List<Entity> hitEntities = new List<Entity>();


	public SpellCastAction(Item weapon, bool mainHand, Spell spell, float manaCost)
		: base("spell_cast", mainHand)
	{
		duration = 1000;

		this.weapon = weapon;
		this.spell = spell;
		this.manaCost = manaCost;
	}

	public override void onStarted(Player player)
	{
		duration = 1.0f / spell.attackRate / weapon.attackRate / player.getAttackSpeedModifier();

		spell.cast(player, weapon);
		player.consumeMana(manaCost);

		if (spell.castSound != null)
			Audio.PlayOrganic(spell.castSound, new Vector3(player.position, 0));
	}

	public override Matrix getItemTransform(Player player)
	{
		Vector2 position = player.getWeaponOrigin(mainHand);
		Item item = mainHand ? player.handItem : player.offhandItem;
		if (item != null)
			position += item.renderOffset;
		float progress = MathF.Min(elapsedTime / duration * 1.5f, 1);
		return Matrix.CreateRotation(Vector3.UnitY, player.direction == -1 ? MathF.PI : 0)
			* Matrix.CreateTranslation(position.x, position.y, 0)
			* Matrix.CreateRotation(Vector3.UnitZ, (player.lookDirection * new Vector2(player.direction, 1)).angle + MathHelper.Lerp(MathF.PI * 0.75f, -0.25f * MathF.PI, progress));
	}
}
