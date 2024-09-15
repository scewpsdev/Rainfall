using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Blacksmith : NPC
{
	public Blacksmith(Random random)
		: base("blacksmith")
	{
		displayName = "Blacksmith";

		sprite = new Sprite(Resource.GetTexture("res/sprites/merchant5.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		saleTax = 0.2f;
		buysItems = true;

		addVoiceLine("Mmh.");

		populateShop(random, 2, 8, 12, ItemType.Weapon, ItemType.Armor, ItemType.Ammo);
	}

	public Blacksmith()
		: this(Random.Shared)
	{
	}
}
