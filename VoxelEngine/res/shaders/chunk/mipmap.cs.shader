

#include "../bgfx/bgfx_compute.shader"
#include "voxel_format.shader"


IMAGE3D_RO(u_src, rg8, 0);
IMAGE3D_WR(u_dst, rg8, 1);


NUM_THREADS(8, 8, 8)
void main()
{
	ivec3 size = imageSize(u_dst);
	if (gl_GlobalInvocationID.x >= size.x || gl_GlobalInvocationID.y >= size.y || gl_GlobalInvocationID.z >= size.z)
		return;
	
	ivec3 samplePoint = gl_GlobalInvocationID;

	int value0 = int(imageLoad(u_src, samplePoint * 2 + ivec3(0, 0, 0)).r * 255 + 0.5);
	int value1 = int(imageLoad(u_src, samplePoint * 2 + ivec3(1, 0, 0)).r * 255 + 0.5);
	int value2 = int(imageLoad(u_src, samplePoint * 2 + ivec3(0, 1, 0)).r * 255 + 0.5);
	int value3 = int(imageLoad(u_src, samplePoint * 2 + ivec3(1, 1, 0)).r * 255 + 0.5);
	int value4 = int(imageLoad(u_src, samplePoint * 2 + ivec3(0, 0, 1)).r * 255 + 0.5);
	int value5 = int(imageLoad(u_src, samplePoint * 2 + ivec3(1, 0, 1)).r * 255 + 0.5);
	int value6 = int(imageLoad(u_src, samplePoint * 2 + ivec3(0, 1, 1)).r * 255 + 0.5);
	int value7 = int(imageLoad(u_src, samplePoint * 2 + ivec3(1, 1, 1)).r * 255 + 0.5);

	bool empty = value0 == 0 && value1 == 0 && value2 == 0 && value3 == 0 && value4 == 0 && value5 == 0 && value6 == 0 && value7 == 0;
	bool leaf = value0 == 2 && value1 == 2 && value2 == 2 && value3 == 2 && value4 == 2 && value5 == 2 && value6 == 2 && value7 == 2;
	int value = empty ? 0 : leaf ? 2 : 1;

	imageStore(u_dst, samplePoint, vec4(value / 255.0, 0.0, 0.0, 0.0));

	/*
	int value0; vec3 normal0; int material0; decodeVoxelData(imageLoad(u_src, samplePoint * 2 + ivec3(0, 0, 0)).rg, value0, normal0, material0);
	int value1; vec3 normal1; int material1; decodeVoxelData(imageLoad(u_src, samplePoint * 2 + ivec3(1, 0, 0)).rg, value1, normal1, material1);
	int value2; vec3 normal2; int material2; decodeVoxelData(imageLoad(u_src, samplePoint * 2 + ivec3(0, 1, 0)).rg, value2, normal2, material2);
	int value3; vec3 normal3; int material3; decodeVoxelData(imageLoad(u_src, samplePoint * 2 + ivec3(1, 1, 0)).rg, value3, normal3, material3);
	int value4; vec3 normal4; int material4; decodeVoxelData(imageLoad(u_src, samplePoint * 2 + ivec3(0, 0, 1)).rg, value4, normal4, material4);
	int value5; vec3 normal5; int material5; decodeVoxelData(imageLoad(u_src, samplePoint * 2 + ivec3(1, 0, 1)).rg, value5, normal5, material5);
	int value6; vec3 normal6; int material6; decodeVoxelData(imageLoad(u_src, samplePoint * 2 + ivec3(0, 1, 1)).rg, value6, normal6, material6);
	int value7; vec3 normal7; int material7; decodeVoxelData(imageLoad(u_src, samplePoint * 2 + ivec3(1, 1, 1)).rg, value7, normal7, material7);

	bool empty = value0 == 0 && value1 == 0 && value2 == 0 && value3 == 0 && value4 == 0 && value5 == 0 && value6 == 0 && value7 == 0;
	bool leaf = value0 == 2 && value1 == 2 && value2 == 2 && value3 == 2 && value4 == 2 && value5 == 2 && value6 == 2 && value7 == 2;
	int value = empty ? 0 : leaf ? 2 : 1;

	vec3 normal = mix(
		mix(
			mix(normal0, normal1, 0.5),
			mix(normal2, normal3, 0.5),
			0.5
		),
		mix(
			mix(normal4, normal5, 0.5),
			mix(normal6, normal7, 0.5),
			0.5
		),
		0.5
	);
	vec2 result = encodeVoxelData(value, normal, 0);

	imageStore(u_dst, samplePoint, vec4(value, 0.0, 0.0, 0.0));
	*/
}
