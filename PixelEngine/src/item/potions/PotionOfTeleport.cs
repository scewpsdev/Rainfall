using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TeleportEffect : PotionEffect
{
	public TeleportEffect()
		: base("Teleport", 9, new Sprite(Item.tileset, 6, 5))
	{
	}

	public override void apply(Entity entity, Potion potion)
	{
		SpellEffects.TeleportEntity(entity);
	}
}

public class PotionOfTeleport : Potion
{
	public PotionOfTeleport()
		: base("potion_of_teleport")
	{
		addEffect(new TeleportEffect());

		displayName = "Potion of Teleport";

		value = 11;
		canDrop = true;

		sprite = new Sprite(tileset, 6, 5);
	}
}
