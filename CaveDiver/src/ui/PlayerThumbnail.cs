using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class PlayerThumbnail
{
	public static void Render(int x, int y, int width, int height)
	{
		Player player = GameState.instance.player;

		int size = 16;
		int xx = x;
		int yy = y + size * 3 / 4;
		Renderer.DrawUISprite(x, y, width, height, null, false, 0xFF050505);

		if (player.offhandItem != null)
		{
			int w = (int)MathF.Round(player.offhandItem.size.x * size);
			int h = (int)MathF.Round(player.offhandItem.size.y * size);
			Renderer.DrawUISprite(xx + width / 2 - w / 2 + (int)(player.getWeaponOrigin(false).x * size + player.offhandItem.renderOffset.x * size), yy + size / 2 - (h - size) - (int)(player.getWeaponOrigin(false).y * size + player.offhandItem.renderOffset.y * size), w, h, player.offhandItem.sprite);
		}

		player.animator.setAnimation("idle");
		player.animator.update(player.sprite);
		Renderer.DrawUISprite(xx + width / 2 - player.sprite.size.x / 2, yy - 16, size * 2, size * 2, player.sprite);

		for (int i = 0; i < player.passiveItems.Count; i++)
		{
			if (player.passiveItems[i] != null && player.passiveItems[i].ingameSprite != null)
			{
				int ss = size * player.passiveItems[i].ingameSpriteSize;
				player.animator.update(player.passiveItems[i].ingameSprite);
				player.passiveItems[i].ingameSprite.position *= player.passiveItems[i].ingameSpriteSize;
				Renderer.DrawUISprite(xx + width / 2 - ss / 2 - 8, yy - (ss - size) / 2 - 8, ss * 2, ss * 2, player.passiveItems[i].ingameSprite, false, MathHelper.VectorToARGB(player.passiveItems[i].ingameSpriteColor));
			}
		}

		if (player.handItem != null)
		{
			int w = (int)MathF.Round(player.handItem.size.x * size);
			int h = (int)MathF.Round(player.handItem.size.y * size);
			Renderer.DrawUISprite(xx + width / 2 - w / 2 + (int)(player.getWeaponOrigin(true).x * size + player.handItem.renderOffset.x * size), yy + size / 2 - (h - size) - (int)(player.getWeaponOrigin(true).y * size + player.handItem.renderOffset.y * size), w, h, player.handItem.sprite);
		}
	}
}
