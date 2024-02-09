using Rainfall;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;


struct ItemCollectedNotification
{
	public Item item;
	public int amount;
	public long timeCollected;
}

public class HUD
{
	Player player;
	GraphicsDevice graphics;

	Texture crosshair;
	Texture crosshairHand;

	Texture minimap;
	uint[] minimapPixels;

	Font stackSizeFont, xpFont, notificationFont;

	long lastHit = 0;

	public float fadeout = 1.0f;

	List<ItemCollectedNotification> collectedItems = new List<ItemCollectedNotification>();


	public HUD(Player player, GraphicsDevice graphics)
	{
		this.player = player;
		this.graphics = graphics;

		crosshair = Resource.GetTexture("res/texture/ui/crosshair.png");
		crosshairHand = Resource.GetTexture("res/texture/ui/crosshair_hand.png");

		stackSizeFont = FontManager.GetFont("baskerville", 20.0f, true);
		xpFont = FontManager.GetFont("baskerville", 20.0f, true);
		notificationFont = FontManager.GetFont("baskerville", 18.0f, true);
	}

	public void init(Level level)
	{
		minimap = graphics.createTexture(level.tilemap.mapSize.x, level.tilemap.mapSize.z, TextureFormat.BGRA8, (uint)SamplerFlags.Point | (uint)SamplerFlags.Clamp);
		minimapPixels = new uint[minimap.width * minimap.height];
	}

	public void destroy()
	{
		graphics.destroyTexture(minimap);
	}

	public void onHit()
	{
		lastHit = Time.currentTime;
	}

	public void onItemCollected(Item item, int amount, long timeCollected)
	{
		for (int i = 0; i < collectedItems.Count; i++)
		{
			ItemCollectedNotification notif = collectedItems[i];
			if (notif.item == item)
			{
				notif.amount += amount;
				notif.timeCollected = timeCollected;
				collectedItems[i] = notif;
				return;
			}
		}

		collectedItems.Add(new ItemCollectedNotification() { item = item, amount = amount, timeCollected = timeCollected });
	}

	void renderCrosshair()
	{
		if (player.interactableInFocus != null)
		{
			int width = crosshairHand.info.width / 2;
			int height = crosshairHand.info.height / 2;
			Renderer.DrawUITexture(Display.viewportSize.x / 2 - width / 2, Display.viewportSize.y / 2 - height / 2, width, height, crosshairHand);
		}
		else
		{
			int width = crosshair.info.width;
			int height = crosshair.info.height;
			Renderer.DrawUITexture(Display.viewportSize.x / 2 - width / 2, Display.viewportSize.y / 2 - height / 2, width, height, crosshair);
		}
	}

	void renderHealthBar()
	{
		int x = 40;
		int y = 40;
		int width = player.stats.maxHealth * 2;
		int height = 10;
		int padding = 2;

		Renderer.DrawUIRect(x - padding, y - padding, width + 2 * padding, height + 2 * padding, 0xFF444444);
		//Renderer.DrawUIRect(x - padding / 2, y - padding / 2, width + padding, height + padding, 0xff222222);

		Renderer.DrawUIRect(x, y, width, height, 0xff331111);
		Renderer.DrawUIRect(x, y, (int)((float)player.stats.health / player.stats.maxHealth * width), height, 0xffcc3333);
	}

	void renderStaminaBar()
	{
		int x = 40;
		int y = 60;
		int width = (int)(player.stats.maxStamina * 10.0f);
		int height = 8;
		int padding = 2;

		Renderer.DrawUIRect(x - padding, y - padding, width + 2 * padding, height + 2 * padding, 0xFF444444);
		//Renderer.DrawUIRect(x - padding / 2, y - padding / 2, width + padding, height + padding, 0xff222222);

		Renderer.DrawUIRect(x, y, width, height, 0xff1c241d);
		Renderer.DrawUIRect(x, y, (int)(player.stats.stamina / player.stats.maxStamina * width), height, 0xff478749);
	}

	void renderManaBar()
	{
		int x = 40;
		int y = 80;
		int width = player.stats.maxMana;
		int height = 10;
		int padding = 2;

		Renderer.DrawUIRect(x - padding, y - padding, width + 2 * padding, height + 2 * padding, 0xFF444444);
		//Renderer.DrawUIRect(x - padding / 2, y - padding / 2, width + padding, height + padding, 0xff222222);

		Renderer.DrawUIRect(x, y, width, height, 0xff1C1D24);
		Renderer.DrawUIRect(x, y, (int)((float)player.stats.mana / player.stats.maxMana * width), height, 0xff7780f7);
	}

	void renderItemSlot(int x, int y, int width, int height, Item item, int stackSize)
	{
		Renderer.DrawUIRect(x, y, width, height, 0xff111111);

		if (item != null)
		{
			Renderer.DrawUITexture(x, y, width, height, item.icon);

			if (item.stackable)
			{
				string stackSizeText = stackSize.ToString();
				Renderer.DrawText(x + width - stackSizeFont.measureText(stackSizeText) - 8, y + height - (int)stackSizeFont.size, 1.0f, stackSizeText, stackSizeFont, 0xffaaaaaa);
			}
		}
	}

