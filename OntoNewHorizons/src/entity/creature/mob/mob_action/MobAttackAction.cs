using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class MobAttackAction : MobAction
{
	public readonly int handID;
	public readonly int index;

	public List<Entity> hitEntities = new List<Entity>();


	public MobAttackAction(int handID, int index)
		: base(MobActionType.Attack, "attack")
	{
		this.handID = handID;
		this.index = index;

		animationName = index == 0 ? "attack_light1" : index == 1 ? "attack_light2" : "default";
		rootMotion = true;

		followUpCancelTime = index == 0 ? 28 / 24.0f / animationSpeed : 100.0f;

		rotationSpeedMultiplier = 0.5f;
	}


	public float damageTimeStart
	{
		get => index == 0 ? 14 / 24.0f / animationSpeed : 7 / 24.0f / animationSpeed;
	}

	public float damageTimeEnd
	{
		get => index == 0 ? 21 / 24.0f / animationSpeed : 14 / 24.0f / animationSpeed;
	}
}
