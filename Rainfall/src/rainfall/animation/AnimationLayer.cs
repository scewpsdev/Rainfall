using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Rainfall
{
	public class AnimationLayer
	{
		public Matrix[] nodeAnimationLocalTransforms { get; private set; }

		public string animationName;
		public Model animationData;

		public bool active = true;
		public bool looping;
		public bool mirrored = false;
		public float timerOffset = 0.0f;

		public bool rootMotion = false;
		public Node rootMotionNode = null;
		public Vector3 rootMotionDisplacement { get; private set; } = Vector3.Zero;
		Matrix lastRootTransform;
		float lastAnimationTimer = float.MaxValue;


		public AnimationLayer(Model animationData, string animationName, bool looping, bool[] mask = null)
		{
			// TODO array might have incorrect size when reusing animation states
			nodeAnimationLocalTransforms = new Matrix[animationData.skeleton.nodes.Length];
			Array.Fill(nodeAnimationLocalTransforms, Matrix.Identity);

			this.animationName = animationName;
			this.looping = looping;

			this.animationData = animationData;
		}

		public void update(float timer)
		{
			unsafe
			{
				timer += timerOffset;

				SceneData* scene = (SceneData*)animationData.sceneDataHandle;
				AnimationData* animation = getAnimationByName(scene, animationName);
				if (animation != null)
				{
					if (looping)
						timer %= animation->duration;
					else
					{
						float startTime = 0.0f / 24.0f;
						timer = Math.Clamp(timer, startTime, animation->duration);
					}

					animateNode(animationData.skeleton.rootNode, animation, timer);
				}
			}
		}

		unsafe AnimationData* getAnimationByName(SceneData* scene, string name)
		{
			if (name == null)
				return null;
			for (int i = 0; i < scene->numAnimations; i++)
			{
				if (StringUtils.CompareStrings(name, scene->animations[i].name))
					return &scene->animations[i];
				//string animationName = Marshal.PtrToStringAnsi((IntPtr)scene->animations[i].name);
				//if (animationName == name)
				//	return &scene->animations[i];
			}
			return null;
		}

		unsafe void animateNode(Node node, AnimationData* animation, float timer)
		{
			int nodeID = node.id;
			if (mirrored)
			{
				Span<byte> mirroredName = stackalloc byte[node.name.Length + 1];
				StringUtils.WriteString(mirroredName, node.name);
				if (StringUtils.EndsWith(node.name, ".R"))
					StringUtils.WriteCharacter(mirroredName, mirroredName.Length - 2, 'L');
				if (StringUtils.EndsWith(node.name, ".L"))
					StringUtils.WriteCharacter(mirroredName, mirroredName.Length - 2, 'R');
				if (StringUtils.EndsWith(node.name, "_r"))
					StringUtils.WriteCharacter(mirroredName, mirroredName.Length - 2, 'l');
				if (StringUtils.EndsWith(node.name, "_l"))
					StringUtils.WriteCharacter(mirroredName, mirroredName.Length - 2, 'r');
				nodeID = animationData.skeleton.getNode(mirroredName).id;
			}
			Native.Animation.Animation_AnimateNode(nodeID, animation, timer, (byte)(looping ? 1 : 0), ref nodeAnimationLocalTransforms[node.id]);
			if (mirrored)
			{
				nodeAnimationLocalTransforms[node.id].m30 *= -1.0f;
				nodeAnimationLocalTransforms[node.id].m01 *= -1.0f;
				nodeAnimationLocalTransforms[node.id].m02 *= -1.0f;
				nodeAnimationLocalTransforms[node.id].m10 *= -1.0f;
				nodeAnimationLocalTransforms[node.id].m20 *= -1.0f;
			}


			// Root Motion
			if (rootMotion && rootMotionNode != null && node.name == rootMotionNode.name)
			{
				rootMotionDisplacement = Vector3.Zero;

				Matrix rootTransform = getNodeTransform(node);
				if (timer > lastAnimationTimer)
				{
					//Vector3 displacement = rootTransform.translation - lastRootTransform.translation;
					//Matrix delta = lastRootTransform.inverted * rootTransform;
					Vector3 displacement = rootTransform.translation - lastRootTransform.translation;
					rootMotionDisplacement = displacement;
				}
				lastRootTransform = rootTransform;
				lastAnimationTimer = timer;

				// Should be X and Z but model is in different coordinate system
				rootTransform.m30 = 0.0f;
				rootTransform.m31 = 0.0f;
				rootTransform.m32 = 0.0f;
				Matrix parentTransform = getNodeTransform(node.parent);
				Matrix rootLocalTransform = parentTransform.inverted * rootTransform;
				nodeAnimationLocalTransforms[node.id] = rootLocalTransform;
			}

			for (int i = 0; i < node.children.Length; i++)
			{
				animateNode(node.children[i], animation, timer);
			}
		}

		internal Matrix getNodeLocalTransform(string name)
		{
			int nodeID = animationData.skeleton.getNode(name).id;
			return nodeAnimationLocalTransforms[nodeID];
		}

		internal Matrix getNodeTransform(Node node)
		{
			//unsafe
			{
				//Matrix inverseBindPose = ((SceneData*)animationData.sceneDataHandle)->skeletons[0].inverseBindPose;
				Matrix transform = nodeAnimationLocalTransforms[node.id];
				while (node.parent != null)
				{
					transform = nodeAnimationLocalTransforms[node.parent.id] * transform;
					node = node.parent;
				}
				return transform;
			}
		}

		internal bool hasNode(string name)
		{
			uint nameHash = Hash.hash(name);
			return animationData.skeleton.nameMap.ContainsKey(nameHash);
		}

		public float duration
		{
			get
			{
				unsafe
				{
					SceneData* scene = (SceneData*)animationData.sceneDataHandle;
					AnimationData* animation = getAnimationByName(scene, animationName);
					return animation->duration;
				}
			}
		}
	}
}
