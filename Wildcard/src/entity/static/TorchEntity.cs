using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TorchEntity : Entity, Interactable
{
	Sprite sprite;
	uint outline = 0;

	ParticleEffect particles;
	Simplex simplex;
	float oscillateFreq = 1.0f;
	float oscillateAmplitude = 3.0f;

	Sound idleSound;
	uint source;


	public TorchEntity()
	{
		sprite = new Sprite(tileset, 1, 3);

		idleSound = Resource.GetSound("sounds/torch.ogg");

		simplex = new Simplex((uint)Time.currentTime, 3);
	}

	public override void init(Level level)
	{
		level.addEntity(particles = ParticleEffects.CreateTorchEffect(this), position + new Vector2(0, 0.25f));
		particles.layer = LAYER_DEFAULT - 0.01f;

		source = Audio.Play(idleSound, new Vector3(position, 0));
		Audio.SetPaused(source, true);
		Audio.SetSourceLooping(source, true);
	}

	public override void destroy()
	{
		if (source != 0)
		{
			Audio.FadeoutSource(source, 1);
			source = 0;
		}

		particles.remove();
	}

	public bool canInteract(Player player)
	{
		return player.handItem == null || !player.handItem.twoHanded && player.offhandItem == null;
	}

	public void interact(Player player)
	{
		player.giveItem(new Torch());
		remove();
	}

	public void onFocusEnter(Player player)
	{
		outline = 0x7FFFFFFF;
	}

	public void onFocusLeft(Player player)
	{
		outline = 0;
	}

	public override void onLevelSwitch(Level newLevel)
	{
		Audio.SetPaused(source, newLevel != level);
	}

	public override unsafe void update()
	{
		float noise = simplex.sample1f(Time.currentTime / 1e9f * oscillateFreq) * 0.5f + 0.5f;
		noise = MathF.Pow(noise, 3);
		float value = 0.2f + noise * oscillateAmplitude;
		particles.systems[0].handle->emissionRate = 40 * value;
		particles.systems[1].handle->emissionRate = 10 * value;
		particles.systems[2].handle->emissionRate = 0.4f * value;
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 1, 1, sprite, false);

		if (outline != 0)
			Renderer.DrawOutline(position.x - 0.5f, position.y - 0.5f, 1, 1, sprite, false, outline);

		Renderer.DrawLight(position, new Vector3(1.0f, 0.9f, 0.7f) * 2, 9);
	}
}
