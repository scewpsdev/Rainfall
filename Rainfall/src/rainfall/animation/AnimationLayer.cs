﻿using System;
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
		public bool[] mask;

		public string animationName;
		public Model animationData;

		public bool active = true;
		public bool looping;
		public bool mirrored = false;
		public float timerOffset = 0.0f;

		public bool rootMotion = false;
		public Node rootMotionNode = null;
		public Matrix rootMotionDisplacement { get; private set; } = Matrix.Identity;
		public bool hasLooped = false;

		Matrix lastRootTransform = Matrix.Identity;
		float lastAnimationTimer = 0;


		public AnimationLayer(Model animationData, string animationName, bool looping, bool[] mask = null)
		{
			nodeAnimationLocalTransforms = new Matrix[animationData.skeleton.nodes.Length];
			Array.Fill(nodeAnimationLocalTransforms, Matrix.Identity);

			this.animationName = animationName;
			this.looping = looping;
			this.mask = mask;

			// The bone mask always has to be made for the animation data node set.
			// If the model the animation is applied to has more bones,
			// creating a bone mask for it will make it incompatible with the animation since the node ids are different.
			// Therefore we check here for compatibility to avoid this mistake in the future.
			if (mask != null)
				Debug.Assert(mask.Length == animationData.skeleton.nodes.Length);

			this.animationData = animationData;
		}

		public void setMask(Node node, bool value)
		{
			if (mask != null)
				mask[node.id] = value;
		}

		public unsafe void update(float timer)
		{
			timer += timerOffset;

			SceneData* scene = animationData.scene;
			AnimationData* animation = getAnimationByName(scene, animationName);
			if (animation != null)
			{
				if (looping)
				{
					timer = (timer + animation->duration) % animation->duration;
				}
				else
				{
					float startTime = 0.0f / 24.0f;
					timer = Math.Clamp(timer, startTime, animation->duration);
				}

				animateNode(animationData.skeleton.rootNode, animation, timer);
			}

			hasLooped = timer < lastAnimationTimer;
			lastAnimationTimer = timer;
		}

		public unsafe void updateRootMotion(float timer)
		{
			// Root Motion
			if (rootMotion && rootMotionNode != null)
			{
				timer += timerOffset;

				SceneData* scene = animationData.scene;
				AnimationData* animation = getAnimationByName(scene, animationName);
				if (animation != null)
				{
					if (looping)
					{
						timer %= animation->duration;
					}
					else
					{
						float startTime = 0.0f / 24.0f;
						timer = Math.Clamp(timer, startTime, animation->duration);
					}

					Node node = animationData.skeleton.getNode(rootMotionNode.name);

					Matrix rootNodeInitialTransform = Matrix.Identity;
					Animation_AnimateNode(node.id, animation, 0.0f, 0, ref rootNodeInitialTransform);

					Matrix rootTransform = Matrix.Identity;
					Animation_AnimateNode(node.id, animation, timer, (byte)(looping ? 1 : 0), ref rootTransform);

					rootMotionDisplacement = rootTransform * rootNodeInitialTransform.inverted;

					nodeAnimationLocalTransforms[node.id] = rootNodeInitialTransform;
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
			if (!(rootMotion && rootMotionNode != null && rootMotionNode.name == node.name))
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
					Node mirroredNode = animationData.skeleton.getNode(mirroredName);
					nodeID = mirroredNode != null ? mirroredNode.id : -1;
				}
				if (nodeID != -1)
				{
					Animation_AnimateNode(nodeID, animation, timer, (byte)(looping ? 1 : 0), ref nodeAnimationLocalTransforms[node.id]);
					if (mirrored)
					{
						nodeAnimationLocalTransforms[node.id].m30 *= -1.0f;
						nodeAnimationLocalTransforms[node.id].m01 *= -1.0f;
						nodeAnimationLocalTransforms[node.id].m02 *= -1.0f;
						nodeAnimationLocalTransforms[node.id].m10 *= -1.0f;
						nodeAnimationLocalTransforms[node.id].m20 *= -1.0f;
					}
				}
			}

			if (node.children != null)
			{
				for (int i = 0; i < node.children.Length; i++)
				{
					animateNode(node.children[i], animation, timer);
				}
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
			Node node = animationData.skeleton.getNode(name);
			if (node != null)
			{
				if (mask != null)
					return mask[node.id];
				return true;
			}
			return false;
		}

		public unsafe float duration
		{
			get => getAnimationByName(animationData.scene, animationName)->duration;
		}

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe void Animation_AnimateNode(int nodeID, AnimationData* animation, float timer, byte looping, ref Matrix outTransform);
	}
}
