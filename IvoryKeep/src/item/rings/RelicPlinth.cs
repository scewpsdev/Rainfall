using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RelicPlinth : Entity, Interactable
{
	Sprite sprite;
	uint outline;

	bool consumed = false;
	bool open = false;
	Player player;

	List<Item> relics;
	int selected;

	Sound sound;
	uint source;


	public RelicPlinth(float value, Random random)
	{
		sprite = new Sprite(tileset, 0, 6);
		collider = new FloatRect(-0.25f, 0.0f, 0.5f, 7 / 16.0f);
		filterGroup = FILTER_DEFAULT | FILTER_OBJECT;
		platformCollider = true;

		generateItems(value, random);

		sound = Resource.GetSound("sounds/plinth.ogg");
	}

	public RelicPlinth()
		: this(25, Random.Shared)
	{
	}

	public override void init(Level level)
	{
		level.addEntityCollider(this);

		source = Audio.Play(sound, new Vector3(position, 0));
		Audio.SetPaused(source, true);
		Audio.SetSourceLooping(source, true);
	}

	public override void destroy()
	{
		level.removeEntityCollider(this);

		if (source != 0)
		{
			Audio.FadeoutSource(source, 1);
			source = 0;
		}
	}

	public override void onLevelSwitch(Level newLevel)
	{
		Audio.SetPaused(source, newLevel != level);
	}

	public void generateItems(float value, Random random)
	{
		relics = new List<Item>();
		for (int i = 0; i < 3; i++)
		{
			Item relic = null;
			do
			{
				relic = Item.CreateRandom(ItemType.Relic, random, value);
			} while (player.hasItemOfType(relic.name) || !relics.TrueForAll((Item item) =>
			{
				return item.name != relic.name;
			}));
			relics.Add(relic);
		}
	}

	public bool canInteract(Player player)
	{
		return !consumed;
	}

	public float getRange()
	{
		return 2;
	}

	public void interact(Player player)
	{
		this.player = player;
		openScreen();
	}

	public void onFocusEnter(Player player)
	{
		outline = OUTLINE_COLOR;
	}

	public void onFocusLeft(Player player)
	{
		outline = 0;
	}

	void openScreen()
	{
		open = true;
		player.numOverlaysOpen++;
	}

	void closeScreen()
	{
		open = false;
		player.numOverlaysOpen--;
	}

	public override void update()
	{
		float maxDistance = getRange();
		if (open && (InputManager.IsPressed("UIQuit") || (player.center - position).lengthSquared > maxDistance * maxDistance))
		{
			closeScreen();
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, 1, 1, sprite);
		if (outline != 0)
			Renderer.DrawOutline(position.x - 0.5f, position.y, 1, 1, sprite, false, outline);

		if (relics != null && !consumed)
		{
			for (int i = 0; i < relics.Count; i++)
			{
				Vector2 anim = new Vector2((i * 2 % 3 - 1) * 0.3f, i / 2 * 0.3f + MathF.Sin(Time.gameTime * 0.25f + i * 12345) * 0.3f);
				float rotation = Time.gameTime * 0.1f + i * 12345;
				float w = relics[i].size.x * 0.5f;
				float h = relics[i].size.y * 0.5f;
				Renderer.DrawSprite(position.x - w * 0.5f + anim.x, position.y + 1.0f - h * 0.5f + anim.y, LAYER_DEFAULT, w, h, rotation, relics[i].sprite, false, relics[i].spriteColor);
			}
		}

		if (open)
		{
			Vector2 menuAnchor = GameState.instance.camera.worldToScreen(position + new Vector2(0, 2));
			int choice = ItemSelector.Render(menuAnchor, "Select relic", relics, null, 0, null, true, null, false, out bool secondary, out bool closed, ref selected);
			if (choice != -1)
			{
				player.giveItem(relics[choice]);
				relics.Clear();
				consumed = true;
				closeScreen();
			}
			else if (closed)
			{
				closeScreen();
			}
		}
	}
}