	void renderHandItem(int x, int y, int width, int height, int handID)
	{
		//Renderer.DrawUIRect(x, y, width, height, 0xff111111);

		ItemSlot slot = player.inventory.getSelectedHandSlot(handID);
		Item item = player.inventory.getSelectedHandItem(handID);

		renderItemSlot(x, y, width, height, item, slot != null ? slot.stackSize : 0);

		/*
			Texture icon = item.icon;
			Renderer.DrawUITexture(x, y, width, height, icon);
		*/

		if (item != null)
		{
			if (item.category == ItemCategory.Weapon && item.weaponType == WeaponType.Bow)
			{
				int numArrows = player.inventory.totalArrowCount;

				int ww = 64;
				int hh = 64;

				int yy = Display.viewportSize.y - 25 - height - 25 - hh;
				renderItemSlot(x, yy, ww, hh, numArrows > 0 ? Item.Get("arrow") : null, numArrows);


				/*
				Renderer.DrawUIRect(x, yy, width, height, 0xff111111);

				if (numArrows > 0)
				{
					Texture arrowIcon = player.inventory.arrows[0].item.icon;
					Renderer.DrawUITexture(x, yy, width, height, arrowIcon);

					string stackSizeText = numArrows.ToString();
					Renderer.DrawText(x + width - stackSizeFont.measureText(stackSizeText) - 8, yy + height - (int)stackSizeFont.size, 1.0f, stackSizeText, stackSizeFont, 0xffaaaaaa);
				}
				*/
			}
			else if (item.category == ItemCategory.Weapon && item.weaponType == WeaponType.Staff)
			{
				ItemSlot spellItem = player.inventory.getSpellSlot(null);

				int ww = 64;
				int hh = 64;

				int yy = Display.viewportSize.y - 25 - height - 25 - hh;
				renderItemSlot(x, yy, ww, hh, spellItem?.item, 1);
			}
		}
	}

	void renderEquipment()
	{
		// Left item
		{
			int width = 96;
			int height = 96;
			int x = Display.width - 40 - width - 10 - width;
			int y = Display.viewportSize.y - 40 - height;

			renderHandItem(x, y, width, height, 1);
		}

		// Right item
		{
			int width = 96;
			int height = 96;
			int x = Display.width - 40 - width;
			int y = Display.viewportSize.y - 40 - height;

			renderHandItem(x, y, width, height, 0);
		}

		// Quick Slot
		{
			int width = 64;
			int height = 64;
			int x = 40;
			int y = Display.viewportSize.y - 40 - height;

			Item item = player.inventory.getCurrentQuickSlotItem();
			ItemSlot slot = player.inventory.getCurrentQuickSlot();
			if (slot != null)
				renderItemSlot(x, y, width, height, item, slot.stackSize);
			else
				renderItemSlot(x, y, width, height, item, 0);

			/*
			Renderer.DrawUIRect(x, y, width, height, 0xff111111);

			if (player.inventory.getQuickSlot(0) != null)
			{
				Texture icon = player.inventory.getQuickSlot(0).item.icon;
				Renderer.DrawUITexture(x, y, width, height, icon);
			}
			*/
		}
	}

	void renderXP()
	{
		int x = Display.viewportSize.x - 220;
		int y = Display.viewportSize.y - 80;
		int width = 140;
		int height = 28;
		int padding = 2;

		Renderer.DrawUIRect(x - padding, y - padding, width + 2 * padding, height + 2 * padding, 0xff888888);
		Renderer.DrawUIRect(x, y, width, height, 0xff222222);

		string text = player.stats.xp.ToString();
		Renderer.DrawText(x + width - xpFont.measureText(text) - 3 * padding, y + (int)((height - xpFont.size) / 2.0f), 1.0f, text, xpFont, 0xffcccccc);
	}

	void renderCollectedItems()
	{
		float windowDuration = 4.0f;

		for (int i = 0; i < collectedItems.Count; i++)
		{
			ItemCollectedNotification notif = collectedItems[i];

			float elapsed = (Time.currentTime - notif.timeCollected) / 1e9f;
			if (elapsed >= windowDuration)
			{
				collectedItems.RemoveAt(i);
				i--;
			}
		}

		for (int i = 0; i < collectedItems.Count; i++)
		{
			ItemCollectedNotification notif = collectedItems[i];

			Item item = notif.item;

			int padding = 3;
			int iconSize = 48;

			int windowSpacing = 20;
			int width = 240;
			int height = iconSize + 2 * padding;
			int x = 10;
			int y = Display.viewportSize.y - 160 + (-collectedItems.Count + i) * (height + windowSpacing);

			Renderer.DrawUIRect(x, y, width, height, 0xff111111);
			{
				Renderer.DrawUITexture(x + padding, y + padding, iconSize, iconSize, item.icon);

				if (notif.amount > 1 || notif.item.stackable)
					Renderer.DrawText(x + padding + iconSize - padding * 3, y + padding + iconSize - (int)stackSizeFont.size, 1.0f, notif.amount.ToString(), stackSizeFont, 0xffaaaaaa);

				Renderer.DrawText(x + padding + iconSize + padding * 5, y + padding * 2, 1.0f, item.displayName, notificationFont, 0xffaaaaaa);
				Renderer.DrawText(x + padding + iconSize + padding * 5, y + padding + iconSize - padding - (int)notificationFont.size, 1.0f, item.typeSpecifier, notificationFont, 0xff777777);
			}
		}
	}

