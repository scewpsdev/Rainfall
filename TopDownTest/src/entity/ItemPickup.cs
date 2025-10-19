using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ItemPickup : Pickup
{
	Item item;

	public ItemPickup(Item item, Vector2 position)
		: base(position)
	{
		this.item = item;
		sprite = new Sprite(itemSprites, 1, 0);
	}

	public override void getInteractionPrompt(Entity entity, out string text, out uint color)
	{
		base.getInteractionPrompt(entity, out text, out color);
		text = "[E] Pick up " + item.displayName;
	}

	protected override bool onPickup(Player player)
	{
		if (player.rightItem == null)
		{
			player.rightItem = item;
			return true;
		}
		return false;
	}
}
