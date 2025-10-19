using Rainfall;
using Rainfall.Native;
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

		public readonly List<IntPtr> boneLinks = new List<IntPtr>();
		public readonly List<Node> nodes = new List<Node>();

		public Node rootNode;

		public readonly Animator animator;


		public Ragdoll(PhysicsEntity entity, Node rootNode, Animator animator, Matrix transform, Vector3 initialVelocity, Dictionary<string, SceneFormat.ColliderData> hitboxData, uint filterGroup = 1, uint filterMask = 1)
		{
			this.entity = entity;
			this.rootNode = rootNode;
			this.animator = animator;
			this.hitboxData = hitboxData;
			this.filterGroup = filterGroup;
			this.filterMask = filterMask;

			init(transform, initialVelocity);
		}

		void processNode(Node node, Matrix parentTransform, IntPtr parentLink, Vector3 initialVelocity)
		{
			Matrix globalTransform = parentTransform * node.transform;
			if (parentLink == IntPtr.Zero)
				globalTransform = globalTransform * node.transform.inverted * animator.getNodeLocalTransform(node);

			bool deforming = node.name.IndexOf("ik", StringComparison.OrdinalIgnoreCase) == -1 && node.name.IndexOf("pole_target", StringComparison.OrdinalIgnoreCase) == -1;
			if (node.parent != null && deforming)
			{
				IntPtr link = IntPtr.Zero;

				if (hitboxData.ContainsKey(node.name))
				{
					bool knee = node.name.IndexOf("leg", StringComparison.OrdinalIgnoreCase) >= 0 && node.name.IndexOf("lower", StringComparison.OrdinalIgnoreCase) >= 0 || node.name.IndexOf("shin", StringComparison.OrdinalIgnoreCase) >= 0;
					bool elbow = node.name.IndexOf("arm", StringComparison.OrdinalIgnoreCase) >= 0 && (node.name.IndexOf("lower", StringComparison.OrdinalIgnoreCase) >= 0 || node.name.IndexOf("fore", StringComparison.OrdinalIgnoreCase) >= 0);

					JointType jointType = JointType.Spherical;
					if (knee)
						jointType = JointType.RevoluteUnwrapped;
					if (elbow)
						jointType = JointType.RevoluteUnwrapped;

					animator.getNodeVelocity(node, out Vector3 velocity, out Quaternion rotationVelocity);
					if (parentLink == IntPtr.Zero)
						velocity += initialVelocity;

					Matrix localPose = node.transform.inverted * animator.getNodeLocalTransform(node);
					link = Physics.Physics_RagdollCreateLink(ragdoll, parentLink, jointType, globalTransform, localPose.translation, localPose.rotation.eulers, velocity, rotationVelocity.eulers);

					if (hitboxData.TryGetValue(node.name, out SceneFormat.ColliderData hitbox))
					{
						if (hitbox.type == SceneFormat.ColliderType.Box)
							Physics.Physics_RagdollLinkAddBoxCollider(link, hitbox.size * 0.5f, hitbox.offset, Quaternion.FromEulerAngles(hitbox.eulers), filterGroup, filterMask);
						else if (hitbox.type == SceneFormat.ColliderType.Sphere)
							Physics.Physics_RagdollLinkAddSphereCollider(link, hitbox.radius, hitbox.offset, filterGroup, filterMask);
						else if (hitbox.type == SceneFormat.ColliderType.Capsule)
							Physics.Physics_RagdollLinkAddCapsuleCollider(link, hitbox.radius, hitbox.height, hitbox.offset, Quaternion.FromEulerAngles(hitbox.eulers), filterGroup, filterMask);
					}

					if (parentLink != IntPtr.Zero)
					{
						if (knee)
						{
							Physics.Physics_RagdollLinkSetSwingXLimit(link, 0.0f, 0.9f * 3.1415f);
						}
						else if (elbow)
						{
							Physics.Physics_RagdollLinkSetSwingZLimit(link, -0.9f * 3.1415f, 0.0f);
						}
						else
						{
							Physics.Physics_RagdollLinkSetSwingXLimit(link, -0.3f * 3.1415f, 0.3f * 3.1415f);
							Physics.Physics_RagdollLinkSetTwistLimit(link, -0.1f * 3.1415f, 0.1f * 3.1415f);
							Physics.Physics_RagdollLinkSetSwingZLimit(link, -0.3f * 3.1415f, 0.3f * 3.1415f);
						}
					}

					RigidBody body = new RigidBody(entity, link, this, filterGroup, filterMask);
					hitboxes.Add(body);
					RigidBody.bodies.Add(link, body);

					boneLinks.Add(link);
					nodes.Add(node);
				}
				else
				{
					Debug.Assert(parentLink != IntPtr.Zero);
				}

				if (node.children != null)
				{
					for (int i = 0; i < node.children.Length; i++)
					{
						processNode(node.children[i], globalTransform, link != IntPtr.Zero ? link : parentLink, initialVelocity);
					}
				}
			}
		}

		void init(Matrix transform, Vector3 initialVelocity)
		{
			ragdoll = Physics.Physics_CreateRagdoll();
			ragdolls.Add(ragdoll, this);

			Matrix armatureTransform = transform * animator.getNodeTransform(rootNode.parent);
			processNode(rootNode, armatureTransform, IntPtr.Zero, initialVelocity);

			Physics.Physics_SpawnRagdoll(ragdoll);
		}

		public void destroy()
		{
			ragdolls.Remove(ragdoll);
			Physics.Physics_DestroyRagdoll(ragdoll);
		}

		public void update()
		{
			Physics.Physics_RagdollLinkGetGlobalTransform(boneLinks[0], out Vector3 rootPosition, out Quaternion rootRotation);
			Matrix transform = Matrix.CreateTranslation(rootPosition) * Matrix.CreateRotation(rootRotation);

			for (int i = 0; i < nodes.Count; i++)
			{
				Node node = nodes[i];

				Physics.Physics_RagdollLinkGetGlobalTransform(boneLinks[i], out Vector3 position, out Quaternion rotation);
				Matrix globalTransform = Matrix.CreateTranslation(position) * Matrix.CreateRotation(rotation);
				Matrix parentTransform = transform * animator.getNodeTransform(node.parent, 0);
				Matrix localTransform = parentTransform.inverted * globalTransform;
				animator.setNodeLocalTransform(node, localTransform);
			}

			animator.applyAnimation();
		}

		public Matrix getTransform()
		{
			Physics.Physics_RagdollLinkGetGlobalTransform(boneLinks[0], out Vector3 position, out Quaternion rotation);
			Matrix transform = Matrix.CreateTransform(position, rotation);
			return transform;
		}

		public void getLinkTransform(int nodeIdx, out Vector3 position, out Quaternion rotation)
		{
			Physics.Physics_RagdollLinkGetGlobalTransform(boneLinks[nodeIdx], out position, out rotation);
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