	void renderMinimap()
	{
		const uint BACKGROUND_COLOR = 0xFF000000; // 0xFFD3B48B;
		const uint PLAYER_COLOR = 0xFFFF0000;
		const uint ROOM_COLOR = 0xFF685A49;
		const uint CORRIDOR_COLOR = 0xFF685A49;
		const uint CORRIDOR_ASTAR_COLOR = 0xFF685A49;

		Level level = DungeonGame.instance.level;

		int playerY = (int)MathF.Floor(player.position.y - level.tilemap.mapPosition.y + 1.5f);// MathHelper.Clamp(playerPos.y, 0, level.tilemap.mapSize.y);

		level.tilemap.getRelativeTilePosition(player.position / LevelGenerator.TILE_SIZE, out Vector3i playerPos);
		for (int z = 0; z < level.tilemap.mapSize.z; z++)
		{
			for (int x = 0; x < level.tilemap.mapSize.x; x++)
			{
				int tile = level.tilemap.getTile(x + level.tilemap.mapPosition.x, playerY + level.tilemap.mapPosition.y, z + level.tilemap.mapPosition.z);
				bool roomExplored = DungeonGame.instance.gameManager.exploredRooms.Contains(tile);
				uint color = BACKGROUND_COLOR;

				if (x == playerPos.x && z == playerPos.z)
					color = PLAYER_COLOR;
				else if (tile != 0 && roomExplored)
				{
					if (tile / 100 == 0xFF) // if astar corridor
					{
						color = CORRIDOR_ASTAR_COLOR;
					}
					else
					{
						RoomType type = RoomType.Get(tile / 100);
						if (type != null)
						{
							if (type.sectorType == SectorType.Room)
								color = ROOM_COLOR;
							else
								color = CORRIDOR_COLOR;
						}
						else
						{
							Debug.Assert(false);
						}
					}
				}

				minimapPixels[x + z * level.tilemap.mapSize.x] = color;
			}
		}
		foreach (Room room in level.rooms)
		{
			bool roomExplored = DungeonGame.instance.gameManager.exploredRooms.Contains(room.id);
			if (roomExplored)
			{
				foreach (Doorway doorway in room.doorways)
				{
					int x = doorway.globalPosition.x - level.tilemap.mapPosition.x;
					int z = doorway.globalPosition.z - level.tilemap.mapPosition.z;
					int y = doorway.globalPosition.y - level.tilemap.mapPosition.y;
					if (x >= 0 && x < level.tilemap.mapSize.x && z >= 0 && z < level.tilemap.mapSize.z && y > playerY - 3 && y < playerY + 3)
					{
						uint color = 0xFF00FF00;
						minimapPixels[x + z * level.tilemap.mapSize.x] = color;
					}
				}
			}
		}

		graphics.setTextureData(minimap, 0, 0, minimap.width, minimap.height, minimapPixels);

		int width = 96;
		int height = 96;
		int scale = 2;
		int xx = Display.viewportSize.x - width * scale - 50;
		int yy = 50;
		int u0 = playerPos.x - width / 2;
		int u1 = playerPos.x + width / 2;
		int v0 = playerPos.z - height / 2;
		int v1 = playerPos.z + height / 2;
		if (u0 < 0)
		{
			u1 -= u0;
			u0 = 0;
		}
		else if (u1 > minimap.width)
		{
			u0 -= u1 - minimap.width;
			u1 = minimap.width;
		}
		if (v0 < 0)
		{
			v1 -= v0;
			v0 = 0;
		}
		else if (v1 > minimap.height)
		{
			v0 -= v1 - minimap.height;
			v1 = minimap.height;
		}
		Renderer.DrawUITexture(xx, yy, width * scale, height * scale, minimap, u0, v0, u1, v1, 0xFFFFFFFF);
	}

	public void draw(GraphicsDevice graphics)
	{
		if (!GraphicsManager.cinematicMode)
		{
			//renderPrompt();
			renderCrosshair();
			renderHealthBar();
			renderStaminaBar();
			renderManaBar();
			renderEquipment();
			//renderXP();
			renderCollectedItems();
			if (DungeonGame.instance.gameManager.mapUnlocked)
				renderMinimap();
		}

		float timeSinceHit = (Time.currentTime - lastHit) / 1e9f;
		float vignetteHitEffect = MathF.Exp(-timeSinceHit * 6.0f);
		Renderer.vignetteColor = Vector3.Lerp(Vector3.Zero, new Vector3(1.0f, 0.0f, 0.0f), vignetteHitEffect);

		Renderer.vignetteFalloff = MathHelper.Lerp(50.0f, 0.37f, MathF.Pow(fadeout, 0.5f));
	}
}
