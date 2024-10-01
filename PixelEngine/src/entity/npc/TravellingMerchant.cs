using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class TravellingMerchant : NPC
{
	public TravellingMerchant(Random random, Level level)
		: base("travelling_merchant")
	{
		displayName = "Siko";

		sprite = new Sprite(Resource.GetTexture("res/sprites/merchant2.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		populateShop(random, 2, 6, level.lootValue, ItemType.Weapon, ItemType.Armor, ItemType.Ring, ItemType.Gem);

		buysItems = true;
	}

	public TravellingMerchant()
		: this(Random.Shared, GameState.instance.level)
	{
	}
}
