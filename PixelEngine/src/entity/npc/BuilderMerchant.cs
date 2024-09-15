using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BuilderMerchant : NPC
{
	public BuilderMerchant(Random random)
		: base("builder_merchant")
	{
		displayName = "John";

		sprite = new Sprite(Resource.GetTexture("res/sprites/merchant.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		saleTax = 0.2f;
		buysItems = true;

		populateShop(random, 3, 9, 15, ItemType.Weapon, ItemType.Armor, ItemType.Scroll, ItemType.Food, ItemType.Utility, ItemType.Ammo);
	}

	public BuilderMerchant()
		: this(Random.Shared)
	{
	}
}
