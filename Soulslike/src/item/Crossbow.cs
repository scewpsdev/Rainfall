using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Crossbow : Item
{
	public Vector3 castOrigin;

	bool loaded = true;

	AnimationState defaultAnim;
	AnimationState loadedAnim;


	public Crossbow(string name, string displayName)
		: base(ItemType.Crossbow, name, displayName)
	{
		twoHanded = true;
		secondaryUseTrigger = false;

		viewmodelAim = 0.8f;

		castOrigin = new Vector3(-0.234552f, 0.057415f, 0);

		defaultAnim = Animator.CreateAnimation(model, "default", true);
		loadedAnim = Animator.CreateAnimation(model, "loaded", true);
	}

	public override void use(Player player, int hand)
	{
		if (player.actionManager.currentAction is CrossbowAimAction)
			player.actionManager.currentAction.cancel();
		player.actionManager.queueAction(new CrossbowShootAction(this, hand));
		loaded = false;
	}

	public override void useSecondary(Player player, int hand)
	{
		player.actionManager.queueAction(new CrossbowAimAction(this, hand));
	}

	public override void update(Player player, Animator animator)
	{
		//if (loaded)
		animator.setAnimation(loadedAnim);
		//else
		//	animator.setAnimation(defaultAnim);
	}
}
