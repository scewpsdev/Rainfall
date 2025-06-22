using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public struct ActionSfx
{
	internal Sound[] sound;
	internal float gain;
	internal float pitch;
	internal float time;
	internal bool organic;

	internal uint source;

	public ActionSfx(Sound[] sound, float gain = 1, float pitch = 1, float time = 0, bool organic = false)
	{
		this.sound = sound;
		this.gain = gain;
		this.pitch = pitch;
		this.time = time;
		this.organic = organic;

		source = 0;
	}

	public ActionSfx(Sound sound, float gain = 1, float pitch = 1, float time = 0, bool organic = false)
		: this([sound], gain, pitch, time, organic)
	{
	}
}

public struct ActionEvent
{
	public Action<Player> callback;
	public float time;
	public bool triggered;
}

public class PlayerAction
{
	public readonly string type;

	public string animationName;
	public Model animationData;
	public float animationTransitionDuration = 0.1f;
	public float followUpCancelTime = 100.0f;
	public float animationSpeed = 1.0f;

	public bool[] overrideHandModels = new bool[2];
	public Item[] handItemModels = new Item[2];
	public bool[] flipHandModels = new bool[2];
	public string[] handItemAnimations = new string[2];

	public float movementSpeedMultiplier = 0.0f;
	public float rotationSpeedMultiplier = 0.0f;

	public float iframesStartTime = 0.0f;
	public float iframesEndTime = 0.0f;

	public float parryFramesStartTime = 0.0f;
	public float parryFramesEndTime = 0.0f;

	public bool lockRotation = false;
	public float overrideRotationLockStartTime = 0.0f;
	public float overrideRotationLockEndTime = 0.0f;
	public bool rotationIsLocked => lockRotation && !(elapsedTime >= overrideRotationLockStartTime && elapsedTime < overrideRotationLockEndTime);

	public bool canJump = false;

	public float staminaCost = 0.0f;
	public float staminaCostTime = 0.0f;

	public long startTime = 0;
	public float elapsedTime { get; protected set; } = 0.0f;
	public float duration = 0.0f;

	List<ActionSfx> soundEffects = new List<ActionSfx>();
	List<ActionEvent> events = new List<ActionEvent>();

	bool staminaConsumed = false;


	public PlayerAction(string type)
	{
		this.type = type;
	}

	protected void addSoundEffect(ActionSfx sfx)
	{
		soundEffects.Add(sfx);
	}

	public virtual void update(Player player)
	{
		elapsedTime += Time.deltaTime * animationSpeed;

		if (staminaCost > 0.0f && elapsedTime >= staminaCostTime && !staminaConsumed)
		{
			//player.stats.consumeStamina(staminaCost);
			staminaConsumed = true;
		}

		for (int i = 0; i < soundEffects.Count; i++)
		{
			ActionSfx sfx = soundEffects[i];

			Vector3 sourcePosition = player.position;

			if (elapsedTime >= sfx.time && sfx.source == 0)
			{
				if (sfx.organic)
					sfx.source = Audio.PlayOrganic(sfx.sound, sourcePosition, sfx.gain, sfx.pitch);
				else
					sfx.source = Audio.Play(sfx.sound, sourcePosition, sfx.gain, sfx.pitch);

				soundEffects[i] = sfx;
			}
			else if (sfx.source != 0)
			{
				Audio.SetSourcePosition(sfx.source, sourcePosition);
			}
		}

		for (int i = 0; i < events.Count; i++)
		{
			ActionEvent ev = events[i];

			if (elapsedTime >= ev.time && !ev.triggered)
			{
				ev.callback(player);
				ev.triggered = true;
			}

			events[i] = ev;
		}
	}

	public virtual void fixedUpdate(Player player, float delta)
	{
	}

	public virtual void draw(Player player)
	{
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
