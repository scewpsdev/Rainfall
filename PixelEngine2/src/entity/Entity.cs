using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Entity
{
	public const uint FILTER_DEFAULT = 1 << 0;
	public const uint FILTER_PLAYER = 1 << 1;
	public const uint FILTER_MOB = 1 << 2;
	public const uint FILTER_ITEM = 1 << 3;
	public const uint FILTER_PROJECTILE = 1 << 4;
	public const uint FILTER_DECORATION = 1 << 5;

	public const float LAYER_DEFAULT = 0.0f;
	public const float LAYER_BGBG = 0.02f;
	public const float LAYER_BG = 0.01f;
	public const float LAYER_INTERACTABLE = -0.005f;
	public const float LAYER_PLAYER_BG = 0.002f;
	public const float LAYER_PLAYER_ARMOR = -0.0005f;
	public const float LAYER_PLAYER_ITEM_MAIN = -0.001f;
	public const float LAYER_PLAYER_ITEM_SECONDARY = 0.001f;
	public const float LAYER_FG = -0.02f;
	public const float LAYER_TILE = 0.5f;
	public const float LAYER_FGFG = -0.9f;

	public const uint OUTLINE_COLOR = 0xBFFFFFFF;

	public static readonly SpriteSheet effectsTileset;
	public static readonly SpriteSheet shadowsTileset;

	static Entity()
	{
		effectsTileset = new SpriteSheet(Resource.GetTexture("res/sprites/effects.png", false), 16, 16);
		shadowsTileset = new SpriteSheet(Resource.GetTexture("res/sprites/shadows.png", false), 16, 16);
	}


	public Vector2 position;
	public float z = 0.0f;
	public float rotation;
	public Vector2 velocity;
	public float zVelocity = 0.0f;
	public Level level;

	public bool removed { get; private set; } = false;
	public List<Action> removeCallbacks = new List<Action>();

	public FloatRect collider;
	public uint filterGroup = FILTER_DEFAULT;

	public string name;
	public string displayName;


	public virtual void init(Level level)
	{
	}

	public virtual void destroy()
	{
	}

	public virtual void update()
	{
	}

	public virtual void render()
	{
	}

	public virtual void onLevelSwitch(Level newLevel)
	{
	}

	public void remove()
	{
		removed = true;
	}

	public Matrix getTransform(Vector2 offset = default)
	{
		return Matrix.CreateTransform(new Vector3(position + offset, 0), Quaternion.FromAxisAngle(Vector3.UnitZ, rotation));
	}
}
