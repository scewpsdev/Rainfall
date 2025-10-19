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

public class FirstPersonAction
{
	public readonly string type;

	public int hand;

	public string[] animationName = new string[3];
	public Model[] animationSet = new Model[3];
	public bool mirrorAnimation = false;
	public bool fullBodyAnimation = false;
	public bool animateCameraRotation = false;
	public bool rootMotion = false;
	public float animationTransitionDuration = 0.1f;
	public float followUpCancelTime = 100.0f;
	public float animationSpeed = 1.0f;

	public bool[] overrideWeaponModel = new bool[2];
	public Model[] weaponModel = new Model[2];
	public bool[] flipHandModels = new bool[2];
	public string[] handItemAnimations = new string[2];

	public float movementSpeedMultiplier = 1.0f;
	public bool lockYaw = false;
	public bool ignorePitch = false;
	public bool lockCameraRotation = false;
	public bool lockMovement = false;
	//public Vector3 movementInput = Vector3.Zero;
	public bool inputLeft, inputRight, inputForward, inputBack;
	public float maxSpeed = 0.0f;
	public float viewmodelAim = Player.DEFAULT_VIEWMODEL_AIM;
	public float swayAmount = 1.0f;

	public float iframesStartTime = 0.0f;
	public float iframesEndTime = 0.0f;

	public float staminaCost = 0;
	public float staminaCostTime = 0.0f;

	public float manaCost = 0;
	public float manaCostTime = 0.0f;

	public long startTime = 0;
	public float elapsedTime = 0;
	public float duration = 0.0f;

	List<ActionSfx> soundEffects = new List<ActionSfx>();
	List<ActionEvent> events = new List<ActionEvent>();

	bool staminaConsumed = false;


	public FirstPersonAction(string type, int hand)
	{
		this.type = type;
		this.hand = hand;
	}

	protected void addSoundEffect(ActionSfx sfx)
	{
		soundEffects.Add(sfx);
	}

	protected void addActionEvent(int frame, Action<Player> callback)
	{
		events.Add(new ActionEvent() { callback = callback, time = frame / 24.0f, triggered = false });
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

			Vector3 sourcePosition = hand == 0 ? (player.rightWeapon != null ? player.rightWeaponTransform * player.rightWeapon.sfxSourcePosition : player.rightWeaponTransform.translation) : (player.leftWeapon != null ? player.leftWeaponTransform.translation * player.leftWeapon.sfxSourcePosition : player.leftWeaponTransform.translation);

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

	public void cancel()
	{
		duration = 0;
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
