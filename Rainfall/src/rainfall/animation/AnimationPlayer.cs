using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public struct AnimationTransition
	{
		public AnimationState from;
		public AnimationState to;
		public float duration;

		public AnimationTransition(AnimationState from, AnimationState to, float duration)
		{
			this.from = from;
			this.to = to;
			this.duration = duration;
		}
	}

	public class Animator
	{
		internal IntPtr handle;
		public Model model { get; private set; }

		List<AnimationState> states = new List<AnimationState>();
		List<float> stateAnimationTimers = new List<float>();
		List<float> stateTransitionTimers = new List<float>();

		public Matrix[] nodeLocalTransforms { get; private set; }
		Matrix[] nodeGlobalTransforms;
		long currentUpdateTime;

		Matrix[] lastNodeGlobalTransforms;
		long lastAnimUpdateTime;

		public readonly List<AnimationTransition> transitions = new List<AnimationTransition>();


		public unsafe Animator(Model model)
		{
			this.model = model;

			handle = Native.Animation.Animation_CreateAnimationState(model.scene);

			unsafe
			{
				int numNodes = model.scene->numNodes;
				nodeLocalTransforms = new Matrix[numNodes];
				nodeGlobalTransforms = new Matrix[numNodes];
				Array.Fill(nodeLocalTransforms, Matrix.Identity);
				Array.Fill(nodeGlobalTransforms, Matrix.Identity);

				lastNodeGlobalTransforms = new Matrix[numNodes];
				Array.Fill(lastNodeGlobalTransforms, Matrix.Identity);
			}
		}

		public void destroy()
		{
			Native.Animation.Animation_DestroyAnimationState(handle);
		}

		float getTransitionDuration(AnimationState from, AnimationState to)
		{
			foreach (AnimationTransition transition in transitions)
			{
				if (transition.from == from && transition.to == to)
				{
					//Console.WriteLine(transition.duration);
					return transition.duration;
				}
			}
			if (from.transitionFromDuration != -1.0f)
				return from.transitionFromDuration;
			return to.transitionDuration;
		}

		public void update()
		{
			/*
			for (int i = 0; i < states.Count; i++)
			{
				Console.Write(states[i].layers[0].animationName + ":" + stateAnimationTimers[i]);
				if (i < states.Count - 1)
					Console.Write(",");
				else
					Console.WriteLine();
			}
			*/

			for (int i = 0; i < states.Count; i++)
				stateAnimationTimers[i] += Time.deltaTime * states[i].animationSpeed;

			for (int i = 0; i < states.Count; i++)
				stateTransitionTimers[i] += Time.deltaTime;

			if (states.Count > 0)
			{
				//states[states.Count - 1].update(model, stateAnimationTimers[states.Count - 1]);
				for (int i = 0; i < states.Count - 1; i++)
				{
					//states[i].update(model, stateAnimationTimers[i]);
					AnimationState nextState = states[i + 1];
					float transitionDuration = getTransitionDuration(states[i], nextState);
					if (stateTransitionTimers[i + 1] > transitionDuration)
					{
						for (int j = i; j >= 0; j--)
						{
							states.RemoveAt(j);
							stateAnimationTimers.RemoveAt(j);
							stateTransitionTimers.RemoveAt(j);
						}
						i = 0;
					}
				}
			}

			if (states.Count > 0)
			{
				states[0].update(model, stateAnimationTimers[0]);
				Array.Copy(states[0].nodeAnimationLocalTransforms, nodeLocalTransforms, Math.Min(states[0].nodeAnimationLocalTransforms.Length, nodeLocalTransforms.Length));

				for (int i = 1; i < states.Count; i++)
				{
					states[i].update(model, stateAnimationTimers[i]);

					float transitionDuration = getTransitionDuration(states[i - 1], states[i]);
					float transitionProgress = Math.Clamp(stateTransitionTimers[i] / transitionDuration, 0.0f, 1.0f);
					Matrix[] transforms0 = nodeLocalTransforms;
					Matrix[] transforms1 = states[i].nodeAnimationLocalTransforms;
					for (int j = 0; j < nodeLocalTransforms.Length; j++)
					{
						Vector3 position0 = transforms0[j].translation;
						Vector3 position1 = transforms1[j].translation;
						Quaternion rotation0 = transforms0[j].rotation;
						Quaternion rotation1 = transforms1[j].rotation;
						Vector3 scale0 = transforms0[j].scale;
						Vector3 scale1 = transforms1[j].scale;
						Vector3 position = Vector3.Lerp(position0, position1, transitionProgress);
						Quaternion rotation = Quaternion.Slerp(rotation0, rotation1, transitionProgress);
						Vector3 scale = Vector3.Lerp(scale0, scale1, transitionProgress);
						nodeLocalTransforms[j] = Matrix.CreateTransform(position, rotation, scale);
					}
				}
			}
		}

		void applyNodeAnimation(Node node, Matrix parentTransform, Matrix[] nodeLocalTransforms)
		{
			nodeGlobalTransforms[node.id] = parentTransform * nodeLocalTransforms[node.id];

			for (int i = 0; i < node.children.Length; i++)
			{
				applyNodeAnimation(node.children[i], nodeGlobalTransforms[node.id], nodeLocalTransforms);
			}
		}

		public unsafe void applyAnimation()
		{
			Array.Copy(nodeGlobalTransforms, lastNodeGlobalTransforms, nodeGlobalTransforms.Length);
			lastAnimUpdateTime = currentUpdateTime;
			currentUpdateTime = Time.currentTime;

			applyNodeAnimation(model.skeleton.rootNode, Matrix.Identity, nodeLocalTransforms);
			Native.Animation.Animation_UpdateAnimationState(handle, model.scene, nodeGlobalTransforms, nodeGlobalTransforms.Length);
		}

		public void setState(AnimationState state)
		{
			//if (states.Count == 0 || state != states[states.Count - 1])
			{
				states.Add(state);
				stateTransitionTimers.Add(0.0f);
				stateAnimationTimers.Add(0.0f);

				//if (!state.layers[0].looping)
				//	state.timer = 0.0f;
			}
		}

		public void setStateIfNot(AnimationState state)
		{
			if (states.Count == 0 || state != states[states.Count - 1])
				setState(state);
		}

		public void setTimer(float timer)
		{
			stateAnimationTimers[stateAnimationTimers.Count - 1] = timer;
		}

		public AnimationState getState()
		{
			return states.Count > 0 ? states[states.Count - 1] : null;
		}

		public Matrix getNodeLocalTransform(Node node)
		{
			return nodeLocalTransforms[node.id];
		}

		public void setNodeLocalTransform(Node node, Matrix transform)
		{
			nodeLocalTransforms[node.id] = transform;
		}

		public unsafe Matrix getNodeTransform(Node node, int skeletonID)
		{
			Matrix inverseBindPose = model.scene->skeletons[skeletonID].inverseBindPose;
			Matrix transform = getNodeLocalTransform(node);
			while (node.parent != null)
			{
				transform = getNodeLocalTransform(node.parent) * transform;
				node = node.parent;
			}
			return inverseBindPose * transform;
		}

		public void copyPose(Animator from)
		{
			Array.Copy(from.nodeLocalTransforms, nodeLocalTransforms, nodeLocalTransforms.Length);
		}

		public void getNodeVelocity(Node node, out Vector3 velocity, out Quaternion rotationVelocity)
		{
			Matrix currentTransform = nodeGlobalTransforms[node.id];
			Vector3 currentPosition = currentTransform.translation;
			Quaternion currentRotation = currentTransform.rotation;

			Matrix lastTransform = lastNodeGlobalTransforms[node.id];
			Vector3 lastPosition = lastTransform.translation;
			Quaternion lastRotation = lastTransform.rotation;

			float dt = (currentUpdateTime - lastAnimUpdateTime) / 1e9f;

			velocity = (currentPosition - lastPosition) / dt;
			rotationVelocity = ((lastRotation.conjugated * currentRotation) * (1.0f / dt)).normalized;
		}

		public bool isPlaying
		{
			get
			{
				if (states.Count > 0)
				{
					AnimationState state = states[states.Count - 1];
					return state.layers[0].looping || stateAnimationTimers[states.Count - 1] < state.layers[0].duration;
				}
				return false;
			}
		}

		public AnimationState currentAnimation
		{
			get { return states.Count > 0 ? states[states.Count - 1] : null; }
		}

		public float timer
		{
			get => stateAnimationTimers[states.Count - 1];
			set
			{
				stateAnimationTimers[states.Count - 1] = value;
			}
		}
	}
}
