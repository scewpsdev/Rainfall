using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Lantern : Item
{
	float rotation = 0.0f;
	float rotationVelocity = 0.0f;
	float lastxvelocity = 0.0f;

	Sprite stick;
	Sprite lanternMini;


	public Lantern()
		: base("lantern", ItemType.Armor)
	{
		displayName = "Lantern";

		value = 10;

		canEquipMultiple = false;

		sprite = new Sprite(tileset, 10, 1);

		stick = new Sprite(tileset, 11, 1);
		lanternMini = new Sprite(tileset, 12, 1);
	}

	public override void render(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;

			float xvelocity = player.velocity.x;
			float acc = xvelocity - lastxvelocity;
			rotationVelocity -= acc * 0.4f;
			lastxvelocity = xvelocity;

			rotationVelocity += -rotation * 20 * Time.deltaTime;

			rotationVelocity = MathHelper.Lerp(rotationVelocity, 0, 1 * Time.deltaTime);

			rotation += rotationVelocity * Time.deltaTime;

			Renderer.DrawSprite(player.position.x - 0.5f, player.position.y, Entity.LAYER_PLAYER_BG, 1, 1, 0.0f, stick, player.direction == -1);
			Renderer.DrawSprite(player.position.x - player.direction * 0.25f - 0.5f, player.position.y + 0.75f - 0.5f, Entity.LAYER_PLAYER_BG, 1, 1, rotation, lanternMini, false);
		}
	}
}
