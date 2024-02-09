#include "Animation.h"

#include "Rainfall.h"
#include "Application.h"

#include "graphics/Model.h"
#include "vector/Math.h"

#include <bx/allocator.h>

#include <stdio.h>


static AnimationChannel* GetAnimationChannel(AnimationData* animation, int nodeID)
{
	for (int i = 0; i < animation->numChannels; i++)
	{
		if (animation->channels[i].node->id == nodeID)
			return &animation->channels[i];
	}
	return nullptr;
}

static Vector3 AnimatePosition(AnimationChannel& channel, AnimationData& animation, float timer, bool looping)
{
	if (channel.positionsCount == 1)
		return channel.positions[0].value;

	for (int i = channel.positionsCount - 1; i >= 0; i--)
	{
		float keyframeTime = channel.positions[i].time;

		if (timer >= keyframeTime)
		{
			int nextKeyframeIdx = (i + 1) % channel.positionsCount;
			PositionKeyframe& keyframe0 = channel.positions[i];
			PositionKeyframe& keyframe1 = channel.positions[nextKeyframeIdx];
			float time0 = keyframe0.time;
			float time1 = keyframe1.time >= keyframe0.time ? keyframe1.time : keyframe1.time + animation.duration;
			float blend = clamp(keyframe0.time != keyframe1.time ? (timer - time0) / (time1 - time0) : 0.0f, 0.0f, 1.0f);
			return mix(keyframe0.value, keyframe1.value, blend);
		}
	}

	PositionKeyframe& keyframe0 = channel.positions[channel.positionsCount - 1];
	PositionKeyframe& keyframe1 = channel.positions[0];
	float blend = (timer - (keyframe0.time - animation.duration)) / (keyframe1.time - (keyframe0.time - animation.duration));
	return mix(keyframe0.value, keyframe1.value, blend);
}

static Quaternion AnimateRotation(AnimationChannel& channel, AnimationData& animation, float timer, bool looping)
{
	if (channel.rotationsCount == 1)
		return channel.rotations[0].value;

	for (int i = channel.rotationsCount - 1; i >= 0; i--)
	{
		float keyframeTime = channel.rotations[i].time;

		if (timer >= keyframeTime)
		{
			int nextKeyframeIdx = (i + 1) % channel.rotationsCount;
			RotationKeyframe& keyframe0 = channel.rotations[i];
			RotationKeyframe& keyframe1 = channel.rotations[nextKeyframeIdx];
			float time0 = keyframe0.time;
			float time1 = keyframe1.time >= keyframe0.time ? keyframe1.time : keyframe1.time + animation.duration;
			float blend = clamp(keyframe0.time != keyframe1.time ? (timer - time0) / (time1 - time0) : 0.0f, 0.0f, 1.0f);
			return slerp(keyframe0.value, keyframe1.value, blend);
		}
	}

	if (looping)
	{
		RotationKeyframe& keyframe0 = channel.rotations[channel.rotationsCount - 1];
		RotationKeyframe& keyframe1 = channel.rotations[0];
		float blend = (timer - (keyframe0.time - animation.duration)) / (keyframe1.time - (keyframe0.time - animation.duration));
		return slerp(keyframe0.value, keyframe1.value, blend);
	}
	else
	{
		return channel.rotations[0].value;
	}
}

static Vector3 AnimateScale(AnimationChannel& channel, AnimationData& animation, float timer, bool looping)
{
	if (channel.scalesCount == 1)
		return channel.scales[0].value;

	for (int i = channel.scalesCount - 1; i >= 0; i--)
	{
		float keyframeTime = channel.scales[i].time;

		if (timer >= keyframeTime)
		{
			int nextKeyframeIdx = (i + 1) % channel.scalesCount;
			ScaleKeyframe& keyframe0 = channel.scales[i];
			ScaleKeyframe& keyframe1 = channel.scales[nextKeyframeIdx];
			float time0 = keyframe0.time;
			float time1 = keyframe1.time >= keyframe0.time ? keyframe1.time : keyframe1.time + animation.duration;
			float blend = clamp(keyframe0.time != keyframe1.time ? (timer - time0) / (time1 - time0) : 0.0f, 0.0f, 1.0f);
			return mix(keyframe0.value, keyframe1.value, blend);
		}
	}

	ScaleKeyframe& keyframe0 = channel.scales[channel.scalesCount - 1];
	ScaleKeyframe& keyframe1 = channel.scales[0];
	float blend = (timer - (keyframe0.time - animation.duration)) / (keyframe1.time - (keyframe0.time - animation.duration));
	return mix(keyframe0.value, keyframe1.value, blend);
}

