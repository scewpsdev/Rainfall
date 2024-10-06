using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class KeenEdge : Item
{
	public KeenEdge()
		: base("keen_edge", ItemType.Relic)
	{
		displayName = "Keen Edge";
		description = "Grants increased chance to land critical hits on enemies";
		stackable = true;
		tumbles = false;

		value = 28;

		sprite = new Sprite(tileset, 1, 7);
	}

	public override void onEquip(Player player)
	{
		player.criticalAttackModifier *= 2.0f;
	}

	public override void onUnequip(Player player)
	{
		player.criticalAttackModifier /= 2.0f;
	}
}
