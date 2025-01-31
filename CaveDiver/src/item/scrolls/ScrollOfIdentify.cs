using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ScrollOfIdentify : Item
{
	int selectedItem = -1;

	public ScrollOfIdentify()
		: base("scroll_of_identify", ItemType.Scroll)
	{
		displayName = "Scroll of Identify";

		value = 20;
		canDrop = false;

		sprite = new Sprite(tileset, 11, 2);
	}

	public override bool use(Player player)
	{
		selectedItem = 0;
		return false;
	}

	public override void render(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;
			if (selectedItem != -1)
			{
				Vector2 pos = GameState.instance.camera.worldToScreen(entity.position + new Vector2(0, 1));
				List<Item> items = new List<Item>();
				for (int i = 0; i < player.items.Count; i++)
					items.Add(player.items[i]);
				int choice = ItemSelector.Render(pos, "Identify item", items, null, -1, player, true, null, false, out bool secondary, out bool closed, ref selectedItem);
				if (choice != -1)
				{
					Item item = items[choice];
					item.identify();
					player.hud.showMessage(item.displayName + " identified.");
					player.removeItemSingle(this);
				}
				if (closed)
					selectedItem = -1;

				player.level.addEntity(ParticleEffects.CreateScrollUseEffect(player), player.position + player.collider.center);
			}
		}
	}
}
