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

	FontData fontData;
	Font promptFont, xpFont, notificationFont, stackSizeFont;
	Font victoryFont;

	Texture crosshair;
	Texture crosshairHand;

	Texture minimap;
	uint[] minimapPixels;

	long lastHit = 0;

	public float fadeout = 1.0f;

	List<ItemCollectedNotification> collectedItems = new List<ItemCollectedNotification>();


	public HUD(Player player, GraphicsDevice graphics)
	{
		this.player = player;
		this.graphics = graphics;

		fontData = Resource.GetFontData("res/fonts/libre-baskerville.regular.ttf");
		promptFont = fontData.createFont(28.0f, true);
		xpFont = fontData.createFont(20.0f, true);
		notificationFont = fontData.createFont(18.0f, true);
		stackSizeFont = fontData.createFont(20.0f, true);
		victoryFont = fontData.createFont(40, true);

		crosshair = Resource.GetTexture("res/texture/ui/crosshair.png");
		crosshairHand = Resource.GetTexture("res/texture/ui/crosshair_hand.png");
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
		int width = player.stats.maxMana * 2;
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

		if (item != null && item.category == ItemCategory.Weapon && item.weaponType == WeaponType.Bow)
		{
			int numArrows = player.inventory.totalArrowCount;

			int yy = Display.viewportSize.y - 25 - height - 25 - height;
			renderItemSlot(x, yy, width, height, numArrows > 0 ? Item.Get("arrow") : null, numArrows);


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
	}

	void renderEquipment()
	{
		// Left item
		{
			int width = 64;
			int height = 64;
			int x = 40;
			int y = Display.viewportSize.y - 40 - height;

			renderHandItem(x, y, width, height, 1);
		}

		// Right item
		{
			int width = 64;
			int height = 64;
			int x = 40 + width + 10;
			int y = Display.viewportSize.y - 40 - height;

			renderHandItem(x, y, width, height, 0);
		}

		// Quick Slot
		{
			int width = 64;
			int height = 64;
			int x = 40 + width + 10 + width + 40;
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
		Level level = DungeonGame.instance.level;

		if (minimap == null)
		{
			minimap = graphics.createTexture(level.tilemap.mapSize.x, level.tilemap.mapSize.z, TextureFormat.BGRA8, (uint)SamplerFlags.Point | (uint)SamplerFlags.Clamp);
			minimapPixels = new uint[minimap.width * minimap.height];
		}

		level.tilemap.getRelativeTilePosition(player.position / LevelGenerator.TILE_SIZE, out Vector3i playerPos);
		for (int z = 0; z < level.tilemap.mapSize.z; z++)
		{
			for (int x = 0; x < level.tilemap.mapSize.x; x++)
			{
				int y = 0; // MathHelper.Clamp(playerPos.y, 0, level.tilemap.mapSize.y);
				int tile = level.tilemap.getTile(x + level.tilemap.mapPosition.x, y + level.tilemap.mapPosition.y, z + level.tilemap.mapPosition.z);
				uint color = 0xFF000000;
				if (x == playerPos.x && z == playerPos.z)
					color = 0xFF77FFFF;
				else if (tile != 0)
				{
					if (tile == 0xFFFF)
					{
						color = 0xFF444444;
					}
					else
					{
						RoomType type = RoomType.Get(tile);
						if (type != null)
						{
							if (type.sectorType == SectorType.Room)
								color = 0xFFFF0000;
							else
								color = 0xFF777777;
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
			foreach (Doorway doorway in room.doorways)
			{
				int x = doorway.globalPosition.x - level.tilemap.mapPosition.x;
				int z = doorway.globalPosition.z - level.tilemap.mapPosition.z;
				if (x >= 0 && x < level.tilemap.mapSize.x && z >= 0 && z < level.tilemap.mapSize.z)
				{
					uint color = 0xFF00FF00;
					minimapPixels[x + z * level.tilemap.mapSize.x] = color;
				}
			}
		}

		graphics.setTextureData(minimap, 0, 0, minimap.width, minimap.height, minimapPixels);

		int scale = 2;
		int xx = Display.viewportSize.x - minimap.width * scale - 150;
		int yy = 50;
		Renderer.DrawUITexture(xx, yy, minimap.width * scale, minimap.height * scale, minimap);
	}

	public void draw(GraphicsDevice graphics)
	{
		//renderPrompt();
		renderCrosshair();
		renderHealthBar();
		renderStaminaBar();
		renderManaBar();
		renderEquipment();
		//renderXP();
		renderCollectedItems();
		renderMinimap();

		if (player.hasWon)
		{
			string text = "V I C T O R Y";
			int width = victoryFont.measureText(text);
			Renderer.DrawText(Display.viewportSize.x / 2 - width / 2, Display.viewportSize.y / 2 - 10, 1.0f, text, victoryFont, 0xFFCCAA66);
		}

		float timeSinceHit = (Time.currentTime - lastHit) / 1e9f;
		float vignetteHitEffect = MathF.Exp(-timeSinceHit * 6.0f);
		Renderer.vignetteColor = Vector3.Lerp(Vector3.Zero, new Vector3(1.0f, 0.0f, 0.0f), vignetteHitEffect);

		Renderer.vignetteFalloff = MathHelper.Lerp(50.0f, 0.37f, fadeout);
	}
}
