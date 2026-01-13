using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LightningSpell : Spell
{
	public LightningSpell()
		: base("lightning")
	{
		displayName = "Lightning";
		description = "Wild lightning that ricochets unpredictably.";

		baseValue = 27;

		baseAttackRate = 1;
		baseDamage = 2.5f;
		baseAttackRange = 5;
		manaCost = 0.15f;
		trigger = false;

		spellIcon = new Sprite(tileset, 3, 6);

		castSound = Resource.GetSounds("sounds/lightning", 4);
	}

	public override bool cast(Player player, Item staff, float manaCost, float duration)
	{
		Vector2 position = player.position + new Vector2(0.0f, 0.5f);
		Vector2 offset = new Vector2(player.direction * 0.5f, 0.0f);

		Vector2 direction = player.lookDirection.normalized;
		//Vector2 inaccuracy = Mathf.RandomPointOnCircle(Random.Shared) * 0.05f;
		//direction = (direction + inaccuracy / (staff.accuracy * player.accuracyModifier)).normalized;

		LightningProjectile projectile = new LightningProjectile(direction, offset, player, this, staff);
		projectile.maxRange += upgradeLevel;
		projectile.maxRicochets += upgradeLevel;

		GameState.instance.level.addEntity(projectile, position);
		GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);

		return true;
	}
}
