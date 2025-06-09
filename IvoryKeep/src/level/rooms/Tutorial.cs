using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class TutorialEntranceDoor : Door
{
	public TutorialEntranceDoor(Level destination, Door otherDoor = null, bool big = false, float layer = 0)
		: base(destination, otherDoor, big, layer)
	{
		sprite = new Sprite(tileset, 6, 9, 3, 2);
		rect = new FloatRect(-1.5f, 0.0f, 3.0f, 2.0f);

		collider = new FloatRect(-1.5f, 0.0f, 3, 2);
	}
}

public class TutorialExitDoor : Door
{
	Sprite layer1, layer2;

	public TutorialExitDoor(Level destination, Door otherDoor = null, bool big = false, float layer = 0)
		: base(destination, otherDoor, big, layer)
	{
		sprite = new Sprite(tileset, 9, 9, 3, 4);
		rect = new FloatRect(-1.5f, -1, 3.0f, 4.0f);

		collider = new FloatRect(-1.5f, 0.0f, 3, 2);

		layer1 = new Sprite(tileset, 9, 13, 3, 4);
		layer2 = new Sprite(tileset, 9, 17, 3, 4);
	}

	public override void render()
	{
		base.render();

		Vector3 vertex = ParallaxObject.ParallaxEffect(position, 0.1f);
		Renderer.DrawSprite(vertex.x + rect.position.x, vertex.y + rect.position.y, vertex.z, rect.size.x, rect.size.y, 0, layer1, false, 0xFFFFFFFF);

		vertex = ParallaxObject.ParallaxEffect(position, 0.2f);
		Renderer.DrawSprite(vertex.x + rect.position.x, vertex.y + rect.position.y, vertex.z, rect.size.x, rect.size.y, 0, layer2, false, new Vector4(3, 3, 3, 1));

		//Renderer.DrawSprite(position.x + 1.5f, position.y, LAYER_BG, 10, 10, 0, null, false, new Vector4(0, 0, 0, 1));
		//Renderer.DrawSprite(position.x - 1.5f - 10, position.y, LAYER_BG, 10, 10, 0, null, false, new Vector4(0, 0, 0, 1));
		//Renderer.DrawSprite(position.x - 1.5f, position.y + 3, LAYER_BG, 10, 10, 0, null, false, new Vector4(0, 0, 0, 1));

		Renderer.DrawLight(position + new Vector2(0, 1), new Vector3(0.6f, 0.8f, 1.0f) * 4, 14);
	}
}


public class Tutorial : Entity
{
	Room room;

	Sound caveAmbience;


	public Tutorial(Room room)
	{
		this.room = room;

		caveAmbience = Resource.GetSound("sounds/ambience.ogg");
	}