RFAPI void Animation_AnimateNode(int nodeID, AnimationData* animation, float timer, bool looping, Matrix* outTransform)
{
	if (animation)
	{
		if (AnimationChannel* channel = GetAnimationChannel(animation, nodeID))
		{
			Vector3 position = AnimatePosition(*channel, *animation, timer, looping);
			Quaternion rotation = AnimateRotation(*channel, *animation, timer, looping);
			Vector3 scale = AnimateScale(*channel, *animation, timer, looping);

			*outTransform = Matrix::Transform(position, rotation, scale);
		}
	}
}

static Matrix GetNodeTransform(NodeData* node)
{
	Matrix transform = node->transform;
	NodeData* parent = node->parent;
	while (parent)
	{
		transform = parent->transform * transform;
		parent = parent->parent;
	}
	return transform;
}

RFAPI AnimationState* Animation_CreateAnimationState(SceneData* scene)
{
	AnimationState* state = BX_NEW(Application_GetAllocator(), AnimationState);

	state->numSkeletons = scene->numMeshes;
	state->skeletons = (SkeletonState**)BX_ALLOC(Application_GetAllocator(), sizeof(SkeletonState*) * state->numSkeletons);

	for (int i = 0; i < scene->numMeshes; i++)
	{
		if (scene->meshes[i].skeletonID != -1)
		{
			SkeletonData& skeletonData = scene->skeletons[scene->meshes[i].skeletonID];
			state->skeletons[i] = BX_NEW(Application_GetAllocator(), SkeletonState);
			state->skeletons[i]->numBones = skeletonData.boneCount;
			state->skeletons[i]->boneTransforms = (Matrix*)BX_ALLOC(Application_GetAllocator(), sizeof(Matrix) * skeletonData.boneCount);
			for (int j = 0; j < skeletonData.boneCount; j++)
			{
				Matrix nodeTransform = GetNodeTransform(skeletonData.bones[j].node);
				state->skeletons[i]->boneTransforms[j] = Matrix::Identity;
			}
		}
		else
		{
			state->skeletons[i] = nullptr;
		}
	}

	return state;
}

RFAPI void Animation_DestroyAnimationState(AnimationState* state)
{
	for (int i = 0; i < state->numSkeletons; i++)
	{
		if (state->skeletons[i])
		{
			BX_FREE(Application_GetAllocator(), state->skeletons[i]->boneTransforms);
			BX_FREE(Application_GetAllocator(), state->skeletons[i]);
		}
	}

	BX_FREE(Application_GetAllocator(), state->skeletons);
	BX_FREE(Application_GetAllocator(), state);
}

RFAPI void Animation_UpdateAnimationState(AnimationState* state, SceneData* scene, const Matrix* nodeAnimationTransforms, int numNodes)
{
	for (int i = 0; i < state->numSkeletons; i++)
	{
		if (SkeletonState* skeleton = state->skeletons[i])
		{
			int skeletonID = scene->meshes[i].skeletonID;
			const SkeletonData& skeletonData = scene->skeletons[skeletonID];
			for (int j = 0; j < skeleton->numBones; j++)
			{
				if (const NodeData* node = skeletonData.bones[j].node)
				{
					skeleton->boneTransforms[j] = skeletonData.inverseBindPose * nodeAnimationTransforms[node->id] * skeletonData.bones[j].offsetMatrix;
				}
			}
		}
	}
}
