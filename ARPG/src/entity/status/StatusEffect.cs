using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class StatusEffect
{
	public static SpriteSheet tileset;

	static StatusEffect()
	{
		tileset = new SpriteSheet(Resource.GetTexture("sprites/status.png", false), 8, 8);
	}


	public string name;
	public Sprite icon;
	public uint iconColor = 0xFFFFFFFF;
	public bool positiveEffect = true;


	public StatusEffect(string name, Sprite icon)
	{
		this.name = name;
		this.icon = icon;
	}

	public virtual void init(Entity entity)
	{
	}

	public virtual void destroy(Entity entity)
	{
	}

	public virtual bool update(Entity entity)
	{
		return true;
	}

	public virtual void render(Entity entity)
	{
	}

	public abstract float getProgress();
}
