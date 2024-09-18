using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Potion : Item
{
	public List<PotionEffect> effects = new List<PotionEffect>();

	public Potion(string name)
		: base(name, ItemType.Potion)
	{
		displayName = "Mixed potion";
		value = 2;
		sprite = new Sprite(tileset, 4, 5);
	}

	public Potion()
		: this("mixed_potion")
	{
	}

	public void addEffect(PotionEffect effect)
	{
		if (effects.Count == 0)
			displayName += " of ";
		if (effects.Count > 0)
			displayName += ", ";
		displayName += effect.name;
		value += effect.value;
		sprite = effect.sprite;
		effects.Add(effect);
	}

	public override bool use(Player player)
	{
		foreach (PotionEffect effect in effects)
			effect.apply(player, this);
		player.removeItemSingle(this);
		player.giveItem(new GlassBottle());
		return false;
	}
}
