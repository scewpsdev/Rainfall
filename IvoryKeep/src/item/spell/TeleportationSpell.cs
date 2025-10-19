using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TeleportationSpell : Spell
{
	public TeleportationSpell()
		: base("teleportation")
	{
		displayName = "Teleportation";

		baseValue = 31;

		baseAttackRate = 0.4f;
		baseDamage = 0;
		manaCost = 1.5f;
		trigger = false;
		upgradable = false;

		spellIcon = new Sprite(tileset, 4, 8);
	}

	public override bool cast(Player player, Item staff, float manaCost, float duration)
	{
		HitData hit = player.level.raycastSolid(Vector2.Floor(player.center) + 0.5f, player.lookDirection, 100);
		if (hit != null)
		{
			Vector2 destination = hit.position + hit.normal * 0.5f;
			player.position = destination;

			player.level.addEntity(ParticleEffects.CreateScrollUseEffect(player), player.position + player.collider.center);

			return true;
		}
		return false;
	}
}
