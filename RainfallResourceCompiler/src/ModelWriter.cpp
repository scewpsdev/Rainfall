#include "ModelWriter.h"


using namespace bx;


static void WriteAABB(FileWriterI* writer, const AABB& aabb, Error* err)
{
	write(writer, aabb.x0, err);
	write(writer, aabb.y0, err);
	write(writer, aabb.z0, err);
	write(writer, aabb.x1, err);
	write(writer, aabb.y1, err);
	write(writer, aabb.z1, err);
}

static void WriteSphere(FileWriterI* writer, const Sphere& sphere, Error* err)
{
	write(writer, sphere.xcenter, err);
	write(writer, sphere.ycenter, err);
	write(writer, sphere.zcenter, err);
	write(writer, sphere.radius, err);
}

static void WriteMesh(FileWriterI* writer, const MeshData& mesh, Error* err)
{
	write(writer, mesh.positions ? 1 : 0, err);
	write(writer, mesh.normals ? 1 : 0, err);
	write(writer, mesh.tangents ? 1 : 0, err);
	write(writer, mesh.texcoords ? 1 : 0, err);
	write(writer, mesh.vertexColors ? 1 : 0, err);
	write(writer, mesh.boneWeights ? 1 : 0, err);

	write(writer, mesh.vertexCount, err);

	if (mesh.positions)
		write(writer, mesh.positions, mesh.vertexCount * sizeof(Vector3), err);
	if (mesh.normals)
		write(writer, mesh.normals, mesh.vertexCount * sizeof(Vector3), err);
	if (mesh.tangents)
		write(writer, mesh.tangents, mesh.vertexCount * sizeof(Vector3), err);
	if (mesh.texcoords)
		write(writer, mesh.texcoords, mesh.vertexCount * sizeof(Vector2), err);
	if (mesh.vertexColors)
		write(writer, mesh.vertexColors, mesh.vertexCount * sizeof(uint32_t), err);
	if (mesh.boneWeights)
		write(writer, mesh.boneWeights, mesh.vertexCount * sizeof(BoneWeights), err);

	write(writer, mesh.indexCount, err);
	write(writer, mesh.indexData, mesh.indexCount * sizeof(int), err);

	write(writer, mesh.materialID, err);
	write(writer, mesh.skeletonID, err);

	WriteAABB(writer, mesh.boundingBox, err);
	WriteSphere(writer, mesh.boundingSphere, err);
}

static void WriteMeshes(FileWriterI* writer, const SceneData& scene, Error* err)
{
	for (int i = 0; i < scene.numMeshes; i++)
	{
		WriteMesh(writer, scene.meshes[i], err);
	}
}

static void WriteTexture(FileWriterI* writer, const TextureData& texture, Error* err)
{
	write(writer, texture.path, sizeof(texture.path), err);
	write(writer, (int)texture.isEmbedded, err);

	if (texture.isEmbedded)
	{
		write(writer, texture.width, err);
		write(writer, texture.height, err);
		if (texture.height == 0)
			write(writer, texture.data, texture.width, err);
		else
			write(writer, texture.data, texture.width * texture.height * sizeof(uint32_t), err);
	}
}

static void WriteMaterial(FileWriterI* writer, const MaterialData& material, Error* err)
{
	write(writer, material.color, err);
	write(writer, material.metallicFactor, err);
	write(writer, material.roughnessFactor, err);
	write(writer, material.emissiveColor, err);
	write(writer, material.emissiveStrength, err);

	write(writer, material.diffuse ? 1 : 0, err);
	write(writer, material.normal ? 1 : 0, err);
	write(writer, material.roughness ? 1 : 0, err);
	write(writer, material.metallic ? 1 : 0, err);
	write(writer, material.emissive ? 1 : 0, err);

	if (material.diffuse)
		WriteTexture(writer, *material.diffuse, err);
	if (material.normal)
		WriteTexture(writer, *material.normal, err);
	if (material.roughness)
		WriteTexture(writer, *material.roughness, err);
	if (material.metallic)
		WriteTexture(writer, *material.metallic, err);
	if (material.emissive)
		WriteTexture(writer, *material.emissive, err);
}

