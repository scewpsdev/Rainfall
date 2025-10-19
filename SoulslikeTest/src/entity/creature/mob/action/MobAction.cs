using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MobAction
{
	public readonly string type;

	public string animationName;
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

	public float staminaCost = 0.0f;
	public float staminaCostTime = 0.0f;

	public long startTime = 0;
	public float elapsedTime { get; protected set; } = 0.0f;
	public float duration = 0.0f;

	List<ActionSfx> soundEffects = new List<ActionSfx>();

	bool staminaConsumed = false;


	public MobAction(string type)
	{
		this.type = type;
	}

	protected void addSoundEffect(ActionSfx sfx)
	{
		soundEffects.Add(sfx);
	}

	public virtual void update(Mob mob)
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
			if (elapsedTime >= sfx.time && !sfx.played)
			{
				//if (sfx.organic)
				//	Audio.PlayOrganic(sfx.sound, player.camera.position + player.camera.rotation.forward);
				//else
				//	Audio.Play(sfx.sound, player.camera.position + player.camera.rotation.forward);
				sfx.played = true;
				soundEffects[i] = sfx;
			}
		}
	}

	public virtual void onQueued(Mob mob)
	{
	}

	public virtual void onStarted(Mob mob)
	{
	}

	public virtual void onFinished(Mob mob)
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
