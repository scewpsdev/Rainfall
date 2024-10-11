using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class KeenEdge : Item
{
	Modifier modifier = new Modifier() { criticalAttackModifier = 2.0f };

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
		player.modifiers.Add(modifier);
	}

	public override void onUnequip(Player player)
	{
		player.modifiers.Remove(modifier);
	}
}