	public override void init(Level level)
	{
		//tutorial.addEntity(tutorialExit, (Vector2)tutorial.rooms[0].getMarker(01));

		//level.addEntity(new TutorialText(InputManager.GetBinding("Up").ToString() + InputManager.GetBinding("Left").ToString() + InputManager.GetBinding("Down").ToString() + InputManager.GetBinding("Right").ToString() + " to move", 0xFFFFFFFF), new Vector2(10, level.height - 8));
		level.addEntity(new TutorialText(InputManager.GetBinding("Jump").ToString() + " to jump", 0xFFFFFFFF), new Vector2(33, 12));
		level.addEntity(new TutorialText("Hold to jump higher", 0xFFFFFFFF), new Vector2(38, 15));
		level.addEntity(new TutorialText(InputManager.GetBinding("Up").ToString() + " to climb", 0xFFFFFFFF), new Vector2(50, 15));
		level.addEntity(new TutorialText(InputManager.GetBinding("Down").ToString() + " to drop", 0xFFFFFFFF), new Vector2(52, 45));
		level.addEntity(new TutorialText(InputManager.GetBinding("Sprint").ToString() + " to dodge", 0xFFFFFFFF), (Vector2)level.rooms[0].getMarker(01));

		level.addEntity(new TutorialText("Hug wall to wall jump", 0xFFFFFFFF), (Vector2)level.rooms[0].getMarker(02));

		level.addEntity(new ItemEntity(new Club()), (Vector2)level.rooms[0].getMarker(03) + Vector2.Up);
		level.addEntity(new ItemEntity(new IronShield()), (Vector2)level.rooms[0].getMarker(04) + Vector2.Up);
		level.addEntity(new ItemEntity(new PotionOfHealing()), (Vector2)level.rooms[0].getMarker(04) + Vector2.Up + Vector2.Right);
		//level.addEntity(new Chest(new TravellingCloak()), (Vector2)level.rooms[0].getMarker(05));
		level.addEntity(new Chest(new TutorialBomb()), (Vector2)level.rooms[0].getMarker(10) + new Vector2(0.5f));
		level.addEntity(new TorchEntity(), (Vector2)level.rooms[0].getMarker(11) + new Vector2(0.5f));
		level.addEntity(new TutorialText(InputManager.GetBinding("Interact").ToString() + " to interact", 0xFFFFFFFF), new Vector2(40, 43.5f));
		level.addEntity(new TutorialText("Down+" + InputManager.GetBinding("Interact").ToString() + " to carry objects", 0xFFFFFFFF), new Vector2(40, 43.0f));
		//level.addEntity(new TutorialText(InputManager.GetBinding("Inventory").ToString() + " to open inventory", 0xFFFFFFFF), new Vector2(40, 43.0f));
		//level.addEntity(new TutorialText(InputManager.GetBinding("Attack").ToString() + " to attack", 0xFFFFFFFF), new Vector2(43, 37.0f));
		level.addEntity(new TutorialText(InputManager.GetBinding("Attack").ToString() + " to attack", 0xFFFFFFFF), new Vector2(43, 37.0f));
		//level.addEntity(new TutorialText(InputManager.GetBinding("Attack2").ToString() + " to block", 0xFFFFFFFF), new Vector2(31, 37.5f));
		level.addEntity(new TutorialText(InputManager.GetBinding("Attack2").ToString() + " to block", 0xFFFFFFFF), new Vector2(31, 37.5f));
		level.addEntity(new TutorialText(InputManager.GetBinding("UseItem").ToString() + " to use item", 0xFFFFFFFF), new Vector2(31, 37));
		level.addEntity(new TutorialText(InputManager.GetBinding("SwitchItem").ToString() + " to switch item", 0xFFFFFFFF), (Vector2)level.rooms[0].getMarker(10) + new Vector2(1, 3.0f));
		level.addEntity(new TutorialText(InputManager.GetBinding("SwitchSpell").ToString() + " to switch spell", 0xFFFFFFFF), (Vector2)level.rooms[0].getMarker(10) + new Vector2(1, 2.5f));
		level.addEntity(new TutorialText(InputManager.GetBinding("Inventory").ToString() + " to open inventory", 0xFFFFFFFF), (Vector2)level.rooms[0].getMarker(05) + new Vector2(0, 1.5f));

		level.addEntity(new Rat() { itemDropChance = 0, dropCoins = false }, (Vector2)level.rooms[0].getMarker(06));
		level.addEntity(new Rat() { itemDropChance = 0, dropCoins = false }, (Vector2)level.rooms[0].getMarker(07));
		level.addEntity(new Rat() { itemDropChance = 0, dropCoins = false }, (Vector2)level.rooms[0].getMarker(08));
		level.addEntity(new Rat() { itemDropChance = 0, dropCoins = false }, (Vector2)level.rooms[0].getMarker(09));

		level.addEntity(new Barrel(), new Vector2(40.5f, 40));
		level.addEntity(new ExplosiveBarrel() { health = 1000 }, new Vector2(22.5f, 24));

		//level.addEntity(new ItemGate(), (Vector2)level.rooms[0].getMarker(09));

		GameState.instance.setAmbience(caveAmbience);
	}
}
