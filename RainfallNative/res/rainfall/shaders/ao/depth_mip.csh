#include "../bgfx/bgfx_compute.shader"
#include "../common/common.shader"


IMAGE2D_RO(u_src, r32f, 0);
IMAGE2D_WR(u_dst, r32f, 1);
IMAGE2D_RO(u_srcNormal, rgba16f, 2);
IMAGE2D_WR(u_dstNormal, rgba16f, 3);

uniform vec4 u_cameraParams;
#define u_near u_cameraParams[0]
#define u_far u_cameraParams[1]


void swap(inout vec4 val1, inout vec4 val2)
{
	vec4 tmp = val1;
	val1 = val2;
	val2 = tmp;
}

NUM_THREADS(16, 16, 1)
void main()
{
	ivec2 coord = gl_GlobalInvocationID.xy;
	ivec2 size = imageSize(u_dst);
	if (coord.x >= size.x || coord.y >= size.y)
		return;

	vec2 ratio = imageSize(u_src) / vec2(size);
	
	ivec2 coord0 = coord * ratio;
	ivec2 coord1 = coord0 + ivec2(1, 0);
	ivec2 coord2 = coord0 + ivec2(0, 1);
	ivec2 coord3 = coord0 + ivec2(1, 1);

	float depth0 = depthToDistance(imageLoad(u_src, coord0).r, u_near, u_far);
	float depth1 = depthToDistance(imageLoad(u_src, coord1).r, u_near, u_far);
	float depth2 = depthToDistance(imageLoad(u_src, coord2).r, u_near, u_far);
    float depth3 = depthToDistance(imageLoad(u_src, coord3).r, u_near, u_far);
	
	vec3 normal0 = imageLoad(u_srcNormal, coord0).rgb * 2 - 1;
	vec3 normal1 = imageLoad(u_srcNormal, coord1).rgb * 2 - 1;
	vec3 normal2 = imageLoad(u_srcNormal, coord2).rgb * 2 - 1;
	vec3 normal3 = imageLoad(u_srcNormal, coord3).rgb * 2 - 1;
	
	vec4 sample0 = vec4(normal0, depth0);
	vec4 sample1 = vec4(normal1, depth1);
	vec4 sample2 = vec4(normal2, depth2);
	vec4 sample3 = vec4(normal3, depth3);
	
	for (int i = 0; i < 3; i++)
	{
		if (sample0.w > sample1.w)
			swap(sample0, sample1);
		if (sample1.w > sample2.w)
			swap(sample1, sample2);
		if (sample2.w > sample3.w)
			swap(sample2, sample3);
	}
	
	float depthThreshhold = 0.1;
	float depthDiff = sample3.w - sample0.w;
    vec4 result = depthDiff <= depthThreshhold ? 0.5 * sample1 + 0.5 * sample2 : sample1;

    imageStore(u_dst, coord, vec4(distanceToDepth(result.w, u_near, u_far), 0, 0, 1));
	imageStore(u_dstNormal, coord, vec4(result.xyz * 0.5 + 0.5, 1));
}
