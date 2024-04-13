using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Chest : Entity, Interactable
{
	Item[] items;
	int[] amounts;
	List<ItemPickup> pickups = new List<ItemPickup>();
	bool itemsSpawned = false;

	Model model;
	Animator animator;
	AnimationState openAnimation, closeAnimation;
	Node lidNode;

	RigidBody chestBody;
	RigidBody lidBody;

	AudioSource audio;
	Sound sfxOpen, sfxClose;
	bool soundPlayed = false;

	bool isOpen = false;


	public Chest(Item[] items, int[] amounts)
	{
		this.items = items;
		this.amounts = amounts;

		model = Resource.GetModel("res/entity/object/chest/chest.gltf");
		animator = new Animator(model);
		lidNode = model.skeleton.getNode("Chest_Lid");

		animator.setState(new AnimationState(model, "default"));
		openAnimation = new AnimationState(model, "open");
		closeAnimation = new AnimationState(model, "close");

		sfxOpen = Resource.GetSound("res/entity/object/chest/sfx/open.ogg");
		sfxClose = Resource.GetSound("res/entity/object/chest/sfx/close.ogg");
	}

	public Chest(params Item[] items)
		: this(items, null)
	{
	}

	public override void init()
	{
		chestBody = new RigidBody(this, RigidBodyType.Kinematic);
		chestBody.addBoxCollider(new Vector3(0.5f, 0.05f, 0.3f), new Vector3(0.0f, -0.05f, 0.0f), Quaternion.Identity, 0.0f);
		chestBody.addBoxCollider(new Vector3(0.5f, 0.25f, 0.05f), new Vector3(0.0f, 0.25f, 0.35f), Quaternion.Identity, 0.0f);
		chestBody.addBoxCollider(new Vector3(0.5f, 0.25f, 0.05f), new Vector3(0.0f, 0.25f, -0.35f), Quaternion.Identity, 0.0f);
		chestBody.addBoxCollider(new Vector3(0.05f, 0.25f, 0.3f), new Vector3(-0.55f, 0.25f, 0.0f), Quaternion.Identity, 0.0f);
		chestBody.addBoxCollider(new Vector3(0.05f, 0.25f, 0.3f), new Vector3(0.55f, 0.25f, 0.0f), Quaternion.Identity, 0.0f);
		chestBody.setTransform(position, rotation);

		lidBody = new RigidBody(this, RigidBodyType.Kinematic);
		lidBody.addBoxCollider(new Vector3(0.5f, 0.05f, 0.3f), new Vector3(0.0f, 0.55f, -0.3f), Quaternion.Identity, 0.0f);
		lidBody.setTransform(position, rotation);

		audio = Audio.CreateSource(position);
	}

	public void open()
	{
		animator.setStateIfNot(openAnimation);

		if (!itemsSpawned)
		{
			for (int i = 0; i < items.Length; i++)
			{
				int amount = amounts != null ? amounts[i] : 1;
				ItemPickup pickup = new ItemPickup(items[i], amount, this);
				Quaternion rotation = items[i].category != ItemCategory.Consumable ?
					Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.PiOver2) * Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.PiOver2) :
					Quaternion.Identity;
				level.addEntity(pickup, position + new Vector3(0.0f, 0.1f, i == 0 ? 0.0f : ((i - 1) % 2 - 0.5f) * ((i + 1) / 2) * 0.1f), rotation);
				pickups.Add(pickup);
			}
			itemsSpawned = true;
		}

		soundPlayed = false;

		isOpen = true;
	}

	public void close()
	{
		animator.setStateIfNot(closeAnimation);

		soundPlayed = false;

		isOpen = false;
	}

	public void removePickup(ItemPickup pickup)
	{
		pickups.Remove(pickup);
	}

	public bool canInteract()
	{
		return !isOpen || pickups.Count == 0;
	}

	public void interact(Entity by)
	{
		if (by is Player)
		{
			Player player = (Player)by;
			if (player.currentAction == null)
			{
				if (!isOpen)
				{
					open();
					player.queueAction(new ChestOpenAction(this));
				}
				else
				{
					close();
				}
			}
		}
	}

	public override void update()
	{
		animator.update();
		animator.applyAnimation();
		//model.applyAnimation(animator.nodeLocalTransforms);

		Matrix lidTransform = getModelMatrix() * Matrix.CreateTranslation(0.0f, 0.0f, 0.3f) * animator.getNodeTransform(lidNode, 0);
		lidBody.setTransform(lidTransform.translation, lidTransform.rotation);

		audio.updateTransform(position);


		if (animator.getState() == openAnimation && animator.timer >= 23 / 24.0f && !soundPlayed)
		{
			audio.playSound(sfxOpen);
			soundPlayed = true;
		}
		else if (animator.getState() == closeAnimation && animator.timer >= 8 / 24.0f && !soundPlayed)
		{
			audio.playSound(sfxClose);
			soundPlayed = true;
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix(), animator);
	}
}
