using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class AttackAction : Action
{
	public Item item;
	public int handID;
	public Attack attack;

	public List<Entity> hitEntities = new List<Entity>();


	public AttackAction(Item item, int handID, Attack attack)
		: base(ActionType.Attack, "attack")
	{
		this.item = item;
		this.handID = handID;
		this.attack = attack;

		if (item.twoHanded)
		{
			animationName[0] = attack.animationName;
			animationName[1] = attack.animationName;
			animationSet[0] = item.moveset;
			animationSet[1] = item.moveset;
		}
		else
		{
			animationName[handID] = attack.animationName;
			animationSet[handID] = item.moveset;
		}

		animationSpeed = 1.0f;
		mirrorAnimation = handID == 1;
		fullBodyAnimation = false;
		animateCameraRotation = false;
		rootMotion = true;
		animationTransitionDuration = 0.2f;
		followUpCancelTime = attack.followUpCancelTime / animationSpeed;

		movementSpeedMultiplier = 0.5f;

		staminaCost = attack.staminaCost;
		staminaCostTime = attack.damageTimeStart / animationSpeed;

		if (item.sfxSwing != null)
			addSoundEffect(item.sfxSwing, handID, attack.damageTimeStart / animationSpeed, true);
	}

	public float damageTimeStart
	{
		get => attack.damageTimeStart / animationSpeed;
	}

	public float damageTimeEnd
	{
		get => attack.damageTimeEnd / animationSpeed;
	}
}
