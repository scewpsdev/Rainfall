using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class BowDrawAction : Action
{
	public BowDrawAction(Item bow, int handID)
		: base(ActionType.BowDraw)
	{
		overrideHandModels[handID ^ 1] = true;
		handItemModels[handID ^ 1] = Item.Get("arrow");
		flipHandModels[handID ^ 1] = true;

		handItemAnimations[handID] = "draw";

		animationName[0] = "attack_draw";
		animationName[1] = "attack_draw";
		animationSet[0] = bow.moveset;
		animationSet[1] = bow.moveset;

		mirrorAnimation = handID == 1;

		duration = 1000.0f;
		followUpCancelTime = bow.moveset.getAnimationData("draw").Value.duration;

		movementSpeedMultiplier = 0.5f;

		addSoundEffect(bow.sfxBowDraw, handID, 20 / 24.0f, true);
	}
}
