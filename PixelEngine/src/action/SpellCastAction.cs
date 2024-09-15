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
		duration = 1.0f / weapon.attackRate;

		this.weapon = weapon;
		this.spell = spell;
	}

	public override void onStarted(Player player)
	{
		spell.cast(player, weapon);
	}

	public override Matrix getItemTransform(Player player)
	{
		Vector2 position = player.getWeaponOrigin(mainHand);
		Item item = mainHand ? player.handItem : player.offhandItem;
		if (item != null)
			position += item.renderOffset;
		return Matrix.CreateRotation(Vector3.UnitY, player.direction == -1 ? MathF.PI : 0)
			* Matrix.CreateTranslation(position.x, position.y, 0)
			* Matrix.CreateRotation(Vector3.UnitZ, 1 - (elapsedTime / duration) * 0.75f);
	}
}
