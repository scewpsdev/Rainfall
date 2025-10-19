using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WhirlyPotion : Potion
{
	public WhirlyPotion()
		: base("whirly_potion", "Whirly Potion", PotionEffectType.Teleport)
	{
		/*
		addEffect(new TeleportEffect());

		displayName = "Whirly Flask";

		value = 17;
		canDrop = true;
		stackable = true;
		//makeThrowable();

		sprite = new Sprite(tileset, 6, 5);
		*/
	}
}
