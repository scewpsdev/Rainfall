using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class PotionEffect
{
	public string name;
	public int value;
	public Sprite sprite;
	public uint color;

	public PotionEffect(string name, int value, Sprite sprite, uint color)
	{
		this.name = name;
		this.value = value;
		this.sprite = sprite;
		this.color = color;
	}

	public abstract void apply(Entity entity, Potion potion);
}
