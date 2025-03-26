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
	Armor,
	Ring,
}

public abstract class Item
{
	public static Sound equipLight = Resource.GetSound("sound/item/equip_light.ogg");
	public static Sound equipMedium = Resource.GetSound("sound/item/equip_medium.ogg");
	public static Sound equipHeavy = Resource.GetSound("sound/item/equip_heavy.ogg");
	public static Sound equipRing = Resource.GetSound("sound/item/equip_ring.ogg");
	public static Sound[] equipArmor = Resource.GetSounds("sound/item/equip_chainmail", 2);


	public ItemType type;
	public string name;
	public string displayName;
	public Model model;
	public Model moveset;
	public List<SceneFormat.ColliderData> colliders;

	public Vector3 sfxSourcePosition = Vector3.Zero;

	public Sound[] equipSound;

	public bool twoHanded = false;
	public float viewmodelAim = Player.DEFAULT_VIEWMODEL_AIM;

	public bool useTrigger = true;
	public bool secondaryUseTrigger = true;

	public bool hidesArms = false;
	public bool hidesHands = false;

	public int damage = 1;


	public Item(ItemType type, string name, string displayName)
	{
		this.type = type;
		this.name = name;
		this.displayName = displayName;

		string typeStr = type.ToString().ToLower();

		if (SceneFormat.Read($"item/{typeStr}/{name}/{name}.rfs", out List<SceneFormat.EntityData> entities, out _))
		{
			SceneFormat.EntityData entity = entities[0];
			model = entity.model;
			moveset = Resource.GetModel($"item/{typeStr}/{name}/{name}_moveset.gltf");

			colliders = entity.colliders;
		}
	}

	public virtual void use(Player player, int hand)
	{
	}

	public virtual void useCharged(Player player, int hand)
	{
	}

	public virtual void useSecondary(Player player, int hand)
	{
	}

	public virtual void update(Player player, Animator animator)
	{
	}

	public virtual void draw(Player player, Animator animator)
	{
	}
}
