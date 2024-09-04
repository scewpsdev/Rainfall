using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Tinkerer : NPC
{
	public Tinkerer(Random random)
		: base("tinkerer_merchant")
	{
		displayName = "Tinker";

		sprite = new Sprite(Resource.GetTexture("res/sprites/merchant6.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		buysItems = true;
		canCraft = true;

		populateShop(random, 9, ItemType.Food, ItemType.Potion, ItemType.Scroll, ItemType.Gem, ItemType.Utility);
	}
}
