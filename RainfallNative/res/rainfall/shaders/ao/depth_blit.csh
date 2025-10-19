#include "../bgfx/bgfx_compute.shader"


IMAGE2D_RO(u_src, r32f, 0);
IMAGE2D_WR(u_dst, r32f, 1);
IMAGE2D_RO(u_srcNormal, rgba16f, 2);
IMAGE2D_WR(u_dstNormal, rgba16f, 3);

uniform mat4 u_viewMatrix;


NUM_THREADS(16, 16, 1)
void main()
{
	ivec2 coord = gl_GlobalInvocationID.xy;
	ivec2 size = imageSize(u_dst);
    if (coord.x >= size.x || coord.y >= size.y)
		return;

	vec2 ratio = imageSize(u_src) / vec2(size);
    ivec2 coord2 = coord * ratio;
    
    vec4 depth = imageLoad(u_src, coord2);
    
    vec4 normal = imageLoad(u_srcNormal, coord2);
    normal.xyz = normal.xyz * 2 - 1;
    vec3 normalViewSpace = mul(u_viewMatrix, vec4(normal.xyz, 0.0)).xyz;
    normal.xyz = normalViewSpace * 0.5 + 0.5;
    
    imageStore(u_dst, coord, depth);
    imageStore(u_dstNormal, coord, normal);
}
