using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public unsafe class Racecar : Entity
{
	int currentPoint;
	Vector3 target;
	float pitch, yaw;
	float segmentLength = -1;

	Vector3 velocity;
	float maxSpeed;
	float speed = 0;

	MeshData* mesh;

	Sound sound;
	AudioSource audio;


	public Racecar(int startPoint)
	{
		load("racecar.rfs");

		sound = Resource.GetSound("sounds/racecar.ogg");

		mesh = GameState.instance.npcPath.getMeshData(0);

		currentPoint = startPoint;

		maxSpeed = MathHelper.RandomFloat(30, 45);
	}

	Vector3 getPoint(int idx)
	{
		Debug.Assert(idx < mesh->vertexCount);
		PositionNormalTangent vertex = mesh->getVertex(idx);
		Vector3 origin = vertex.position;
		HitData? hit = Physics.Raycast(origin, Vector3.Down, 50, QueryFilterFlags.Static);
		Debug.Assert(hit != null);
		Vector3 point = origin + hit.Value.distance * Vector3.Down;
		return point;
	}

	public override void init()
	{
		base.init();

		target = getPoint(currentPoint);

		audio = new AudioSource(position);
		audio.playSound(sound, 5);
		audio.isLooping = true;

		sound.singleInstance = false;
	}

	public override void destroy()
	{
		unload();

		Resource.FreeSound(sound);
		audio.destroy();
	}

	public override void fixedUpdate(float delta)
	{
		base.fixedUpdate(delta);

		speed = MathHelper.Lerp(speed, segmentLength != -1 ? MathHelper.Remap(segmentLength / 50, 0, 1, 20, maxSpeed) : 45, 0.5f * delta);
		Vector3 direction = target - position;
		float distance = direction.length;
		direction /= distance;

		velocity = direction * speed;

		position += velocity * delta;

		pitch = MathHelper.Lerp(pitch, MathF.Atan2(direction.y, direction.xz.length), 5 * delta);

		float destYaw = (direction.xz * new Vector2(1, -1)).angle - MathF.PI * 0.5f;
		yaw = MathHelper.LerpAngle(yaw, destYaw, 5 * delta);
		rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw) * Quaternion.FromAxisAngle(Vector3.Right, pitch);

		float pointReachDistance = 2;
		if (distance < pointReachDistance)
		{
			currentPoint = (currentPoint + 1) % mesh->vertexCount;
			Vector3 newTarget = getPoint(currentPoint);
			segmentLength = (newTarget - target).length;
			target = newTarget;
		}

		audio.pitch = MathHelper.Remap(speed, 20, 35, 0.5f, 1.5f);
	}

	public override void update()
	{
		base.update();

		audio.setPosition(position);
		audio.setVelocity(-velocity);

		//sound.singleInstance = false;
	}
}
