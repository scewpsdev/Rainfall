using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HubRoom : Entity
{
	Room room;

	Texture stairs;


	public HubRoom(Room room)
	{
		this.room = room;

		stairs = Resource.GetTexture("res/level/hub/stairs.png", false);
	}

	public override void render()
	{
		Vector2 dungeonEntrancePosition = (Vector2)room.getMarker(0x0b);
		int numSteps = 20;
		float width = 1.2f;
		float z = 0.75f;
		for (int i = 0; i < numSteps; i++)
		{
			// vertical
			{
				Vector3 vertex0 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(-width, 0.5f / 16 - 2 + i / (float)numSteps * 2), i / (float)numSteps * z));
				Vector3 vertex1 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(width, 0.5f / 16 - 2 + i / (float)numSteps * 2), i / (float)numSteps * z));
				Vector3 vertex2 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), i / (float)numSteps * z));
				Vector3 vertex3 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(-width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), i / (float)numSteps * z));
				Renderer.DrawSpriteEx(vertex0, vertex1, vertex2, vertex3, null, 0, 0, 0, 0, 0xFF6e6e6e);
			}
			// horizontal
			{
				Vector3 vertex0 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(-width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), i / (float)numSteps * z));
				Vector3 vertex1 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), i / (float)numSteps * z));
				Vector3 vertex2 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), (i + 1) / (float)numSteps * z));
				Vector3 vertex3 = ParallaxObject.ParallaxEffect(new Vector3(dungeonEntrancePosition + new Vector2(-width, 0.5f / 16 - 2 + (i + 1) / (float)numSteps * 2), (i + 1) / (float)numSteps * z));
				Renderer.DrawSpriteEx(vertex0, vertex1, vertex2, vertex3, null, 0, 0, 0, 0, 0xFF767676);
			}
		}
	}
}
