using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public unsafe class Car : Entity
{
	int currentPoint;
	Vector3 target;
	float pitch, yaw;

	MeshData* mesh;

	public Car(int startPoint)
	{
		load("car.rfs");

		mesh = GameState.instance.npcPath.getMeshData(0);

		currentPoint = startPoint;
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
	}

	public override void fixedUpdate(float delta)
	{
		base.fixedUpdate(delta);

		float speed = 15;
		Vector3 direction = target - position;
		float distance = direction.length;
		direction /= distance;

		position += direction * speed * delta;

		pitch = MathHelper.Lerp(pitch, MathF.Atan2(direction.y, direction.xz.length), 5 * delta);
		yaw = MathHelper.LerpAngle(yaw, (direction.xz * new Vector2(1, -1)).angle - MathF.PI * 0.5f, 5 * delta);
		rotation = Quaternion.FromAxisAngle(Vector3.Up, yaw) * Quaternion.FromAxisAngle(Vector3.Right, pitch);

		float pointReachDistance = 2;
		if (distance < pointReachDistance)
		{
			currentPoint = (currentPoint + 1) % mesh->vertexCount;
			target = getPoint(currentPoint);
		}
	}
}
