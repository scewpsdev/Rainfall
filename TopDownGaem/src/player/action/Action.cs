using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public struct ActionSfx
{
	internal Sound sound;
	internal float gain;
	internal float time;
	internal bool organic;

	internal bool played;

	public ActionSfx(Sound sound, float gain = 1.0f, float time = 0.0f, bool organic = false)
	{
		this.sound = sound;
		this.gain = gain;
		this.time = time;
		this.organic = organic;
		played = false;
	}
}

public class Action
{
	public readonly string type;

	public string[] animationName = new string[3];
	public Model[] animationSet = new Model[3];
	public bool mirrorAnimation = false;
	public bool fullBodyAnimation = false;
	public bool animateCameraRotation = false;
	public bool rootMotion = false;
	public float animationTransitionDuration = 0.1f;
	public float followUpCancelTime = 100.0f;
	public float animationSpeed = 1.0f;

	public bool[] overrideHandModels = new bool[2];
	public string[] handItemModels = new string[2];
	public bool[] flipHandModels = new bool[2];
	public string[] handItemAnimations = new string[2];

	public float movementSpeedMultiplier = 1.0f;
	public float rotationSpeedMultiplier = 1.0f;
	public Vector3 movementInput = Vector3.Zero;
	public float maxSpeed = 0.0f;

	public float iframesStartTime = 0.0f;
	public float iframesEndTime = 0.0f;

	public float staminaCost = 0;
	public float staminaCostTime = 0.0f;

	public float manaCost = 0;
	public float manaCostTime = 0.0f;

	public long startTime = 0;
	public float elapsedTime { get; protected set; } = 0.0f;
	public float duration = 0.0f;

	List<ActionSfx> soundEffects = new List<ActionSfx>();

	bool staminaConsumed = false;


	public Action(string type)
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
			player.stats.consumeStamina(staminaCost);
			staminaConsumed = true;
		}

		for (int i = 0; i < soundEffects.Count; i++)
		{
			ActionSfx sfx = soundEffects[i];
			if (elapsedTime >= sfx.time && !sfx.played)
			{
				if (sfx.organic)
					player.playSoundOrganic(sfx.sound, sfx.gain);
				else
					player.playSound(sfx.sound, sfx.gain);
				sfx.played = true;
				soundEffects[i] = sfx;
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
