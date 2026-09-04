using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Barbarian : NPC
{
	public Barbarian()
		: base("barbarian")
	{
		displayName = "Bjorn";

		sprite = new Sprite(Resource.GetTexture("sprites/npc/barbarian.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 1, true);
		animator.setAnimation("idle");

		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_BARBARIAN_MET))
		{
			save.setInititalDialogue("""
				Hah! About time someone with some sense came along.
				For a while I thought this damn prison cell would be the last thing I ever see!
				I owe you stranger...
				""").addCallback(() =>
			{
				GameState.instance.save.setFlag(SaveFile.FLAG_NPC_BARBARIAN_MET);
				GameState.instance.save.unlockStartingClass(StartingClass.barbarian);
			});
		}
	}
}
