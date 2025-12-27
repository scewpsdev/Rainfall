using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfTears : Item
{
	float dmgBuff = 0.5f;


	public RingOfTears()
		: base("ring_of_tears", ItemType.Relic)
	{
		displayName = "Ring of Tears";

		description = "Increases critical attack chance when health is low";

		baseValue = 110;

		sprite = new Sprite(tileset, 10, 2);

		buff = new ItemBuff(this);
		buff.renderAura = true;
		buff.auraColor = 0xFFd82b2b;
	}

	public override void update(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;
			buff.active = player.health <= 1.1f;
		}

		buff.criticalChanceModifier = 8;
		//buff.meleeDamageModifier = 1 + dmgBuff;
	}

	public override void upgrade()
	{
		base.upgrade();
		buff.criticalChanceModifier += 4;
		//dmgBuff += 0.1f;
	}
}
