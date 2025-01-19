using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Parachute : Item
{
	float activateSpeed = -15;
	float fallSpeed = -5;

	bool active = false;
	float rotation = 0;

	Sprite parachute;


	public Parachute()
		: base("parachute", ItemType.Armor)
	{
		displayName = "Parachute";

		baseArmor = 1;
		armorSlot = ArmorSlot.Back;
		value = 35;

		sprite = new Sprite(tileset, 4, 9);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/parachute.png", false), 0, 0, 16, 16);
		parachute = new Sprite(Entity.tileset, 4, 1, 2, 2);
	}

	public override void update(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;

			if (!active && player.velocity.y < activateSpeed)
				active = true;

			if (active)
			{
				player.velocity.y = MathF.Max(player.velocity.y, fallSpeed);
				if (player.isGrounded || player.velocity.y > 0)
					active = false;
			}
		}
	}

	public override void render(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;

			if (active)
			{
				rotation = MathHelper.Lerp(rotation, player.velocity.angle + MathF.PI * 0.5f, 5 * Time.deltaTime);
				Vector2 offset = new Vector2(-MathF.Sin(rotation), MathF.Cos(rotation) - 1);
				Renderer.DrawSprite(player.position.x - 1 + offset.x, player.position.y + 0.5f + offset.y, Entity.LAYER_BG, 2, 2, rotation, parachute);
			}
		}
	}
}
