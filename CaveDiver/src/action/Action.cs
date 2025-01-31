using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EntityAction
{
	public readonly string type;

	public string animation = null;
	public bool mainHand;
	public Item renderWeaponMain = null;
	public Item renderWeaponSecondary = null;
	public bool canMove = true;
	public bool canJump = true;
	public bool turnToCrosshair = true;
	public float actionMovement = 0;

	public float speedMultiplier = 1.0f;

	public float followUpCancelTime = 100.0f;
	public float animationSpeed = 1.0f;
	public float postActionLinger = 0.0f;

	public float iframesStartTime = 0.0f;
	public float iframesEndTime = 0.0f;

	public long startTime = 0;
	public float elapsedTime { get; protected set; } = 0.0f;
	public float duration = 0.0f;


	public EntityAction(string type, bool mainHand = true)
	{
		this.type = type;
		this.mainHand = mainHand;
	}

	public virtual void update(Player player)
	{
		elapsedTime += Time.deltaTime * animationSpeed;
	}

	public void cancel()
	{
		duration = 0;
	}

	public virtual void render(Player player)
	{
	}

	public virtual void onQueued(Player player)
	{
	}

	public virtual void onStarted(Player player)
	{
	}

	public virtual void onFinished(Player player)
	{
	}

	public Item getRenderWeapon(bool mainHand) => mainHand ? renderWeaponMain : renderWeaponSecondary;

	public void setRenderWeapon(bool mainHand, Item renderWeapon)
	{
		if (mainHand)
			renderWeaponMain = renderWeapon;
		else
			renderWeaponSecondary = renderWeapon;
	}

	public virtual Matrix getItemTransform(Player player, bool mainHand)
	{
		Vector2 position = player.getWeaponOrigin(mainHand);
		Item item = mainHand ? player.handItem : player.offhandItem;
		if (item != null)
			position += item.renderOffset;
		return Matrix.CreateRotation(Vector3.UnitY, player.direction == -1 ? MathF.PI : 0)
			* Matrix.CreateTranslation(position.x, position.y, 0);
	}

	public bool hasStarted
	{
		get => startTime > 0;
	}

	public bool hasFinished
	{
		get => hasStarted && elapsedTime >= duration + postActionLinger;
	}
}
