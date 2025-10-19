using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum MobActionType
{
	None = 0,

	Dodge,
	Attack,
	BowDraw,
	BowShoot,
	SpellCast,
	ShieldRaise,
	ShieldBlock,
	ShieldGuardBreak,
	ShieldParry,
	ConsumableUse,
	WeaponDraw,
	StaggerShort,
	StaggerBlocked,
	StaggerParry,
}

public class MobAction
{
	public MobActionType type;

	public string animationName;
	public bool rootMotion = false;
	public float animationTransitionDuration = 0.1f;
	public float followUpCancelTime = 100.0f;
	public float animationSpeed = 1.0f;

	public float movementSpeedMultiplier = 1.0f;
	public float rotationSpeedMultiplier = 0.0f;

	public long startTime = 0;
	public float duration = 0.0f;


	public MobAction(MobActionType type)
	{
		this.type = type;
	}

	public virtual void update(Creature mob)
	{
	}

	public virtual void onQueued(Creature mob)
	{
	}

	public virtual void onStarted(Creature mob)
	{
	}

	public virtual void onFinished(Creature mob)
	{
	}

	public bool hasStarted
	{
		get => startTime > 0;
	}

	public bool hasFinished
	{
		get => hasStarted && elapsedTime >= duration;
	}

	public float elapsedTime
	{
		get => startTime > 0 ? (Time.currentTime - startTime) / 1e9f : 0.0f;
	}
}
