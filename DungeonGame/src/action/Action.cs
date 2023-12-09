using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


public enum ActionType
{
	None = 0,

	Dodge,
	Attack,
	BowDraw,
	BowShoot,
	SpellCast,
	ShieldRaise,
	ShieldStance,
	BlockingHit,
	ShieldGuardBreak,
	Parry,
	TorchRaise,
	ConsumableUse,
	WeaponDraw,
	ItemPickUp,
	ItemThrow,
	ItemWieldTwoHand,
	ChestOpen,
	DoorOpen,
	DoorClose,
	Spawn,
	Death,
}

public enum ActionPriority
{
	Normal = 0,

	Stagger,
	Death,
}

public class Action
{
	struct ActionSfx
	{
		internal Sound sound;
		internal int handID;
		internal float time;
		internal bool organic;
		internal bool played;
	}

	public readonly ActionType type;
	public readonly ActionPriority priority;

	public string[] animationName = new string[2];
	public Model[] animationSet = new Model[2];
	public bool mirrorAnimation = false;
	public bool fullBodyAnimation = false;
	public bool animateCameraRotation = false;
	public bool rootMotion = false;
	public float animationTransitionDuration = 0.1f;
	public float followUpCancelTime = 100.0f;
	public float animationSpeed = 1.0f;

	public bool[] overrideHandModels = new bool[2];
	public Model[] handItemModels = new Model[2];
	public bool[] flipHandModels = new bool[2];
	public string[] handItemAnimations = new string[2];

	public float movementSpeedMultiplier = 1.0f;
	public bool lockMovement = false;
	public bool lockRotation = false;
	public Vector3 movementInput = Vector3.Zero;
	public float maxSpeed = 0.0f;

	public float iframesStartTime = 0.0f;
	public float iframesEndTime = 0.0f;

	public float parryFramesStartTime = 0.0f;
	public float parryFramesEndTime = 0.0f;

	public float staminaCost = 0.0f;
	public float staminaCostTime = 0.0f;

	public long startTime = 0;
	public float elapsedTime { get; protected set; } = 0.0f;
	public float duration = 0.0f;

	List<ActionSfx> soundEffects = new List<ActionSfx>();

	bool staminaConsumed = false;


	public Action(ActionType type, ActionPriority priority = ActionPriority.Normal)
	{
		this.type = type;
		this.priority = priority;
	}

	protected void addSoundEffect(Sound sound, int handID, float time, bool organic)
	{
		soundEffects.Add(new ActionSfx { sound = sound, handID = handID, time = time, organic = organic, played = false });
	}

	public virtual void update(Player player)
	{
		elapsedTime += Time.deltaTime * animationSpeed;

		if (staminaCost > 0.0f && elapsedTime >= staminaCostTime && !staminaConsumed)
		{
			player.stats.consumeStamina(staminaCost);
			staminaConsumed = true;
		}

		for (int i = 0; i < soundEffects.Count; i++)
		{
			if (elapsedTime >= soundEffects[i].time && !soundEffects[i].played)
			{
				AudioSource audio = soundEffects[i].handID != -1 ? player.handEntities[soundEffects[i].handID].audio : player.audioAction;

				if (soundEffects[i].organic)
					audio.playSoundOrganic(soundEffects[i].sound);
				else
					audio.playSound(soundEffects[i].sound);
				soundEffects[i] = new ActionSfx { sound = soundEffects[i].sound, time = soundEffects[i].time, played = true };
			}
		}
	}

	public virtual void onQueued(Player player)
	{
	}

	public virtual void onStarted(Player player)
	{
	}

	public virtual void onFinished(Player player)
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
}
