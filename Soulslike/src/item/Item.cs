using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum ItemType
{
	None = 0,

	Weapon,
	Staff,
	Crossbow,
}

public abstract class Item
{
	public ItemType type;
	public string name;
	public string displayName;
	public Model model;
	public Model moveset;
	public List<SceneFormat.ColliderData> colliders;

	public bool twoHanded = false;
	public float viewmodelAim = Player.DEFAULT_VIEWMODEL_AIM;

	public bool useTrigger = true;
	public bool secondaryUseTrigger = true;


	public Item(ItemType type, string name, string displayName)
	{
		this.type = type;
		this.name = name;
		this.displayName = displayName;

		if (SceneFormat.Read($"item/{name}/{name}.rfs", out List<SceneFormat.EntityData> entities, out _))
		{
			SceneFormat.EntityData entity = entities[0];
			model = entity.model;
			moveset = Resource.GetModel($"item/{name}/{name}_moveset.gltf");

			colliders = entity.colliders;
		}
	}

	public virtual void use(Player player)
	{
	}

	public virtual void useSecondary(Player player)
	{
	}

	public virtual void update(Player player, Animator animator)
	{
	}
}
