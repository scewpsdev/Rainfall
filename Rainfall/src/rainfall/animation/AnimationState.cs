using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public class AnimationState
	{
		Model model;
		public AnimationLayer[] layers;
		public float transitionDuration;
		public float transitionFromDuration = -1.0f;
		public float animationSpeed = 1.0f;

		public Matrix[] nodeAnimationLocalTransforms { get; private set; }


		internal AnimationState(Model model, AnimationLayer[] layers, float transitionDuration = 0.0f)
		{
			this.model = model;
			this.layers = layers;
			this.transitionDuration = transitionDuration;

			nodeAnimationLocalTransforms = new Matrix[model.skeleton.nodes.Length];
			Array.Fill(nodeAnimationLocalTransforms, Matrix.Identity);
		}

		public Matrix getNodeLocalTransform(Node node)
		{
			if (layers.Length > 0)
				return layers[0].nodeAnimationLocalTransforms[node.id];
			return Matrix.Identity;
		}

		public void setNodeLocalTransform(Node node, Matrix transform)
		{
			if (layers.Length > 0)
				layers[0].nodeAnimationLocalTransforms[node.id] = transform;
		}

		public Matrix getNodeTransform(Node node, Model model, int skeletonID)
		{
			unsafe
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
		}

		internal void update(/*Model model,*/ float timer)
		{
			for (int i = 0; i < layers.Length; i++)
			{
				if (layers[i] != null && layers[i].active)
				{
					layers[i].update(timer);
					for (int j = 0; j < nodeAnimationLocalTransforms.Length; j++)
					{
						string nodeName = model.skeleton.nodes[j].name;
						if (layers[i].hasNode(nodeName))
							nodeAnimationLocalTransforms[j] = layers[i].getNodeLocalTransform(nodeName);
					}
				}
			}
		}

		internal void updateRootMotion(float timer)
		{
			for (int i = 0; i < layers.Length; i++)
			{
				if (layers[i] != null && layers[i].active)
				{
					layers[i].updateRootMotion(timer);
				}
			}
		}
	}
}
