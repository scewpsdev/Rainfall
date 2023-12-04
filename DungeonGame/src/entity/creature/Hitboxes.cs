using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


public static class Hitboxes
{
	static bool FindEndPoint(Node node, out Vector3 endPoint)
	{
		endPoint = Vector3.Zero;

		if (node.children.Length == 0)
			return false;
		if (node.children.Length == 1)
		{
			endPoint = node.children[0].transform.translation;
			return true;
		}
		else if (node.children.Length > 1)
		{
			endPoint = node.children[0].transform.translation;
			return true;
		}
		else
		{
			Debug.Assert(false);
		}
		return false;
	}

	static void ProcessNode(Node node, Dictionary<string, BoneHitbox> hitboxData, Creature creature, List<RigidBody> hitboxes, Dictionary<Node, int> hitboxesNodeMap)
	{
		if (hitboxData.ContainsKey(node.name))
		{
			BoneHitbox hitbox = hitboxData[node.name];
			Matrix globalTransform = creature.getModelMatrix() * Matrix.CreateRotation(Vector3.Up, MathF.PI) * creature.animator.getNodeTransform(node, 0);

			RigidBody body = new RigidBody(creature, RigidBodyType.Kinematic, globalTransform.translation, globalTransform.rotation, 1.0f, Vector3.Zero, (uint)PhysicsFilterGroup.CreatureHitbox, (uint)PhysicsFilterMask.CreatureHitbox);

			FindEndPoint(node, out Vector3 endPoint);

			if (hitbox.isBox)
			{
				float height = endPoint.length;

				float startDistance = hitbox.startDistance;
				float endDistance = hitbox.endDistance;
				Vector3 midPoint = Vector3.Lerp(Vector3.Zero, endPoint, (startDistance + (height - startDistance - endDistance) * 0.5f) / height);
				height -= startDistance + endDistance;

				Vector3 halfExtents = new Vector3(hitbox.radius, 0.5f * height, hitbox.radius2);

				body.addBoxCollider(halfExtents, midPoint, Quaternion.Identity);
			}
			else
			{
				float height = endPoint.length;
				float radius = hitbox.radius;

				float startDistance = hitbox.startDistance;
				float endDistance = hitbox.endDistance;
				Vector3 midPoint = Vector3.Lerp(Vector3.Zero, endPoint, (startDistance + (height - startDistance - endDistance) * 0.5f) / height);
				height -= startDistance + endDistance;

				height = MathF.Max(height, 2 * radius + 0.01f);

				body.addCapsuleCollider(radius, height, midPoint, Quaternion.Identity);
			}

			hitboxes.Add(body);
			hitboxesNodeMap.Add(node, hitboxes.Count - 1);
		}

		for (int i = 0; i < node.children.Length; i++)
		{
			ProcessNode(node.children[i], hitboxData, creature, hitboxes, hitboxesNodeMap);
		}
	}

	public static void GenerateHitboxBodies(Dictionary<string, BoneHitbox> hitboxData, Node rootNode, Creature creature, List<RigidBody> hitboxes, Dictionary<Node, int> hitboxesNodeMap)
	{
		ProcessNode(rootNode, hitboxData, creature, hitboxes, hitboxesNodeMap);
	}

	public static void UpdateHitboxBodyTransforms(Creature creature)
	{
		foreach (var pair in creature.hitboxesNodeMap)
		{
			Node node = pair.Key;
			RigidBody body = creature.hitboxes[pair.Value];
			Matrix globalTransform = creature.getModelMatrix() * Matrix.CreateRotation(Vector3.Up, MathF.PI) * creature.animator.getNodeTransform(node, 0);
			body.setTransform(globalTransform.translation, globalTransform.rotation);
		}
	}
}
