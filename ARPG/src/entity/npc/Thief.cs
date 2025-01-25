using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Thief : NPC
{
	public Thief()
		: base("thief")
	{
		displayName = "Tyris";

		sprite = new Sprite(Resource.GetTexture("sprites/npc/thief.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation_("idle", 0, 0, 16, 0, 2, 2, true);
		animator.setAnimation("idle");

		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_THIEF_MET))
		{
			initialDialogue = new Dialogue();
			initialDialogue.addVoiceLine("thx i guess");
			initialDialogue.addVoiceLine("hier könnte ihre werbung stehen").addCallback(() =>
			{
				GameState.instance.save.setFlag(SaveFile.FLAG_NPC_THIEF_MET);
				GameState.instance.save.unlockStartingClass(StartingClass.thief);
			});
		}
	}
}
