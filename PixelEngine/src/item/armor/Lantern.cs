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

	ParticleEffect particles;


	public Lantern()
		: base("lantern", ItemType.Utility)
	{
		displayName = "Lantern";

		armor = 1;
		value = 10;
		armorSlot = ArmorSlot.Back;
		isActiveItem = false;
		isPassiveItem = true;
		baseWeight = 0.5f;

		sprite = new Sprite(tileset, 10, 1);

		stick = new Sprite(tileset, 11, 1);
		lanternMini = new Sprite(tileset, 12, 1);
	}

	public override void onEquip(Player player)
	{
		GameState.instance.level.addEntity(particles = new ParticleEffect(player, "res/effects/lantern.rfs"), player.position + new Vector2(0, 0.5f));
	}

	public override void onUnequip(Player player)
	{
		particles.remove();
		particles = null;
	}

	public override void render(Entity entity)
	{
		if (entity is Player && (entity as Player).isAlive)
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

			Debug.Assert(particles != null);
			particles.offset.x = -player.direction * 0.25f + MathF.Sin(rotation) * 0.25f;
			particles.offset.y = 0.5f + MathF.Cos(rotation) * 0.25f;
		}

		Renderer.DrawLight(entity.position + new Vector2(0, 0.5f), new Vector3(1.0f, 0.8f, 0.5f) * 2, 12);
	}
}
