using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TravellingMerchant : NPC
{
	public TravellingMerchant()
		: base("travelling_merchant")
	{
		displayName = "Traveller";

		sprite = new Sprite(Resource.GetTexture("res/sprites/merchant2.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");
	}

	public override void populateShop(Random random)
	{
		int numShopItems = MathHelper.RandomInt(1, 5, random);
		for (int i = 0; i < numShopItems; i++)
		{
			Item item = Item.CreateRandom(random, 20);
			if (item.stackable || !hasShopItem(item.name))
				addShopItem(item);
		}
	}
}
