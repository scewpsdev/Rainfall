

#include "../bgfx/bgfx_compute.shader"


IMAGE3D_RO(u_src, rg8, 0);
IMAGE3D_WR(u_dst, rg8, 1);


NUM_THREADS(8, 8, 8)
void main()
{
	ivec3 size = imageSize(u_dst);
	if (gl_GlobalInvocationID.x >= size.x || gl_GlobalInvocationID.y >= size.y || gl_GlobalInvocationID.z >= size.z)
		return;
	
	ivec3 samplePoint = gl_GlobalInvocationID;

	uint value0 = uint(imageLoad(u_src, samplePoint * 2 + ivec3(0, 0, 0)).r * 255 + 0.5);
	uint value1 = uint(imageLoad(u_src, samplePoint * 2 + ivec3(1, 0, 0)).r * 255 + 0.5);
	uint value2 = uint(imageLoad(u_src, samplePoint * 2 + ivec3(0, 1, 0)).r * 255 + 0.5);
	uint value3 = uint(imageLoad(u_src, samplePoint * 2 + ivec3(1, 1, 0)).r * 255 + 0.5);
	uint value4 = uint(imageLoad(u_src, samplePoint * 2 + ivec3(0, 0, 1)).r * 255 + 0.5);
	uint value5 = uint(imageLoad(u_src, samplePoint * 2 + ivec3(1, 0, 1)).r * 255 + 0.5);
	uint value6 = uint(imageLoad(u_src, samplePoint * 2 + ivec3(0, 1, 1)).r * 255 + 0.5);
	uint value7 = uint(imageLoad(u_src, samplePoint * 2 + ivec3(1, 1, 1)).r * 255 + 0.5);

	bool empty = value0 == 0 && value1 == 0 && value2 == 0 && value3 == 0 && value4 == 0 && value5 == 0 && value6 == 0 && value7 == 0;
	bool leaf = value0 == 2 && value1 == 2 && value2 == 2 && value3 == 2 && value4 == 2 && value5 == 2 && value6 == 2 && value7 == 2;
	uint value = empty ? 0 : leaf ? 2 : 1;

	imageStore(u_dst, samplePoint, vec4(value, 0.0, 0.0, 0.0));
}
