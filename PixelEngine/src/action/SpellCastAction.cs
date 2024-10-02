using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SpellCastAction : EntityAction
{
	Item weapon;
	Spell spell;

	public List<Entity> hitEntities = new List<Entity>();


	public SpellCastAction(Item weapon, bool mainHand, Spell spell)
		: base("spell_cast", mainHand)
	{
		duration = 1000;

		this.weapon = weapon;
		this.spell = spell;
	}

	public override void onStarted(Player player)
	{
		duration = 1.0f / weapon.attackRate / player.attackSpeedModifier;

		float manaCost = spell.manaCost * weapon.manaCost * player.manaCostModifier;

		if (player.mana >= manaCost)
		{
			spell.cast(player, weapon);

			player.consumeMana(manaCost);

			if (spell.useSound != null)
				Audio.PlayOrganic(spell.useSound, new Vector3(player.position, 0));
		}
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