static void WriteMaterials(FileWriterI* writer, const SceneData& scene, Error* err)
{
	for (int i = 0; i < scene.numMaterials; i++)
	{
		WriteMaterial(writer, scene.materials[i], err);
	}
}

static void WriteSkeleton(FileWriterI* writer, const SkeletonData& skeleton, Error* err)
{
	write(writer, skeleton.boneCount, err);
	write(writer, skeleton.bones, skeleton.boneCount * sizeof(BoneData), err);
}

static void WriteSkeletons(FileWriterI* writer, const SceneData& scene, Error* err)
{
	for (int i = 0; i < scene.numSkeletons; i++)
	{
		WriteSkeleton(writer, scene.skeletons[i], err);
	}
}

static void WriteAnimation(FileWriterI* writer, const AnimationData& animation, Error* err)
{
	write(writer, animation.name, sizeof(animation.name), err);

	write(writer, animation.duration, err);

	write(writer, animation.numPositions, err);
	write(writer, animation.numRotations, err);
	write(writer, animation.numScales, err);
	write(writer, animation.numChannels, err);

	write(writer, animation.positionKeyframes, animation.numPositions * sizeof(PositionKeyframe), err);
	write(writer, animation.rotationKeyframes, animation.numRotations * sizeof(RotationKeyframe), err);
	write(writer, animation.scaleKeyframes, animation.numScales * sizeof(ScaleKeyframe), err);
	write(writer, animation.channels, animation.numChannels * sizeof(AnimationChannel), err);
}

static void WriteAnimations(FileWriterI* writer, const SceneData& scene, Error* err)
{
	for (int i = 0; i < scene.numAnimations; i++)
	{
		WriteAnimation(writer, scene.animations[i], err);
	}
}

static void WriteNode(FileWriterI* writer, const NodeData& node, Error* err)
{
	write(writer, node.id, err);
	write(writer, node.name, sizeof(node.name), err);
	write(writer, node.transform, sizeof(node.transform), err);

	write(writer, node.numChildren, err);
	write(writer, node.numMeshes, err);

	write(writer, node.children, node.numChildren * sizeof(int), err);
	write(writer, node.meshes, node.numMeshes * sizeof(int), err);
}

static void WriteNodes(FileWriterI* writer, const SceneData& scene, Error* err)
{
	for (int i = 0; i < scene.numNodes; i++)
	{
		WriteNode(writer, scene.nodes[i], err);
	}
}

static void WriteLight(FileWriterI* writer, const LightData& light, Error* err)
{
	write(writer, light.name, sizeof(light.name), err);

	write(writer, light.type, err);
	write(writer, light.x, err);
	write(writer, light.y, err);
	write(writer, light.z, err);
	write(writer, light.xdir, err);
	write(writer, light.ydir, err);
	write(writer, light.zdir, err);
	write(writer, light.color, err);
}

static void WriteLights(FileWriterI* writer, const SceneData& scene, Error* err)
{
	for (int i = 0; i < scene.numLights; i++)
	{
		WriteLight(writer, scene.lights[i], err);
	}
}

void WriteSceneData(FileWriterI* writer, const SceneData& scene, const char* out)
{
	Error err;

	if (open(writer, out))
	{
		write(writer, scene.numMeshes, &err);
		write(writer, scene.numMaterials, &err);
		write(writer, scene.numSkeletons, &err);
		write(writer, scene.numAnimations, &err);
		write(writer, scene.numNodes, &err);
		write(writer, scene.numLights, &err);

		WriteMeshes(writer, scene, &err);
		WriteMaterials(writer, scene, &err);
		WriteSkeletons(writer, scene, &err);
		WriteAnimations(writer, scene, &err);
		WriteNodes(writer, scene, &err);
		WriteLights(writer, scene, &err);

		write(writer, scene.boundingBox, &err);
		write(writer, scene.boundingSphere, &err);

		close(writer);
	}
}
