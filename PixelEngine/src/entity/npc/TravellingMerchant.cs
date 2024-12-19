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

		initialDialogue = new Dialogue();
		initialDialogue.addVoiceLine("Ah, a customer! You'd be surprised how good business is down here.");
		initialDialogue.addVoiceLine("But don't ask me how I get by wares. Just know that they can be yours...\\3 For a price, of course.");

		{
			Dialogue dialogue = new Dialogue();
			dialogue.addVoiceLine("You're looking for the lost sigil, aren't you? Everyone is. Let me know if you find it.");
			addDialogue(dialogue);
		}

		{
			Dialogue dialogue = new Dialogue();
			dialogue.addVoiceLine("Let's talk trade.");
			addDialogue(dialogue);
		}

		populateShop(random, 3, 7, level.lootValue, ItemType.Weapon, ItemType.Armor, ItemType.Relic, ItemType.Gem);

		buysItems = true;
	}

	public TravellingMerchant()
		: this(Random.Shared, GameState.instance.level)
	{
	}
}
