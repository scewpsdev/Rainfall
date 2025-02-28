using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CrossbowAimAction : PlayerAction
{
	public CrossbowAimAction(Crossbow crossbow, int hand)
		: base("crossbow_aim", hand)
	{
		animationName[hand] = "aim";
		animationSet[hand] = crossbow.moveset;

		animationName[hand ^ 1] = "aim";
		animationSet[hand ^ 1] = crossbow.moveset;

		viewmodelAim = 1;
		swayAmount = 0.1f;

		animationTransitionDuration = 0.3f;
		duration = 1000;
	}

	public override void update(Player player)
	{
		base.update(player);

		if (!InputManager.IsDown("AttackLeft"))
			cancel();
	}
}
