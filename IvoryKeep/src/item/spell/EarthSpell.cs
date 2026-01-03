using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EarthSpell : Spell
{
	public EarthSpell()
		: base("earth")
	{
		displayName = "Earth";

		baseValue = 28;

		baseAttackRate = 0.4f;
		baseDamage = 0;
		manaCost = 1.5f;
		trigger = false;
		maxUpgradeLevel = 0;

		spellIcon = new Sprite(tileset, 4, 8);
	}

	public override bool cast(Player player, Item staff, float manaCost, float duration)
	{
		Vector2 pos = player.position + player.collider.center;
		int numSpikes = 5;
		int x0 = Math.Max((int)pos.x + 1 * player.direction, 0);
		int x1 = Math.Min((int)pos.x + (1 + numSpikes) * player.direction, GameState.instance.level.width - 1);
		for (int x = x0; x != x1; x += player.direction)
		{
			HitData hit = GameState.instance.level.raycastSolid(new Vector2(x + 0.5f, (int)pos.y + 0.5f), new Vector2(0, 1), 20);
			if (hit != null && hit.distance > 1)
			{
				SpikeTrap spike = new SpikeTrap();
				spike.trigger();
				GameState.instance.level.addEntity(spike, new Vector2(hit.tile.x + 0.5f, hit.tile.y - 0.5f));
			}
		}
		player.hud.showMessage("The earth rumbles.");

		player.level.addEntity(ParticleEffects.CreateScrollUseEffect(player), player.position + player.collider.center);

		return true;
	}
}
