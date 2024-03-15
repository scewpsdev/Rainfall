using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Rainfall
{
	public class Ragdoll
	{
		static Dictionary<IntPtr, Ragdoll> ragdolls = new Dictionary<IntPtr, Ragdoll>();


		public readonly PhysicsEntity entity;

		IntPtr ragdoll;
		uint filterGroup, filterMask;

		Dictionary<string, SceneFormat.ColliderData> hitboxData;
		public readonly List<RigidBody> hitboxes = new List<RigidBody>();
		//public readonly Dictionary<Node, Tuple<Vector3, Vector3>> ragdollColliderData = new Dictionary<Node, Tuple<Vector3, Vector3>>();

		public readonly List<IntPtr> boneLinks = new List<IntPtr>();
		public readonly List<Node> nodes = new List<Node>();

		Node rootNode;

		public readonly Animator animator;


		public Ragdoll(PhysicsEntity entity, Model model, Node rootNode, Animator animator, Matrix transform, Dictionary<string, SceneFormat.ColliderData> hitboxData = null, uint filterGroup = 1, uint filterMask = 1)
		{
			this.entity = entity;
			this.rootNode = rootNode;
			this.animator = animator;
			this.hitboxData = hitboxData;
			this.filterGroup = filterGroup;
			this.filterMask = filterMask;

			init(transform);
		}

		void processNode(Node node, Matrix parentTransform, IntPtr parentLink)
		{
			Matrix localTransform = animator.getNodeLocalTransform(node);
			Matrix globalTransform = parentTransform * localTransform;
			Matrix globalTransformInDefaultPose = parentTransform * node.transform;

			bool deforming = node.name.IndexOf("ik", StringComparison.OrdinalIgnoreCase) == -1 && node.name.IndexOf("pole_target", StringComparison.OrdinalIgnoreCase) == -1;
			if (node.parent != null && deforming)
			{
				animator.getNodeVelocity(node, out Vector3 velocity, out Quaternion rotationVelocity);

				IntPtr link = IntPtr.Zero;

				if (hitboxData.ContainsKey(node.name))
				{
					SceneFormat.ColliderData hitbox = hitboxData[node.name];

					if (hitbox.type == SceneFormat.ColliderType.Box)
					{
						link = Native.Physics.Physics_RagdollAddLinkBox(ragdoll, parentLink, globalTransformInDefaultPose.translation, globalTransformInDefaultPose.rotation, velocity, rotationVelocity.eulers, hitbox.size * 0.5f, hitbox.offset, Quaternion.FromEulerAngles(hitbox.eulers), filterGroup, filterMask);
					}
					else if (hitbox.type == SceneFormat.ColliderType.Sphere)
					{
						link = Native.Physics.Physics_RagdollAddLinkSphere(ragdoll, parentLink, globalTransformInDefaultPose.translation, globalTransformInDefaultPose.rotation, velocity, rotationVelocity.eulers, hitbox.radius, hitbox.offset, filterGroup, filterMask);
					}
					else if (hitbox.type == SceneFormat.ColliderType.Capsule)
					{
						link = Native.Physics.Physics_RagdollAddLinkCapsule(ragdoll, parentLink, globalTransformInDefaultPose.translation, globalTransformInDefaultPose.rotation, velocity, rotationVelocity.eulers, hitbox.radius, 0.5f * hitbox.height - hitbox.radius, hitbox.offset, Quaternion.FromEulerAngles(hitbox.eulers), filterGroup, filterMask);
					}

					Native.Physics.Physics_RagdollLinkSetGlobalTransform(link, globalTransform.translation, globalTransform.rotation);
				}
				else
				{
					//link = Rainfall.Native.Physics.Physics_RagdollAddLinkEmpty(ragdoll, parentLink, globalTransform.translation, globalTransform.rotation, velocity, rotationVelocity.eulers);
				}

				if (link != IntPtr.Zero)
				{
					RigidBody body = new RigidBody(entity, link, this, filterGroup, filterMask);
					hitboxes.Add(body);
					RigidBody.bodies.Add(link, body);

					/*
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
					*/

					boneLinks.Add(link);
					nodes.Add(node);
				}

				//if (link != IntPtr.Zero || parentLink != IntPtr.Zero)
				{
					for (int i = 0; i < node.children.Length; i++)
					{
						processNode(node.children[i], globalTransformInDefaultPose, link != IntPtr.Zero ? link : parentLink);
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
			Native.Physics.Physics_RagdollLinkGetGlobalTransform(boneLinks[0], out position, out rotation);
		}

		public void getLinkTransform(int nodeIdx, out Vector3 position, out Quaternion rotation)
		{
			Native.Physics.Physics_RagdollLinkGetGlobalTransform(boneLinks[nodeIdx], out position, out rotation);
		}

		public RigidBody getHitboxForNode(Node node)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				if (nodes[i] == node)
					return hitboxes[i];
			}
			return null;
		}

		internal static Ragdoll GetRagdollFromHandle(IntPtr handle)
		{
			if (ragdolls.ContainsKey(handle))
				return ragdolls[handle];
			return null;
		}
	}
}
