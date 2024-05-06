#include "../bgfx/bgfx_compute.shader"


IMAGE2D_RO(u_src, r16f, 0);
IMAGE2D_WR(u_dst, r16f, 1);


NUM_THREADS(16, 16, 1)
void main()
{
	ivec2 size = imageSize(u_dst);
	if (gl_GlobalInvocationID.x >= size.x || gl_GlobalInvocationID.y >= size.y)
		return;
	
	vec2 samplePoint = (gl_GlobalInvocationID.xy + 0.5) / vec2(size);
	ivec2 sample0 = ivec2(samplePoint * imageSize(u_src) - vec2_splat(0.25));
	ivec2 sample1 = sample0 + ivec2(1, 0);
	ivec2 sample2 = sample0 + ivec2(0, 1);
	ivec2 sample3 = sample0 + ivec2(1, 1);

	float value0 = imageLoad(u_src, sample0).r;
	float value1 = imageLoad(u_src, sample1).r;
	float value2 = imageLoad(u_src, sample2).r;
	float value3 = imageLoad(u_src, sample3).r;
	
	float result = max(max(value0, value1), max(value2, value3));

	imageStore(u_dst, gl_GlobalInvocationID.xy, vec4(result, 0.0, 0.0, 0.0));
}
