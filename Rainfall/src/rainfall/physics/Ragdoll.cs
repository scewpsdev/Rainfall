using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Rainfall
{
	public struct BoneHitbox
	{
		public float startDistance;
		public float endDistance;
		public float radius;
		public float radius2;
		public bool isBox;

		public BoneHitbox(float radius, float startDistance = 0.0f, float endDistance = 0.0f)
		{
			this.startDistance = startDistance;
			this.endDistance = endDistance;
			this.radius = radius;
			radius2 = 0.0f;
			isBox = false;
		}

		public BoneHitbox(Vector2 halfExtents, float startDistance = 0.0f, float endDistance = 0.0f)
		{
			this.startDistance = startDistance;
			this.endDistance = endDistance;
			this.radius = halfExtents.x;
			this.radius2 = halfExtents.y;
			isBox = true;
		}
	}

	public class Ragdoll
	{
		static Dictionary<IntPtr, Ragdoll> ragdolls = new Dictionary<IntPtr, Ragdoll>();


		public readonly PhysicsEntity entity;

		IntPtr ragdoll;
		uint filterMask;

		Dictionary<string, BoneHitbox> hitboxData;
		public readonly List<RigidBody> hitboxes = new List<RigidBody>();
		//public readonly Dictionary<Node, Tuple<Vector3, Vector3>> ragdollColliderData = new Dictionary<Node, Tuple<Vector3, Vector3>>();

		public readonly List<IntPtr> boneLinks = new List<IntPtr>();
		public readonly List<Node> nodes = new List<Node>();

		Node rootNode;

		public readonly Animator animator;


		public Ragdoll(PhysicsEntity entity, Model model, Node rootNode, Animator animator, Matrix transform, Dictionary<string, BoneHitbox> hitboxData = null, uint filterMask = 1)
		{
			this.entity = entity;
			this.rootNode = rootNode;
			this.animator = animator;
			this.hitboxData = hitboxData;
			this.filterMask = filterMask;

			init(transform);
		}

		bool findEndPoint(Node node, out Vector3 endPoint)
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

		void processNode(Node node, Matrix parentTransform, IntPtr parentLink)
		{
			Matrix localTransform = animator.getNodeLocalTransform(node);
			Matrix globalTransform = parentTransform * localTransform;
			Matrix globalTransformInDefaultPose = parentTransform * node.transform;

			if (node.parent != null && node.children.Length > 0 && !node.name.Contains("IK"))
			{
				if (findEndPoint(node, out Vector3 endPoint))
				{
					animator.getNodeVelocity(node, out Vector3 velocity, out Quaternion rotationVelocity);

					IntPtr link = IntPtr.Zero;

					if (hitboxData.ContainsKey(node.name))
					{
						BoneHitbox hitbox = hitboxData[node.name];

						if (hitbox.isBox)
						{
							float height = endPoint.length;

							float startDistance = hitbox.startDistance;
							float endDistance = hitbox.endDistance;
							Vector3 midPoint = Vector3.Lerp(Vector3.Zero, endPoint, (startDistance + (height - startDistance - endDistance) * 0.5f) / height);
							height -= startDistance + endDistance;

							Vector3 halfExtents = new Vector3(hitbox.radius, 0.5f * height, hitbox.radius2);

							//ragdollColliderData.Add(node, new Tuple<Vector3, Vector3>(midPoint, halfExtents));

							link = Native.Physics.Physics_RagdollAddLinkBox(ragdoll, parentLink, globalTransformInDefaultPose.translation, globalTransformInDefaultPose.rotation, velocity, rotationVelocity.eulers, halfExtents, midPoint, Quaternion.Identity, filterMask);
							Native.Physics.Physics_RagdollLinkSetGlobalTransform(link, globalTransform.translation, globalTransform.rotation);
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

							//ragdollColliderData.Add(node, new Tuple<Vector3, Vector3>(midPoint, new Vector3(radius, 0.5f * height, radius)));

							link = Native.Physics.Physics_RagdollAddLinkCapsule(ragdoll, parentLink, globalTransformInDefaultPose.translation, globalTransformInDefaultPose.rotation, velocity, rotationVelocity.eulers, radius, 0.5f * height - radius, midPoint, Quaternion.Identity, filterMask);
							Native.Physics.Physics_RagdollLinkSetGlobalTransform(link, globalTransform.translation, globalTransform.rotation);
						}
					}
					else
					{
						//link = Rainfall.Native.Physics.Physics_RagdollAddLinkEmpty(ragdoll, parentLink, globalTransform.translation, globalTransform.rotation, velocity, rotationVelocity.eulers);
					}

					if (link != IntPtr.Zero)
					{
						RigidBody body = new RigidBody(entity, link, this, filterMask);
						hitboxes.Add(body);
						RigidBody.bodies.Add(link, body);

						if (node.name.Contains("Leg") && node.name.Contains("Lower"))
						{
							Native.Physics.Physics_RagdollLinkSetSwingLimit(link, 0.1f, 0.18f * 3);
							Native.Physics.Physics_RagdollLinkSetTwistLimit(link, -0.1f, 0.1f);
						}
						if (node.name.Contains("Shoulder"))
						{
							Native.Physics.Physics_RagdollLinkSetSwingLimit(link, 0.01f, 0.01f);
							Native.Physics.Physics_RagdollLinkSetTwistLimit(link, -0.01f, 0.01f);
						}

						boneLinks.Add(link);
						nodes.Add(node);
					}

					if (link != IntPtr.Zero || parentLink != IntPtr.Zero)
					{
						for (int i = 0; i < node.children.Length; i++)
						{
							processNode(node.children[i], globalTransformInDefaultPose, link != IntPtr.Zero ? link : parentLink);
						}
					}
				}
			}
		}

		void init(Matrix transform)
		{
			ragdoll = Native.Physics.Physics_CreateRagdoll();
			ragdolls.Add(ragdoll, this);

			processNode(rootNode, transform * Matrix.CreateRotation(Vector3.Up, MathF.PI) * animator.getNodeTransform(rootNode.parent, 0), IntPtr.Zero);

			Native.Physics.Physics_SpawnRagdoll(ragdoll);
		}

		public void destroy()
		{
			ragdolls.Remove(ragdoll);
			Native.Physics.Physics_DestroyRagdoll(ragdoll);
		}

		public void update()
		{
			Native.Physics.Physics_RagdollLinkGetGlobalTransform(boneLinks[0], out Vector3 rootPosition, out Quaternion rootRotation);
			Matrix transform = Matrix.CreateTranslation(rootPosition) * Matrix.CreateRotation(rootRotation);

			for (int i = 0; i < nodes.Count; i++)
			{
				Node node = nodes[i];

				Native.Physics.Physics_RagdollLinkGetGlobalTransform(boneLinks[i], out Vector3 position, out Quaternion rotation);
				Matrix globalTransform = Matrix.CreateTranslation(position) * Matrix.CreateRotation(rotation);
				Matrix parentTransform = transform * Matrix.CreateRotation(Vector3.Up, MathF.PI) * (node.parent != rootNode ? animator.getNodeTransform(node.parent, 0) : animator.getNodeTransform(rootNode, 0));
				Matrix localTransform = parentTransform.inverted * globalTransform;
				animator.setNodeLocalTransform(node, localTransform);
			}

			animator.applyAnimation();
		}

		public void getTransform(out Vector3 position, out Quaternion rotation)
		{
			/*
			Rainfall.Native.Physics.Physics_RagdollLinkGetGlobalTransform(boneLinks[0], out Vector3 rootNodePosition, out Quaternion rootNodeRotation);
			Matrix rootNodeTransform = Matrix.CreateTransform(rootNodePosition, rootNodeRotation);
			Matrix transform = startingPose.getNodeLocalTransform(nodes[0].parent) * rootNodeTransform;
			position = transform.translation;
			rotation = transform.rotation;
			*/
			Rainfall.Native.Physics.Physics_RagdollLinkGetGlobalTransform(boneLinks[0], out position, out rotation);
		}

		public void getLinkTransform(int nodeIdx, out Vector3 position, out Quaternion rotation)
		{
			Rainfall.Native.Physics.Physics_RagdollLinkGetGlobalTransform(boneLinks[nodeIdx], out position, out rotation);
		}

		internal static Ragdoll GetRagdollFromHandle(IntPtr handle)
		{
			if (ragdolls.ContainsKey(handle))
				return ragdolls[handle];
			return null;
		}
	}
}
