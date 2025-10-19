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
		displayName = "Ring of Laceration";
		description = "Grants increased chance to land critical hits on enemies";
		stackable = true;
		tumbles = false;

		baseValue = 28;

		//sprite = new Sprite(tileset, 1, 7);
		sprite = new Sprite(tileset, 2, 10);

		buff = new ItemBuff(this) { criticalChanceModifier = 2.0f };
	}
}
